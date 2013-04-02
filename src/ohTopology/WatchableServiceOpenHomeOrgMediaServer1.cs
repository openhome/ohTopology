using System;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Av
{
    public interface IServiceAvOpenHomeOrgMediaServer1BrowseResult
    {
    }

    public interface IServiceAvOpenHomeOrgMediaServer1
    {
        IWatchable<uint> UpdateCount { get; }

        void Browse(string aId, Action<IContentDirectoryBrowseResult> aCallback);
    }

    public abstract class MediaServer : IWatchableService, IServiceAvOpenHomeOrgMediaServer1
    {
        protected MediaServer(string aId, IServiceUpnpOrgContentDirectory1 aService)
        {
            iId = aId;
            iService = aService;
        }

        public abstract void Dispose();

        public string Id
        {
            get
            {
                return iId;
            }
        }

        public IWatchable<uint> UpdateCount
        {
            get
            {
                return iService.SystemUpdateId;
            }
        }

        public void Browse(string aId, Action<IContentDirectoryBrowseResult> aCallback)
        {
            aCallback(null);
        }

        private string iId;

        protected IServiceUpnpOrgContentDirectory1 iService;
    }

    public class ServiceAvOpenHomeOrgMediaServer1 : IServiceAvOpenHomeOrgMediaServer1, IDisposable
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

        public void Browse(string aId, Action<IContentDirectoryBrowseResult> aCallback)
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

    public class MockServiceAvOpenHomeOrgMediaServer1 : IServiceAvOpenHomeOrgMediaServer1, IMockable
    {
        public MockServiceAvOpenHomeOrgMediaServer1(IWatchableThread aThread, string aId, uint aUpdateCount)
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

        public void Browse(string aId, Action<IContentDirectoryBrowseResult> aCallback)
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
