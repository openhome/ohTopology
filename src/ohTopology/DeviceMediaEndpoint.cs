using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
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

    public class InjectorMediaEndpoint : IDisposable
    {
        private readonly Network iNetwork;
        private readonly ILog iLog;

        private readonly DisposeHandler iDisposeHandler;
        private readonly Dictionary<string, IDisposable> iDevices;
        private readonly CpDeviceListUpnpServiceType iDeviceList;

        public InjectorMediaEndpoint(Network aNetwork, ILog aLog)
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
            iNetwork.Assert();
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

            iLog.Write("+DeviceInjectorMediaEndpoint {0}\n", udn);

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
                        var device = new InjectorDeviceOpenHome(this, udn, uri, aDevice, iLog);
                        iDevices.Add(udn, device);
                    }
                }
            }
            else
            {
                lock (iDevices)
                {
                    var device = new InjectorDeviceContentDirectory(this, udn, aDevice, xml, iLog);
                    iDevices.Add(udn, device);
                }
            }
        }

        private void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            var udn = aDevice.Udn();

            iLog.Write("-DeviceInjectorMediaEndpoint {0}\n", udn);

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
            using (iDisposeHandler.Lock())
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

    internal class InjectorDeviceContentDirectory : IDisposable
    {
        private readonly InjectorMediaEndpoint iInjector;

        private readonly DeviceMediaEndpointContentDirectory iDevice;

        public InjectorDeviceContentDirectory(InjectorMediaEndpoint aInjector, string aUdn, CpDevice aDevice, XDocument aXml, ILog aLog)
        {
            iInjector = aInjector;
            iDevice = new DeviceMediaEndpointContentDirectory(iInjector.Network, aUdn, aDevice, aXml, aLog);
            iInjector.AddDevice(iDevice);
        }

        // IDisposable

        public void Dispose()
        {
            iInjector.RemoveDevice(iDevice);
        }
    }

    internal class InjectorDeviceOpenHome : IDisposable
    {
        private readonly InjectorMediaEndpoint iInjector;
        private readonly string iUdn;
        private readonly Uri iUri;
        private readonly CpDevice iDevice;
        private readonly ILog iLog;

        private readonly Encoding iEncoding;

        private Dictionary<string, IInjectorDevice> iEndpoints;
        private Dictionary<string, List<IDisposable>> iEventHandlers;

        private CancellationTokenSource iCancellationTokenSource;

        private IEventSupervisorSession iEventSession;
        private IDisposable iEventMediaEndpoints;

        private bool iDisposed;

        public InjectorDeviceOpenHome(InjectorMediaEndpoint aInjector, string aUdn, Uri aUri, CpDevice aDevice, ILog aLog)
        {
            iInjector = aInjector;
            iUdn = aUdn;
            iUri = aUri;
            iDevice = aDevice;
            iDevice.AddRef();
            iLog = aLog;

            iEncoding = new UTF8Encoding(false);

            iEndpoints = new Dictionary<string, IInjectorDevice>();
            iEventHandlers = new Dictionary<string, List<IDisposable>>();

            iCancellationTokenSource = new CancellationTokenSource();

            iDisposed = false;

            var client = new HttpClient(iInjector.Network);

            client.Read(iUri + "/es", iCancellationTokenSource.Token, (buffer) =>
            {
                if (!iDisposed && buffer != null)
                {
                    var value = iEncoding.GetString(buffer);

                    try
                    {
                        var json = JsonParser.Parse(value) as JsonString;

                        var resolved = ResolveEndpoint(json.Value);

                        if (resolved != null)
                        {
                            iEventSession = iInjector.CreateEventSession(resolved);
                            iEventMediaEndpoints = iEventSession.Create("ps.me", Update);
                        }
                    }
                    catch
                    {
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

        // Update the network with the current list of endpoints for this device
        // Add and remove devices from the network as necessary. Schedule this
        // every time the list of endpoints changes.

        private void Update(string aId, uint aSequence)
        {
            var client = new HttpClient(iInjector.Network);

            client.Read(iUri + "/me", CancellationToken.None, (buffer) =>
            {
                if (!iDisposed && buffer != null)
                {
                    var value = iEncoding.GetString(buffer);

                    try
                    {
                        var json = JsonParser.Parse(value) as JsonObject;
                        UpdateEndpoints(json);
                    }
                    catch
                    {
                    }
                }
            });
        }

        private void UpdateEndpoints(JsonObject aEndpoints)
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
                        List<IDisposable> handlers;

                        if (iEventHandlers.TryGetValue(entry.Key, out handlers))
                        {
                            handlers.Add(iEventSession.Create(id, action));
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
            iCancellationTokenSource.Cancel();

            iInjector.Network.Schedule(() =>
            {
                iDisposed = true;

                iDevice.RemoveRef();

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

