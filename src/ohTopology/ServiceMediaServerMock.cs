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
    public class ServiceMediaServerMock : ServiceMediaServer
    {
        private readonly IEnumerable<IMediaMetadata> iMetadata;
        private readonly IDeviceMediaServerMockUriProvider iUriProvider;

        private readonly List<IMediaServerSession> iSessions;
        
        public ServiceMediaServerMock(INetwork aNetwork, IEnumerable<string> aAttributes, 
            string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl,
            string aModelImageUri, string aModelInfo, string aModelName, string aModelUrl,
            string aProductImageUri, string aProductInfo, string aProductName, string aProductUrl,
            IEnumerable<IMediaMetadata> aMetadata, IDeviceMediaServerMockUriProvider aUriProvider)
            : base(aNetwork, aAttributes,
            aManufacturerImageUri, aManufacturerInfo, aManufacturerName, aManufacturerUrl,
            aModelImageUri, aModelInfo, aModelName, aModelUrl,
            aProductImageUri, aProductInfo, aProductName, aProductUrl)
        {
            iMetadata = aMetadata;
            iUriProvider = aUriProvider;

            iSessions = new List<IMediaServerSession>();
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return (new ProxyMediaServer(aDevice, this));
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

        private IWatchableContainer<IMediaDatum> iContainer;


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

        private Task<IWatchableContainer<IMediaDatum>> BrowseRootTracks()
        {
            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Title] != null)
                .OrderBy(m => m[iNetwork.TagManager.Audio.Title].Value)
                .Select(m => new MediaDatum(m));

            return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(tracks));
                return (iContainer);
            }));
        }

        private Task<IWatchableContainer<IMediaDatum>> BrowseRootArtists()
        {
            return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(iArtists));
                return (iContainer);
            }));
        }

        private Task<IWatchableContainer<IMediaDatum>> BrowseRootAlbums()
        {
            return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(iAlbums));
                return (iContainer);
            }));
        }

        private Task<IWatchableContainer<IMediaDatum>> BrowseRootGenres()
        {
            return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(iGenres));
                return (iContainer);
            }));
        }

        private Task<IWatchableContainer<IMediaDatum>> BrowseArtistAlbums(string aArtist)
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

            return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(albums));
                return (iContainer);
            }));
        }

        private Task<IWatchableContainer<IMediaDatum>> BrowseAlbumTracks(string aAlbum)
        {
            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Album].Value == aAlbum)
                .OrderBy(m => m[iNetwork.TagManager.Audio.Track] != null ? uint.Parse(m[iNetwork.TagManager.Audio.Track].Value) : 0)
                .Select(m => new MediaDatum(m));

            return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(tracks));
                return (iContainer);
            }));
        }

        private Task<IWatchableContainer<IMediaDatum>> BrowseGenreTracks(string aGenre)
        {
            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Genre] != null)
                .Where(m => m[iNetwork.TagManager.Audio.Genre].Values.Contains(aGenre))
                .OrderBy(m => m[iNetwork.TagManager.Audio.Title] != null ? m[iNetwork.TagManager.Audio.Title].Value : "")
                .Select(m => new MediaDatum(m));

            return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                iContainer = new MediaServerContainerMock(iNetwork, new MediaServerSnapshotMock(tracks));
                return (iContainer);
            }));
        }

        internal void Destroy(IWatchableContainer<IMediaDatum> aContainer)
        {
        }

        // IMediaServerSession

        public Task<IWatchableContainer<IMediaDatum>> Browse(IMediaDatum aDatum)
        {
            if (aDatum == null)
            {
                return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
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

        public Task<IWatchableContainer<IMediaDatum>> Link(ITag aTag, string aValue)
        {
            if (aTag == iNetwork.TagManager.Audio.Artist)
            {
                return (BrowseArtistAlbums(aValue));
            }

            if (aTag == iNetwork.TagManager.Audio.Album)
            {
                return (BrowseAlbumTracks(aValue));
            }

            if (aTag == iNetwork.TagManager.Audio.Genre)
            {
                return (BrowseGenreTracks(aValue));
            }

            return (null);
        }

        public Task<IWatchableContainer<IMediaDatum>> Query(string aValue)
        {
            throw new NotImplementedException();
        }

        // Disposable

        public void Dispose()
        {
            iService.Destroy(this);
        }
    }

    internal class MediaServerContainerMock : IWatchableContainer<IMediaDatum>
    {
        private readonly Watchable<IWatchableSnapshot<IMediaDatum>> iSnapshot;

        public MediaServerContainerMock(INetwork aNetwork, IWatchableSnapshot<IMediaDatum> aSnapshot)
        {
            iSnapshot = new Watchable<IWatchableSnapshot<IMediaDatum>>(aNetwork, "snapshot", aSnapshot);
        }

        // IMediaServerContainer

        public IWatchable<IWatchableSnapshot<IMediaDatum>> Snapshot
        {
            get { return (iSnapshot); }
        }
    }


    internal class MediaServerSnapshotMock : IWatchableSnapshot<IMediaDatum>
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

        public Task<IWatchableFragment<IMediaDatum>> Read(uint aIndex, uint aCount)
        {
            Do.Assert(aIndex + aCount <= Total);

            return (Task.Factory.StartNew<IWatchableFragment<IMediaDatum>>(() =>
            {
                return (new WatchableFragment<IMediaDatum>(aIndex, iData.Skip((int)aIndex).Take((int)aCount)));
            }));
        }
    }
}
