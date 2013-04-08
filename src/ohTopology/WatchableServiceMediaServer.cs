using System;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.MediaServer;

namespace OpenHome.Av
{
    public interface ISessionValue
    {
        string Value { get; }
        IEnumerable<string> Values { get; }
    }

    public interface ISessionDatum
    {
        string Id { get; }
        bool IsContainer { get; }
        ISessionValue this[ITag aTag] { get; }
        ITag PrimaryTag { get; }
    }

    public interface ISessionContents
    {
        uint Index { get; }
        IEnumerable<ISessionDatum> Data { get; }
    }

    public interface IMediaServerContainer
    {

        uint Total { get; }
        IEnumerable<uint> Alpha { get; } // null if no alpha map
        IEnumerable<string> Path { get; } // ordered list of container titles
        IEnumerable<string> Location { get; } // ordered list of container ids
        void Fetch(uint aIndex, uint aCount, Action<IMediaServerContainer, ISessionContents> aCallback);
    }

    public interface IMediaServerSession : IDisposable
    {
        void Open(IEnumerable<string> aPath); // ordered list of container id's, empty for root
        Watchable<IMediaServerContainer> Container { get; }
    }

    public interface IServiceMediaServer : IWatchableService
    {
        IMediaServerSession CreateSession();
    }

    public class ServiceAvOpenHomeOrgMediaServer1 : IServiceMediaServer
    {
        public ServiceAvOpenHomeOrgMediaServer1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgMediaServer1 aService)
        {
            iLock = new object();
            iDisposed = false;

            iService = aService;

            iService.SetPropertyUpdateCountChanged(HandleUpdateCountChanged);

            iUpdateCount = new Watchable<uint>(aThread, string.Format("UpdateCount({0})", aId), iService.PropertyUpdateCount());
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
            aCallback(null);
        }

        private void HandleUpdateCountChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iUpdateCount.Update(iService.PropertyUpdateCount());
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgMediaServer1 iService;

        private Watchable<uint> iUpdateCount;
    }

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

    public class MockServiceMediaServer : IServiceMediaServer, IMockable
    {
        public MockServiceMediaServer(IWatchableThread aThread, string aId, uint aUpdateCount)
        {
            iThread = aThread;
            iUpdateCount = new Watchable<uint>(aThread, string.Format("UpdateCount({0})", aId), aUpdateCount);
        }

        public void Dispose()
        {
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
            iThread.Schedule(() =>
            {
                aCallback(null);
            });
        }

        public void Execute(IEnumerable<string> aValue)
        {
            /*string command = aValue.First().ToLowerInvariant();
            if (command == "balance")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iBalance.Update(int.Parse(value.First()));
            }
            else if (command == "fade")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iFade.Update(int.Parse(value.First()));
            }
            else if (command == "mute")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iMute.Update(bool.Parse(value.First()));
            }
            else if (command == "volume")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iVolume.Update(uint.Parse(value.First()));
            }
            else if (command == "volumeinc")
            {
                VolumeInc();
            }
            else if (command == "volumedec")
            {
                VolumeDec();
            }
            else
            {*/
            throw new NotSupportedException();
            //}
        }

        private IWatchableThread iThread;

        private Watchable<uint> iUpdateCount;
    }

    /*
    public class WatchableContentDirectoryFactory : IWatchableServiceFactory
    {
        public WatchableContentDirectoryFactory(IWatchableThread aThread)
        {
            iThread = aThread;

            iService = null;
        }

        public void Subscribe(WatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            if (iService == null && iPendingService == null)
            {
                iPendingService = new CpProxyUpnpOrgContentDirectory1(aDevice.Device);
                iPendingService.SetPropertyInitialEvent(delegate
                {
                    iThread.Schedule(() =>
                    {
                        iService = new WatchableContentDirectory(iThread, string.Format("ContentDirectory({0})", aDevice.Udn), iPendingService);
                        iPendingService = null;
                        aCallback(iService);
                    });
                });
                iPendingService.Subscribe();
            }
        }

        public void Unsubscribe()
        {
            if (iPendingService != null)
            {
                iPendingService.Dispose();
                iPendingService = null;
            }

            if (iService != null)
            {
                iService.Dispose();
                iService = null;
            }
        }

        private CpProxyUpnpOrgContentDirectory1 iPendingService;
        private WatchableContentDirectory iService;
        private IWatchableThread iThread;
    }

    public class WatchableContentDirectory : ContentDirectory
    {
        public WatchableContentDirectory(IWatchableThread aThread, string aId, CpProxyUpnpOrgContentDirectory1 aService)
            : base(aId, new ServiceUpnpOrgContentDirectory1(aThread, aId, aService))
        {
            iCpService = aService;
        }

        public override void Dispose()
        {
            if (iCpService != null)
            {
                iCpService.Dispose();
            }
        }

        private CpProxyUpnpOrgContentDirectory1 iCpService;
    }

    public class MockWatchableContentDirectory : ContentDirectory, IMockable
    {
        public MockWatchableContentDirectory(IWatchableThread aThread, string aId, uint aSystemUpdateId, string aContainerUpdateIds)
            : base(aId, new MockServiceUpnpOrgContentDirectory1(aThread, aId, aSystemUpdateId, aContainerUpdateIds))
        {
        }

        public MockWatchableContentDirectory(IWatchableThread aThread, string aId)
            : base(aId, new MockServiceUpnpOrgContentDirectory1(aThread, aId, 0, string.Empty))
        {
        }

        public override void Dispose()
        {
        }

        public void Execute(IEnumerable<string> aValue)
        {
            MockServiceUpnpOrgContentDirectory1 i = iService as MockServiceUpnpOrgContentDirectory1;
            i.Execute(aValue);
        }
    }
    */
}
