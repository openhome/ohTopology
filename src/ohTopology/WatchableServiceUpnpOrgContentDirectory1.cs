using System;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Av
{
    public class BrowseResult
    {
        public BrowseResult(string aResult, uint aNumberReturned, uint aTotalMatches, uint aUpdateId)
        {
            iResult = aResult;
            iNumberReturned = aNumberReturned;
            iTotalMatches = aTotalMatches;
            iUpdateId = aUpdateId;
        }

        public string Result
        {
            get
            {
                return iResult;
            }
        }

        public uint NumberReturned
        {
            get
            {
                return iNumberReturned;
            }
        }

        public uint TotalMatches
        {
            get
            {
                return iTotalMatches;
            }
        }

        public uint UpdateId
        {
            get
            {
                return iUpdateId;
            }
        }

        private string iResult;
        private uint iNumberReturned;
        private uint iTotalMatches;
        private uint iUpdateId;
    }

    public interface IServiceUpnpOrgContentDirectory1
    {
        IWatchable<uint> SystemUpdateId { get; }
        IWatchable<string> ContainerUpdateIds { get; }

        void Browse(string aObjectId, string aBrowseFlag, string aFilter, uint aStartingIndex, uint aRequestedCount, string aSortCriteria, ContentDirectory.BrowseResultHandler aHandler);
    }

    public abstract class ContentDirectory : IWatchableService, IServiceUpnpOrgContentDirectory1, IDisposable
    {
        public const string kBrowseMetadata = "BrowseMetadata";
        public const string kBrowseDirectChildren = "BrowseDirectChildren";

        protected ContentDirectory(string aId, IServiceUpnpOrgContentDirectory1 aService)
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

        public string Type
        {
            get
            {
                return "AvUpnpContentDirectory1";
            }
        }

        public IWatchable<uint> SystemUpdateId
        {
            get
            {
                return iService.SystemUpdateId;
            }
        }

        public IWatchable<string> ContainerUpdateIds
        {
            get
            {
                return iService.ContainerUpdateIds;
            }
        }

        public delegate void BrowseResultHandler(BrowseResult aResult);

        public void Browse(string aObjectId, string aBrowseFlag, string aFilter, uint aStartingIndex, uint aRequestedCount, string aSortCriteria, BrowseResultHandler aHandler)
        {
            iService.Browse(aObjectId, aBrowseFlag, aFilter, aStartingIndex, aRequestedCount, aSortCriteria, aHandler);
        }

        private string iId;
        protected IServiceUpnpOrgContentDirectory1 iService;
    }

    public class ServiceUpnpOrgContentDirectory1 : IServiceUpnpOrgContentDirectory1, IDisposable
    {
        private class BrowseAsyncHandler
        {
            public BrowseAsyncHandler(CpProxyUpnpOrgContentDirectory1 aService, ContentDirectory.BrowseResultHandler aHandler)
            {
                iService = aService;
                iHandler = aHandler;
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

                iHandler(new BrowseResult(result, numberReturned, totalMatches, updateId));
            }

            private CpProxyUpnpOrgContentDirectory1 iService;
            private ContentDirectory.BrowseResultHandler iHandler;
        }

        public ServiceUpnpOrgContentDirectory1(IWatchableThread aThread, string aId, CpProxyUpnpOrgContentDirectory1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertySystemUpdateIDChanged(HandleSystemUpdateIDChanged);
                iService.SetPropertyContainerUpdateIDsChanged(HandleContainerUpdateIDsChanged);

                iSystemUpdateId = new Watchable<uint>(aThread, string.Format("SystemUpdateId({0})", aId), iService.PropertySystemUpdateID());
                iContainerUpdateIds = new Watchable<string>(aThread, string.Format("ContainerUpdateId({0})", aId), iService.PropertyContainerUpdateIDs());
            }
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

        public IWatchable<uint> SystemUpdateId
        {
            get
            {
                return iSystemUpdateId;
            }
        }

        public IWatchable<string> ContainerUpdateIds
        {
            get
            {
                return iContainerUpdateIds;
            }
        }

        public void Browse(string aObjectId, string aBrowseFlag, string aFilter, uint aStartingIndex, uint aRequestedCount, string aSortCriteria, ContentDirectory.BrowseResultHandler aHandler)
        {
            BrowseAsyncHandler handler = new BrowseAsyncHandler(iService, aHandler);
            handler.Browse(aObjectId, aBrowseFlag, aFilter, aStartingIndex, aRequestedCount, aSortCriteria);
        }

        private void HandleSystemUpdateIDChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iSystemUpdateId.Update(iService.PropertySystemUpdateID());
            }
        }

        private void HandleContainerUpdateIDsChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iContainerUpdateIds.Update(iService.PropertyContainerUpdateIDs());
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyUpnpOrgContentDirectory1 iService;

        private Watchable<uint> iSystemUpdateId;
        private Watchable<string> iContainerUpdateIds;
    }

    public class MockServiceUpnpOrgContentDirectory1 : IServiceUpnpOrgContentDirectory1, IMockable, IDisposable
    {
        public MockServiceUpnpOrgContentDirectory1(IWatchableThread aThread, string aId, uint aSystemUpdateId, string aContainerUpdateIds)
        {
            iThread = aThread;

            iSystemUpdateId = new Watchable<uint>(aThread, string.Format("SystemUpdateId({0})", aId), aSystemUpdateId);
            iContainerUpdateIds = new Watchable<string>(aThread, string.Format("ContainerUpdateIds({0})", aId), aContainerUpdateIds);
        }

        public void Dispose()
        {
        }

        public IWatchable<uint> SystemUpdateId
        {
            get
            {
                return iSystemUpdateId;
            }
        }

        public IWatchable<string> ContainerUpdateIds
        {
            get
            {
                return iContainerUpdateIds;
            }
        }

        public void Browse(string aObjectId, string aBrowseFlag, string aFilter, uint aStartingIndex, uint aRequestedCount, string aSortCriteria, ContentDirectory.BrowseResultHandler aHandler)
        {
            iThread.Schedule(() =>
            {
                aHandler(new BrowseResult("", 0, 0, 0));
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

        private Watchable<uint> iSystemUpdateId;
        private Watchable<string> iContainerUpdateIds;
    }

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
}
