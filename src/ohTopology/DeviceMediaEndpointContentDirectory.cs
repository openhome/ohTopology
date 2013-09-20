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
    internal class DeviceMediaEndpointContentDirectory : Device
    {
        private readonly CpDevice iDevice;
        private readonly CpProxyUpnpOrgContentDirectory1 iProxy;
        private readonly ServiceMediaEndpointContentDirectory iService;

        public DeviceMediaEndpointContentDirectory(INetwork aNetwork, string aUdn, CpDevice aDevice, XDocument aXml)
            : base(aUdn)
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

            iProxy = new CpProxyUpnpOrgContentDirectory1(iDevice);
            iProxy.SetPropertySystemUpdateIDChanged(OnSystemUpdateIdChanged);

            iService = new ServiceMediaEndpointContentDirectory(aNetwork, this, iDevice.Udn(), "Music",
                            name, info, url, artwork,
                            manufacturerName, manufacturerInfo, manufacturerUrl, manufacturerArtwork,
                            modelName, modelInfo, modelUrl, modelArtwork,
                            DateTime.Now,
                            new string[] { "Browse" },
                            iProxy);

            iProxy.Subscribe();

            Add<IProxyMediaEndpoint>(iService);
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
            iProxy.Dispose();
            iDevice.RemoveRef();
            base.Dispose();
        }
    }
}

