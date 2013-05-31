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

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using OpenHome.Os.App;
using OpenHome.MediaServer;
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

        public DeviceMediaServerUpnp(INetwork aNetwork, CpDevice aDevice, CpProxyUpnpOrgContentDirectory1 aUpnpProxy)
            : base(aDevice.Udn())
        {
            iNetwork = aNetwork;
            iDevice = aDevice;
            iUpnpProxy = aUpnpProxy;

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

            Add<IProxyMediaServer>(new ServiceMediaServerUpnp(aNetwork, new string[] { "Browse", "Query" },
                manufacturerImageUri, manufacturerInfo, manufacturerName, manufacturerUrl,
                modelImageUri, modelInfo, modelName, modelUrl,
                productImageUri, productInfo, productName, productUrl,
                iUpnpProxy));

            // content directory service
            //MockWatchableContentDirectory contentDirectory = new MockWatchableContentDirectory(aThread, aUdn, 0, "");
            //Add<ContentDirectory>(contentDirectory);
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
    }
}
/*
public class ServiceUpnpOrgContentDirectory1 : IServiceMediaServer
{
    private class BrowseAsyncHandler
    {
        public BrowseAsyncHandler(CpProxyUpnpOrgContentDirectory1 aService, Action<IServiceMediaServerBrowseResult> aCallback)
        {
            iService = aService;
            iCallback = aCallback;
        }

        public void Browse(string aObjectId, string aBrowseFlag, string aFilter, uint aStartingIndex, uint aRequestedCount, string aSortCriteria)
        {
            iService.BeginBrowse(aObjectId, aBrowseFlag, aFilter, aStartingIndex, aRequestedCount, aSortCriteria, Callback);
        }

        private void Callback(IntPtr aAsyncHandle)
        {
            string result;
            uint numberReturned;
            uint totalMatches;
            uint updateId;

            iService.EndBrowse(aAsyncHandle, out result, out numberReturned, out totalMatches, out updateId);

            iCallback(null); // TODO
        }

        private CpProxyUpnpOrgContentDirectory1 iService;
        private Action<IServiceMediaServerBrowseResult> iCallback;
    }

    public ServiceUpnpOrgContentDirectory1(IWatchableThread aThread, string aId, CpProxyUpnpOrgContentDirectory1 aService)
    {
        iLock = new object();
        iDisposed = false;

        iService = aService;

        iService.SetPropertySystemUpdateIDChanged(HandleSystemUpdateIDChanged);

        iUpdateCount = new Watchable<uint>(aThread, string.Format("UpdateCount({0})", aId), iService.PropertySystemUpdateID());
    }

    public void Dispose()
    {
        lock (iLock)
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("ServiceUpnpOrgContentDirectory1.Dispose");
            }

            iService = null;

            iDisposed = true;
        }
    }

    public IWatchable<uint> UpdateCount
    {
        get
        {
            return iUpdateCount;
        }
    }

    public void Browse(string aId, Action<IServiceMediaServerBrowseResult> aCallback)
    {
        BrowseAsyncHandler handler = new BrowseAsyncHandler(iService, aCallback);
        handler.Browse(aId);
    }

    private void HandleSystemUpdateIDChanged()
    {
        lock (iLock)
        {
            if (iDisposed)
            {
                return;
            }

            iUpdateCount.Update(iService.PropertySystemUpdateID());
        }
    }

    private object iLock;
    private bool iDisposed;

    private CpProxyUpnpOrgContentDirectory1 iService;

    private Watchable<uint> iUpdateCount;
}
*/
