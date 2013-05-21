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

    public interface IServiceMediaServer : IProxy
    {
        IEnumerable<string> Attributes { get; }
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

    public class ServiceFactoryMediaServer
    {
        private readonly IWatchableThread iWatchableThread;
        private readonly IEnumerable<string> iAttributes;
        private readonly string iManufacturerImageUri;
        private readonly string iManufacturerInfo;
        private readonly string iManufacturerName;
        private readonly string iManufacturerUrl;
        private readonly string iModelImageUri;
        private readonly string iModelInfo;
        private readonly string iModelName;
        private readonly string iModelUrl;
        private readonly string iProductImageUri;
        private readonly string iProductInfo;
        private readonly string iProductName;
        private readonly string iProductUrl;

        protected ServiceFactoryMediaServer(IWatchableThread aWatchableThread, IEnumerable<string> aAttributes, 
            string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl,
            string aModelImageUri, string aModelInfo, string aModelName, string aModelUrl,
            string aProductImageUri, string aProductInfo, string aProductName, string aProductUrl
            )
        {
            iWatchableThread = aWatchableThread;
            iAttributes = aAttributes;
            iManufacturerImageUri = aManufacturerImageUri;
            iManufacturerInfo = aManufacturerInfo;
            iManufacturerName = aManufacturerName;
            iManufacturerUrl = aManufacturerUrl;
            iModelImageUri = aModelImageUri;
            iModelInfo = aModelInfo;
            iModelName = aModelName;
            iModelUrl = aModelUrl;
            iProductImageUri = aProductImageUri;
            iProductInfo = aProductInfo;
            iProductName = aProductName;
            iProductUrl = aProductUrl;
        }

        // IService

        public IWatchableDevice Device { get { return (null); } } // TODO REmove this once it is removed from IService

        // IServiceMediaServer

        public IEnumerable<string> Attributes
        {
            get { return (iAttributes); }
        }

        public string ManufacturerImageUri
        {
            get { return (iManufacturerImageUri); }
        }

        public string ManufacturerInfo
        {
            get { return (iManufacturerInfo); }
        }

        public string ManufacturerName
        {
            get { return (iManufacturerName); }
        }

        public string ManufacturerUrl
        {
            get { return (iManufacturerUrl); }
        }

        public string ModelImageUri
        {
            get { return (iModelImageUri); }
        }

        public string ModelInfo
        {
            get { return (iModelInfo); }
        }

        public string ModelName
        {
            get { return (iModelName); }
        }

        public string ModelUrl
        {
            get { return (iModelUrl); }
        }

        public string ProductImageUri
        {
            get { return (iProductImageUri); }
        }

        public string ProductInfo
        {
            get { return (iProductInfo); }
        }

        public string ProductName
        {
            get { return (iProductName); }
        }

        public string ProductUrl
        {
            get { return (iProductUrl); }
        }

        public Task<IMediaServerSession> CreateSession()
        {
            throw new NotImplementedException();
        }
    }

    internal class ServiceMediaServerMock : IServiceMediaServer
    {
        ServiceFactoryMediaServerMock iFactory;

        public ServiceMediaServerMock(ServiceFactoryMediaServerMock aFactory)
        {
        }

        // IService

        public IWatchableDevice Device { get { return (null); } } // TODO REmove this once it is removed from IService

        // IServiceMediaServer

        public IEnumerable<string> Attributes
        {
            get { return (iFactory.Attributes); }
        }

        public string ManufacturerImageUri
        {
            get { return (iFactory.ManufacturerImageUri); }
        }

        public string ManufacturerInfo
        {
            get { return (iFactory.ManufacturerInfo); }
        }

        public string ManufacturerName
        {
            get { return (iFactory.ManufacturerName); }
        }

        public string ManufacturerUrl
        {
            get { return (iFactory.ManufacturerUrl); }
        }

        public string ModelImageUri
        {
            get { return (iFactory.ModelImageUri); }
        }

        public string ModelInfo
        {
            get { return (iFactory.ModelInfo); }
        }

        public string ModelName
        {
            get { return (iFactory.ModelName); }
        }

        public string ModelUrl
        {
            get { return (iFactory.ModelUrl); }
        }

        public string ProductImageUri
        {
            get { return (iFactory.ProductImageUri); }
        }

        public string ProductInfo
        {
            get { return (iFactory.ProductInfo); }
        }

        public string ProductName
        {
            get { return (iFactory.ProductName); }
        }

        public string ProductUrl
        {
            get { return (iFactory.ProductUrl); }
        }

        public Task<IMediaServerSession> CreateSession()
        {
            return (iFactory.CreateSession());
        }

        // IDisposable

        public void Dispose()
        {
            iFactory.Destroy(this);
        }
    }

    public class ServiceFactoryMediaServerMock : ServiceFactoryMediaServer, IServiceFactory
    {
        public ServiceFactoryMediaServerMock(IWatchableThread aWatchableThread, IEnumerable<string> aAttributes, 
            string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl,
            string aModelImageUri, string aModelInfo, string aModelName, string aModelUrl,
            string aProductImageUri, string aProductInfo, string aProductName, string aProductUrl)
            : base(aWatchableThread, aAttributes,
            aManufacturerImageUri, aManufacturerInfo, aManufacturerName, aManufacturerUrl,
            aModelImageUri, aModelInfo, aModelName, aModelUrl,
            aProductImageUri, aProductInfo, aProductName, aProductUrl)
        {
        }

        internal void Destroy(IServiceMediaServer aService)
        {
        }

        // IServiceFactory

        public IProxy Create(IManagableWatchableDevice aDevice)
        {
            return (new ServiceMediaServerMock(this));
        }

        // IDispose

        public void Dispose()
        {
        }
    }



    public class WatchableMediaServerFactory : IWatchableServiceFactory
    {
        private readonly IWatchableThread iWatchableThread;
        private readonly IWatchableThread iSubscribeThread;

        private readonly object iLock;
        private CpProxyAvOpenhomeOrgReceiver1 iPendingService;
        
        private WatchableReceiver iService;
        private List<Action<IServiceFactory>> iPendingSubscribes;

        private bool iDisposed;

        public WatchableMediaServerFactory(IWatchableThread aWatchableThread, IWatchableThread aSubscribeThread)
        {
            iWatchableThread = aWatchableThread;
            iSubscribeThread = aSubscribeThread;

            iLock = new object();
            iPendingSubscribes = new List<Action<IServiceFactory>>();

            iDisposed = false;
        }

        public void Dispose()
        {
            iSubscribeThread.Execute(() =>
            {
                Unsubscribe();
                iDisposed = true;
            });
        }

        public void Subscribe(IWatchableDevice aDevice, Action<IServiceFactory> aCallback)
        {
            iSubscribeThread.Schedule(() =>
            {
                lock (iLock)
                {
                    if (!iDisposed)
                    {
                        if (iPendingService == null)
                        {
                            WatchableDevice d = aDevice as WatchableDevice;
                            iPendingService = new CpProxyAvOpenhomeOrgReceiver1(d.Device);
                            iPendingService.SetPropertyInitialEvent(delegate
                            {
                                lock (iLock)
                                {
                                    if (iPendingService != null)
                                    {
                                        iService = new WatchableReceiver(iWatchableThread, string.Format("Receiver({0})", aDevice.Udn), iPendingService);
                                        iPendingService = null;
                                        aCallback(iService);
                                        foreach (Action<IServiceFactory> c in iPendingSubscribes)
                                        {
                                            c(iService);
                                        }
                                        iPendingSubscribes.Clear();
                                    }
                                }
                            });
                            iPendingService.Subscribe();
                        }
                        else
                        {
                            iPendingSubscribes.Add(aCallback);
                        }
                    }
                }
            });
        }

        public void Unsubscribe()
        {
            iSubscribeThread.Schedule(() =>
            {
                lock (iLock)
                {
                    if (iPendingService != null)
                    {
                        iPendingService.Dispose();
                        iPendingService = null;
                    }
                }
            });
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
