using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using OpenHome.Os.App;

using OpenHome.Http;
using OpenHome.Http.Owin;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public class DeviceMediaServerUpnp : Device
    {
        private readonly INetwork iNetwork;
        private readonly CpDevice iDevice;
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;
        private readonly ServiceMediaServerUpnp iService;

        public DeviceMediaServerUpnp(INetwork aNetwork, CpDevice aDevice)
            : base(aDevice.Udn())
        {
            iNetwork = aNetwork;
            iDevice = aDevice;
            iDevice.AddRef();
            iUpnpProxy = new CpProxyUpnpOrgContentDirectory1(iDevice);

            string deviceXml;

            iDevice.GetAttribute("Upnp.DeviceXml", out deviceXml);

            var reader = XmlReader.Create(new StringReader(deviceXml));

            var xml = XDocument.Load(reader);
            
            var elements = xml.Descendants(XName.Get("device", "urn:schemas-upnp-org:device-1-0"));

            var upnpFriendlyName = GetDeviceElementValue(elements, "friendlyName");
            var upnpManufacturer = GetDeviceElementValue(elements, "manufacturer");
            var upnpManufacturerUrl = GetDeviceElementValue(elements, "manufacturerURL");
            var upnpModelName = GetDeviceElementValue(elements, "modelName");
            var upnpModelUrl = GetDeviceElementValue(elements, "modelURL");
            var upnpModelDescription = GetDeviceElementValue(elements, "modelDescription");
            var upnpModelNumber = GetDeviceElementValue(elements, "modelNumber");
            var upnpSerialNumber = GetDeviceElementValue(elements, "serialNumber");
            var upnpPresentationUrl = GetDeviceElementValue(elements, "presentationURL");

            // TODO: Analyse icons to get ImageUrl

            string manufacturerImageUri = String.Empty;
            string manufacturerInfo = GetDeviceValueFrom(upnpManufacturer);
            string manufacturerName = GetDeviceValueFrom(upnpManufacturer);
            string manufacturerUrl = GetDeviceValueFrom(upnpManufacturerUrl);
            string modelImageUri = manufacturerUrl;
            string modelInfo = GetDeviceValueFrom(upnpModelDescription, upnpModelNumber);
            string modelName = GetDeviceValueFrom(upnpModelName);
            string modelUrl = GetDeviceValueFrom(upnpModelUrl);
            string productImageUri = manufacturerUrl;
            string productInfo = GetDeviceValueFrom(upnpSerialNumber);
            string productName = GetDeviceValueFrom(upnpFriendlyName);
            string productUrl = GetDeviceValueFrom(upnpPresentationUrl);

            iService = new ServiceMediaServerUpnp(aNetwork, this, new string[] { "Browse" },
                            manufacturerImageUri, manufacturerInfo, manufacturerName, manufacturerUrl,
                            modelImageUri, modelInfo, modelName, modelUrl,
                            productImageUri, productInfo, productName, productUrl,
                            iUpnpProxy);

            Add<IProxyMediaServer>(iService);

            iUpnpProxy.SetPropertySystemUpdateIDChanged(OnSystemUpdateIdChanged);
            iUpnpProxy.Subscribe();
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

        public override void Dispose()
        {
            iUpnpProxy.Dispose();
            iDevice.RemoveRef();
            base.Dispose();
        }
    }
}

