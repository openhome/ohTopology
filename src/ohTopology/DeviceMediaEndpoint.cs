using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using System.Net;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using OpenHome.Os.App;

using OpenHome.Http;
using OpenHome.Http.Owin;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{

    // Discover upnp devices implementing the content diurectory service and either:
    //   * create a single media endpoint wrapper around a traditional upnp media server
    //   * identify it as an openhome device and enumerate its media endpoints accordingly

    public class DeviceInjectorMediaEndpoint : IDisposable
    {
        private readonly Network iNetwork;
        private readonly ILog iLog;

        private readonly DisposeHandler iDisposeHandler;
        private readonly Dictionary<string, IDisposable> iDevices;
        private readonly CpDeviceListUpnpServiceType iDeviceList;

        public DeviceInjectorMediaEndpoint(Network aNetwork, ILog aLog)
        {
            iNetwork = aNetwork;
            iLog = aLog;

            iDisposeHandler = new DisposeHandler();
            iDevices = new Dictionary<string, IDisposable>();
            iDeviceList = new CpDeviceListUpnpServiceType("upnp.org", "ContentDirectory", 1, Added, Removed);
        }

        internal INetwork Network
        {
            get
            {
                return (iNetwork);
            }
        }

        internal void AddDevice(IInjectorDevice aDevice)
        {
            // Console.WriteLine("Add    : {0}", aDevice.Udn);

            iNetwork.Add(aDevice);
        }

        internal void RemoveDevice(IInjectorDevice aDevice)
        {
            Console.WriteLine("Remove : {0}", aDevice.Udn);

            iNetwork.Remove(aDevice);
        }

        internal IEventSupervisorSession CreateEventSession(string aEndpoint)
        {
            return (iNetwork.EventSupervisor.Create(aEndpoint));
        }

        private string GetDeviceElementValue(IEnumerable<XElement> aElements, string aName)
        {
            var children = aElements.Descendants(XName.Get(aName, "urn:schemas-upnp-org:device-1-0"));

            if (children.Any())
            {
                return (children.First().Value);
            }

            return (null);
        }

        private string GetOpenHomeElementValue(IEnumerable<XElement> aElements, string aName)
        {
            var children = aElements.Descendants(XName.Get(aName, "http://www.openhome.org"));

            if (children.Any())
            {
                return (children.First().Value);
            }

            return (null);
        }

        private void Added(CpDeviceList aList, CpDevice aDevice)
        {
            var udn = aDevice.Udn();

            string deviceXml;

            aDevice.GetAttribute("Upnp.DeviceXml", out deviceXml);

            var reader = XmlReader.Create(new StringReader(deviceXml));

            var xml = XDocument.Load(reader);

            var elements = xml.Descendants(XName.Get("device", "urn:schemas-upnp-org:device-1-0"));

            var path = GetOpenHomeElementValue(elements, "X_PATH");

            if (path != null)
            {
                var presentation = GetDeviceElementValue(elements, "presentationURL");

                if (presentation != null)
                {
                    var presentationUri = new Uri(presentation);

                    // get the uri to the openhome node's property server

                    var uri = new Uri(presentationUri, path);

                    lock (iDevices)
                    {
                        var device = new DeviceInjectorDeviceOpenHome(this, udn, uri, aDevice, iLog);
                        iDevices.Add(udn, device);
                    }
                }
            }
            else
            {
                lock (iDevices)
                {
                    var device = new DeviceInjectorDeviceContentDirectory(this, udn, aDevice, xml, iLog);
                    iDevices.Add(udn, device);
                }
            }
        }

        private void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            var udn = aDevice.Udn();

            lock (iDevices)
            {
                IDisposable device;

                if (iDevices.TryGetValue(udn, out device))
                {
                    device.Dispose();
                    iDevices.Remove(udn);
                }
            }
        }

        public void Refresh()
        {
            using (iDisposeHandler.Lock)
            {
                iDeviceList.Refresh();
            }
        }

        // IDisposable

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iDeviceList.Dispose();

            lock (iDevices)
            {
                foreach (var device in iDevices.Values)
                {
                    device.Dispose();
                }
            }
        }
    }

    internal class DeviceInjectorDeviceContentDirectory : IDisposable
    {
        private readonly DeviceInjectorMediaEndpoint iInjector;

        private readonly DeviceMediaEndpointContentDirectory iDevice;

        public DeviceInjectorDeviceContentDirectory(DeviceInjectorMediaEndpoint aInjector, string aUdn, CpDevice aDevice, XDocument aXml, ILog aLog)
        {
            iInjector = aInjector;
            iDevice = new DeviceMediaEndpointContentDirectory(iInjector.Network, aUdn, aDevice, aXml, aLog);
            iInjector.AddDevice(iDevice);
        }

        // IDisposable

        public void Dispose()
        {
        }
    }

    internal class DeviceInjectorDeviceOpenHome : IDisposable
    {
        private readonly DeviceInjectorMediaEndpoint iInjector;
        private readonly string iUdn;
        private readonly Uri iUri;
        private readonly CpDevice iDevice;
        private readonly ILog iLog;

        private Dictionary<string, IInjectorDevice> iEndpoints;
        private Dictionary<string, List<IDisposable>> iEventHandlers;

        private IEventSupervisorSession iEventSession;
        private IDisposable iEventMediaEndpoints;

        private bool iDisposed;

        public DeviceInjectorDeviceOpenHome(DeviceInjectorMediaEndpoint aInjector, string aUdn, Uri aUri, CpDevice aDevice, ILog aLog)
        {
            iInjector = aInjector;
            iUdn = aUdn;
            iUri = aUri;
            iDevice = aDevice;
            iDevice.AddRef();
            iLog = aLog;

            iEndpoints = new Dictionary<string, IInjectorDevice>();
            iEventHandlers = new Dictionary<string, List<IDisposable>>();

            iDisposed = false;

            Task.Factory.StartNew(() =>
            {
                lock (iEndpoints)
                {
                    if (iDisposed)
                    {
                        return;
                    }
                }

                string endpoint;

                using (var client = new WebClient())
                {
                    try
                    {
                        var es = client.DownloadString(iUri + "/es");

                        var json = JsonParser.Parse(es) as JsonString;

                        endpoint = json.Value;
                    }
                    catch
                    {
                        return;
                    }
                }

                var resolved = ResolveEndpoint(endpoint);

                if (resolved == null)
                {
                    return;
                }

                var session = iInjector.CreateEventSession(resolved);

                lock (iEndpoints)
                {
                    if (iDisposed)
                    {
                        session.Dispose();
                    }
                    else
                    {
                        iEventSession = session;
                        iEventMediaEndpoints = iEventSession.Create("ps.me", Update);
                    }
                }
            });
        }

        private string ResolveEndpoint(string aEndpoint)
        {
            var parts = aEndpoint.Split(':');

            if (parts.Count() != 2)
            {
                return (null);
            }

            var address = parts[0];

            if (address == "0.0.0.0")
            {
                address = iUri.Host;
            }

            uint port = 0;

            if (!uint.TryParse(parts[1], out port))
            {
                return (null);
            }

            if (port < IPEndPoint.MinPort)
            {
                return (null);
            }

            if (port > IPEndPoint.MaxPort)
            {
                return (null);
            }

            return (address + ":" + port);
        }

        private void Update(string aId, uint aSequence)
        {
            Task.Factory.StartNew(() =>
            {
                lock (iEndpoints)
                {
                    if (iDisposed)
                    {
                        return;
                    }
                }

                var endpoints = GetEndpoints();

                iInjector.Network.Schedule(() =>
                {
                    lock (iEndpoints)
                    {
                        if (!iDisposed)
                        {
                            UpdateEndpointsLocked(endpoints);
                        }
                    }
                });
            });
        }

        // Update the network with the current list of endpoints for this device
        // Add and remove devices from the network as necessary. Schedule this
        // every time the list of endpoints changes.

        private JsonObject GetEndpoints()
        {
            using (var client = new WebClient())
            {
                try
                {
                    var me = client.DownloadString(iUri + "/me");

                    var json = JsonParser.Parse(me) as JsonObject;

                    if (json != null)
                    {
                        return (json);
                    }
                }
                catch
                {
                }
            }

            return (new JsonObject());
        }

        private void UpdateEndpointsLocked(JsonObject aEndpoints)
        {
            var refresh = new Dictionary<string, IInjectorDevice>();

            // remove old device

            foreach (var entry in iEndpoints)
            {
                JsonValue device = aEndpoints[entry.Key];

                if (device != null)
                {
                    refresh.Add(entry.Key, entry.Value);
                }
                else
                {
                    List<IDisposable> handlers;

                    if (iEventHandlers.TryGetValue(entry.Key, out handlers))
                    {
                        iEventHandlers.Remove(entry.Key);

                        foreach (var handler in handlers)
                        {
                            handler.Dispose();
                        }
                    }

                    iInjector.RemoveDevice(entry.Value);
                }
            }

            // add new devices

            foreach (var entry in aEndpoints)
            {
                IInjectorDevice device;

                if (!refresh.TryGetValue(entry.Key, out device))
                {
                    iEventHandlers.Add(entry.Key, new List<IDisposable>());

                    device = new DeviceMediaEndpointOpenHome(iInjector.Network, iUri, entry.Key, entry.Value, (id, action) =>
                    {
                        lock (iEndpoints)
                        {
                            List<IDisposable> handlers;

                            if (iEventHandlers.TryGetValue(entry.Key, out handlers))
                            {
                                handlers.Add(iEventSession.Create(id, action));
                            }
                        }
                    }, iLog);

                    refresh.Add(entry.Key, device);

                    iInjector.AddDevice(device);
                }
            }

            Console.WriteLine("{0} has {1} endpoints", iUdn, refresh.Count);

            iEndpoints = refresh;
        }

        // IDisposable

        public void Dispose()
        {
            lock (iEndpoints)
            {
                iDisposed = true;

                iDevice.RemoveRef();

                iInjector.Network.Schedule(() =>
                {
                    if (iEventSession != null)
                    {
                        iEventMediaEndpoints.Dispose();

                        foreach (var handlers in iEventHandlers.Values)
                        {
                            foreach (var entry in handlers)
                            {
                                entry.Dispose();
                            }
                        }

                        iEventHandlers.Clear();

                        iEventSession.Dispose();

                        foreach (var entry in iEndpoints)
                        {
                            iInjector.RemoveDevice(entry.Value);
                        }
                    }
                });
            }
        }
    }
}

