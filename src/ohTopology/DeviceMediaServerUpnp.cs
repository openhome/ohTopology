using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

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

            Add<IProxyMediaServer>(new ServiceMediaServerUpnp(aNetwork, new string[] {"Browse", "Query"},
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                iUpnpProxy));

            // content directory service
            //MockWatchableContentDirectory contentDirectory = new MockWatchableContentDirectory(aThread, aUdn, 0, "");
            //Add<ContentDirectory>(contentDirectory);
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
