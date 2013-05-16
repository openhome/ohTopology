using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.MediaServer;


namespace OpenHome.Av
{
    public interface IMediaValue
    {
        string Value { get; }
        IEnumerable<string> Values { get; }
    }

    public interface IMediaMetadata : IEnumerable<KeyValuePair<ITag, IMediaValue>>
    {
        IMediaValue this[ITag aTag] { get; }
    }

    public interface IMediaDatum : IMediaMetadata
    {
        IEnumerable<ITag> Type { get; }
    }

    public interface IMediaServerFragment
    {
        uint Index { get; }
        uint Sequence { get; }
        IEnumerable<IMediaDatum> Data { get; }
    }

    public interface IMediaServerSnapshot
    {
        uint Total { get; }
        uint Sequence { get; }
        IEnumerable<uint> AlphaMap { get; } // null if no alpha map
        Task<IMediaServerFragment> Read(uint aIndex, uint aCount);
    }

    public interface IMediaServerContainer
    {
        IWatchable<IMediaServerSnapshot> Snapshot { get; }
    }

    public interface IMediaServerSession : IDisposable
    {
        Task<IMediaServerContainer> Query(string aValue);
        Task<IMediaServerContainer> Browse(IMediaDatum aDatum); // null = home
    }

    public interface IServiceMediaServer
    {
        string Name { get; }
        string Attributes { get; }
        string ManufacturerImageUri { get; }
        string ManufacturerInfo { get; }
        string ManufacturerName { get; }
        string ManufacturerUrl { get; }
        string ModelImageUri { get; }
        string ModelInfo { get; }
        string ModelName { get; }
        string ModelUrl { get; }
        string ProductImageUri { get; }
        string ProductInfo { get; }
        string ProductName { get; }
        string ProductUrl { get; }
        bool SupportsQuery { get; }
        bool SupportsBrowse { get; }
        Task<IMediaServerSession> CreateSession();
    }

    public class MediaServerValue : IMediaValue
    {
        private readonly string iValue;
        private readonly List<string> iValues;

        public MediaServerValue(string aValue)
        {
            iValue = aValue;
            iValues = new List<string>(new string[] { aValue });
        }

        public MediaServerValue(IEnumerable<string> aValues)
        {
            iValue = aValues.First();
            iValues = new List<string>(aValues);
        }

        // IMediaServerValue

        public string Value
        {
            get { return (iValue); }
        }

        public IEnumerable<string> Values
        {
            get { return (iValues); }
        }
    }

    public class MediaMetadata : IMediaMetadata
    {
        private Dictionary<ITag, IMediaValue> iMetadata;

        public MediaMetadata()
        {
            iMetadata = new Dictionary<ITag, IMediaValue>();
        }

        public void Add(ITag aTag, string aValue)
        {
            IMediaValue value = null;

            iMetadata.TryGetValue(aTag, out value);

            if (value == null)
            {
                iMetadata[aTag] = new MediaServerValue(aValue);
            }
            else
            {
                iMetadata[aTag] = new MediaServerValue(value.Values.Concat(new string[] { aValue }));
            }
        }

        public IDictionary<ITag, IMediaValue> Metadata
        {
            get
            {
                return (iMetadata);
            }
        }

        // IMediaServerMetadata

        public IMediaValue this[ITag aTag]
        {
            get
            {
                IMediaValue value = null;
                iMetadata.TryGetValue(aTag, out value);
                return (value);
            }
        }


        // IEnumerable<KeyValuePair<ITag, IMediaServer>>

        public IEnumerator<KeyValuePair<ITag, IMediaValue>> GetEnumerator()
        {
            return (iMetadata.GetEnumerator());
        }

        // IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (iMetadata.GetEnumerator());
        }
    }

    /*
    public class ServiceAvOpenHomeOrgMediaServer1 : IServiceMediaServer
    {
        private readonly IWatchableThread iWatchableThread;
        private readonly CpProxyAvOpenhomeOrgMediaServer1 iService;

        public ServiceAvOpenHomeOrgMediaServer1(IWatchableThread aWatchableThread, CpProxyAvOpenhomeOrgMediaServer1 aService)
        {
            iWatchableThread = aWatchableThread;
            iService = aService;
            iService.Subscribe();
        }        

        public void Dispose()
        {
            iService.Dispose();
        }
    }
    */

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

    /*
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
            
            string command = aValue.First().ToLowerInvariant();
            
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
            {
                throw new NotSupportedException();
            }
        }

        private IWatchableThread iThread;

        private Watchable<uint> iUpdateCount;
    }
    */

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
