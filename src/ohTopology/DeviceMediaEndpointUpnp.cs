using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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
    public class DeviceInjectorMediaEndpoint : IDisposable
    {
        private readonly Network iNetwork;

        private readonly DisposeHandler iDisposeHandler;
        private readonly Dictionary<string, IDictionary<string, Device>> iDeviceLookup;
        private readonly CpDeviceListUpnpServiceType iDeviceList;

        public DeviceInjectorMediaEndpoint(Network aNetwork)
        {
            iNetwork = aNetwork;

            iDisposeHandler = new DisposeHandler();
            iDeviceLookup = new Dictionary<string,IDictionary<string,Device>>();
            iDeviceList = new CpDeviceListUpnpServiceType("upnp.org", "ContentDirectory", 1, Added, Removed);
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

        // Update the network with the current list of endpoints for this device
        // Add and remove devices from the network as necessary. Schedule this
        // every time the list of endpoints changes.

        private void PopulateEndpoints(string aUdn, string aUri)
        {
            var client = new WebClient();

            var me = client.DownloadString(aUri + "/me");

            var json = JsonParser.Parse(me) as JsonObject;

            var endpoints = new Dictionary<string, Device>();

            foreach (var entry in json)
            {
                endpoints.Add(entry.Key, new DeviceMediaEndpointOpenHome(iNetwork, aUri, entry.Key, entry.Value));
            }

            lock (iDeviceLookup)
            {
                IDictionary<string, Device> lookup;

                if (iDeviceLookup.TryGetValue(aUdn, out lookup))
                {
                    var refresh = new Dictionary<string,Device>();

                    // remove old device

                    foreach (var entry in lookup)
                    {
                        Device device;

                        if (endpoints.TryGetValue(entry.Key, out device))
                        {
                            refresh.Add(entry.Key, device);
                        }
                        else
                        {
                            iNetwork.Schedule(() =>
                            {
                                iNetwork.Remove(entry.Value);
                            });
                        }
                    }

                    // add new devices

                    foreach (var entry in endpoints)
                    {
                        Device device;

                        if (!refresh.TryGetValue(entry.Key, out device))
                        {
                            refresh.Add(entry.Key, entry.Value);

                            iNetwork.Schedule(() =>
                            {
                                iNetwork.Add(entry.Value);
                            });
                        }
                    }

                    iDeviceLookup[aUdn] = refresh;
                }
            }
        }

        private void Added(CpDeviceList aList, CpDevice aDevice)
        {
            var udn = aDevice.Udn();

            string deviceXml;

            aDevice.GetAttribute("Upnp.DeviceXml", out deviceXml);

            var reader = XmlReader.Create(new StringReader(deviceXml));

            var xml = XDocument.Load(reader);

            var elements = xml.Descendants(XName.Get("device", "urn:schemas-upnp-org:device-1-0"));

            var ohPath = GetOpenHomeElementValue(elements, "X_PATH");

            if (ohPath != null)
            {
                var ohPresentation = new Uri(GetDeviceElementValue(elements, "presentationURL"));

                var uri = new Uri(ohPresentation, ohPath);

                var devices = new Dictionary<string, Device>();

                lock (iDeviceLookup)
                {
                    iDeviceLookup.Add(udn, devices);

                    Task.Factory.StartNew(() =>
                    {
                        PopulateEndpoints(udn, uri.ToString());
                    });
                }
            }
            else
            {
                iNetwork.Execute(() =>
                {
                    var device = new DeviceMediaEndpointContentDirectory(iNetwork, aDevice, xml);
                    
                    var devices = new Dictionary<string, Device>();
                    
                    devices.Add("upnp", device);

                    lock (iDeviceLookup)
                    {
                        iDeviceLookup.Add(aDevice.Udn(), devices);
                        iNetwork.Add(device);
                    }
                });
            }
        }

        private void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            var udn = aDevice.Udn();

            iNetwork.Execute(() =>
            {
                lock (iDeviceLookup)
                {
                    IDictionary<string, Device> devices;

                    if (iDeviceLookup.TryGetValue(udn, out devices))
                    {
                        iDeviceLookup.Remove(udn);

                        foreach (var device in devices.Values)
                        {
                            iNetwork.Remove(device);
                            device.Dispose();
                        }
                    }
                }
            });
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

            iNetwork.Execute(() =>
            {
                foreach (var devices in iDeviceLookup.Values)
                {
                    foreach (var device in devices.Values)
                    {
                        device.Dispose();
                    }
                }
            });
        }
    }

    internal class DeviceMediaEndpointContentDirectory : Device
    {
        private readonly CpDevice iDevice;
        private readonly ServiceMediaEndpointContentDirectory iService;
        private readonly CpProxyUpnpOrgContentDirectory1 iProxy;

        public DeviceMediaEndpointContentDirectory(INetwork aNetwork, CpDevice aDevice, XDocument aXml)
            : base(aDevice.Udn())
        {
            iDevice = aDevice;

            iDevice.AddRef();

            var elements = aXml.Descendants(XName.Get("device", "urn:schemas-upnp-org:device-1-0"));

            var upnpFriendlyName = GetDeviceElementValue(elements, "friendlyName");
            var upnpManufacturer = GetDeviceElementValue(elements, "manufacturer");
            var upnpManufacturerUrl = GetDeviceElementValue(elements, "manufacturerURL");
            var upnpModelName = GetDeviceElementValue(elements, "modelName");
            var upnpModelUrl = GetDeviceElementValue(elements, "modelURL");
            var upnpModelDescription = GetDeviceElementValue(elements, "modelDescription");
            var upnpModelNumber = GetDeviceElementValue(elements, "modelNumber");
            var upnpSerialNumber = GetDeviceElementValue(elements, "serialNumber");
            var upnpPresentationUrl = GetDeviceElementValue(elements, "presentationURL");

            // TODO: Analyse icons to get artwork

            string name = GetDeviceValueFrom(upnpFriendlyName);
            string info = GetDeviceValueFrom(upnpSerialNumber);
            string url = GetDeviceValueFrom(upnpPresentationUrl);
            string artwork = String.Empty;
            string manufacturerName = GetDeviceValueFrom(upnpManufacturer);
            string manufacturerInfo = GetDeviceValueFrom(upnpManufacturer);
            string manufacturerUrl = GetDeviceValueFrom(upnpManufacturerUrl);
            string manufacturerArtwork = String.Empty;
            string modelName = GetDeviceValueFrom(upnpModelName);
            string modelInfo = GetDeviceValueFrom(upnpModelDescription, upnpModelNumber);
            string modelUrl = GetDeviceValueFrom(upnpModelUrl);
            string modelArtwork = String.Empty;

            iService = new ServiceMediaEndpointContentDirectory(aNetwork, this, iDevice.Udn(), "Music",
                            name, info, url, artwork,
                            manufacturerName, manufacturerInfo, manufacturerUrl, manufacturerArtwork,
                            modelName, modelInfo, modelUrl, modelArtwork,
                            DateTime.Now,
                            new string[] { "Browse" },
                            iProxy);

            Add<IProxyMediaEndpoint>(iService);

            iProxy = new CpProxyUpnpOrgContentDirectory1(iDevice);
            iProxy.SetPropertySystemUpdateIDChanged(OnSystemUpdateIdChanged);
            iProxy.Subscribe();
        }

        private void OnSystemUpdateIdChanged()
        {
            iService.Refresh();
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

        private string GetDeviceValueFrom(params string[] aValues)
        {
            var sb = new StringBuilder();

            bool first = true;

            foreach (var value in aValues)
            {
                if (value != null)
                {
                    if (first)
                    {
                        sb.Append(value);
                        first = false;
                    }
                    else
                    {
                        sb.Append("[");
                        sb.Append(value);
                        sb.Append("]");
                    }
                }
            }

            return (sb.ToString());
        }

        // IDisposable

        public void Dispsoe()
        {
            iProxy.Dispose();
            iDevice.RemoveRef();
            base.Dispose();
        }
    }

    internal class DeviceMediaEndpointOpenHome : Device
    {
        private readonly ServiceMediaEndpointOpenHome iService;

        public DeviceMediaEndpointOpenHome(INetwork aNetwork, string aBaseUri, string aId, JsonValue aJson)
            : base(aId)
        {
            var json = aJson as JsonObject;

            var type = json["Type"].Value();
            var name = json["Name"].Value();
            var info = json["Info"].Value();
            var url = json["Url"].Value();
            var artwork = json["Artwork"].Value();
            var manufacturerName = json["ManufacturerName"].Value();
            var manufacturerInfo = json["ManufacturerInfo"].Value();
            var manufacturerUrl = json["ManufacturerUrl"].Value();
            var manufacturerArtwork = json["ManufacturerArtwork"].Value();
            var modelName = json["ModelName"].Value();
            var modelInfo = json["ModelInfo"].Value();
            var modelUrl = json["ModelUrl"].Value();
            var modelArtwork = json["ModelArtwork"].Value();
            var started = DateTime.Parse(json["Started"].Value());
            var path = json["Path"].Value();

            url = ResolveUri(aBaseUri, url);
            artwork = ResolveUri(aBaseUri, artwork);
            manufacturerUrl = ResolveUri(aBaseUri, manufacturerUrl);
            manufacturerArtwork = ResolveUri(aBaseUri, manufacturerArtwork);
            modelUrl = ResolveUri(aBaseUri, modelUrl);
            modelArtwork = ResolveUri(aBaseUri, modelArtwork);
            
            var uri = ResolveUri(aBaseUri, path);

            var attributes = new List<string>();

            foreach (var attribute in json["Attributes"] as JsonArray)
            {
                attributes.Add(attribute.Value());
            }

            iService = new ServiceMediaEndpointOpenHome(aNetwork, this, aId, type,
                            name, info, url, artwork,
                            manufacturerName, manufacturerInfo, manufacturerUrl, manufacturerArtwork,
                            modelName, modelInfo, modelUrl, modelArtwork,
                            started,
                            attributes, uri);

            Add<IProxyMediaEndpoint>(iService);
        }

        private string ResolveUri(string aBaseUri, string aUri)
        {
            try
            {
                var uri = new Uri(aUri);
                return (aUri);
            }
            catch
            {
                var baseUri = new Uri(aBaseUri);
                var uri = new Uri(baseUri, aUri);
                return (uri.ToString());
            }
        }

        // IDisposable

        public void Dispsoe()
        {
            base.Dispose();
        }
    }
}

