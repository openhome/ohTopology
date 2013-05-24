using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using OpenHome.Os.App;
using OpenHome.MediaServer;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
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

    public interface IProxyMediaServer : IProxy
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

    public class MediaServerFragment : IMediaServerFragment
    {
        private readonly uint iIndex;
        private readonly uint iSequence;
        private readonly IEnumerable<IMediaDatum> iData;

        public MediaServerFragment(uint aIndex, uint aSequence, IEnumerable<IMediaDatum> aData)
        {
            iIndex = aIndex;
            iSequence = aSequence;
            iData = aData;
        }

        // IMediaServerFragment

        public uint Index
        {
            get { return (iIndex); }
        }

        public uint Sequence
        {
            get { return (iSequence); }
        }

        public IEnumerable<IMediaDatum> Data
        {
            get { return (iData); }
        }
    }

    internal class ProxyMediaServer : Proxy<ServiceMediaServer>, IProxyMediaServer
    {
        public ProxyMediaServer(IWatchableDevice aDevice, ServiceMediaServerMock aService)
            : base(aDevice, aService)
        {
        }

        // IProxyMediaServer

        public IEnumerable<string> Attributes
        {
            get { return (iService.Attributes); }
        }

        public string ManufacturerImageUri
        {
            get { return (iService.ManufacturerImageUri); }
        }

        public string ManufacturerInfo
        {
            get { return (iService.ManufacturerInfo); }
        }

        public string ManufacturerName
        {
            get { return (iService.ManufacturerName); }
        }

        public string ManufacturerUrl
        {
            get { return (iService.ManufacturerUrl); }
        }

        public string ModelImageUri
        {
            get { return (iService.ModelImageUri); }
        }

        public string ModelInfo
        {
            get { return (iService.ModelInfo); }
        }

        public string ModelName
        {
            get { return (iService.ModelName); }
        }

        public string ModelUrl
        {
            get { return (iService.ModelUrl); }
        }

        public string ProductImageUri
        {
            get { return (iService.ProductImageUri); }
        }

        public string ProductInfo
        {
            get { return (iService.ProductInfo); }
        }

        public string ProductName
        {
            get { return (iService.ProductName); }
        }

        public string ProductUrl
        {
            get { return (iService.ProductUrl); }
        }

        public Task<IMediaServerSession> CreateSession()
        {
            return (iService.CreateSession());
        }
    }

    public abstract class ServiceMediaServer : Service
    {
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

        protected ServiceMediaServer(INetwork aNetwork, IEnumerable<string> aAttributes, 
            string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl,
            string aModelImageUri, string aModelInfo, string aModelName, string aModelUrl,
            string aProductImageUri, string aProductInfo, string aProductName, string aProductUrl)
            : base (aNetwork)
        {
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

        // IProxyMediaServer

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

        public abstract Task<IMediaServerSession> CreateSession();
    }

    public class ServiceMediaServerMock : ServiceMediaServer
    {
        private readonly IEnumerable<IMediaMetadata> iMetadata;
        private readonly IMockMediaServerUriProvider iUriProvider;

        private readonly List<IMediaServerSession> iSessions;
        
        public ServiceMediaServerMock(INetwork aNetwork, IEnumerable<string> aAttributes, 
            string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl,
            string aModelImageUri, string aModelInfo, string aModelName, string aModelUrl,
            string aProductImageUri, string aProductInfo, string aProductName, string aProductUrl,
            IEnumerable<IMediaMetadata> aMetadata, IMockMediaServerUriProvider aUriProvider)
            : base(aNetwork, aAttributes,
            aManufacturerImageUri, aManufacturerInfo, aManufacturerName, aManufacturerUrl,
            aModelImageUri, aModelInfo, aModelName, aModelUrl,
            aProductImageUri, aProductInfo, aProductName, aProductUrl)
        {
            iMetadata = aMetadata;
            iUriProvider = aUriProvider;

            iSessions = new List<IMediaServerSession>();
        }

        public override IProxy OnCreate(IWatchableDevice aDevice)
        {
            return (new ProxyMediaServer(aDevice, this));
        }

        protected override void  OnSubscribe()
        {
        }

        protected override void  OnUnsubscribe()
        {
        }

        public override Task<IMediaServerSession> CreateSession()
        {
            return (Task.Factory.StartNew<IMediaServerSession>(() =>
            {
                var session = new MediaServerSessionMock(Network, iMetadata, this);
                iSessions.Add(session);
                return (session);
            }));
        }

        internal void Destroy(IMediaServerSession aSession)
        {
            iSessions.Remove(aSession);
        }

        // IDispose

        public override void Dispose()
        {
            base.Dispose();
            Do.Assert(iSessions.Count == 0);
        }
    }

    internal class MediaServerSessionMock : IMediaServerSession
    {
        private readonly INetwork iNetwork;
        private readonly IEnumerable<IMediaMetadata> iMetadata;
        private readonly ServiceMediaServerMock iService;
        
        private readonly IEnumerable<IMediaDatum> iArtists;
        private readonly IEnumerable<IMediaDatum> iAlbums;
        private readonly IEnumerable<IMediaDatum> iGenres;
        private readonly List<IMediaDatum> iRoot;

        private IMediaServerContainer iContainer;


        public MediaServerSessionMock(INetwork aNetwork, IEnumerable<IMediaMetadata> aMetadata, ServiceMediaServerMock aService)
        {
            iNetwork = aNetwork;
            iMetadata = aMetadata;
            iService = aService;

            iArtists = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Artist] != null)
                .SelectMany(m => m[iNetwork.TagManager.Audio.Artist].Values)
                .Distinct()
                .OrderBy(v => v)
                .Select(v =>
                {
                    var datum = new MediaDatum(iNetwork.TagManager.Audio.Artist, iNetwork.TagManager.Audio.Album);
                    datum.Add(iNetwork.TagManager.Audio.Artist, v);
                    return (datum);
                });

            iAlbums = iMetadata.GroupBy(m => m[iNetwork.TagManager.Audio.Album].Value)
                .Select(m =>
                {
                    var datum = new MediaDatum(iNetwork.TagManager.Audio.Album);
                    datum.Add(iNetwork.TagManager.Audio.Album, m.Key);
                    datum.Add(iNetwork.TagManager.Audio.AlbumTitle, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumArtist, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumArtworkCodec, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumArtworkFilename, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumDiscs, m.First());
                    return (datum);
                });


            iGenres = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Genre] != null)
                .SelectMany(m => m[iNetwork.TagManager.Audio.Genre].Values)
                .Distinct()
                .OrderBy(v => v)
                .Select(v =>
                {
                    var datum = new MediaDatum(iNetwork.TagManager.Audio.Genre);
                    datum.Add(iNetwork.TagManager.Audio.Genre, v);
                    return (datum);
                });

            iRoot = new List<IMediaDatum>();
            iRoot.Add(GetRootContainerTracks());
            iRoot.Add(GetRootContainerArtists());
            iRoot.Add(GetRootContainerAlbums());
            iRoot.Add(GetRootContainerGenres());
        }

        private IMediaDatum GetRootContainerTracks()
        {
            var datum = new MediaDatum(iNetwork.TagManager.Container.Title);
            datum.Add(iNetwork.TagManager.Container.Title, "Tracks");
            return (datum);
        }

        private IMediaDatum GetRootContainerArtists()
        {
            var datum = new MediaDatum(iNetwork.TagManager.Container.Title, iNetwork.TagManager.Audio.Artist, iNetwork.TagManager.Audio.Album);
            datum.Add(iNetwork.TagManager.Container.Title, "Artists");
            return (datum);
        }

        private IMediaDatum GetRootContainerAlbums()
        {
            var datum = new MediaDatum(iNetwork.TagManager.Container.Title, iNetwork.TagManager.Audio.Album);
            datum.Add(iNetwork.TagManager.Container.Title, "Albums");
            return (datum);
        }

        private IMediaDatum GetRootContainerGenres()
        {
            var datum = new MediaDatum(iNetwork.TagManager.Container.Title, iNetwork.TagManager.Audio.Genre);
            datum.Add(iNetwork.TagManager.Container.Title, "Genres");
            return (datum);
        }

        private Task<IMediaServerContainer> BrowseRootTracks()
        {
            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Title] != null)
                .OrderBy(m => m[iNetwork.TagManager.Audio.Title].Value)
                .Select(m => new MediaDatum(m));

            return (Task.Factory.StartNew<IMediaServerContainer>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(tracks));
                return (iContainer);
            }));
        }

        private Task<IMediaServerContainer> BrowseRootArtists()
        {
            return (Task.Factory.StartNew<IMediaServerContainer>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(iArtists));
                return (iContainer);
            }));
        }

        private Task<IMediaServerContainer> BrowseRootAlbums()
        {
            return (Task.Factory.StartNew<IMediaServerContainer>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(iAlbums));
                return (iContainer);
            }));
        }

        private Task<IMediaServerContainer> BrowseRootGenres()
        {
            return (Task.Factory.StartNew<IMediaServerContainer>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(iGenres));
                return (iContainer);
            }));
        }

        private Task<IMediaServerContainer> BrowseArtistAlbums(string aArtist)
        {
            var albums = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Artist] != null)
                .Where(m => m[iNetwork.TagManager.Audio.Artist].Values.Contains(aArtist))
                .GroupBy(m => m[iNetwork.TagManager.Audio.Album].Value)
                .Select(m =>
                {
                    var datum = new MediaDatum(iNetwork.TagManager.Audio.Album);
                    datum.Add(iNetwork.TagManager.Audio.Album, m.Key);
                    datum.Add(iNetwork.TagManager.Audio.AlbumTitle, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumArtist, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumArtworkCodec, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumArtworkFilename, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumDiscs, m.First());
                    datum.Add(iNetwork.TagManager.Audio.Artist, aArtist);
                    return (datum);
                });

            return (Task.Factory.StartNew<IMediaServerContainer>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(albums));
                return (iContainer);
            }));
        }

        private Task<IMediaServerContainer> BrowseAlbumTracks(string aAlbum)
        {
            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Album].Value == aAlbum)
                .OrderBy(m => uint.Parse(m[iNetwork.TagManager.Audio.Track].Value))
                .Select(m => new MediaDatum(m));

            return (Task.Factory.StartNew<IMediaServerContainer>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(tracks));
                return (iContainer);
            }));
        }

        private Task<IMediaServerContainer> BrowseGenreTracks(string aGenre)
        {
            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Genre] != null)
                .Where(m => m[iNetwork.TagManager.Audio.Genre].Values.Contains(aGenre))
                .OrderBy(m => m[iNetwork.TagManager.Audio.Title].Value)
                .Select(m => new MediaDatum(m));

            return (Task.Factory.StartNew<IMediaServerContainer>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(tracks));
                return (iContainer);
            }));
        }

        internal void Destroy(IMediaServerContainer aContainer)
        {
        }

        // IMediaServerSession

        public Task<IMediaServerContainer> Query(string aValue)
        {
            throw new NotImplementedException();
        }

        public Task<IMediaServerContainer> Browse(IMediaDatum aDatum)
        {
            if (aDatum == null)
            {
                return (Task.Factory.StartNew<IMediaServerContainer>(() =>
                {
                    iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(iRoot));
                    return (iContainer);
                }));
            }

            Do.Assert(aDatum.Type.Any());

            if (aDatum.Type.First() == iNetwork.TagManager.Container.Title)
            {
                // Top Level Container

                if (aDatum.Type.Skip(1).Any())
                {
                    ITag tag = aDatum.Type.Skip(1).First();

                    if (tag == iNetwork.TagManager.Audio.Artist)
                    {
                        return (BrowseRootArtists());
                    }

                    if (tag == iNetwork.TagManager.Audio.Album)
                    {
                        return (BrowseRootAlbums());
                    }

                    if (tag == iNetwork.TagManager.Audio.Genre)
                    {
                        return (BrowseRootGenres());
                    }

                    Do.Assert(false);
                }

                return (BrowseRootTracks());
            }

            if (aDatum.Type.First() == iNetwork.TagManager.Audio.Artist)
            {
                // Artist/Album

                var artist = aDatum[iNetwork.TagManager.Audio.Artist].Value;

                return (BrowseArtistAlbums(artist));
            }

            if (aDatum.Type.First() == iNetwork.TagManager.Audio.Album)
            {
                // Artist/Album

                var album = aDatum[iNetwork.TagManager.Audio.Album].Value;

                return (BrowseAlbumTracks(album));
            }

            Do.Assert(aDatum.Type.First() == iNetwork.TagManager.Audio.Genre);

            // Genre/Tracks

            var genre = aDatum[iNetwork.TagManager.Audio.Genre].Value;

            return (BrowseGenreTracks(genre));
        }

        // Disposable

        public void Dispose()
        {
            iService.Destroy(this);
        }
    }

    internal class MediaServerContainerMock : IMediaServerContainer
    {
        private readonly Watchable<IMediaServerSnapshot> iSnapshot;

        public MediaServerContainerMock(INetwork aNetwork, IMediaServerSnapshot aSnapshot)
        {
            iSnapshot = new Watchable<IMediaServerSnapshot>(aNetwork.WatchableThread, "snapshot", aSnapshot);
        }

        // IMediaServerContainer

        public IWatchable<IMediaServerSnapshot> Snapshot
        {
            get { return (iSnapshot); }
        }
    }


    internal class MediaServerSnapshotMock : IMediaServerSnapshot
    {
        private readonly IEnumerable<IMediaDatum> iData;
        private readonly IEnumerable<uint> iAlphaMap;

        public MediaServerSnapshotMock(IEnumerable<IMediaDatum> aData)
        {
            iData = aData;
            iAlphaMap = null;
        }

        // IMediaServerSnapshot

        public uint Total
        {
            get { return ((uint)iData.Count()); }
        }

        public uint Sequence
        {
            get { return (0); }
        }

        public IEnumerable<uint> AlphaMap
        {
            get { return (iAlphaMap); }
        }

        public Task<IMediaServerFragment> Read(uint aIndex, uint aCount)
        {
            Do.Assert(aIndex + aCount <= Total);

            return (Task.Factory.StartNew<IMediaServerFragment>(() =>
            {
                return (new MediaServerFragment(aIndex, 0, iData.Skip((int)aIndex).Take((int)aCount)));
            }));
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

    public static class ServiceMediaServerExtensions
    {
        public static bool SupportsBrowse(this IProxyMediaServer aProxy)
        {
            return (aProxy.Attributes.Contains("Browse"));
        }

        public static bool SupportsQuery(this IProxyMediaServer aProxy)
        {
            return (aProxy.Attributes.Contains("Query"));
        }
    }
}
