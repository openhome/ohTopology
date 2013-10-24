using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public class ServiceMediaEndpointMock : ServiceMediaEndpoint, IMediaEndpointClient
    {
        private readonly IEnumerable<IMediaMetadata> iMetadata;
        //private readonly IDeviceMediaEndpointMockUriProvider iUriProvider;
        private readonly MediaEndpointSupervisor iSupervisor;

        private readonly IEnumerable<IMediaDatum> iArtists;
        private readonly IEnumerable<IMediaDatum> iAlbums;
        private readonly IEnumerable<IMediaDatum> iGenres;
        private readonly List<IMediaDatum> iRoot;

        public ServiceMediaEndpointMock(INetwork aNetwork, IInjectorDevice aDevice, string aId, string aType, string aName, string aInfo,
            string aUrl, string aArtwork, string aManufacturerName, string aManufacturerInfo, string aManufacturerUrl,
            string aManufacturerArtwork, string aModelName, string aModelInfo, string aModelUrl, string aModelArtwork,
            DateTime aStarted, IEnumerable<string> aAttributes, IEnumerable<IMediaMetadata> aMetadata, IDeviceMediaEndpointMockUriProvider aUriProvider, ILog aLog)
            : base (aNetwork, aDevice, aId, aType, aName, aInfo, aUrl, aArtwork, aManufacturerName, aManufacturerInfo,
            aManufacturerUrl, aManufacturerArtwork, aModelName, aModelInfo, aModelUrl, aModelArtwork, aStarted, aAttributes, aLog)
        {
            iMetadata = aMetadata;
            //iUriProvider = aUriProvider;
            iSupervisor = new MediaEndpointSupervisor(this);

            iArtists = iMetadata.Where(m => m[iNetwork.TagManager.Audio.AlbumArtist] != null)
                .SelectMany(m => m[iNetwork.TagManager.Audio.AlbumArtist].Values)
                .Distinct()
                .OrderBy(v => v)
                .Select(v =>
                {
                    var datum = new MediaDatum(null, iNetwork.TagManager.Audio.AlbumArtist, iNetwork.TagManager.Audio.Album);
                    datum.Add(iNetwork.TagManager.Container.Title, v);
                    datum.Add(iNetwork.TagManager.Audio.AlbumArtist, v);
                    return (datum);
                });

            iAlbums = iMetadata.GroupBy(m => m[iNetwork.TagManager.Audio.Album].Value)
                .Select(m =>
                {
                    var datum = new MediaDatum(null, iNetwork.TagManager.Audio.Album);
                    
                    var title = m.First()[iNetwork.TagManager.Audio.AlbumTitle];

                    if (title != null)
                    {
                        datum.Add(iNetwork.TagManager.Container.Title, title);
                    }

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
                    var datum = new MediaDatum(null, iNetwork.TagManager.Audio.Genre);
                    datum.Add(iNetwork.TagManager.Container.Title, v);
                    datum.Add(iNetwork.TagManager.Audio.Genre, v);
                    return (datum);
                });

            iRoot = new List<IMediaDatum>();
            iRoot.Add(GetRootContainerTracks());
            iRoot.Add(GetRootContainerArtists());
            iRoot.Add(GetRootContainerAlbums());
            iRoot.Add(GetRootContainerGenres());
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return (new ProxyMediaEndpoint(this, aDevice));
        }

        public override Task<IMediaEndpointSession> CreateSession()
        {
            return (iSupervisor.CreateSession());
        }

        private IMediaDatum GetRootContainerTracks()
        {
            var datum = new MediaDatum(null, iNetwork.TagManager.Container.Title);
            datum.Add(iNetwork.TagManager.Container.Title, "Tracks");
            return (datum);
        }

        private IMediaDatum GetRootContainerArtists()
        {
            var datum = new MediaDatum(null, iNetwork.TagManager.Container.Title, iNetwork.TagManager.Audio.Artist, iNetwork.TagManager.Audio.Album);
            datum.Add(iNetwork.TagManager.Container.Title, "Artist");
            return (datum);
        }

        private IMediaDatum GetRootContainerAlbums()
        {
            var datum = new MediaDatum(null, iNetwork.TagManager.Container.Title, iNetwork.TagManager.Audio.Album);
            datum.Add(iNetwork.TagManager.Container.Title, "Album");
            return (datum);
        }

        private IMediaDatum GetRootContainerGenres()
        {
            var datum = new MediaDatum(null, iNetwork.TagManager.Container.Title, iNetwork.TagManager.Audio.Genre);
            datum.Add(iNetwork.TagManager.Container.Title, "Genre");
            return (datum);
        }

        private IMediaEndpointClientSnapshot BrowseRootTracks()
        {
            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Title] != null)
                .OrderBy(m => m[iNetwork.TagManager.Audio.Title].Value)
                .Select(m => new MediaDatum(m, null));

            return (new MediaEndpointSnapshotMock(tracks));
        }

        private IMediaEndpointClientSnapshot BrowseRootArtists()
        {
            return (new MediaEndpointSnapshotMock(iArtists));
        }

        private IMediaEndpointClientSnapshot BrowseRootAlbums()
        {
            return (new MediaEndpointSnapshotMock(iAlbums));
        }

        private IMediaEndpointClientSnapshot BrowseRootGenres()
        {
            return (new MediaEndpointSnapshotMock(iGenres));
        }

        private IMediaEndpointClientSnapshot BrowseArtistAlbums(string aArtist)
        {
            var albums = iMetadata.Where(m => m[iNetwork.TagManager.Audio.AlbumArtist] != null)
                .Where(m => m[iNetwork.TagManager.Audio.AlbumArtist].Values.Contains(aArtist))
                .GroupBy(m => m[iNetwork.TagManager.Audio.Album].Value)
                .Select(m =>
                {
                    var datum = new MediaDatum(null, iNetwork.TagManager.Audio.Album);

                    datum.Add(iNetwork.TagManager.Audio.Album, m.Key);

                    var title = m.First()[iNetwork.TagManager.Audio.AlbumTitle];

                    if (title != null)
                    {
                        datum.Add(iNetwork.TagManager.Container.Title, title);
                        datum.Add(iNetwork.TagManager.Audio.AlbumTitle, title);
                    }

                    datum.Add(iNetwork.TagManager.Audio.AlbumArtist, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumArtworkCodec, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumArtworkFilename, m.First());
                    datum.Add(iNetwork.TagManager.Audio.AlbumDiscs, m.First());
                    datum.Add(iNetwork.TagManager.Audio.Artist, aArtist);
                    return (datum);
                });

            return (new MediaEndpointSnapshotMock(albums));
        }

        private IMediaEndpointClientSnapshot BrowseAlbumTracks(string aAlbum)
        {
            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Album].Value == aAlbum)
                .OrderBy(m => m[iNetwork.TagManager.Audio.Track] != null ? uint.Parse(m[iNetwork.TagManager.Audio.Track].Value) : 0)
                .Select(m => new MediaDatum(m, null));

            return (new MediaEndpointSnapshotMock(tracks));
        }

        private IMediaEndpointClientSnapshot BrowseGenreTracks(string aGenre)
        {
            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Genre] != null)
                .Where(m => m[iNetwork.TagManager.Audio.Genre].Values.Contains(aGenre))
                .OrderBy(m => m[iNetwork.TagManager.Audio.Title] != null ? m[iNetwork.TagManager.Audio.Title].Value : "")
                .Select(m => new MediaDatum(m, null));

            return (new MediaEndpointSnapshotMock(tracks));
        }

        private IEnumerable<IMediaDatum> MatchMetadata(ITag aTag, string aValue)
        {
            var values = Tokeniser.Parse(aValue.ToLower());

            if (values.Any())
            {
                foreach (var metadata in iMetadata)
                {
                    if (MatchMetadata(aTag, values, metadata))
                    {
                        yield return new MediaDatum(metadata, null);
                    }
                }
            }
        }

        private bool MatchMetadata(ITag aTag, IEnumerable<string> aValues, IMediaMetadata aMetadata)
        {
            foreach (var value in aValues)
            {
                if (!MatchMetadataToken(aTag, value, aMetadata))
                {
                    return (false);
                }
            }

            return (true);
        }

        private bool MatchMetadataToken(ITag aTag, string aValue, IMediaMetadata aMetadata)
        {
            if (aValue.EndsWith("*"))
            {
                return (MatchMetadataTokenStartsWith(aTag, aValue.Substring(0, aValue.Length - 1), aMetadata));
            }

            foreach (var metadatum in aMetadata)
            {
                if (metadatum.Key == aTag)
                {
                    foreach (var value in metadatum.Value.Values)
                    {
                        var tokens = Tokeniser.Parse(value);

                        foreach (var token in tokens)
                        {
                            if (token.ToLower() == aValue)
                            {
                                return (true);
                            }
                        }
                    }
                }
            }

            return (false);
        }

        private bool MatchMetadataTokenStartsWith(ITag aTag, string aValue, IMediaMetadata aMetadata)
        {
            if (aValue.Length == 0)
            {
                return (true);
            }

            foreach (var metadatum in aMetadata)
            {
                if (metadatum.Key == aTag)
                {
                    foreach (var value in metadatum.Value.Values)
                    {
                        var tokens = Tokeniser.Parse(value);

                        foreach (var token in tokens)
                        {
                            if (token.ToLower().StartsWith(aValue))
                            {
                                return (true);
                            }
                        }
                    }
                }
            }

            return (false);
        }

        private IEnumerable<IMediaDatum> SearchMetadata(string aValue)
        {
            var values = Tokeniser.Parse(aValue.ToLower());

            if (values.Any())
            {
                foreach (var metadata in iMetadata)
                {
                    if (SearchMetadata(values, metadata))
                    {
                        yield return new MediaDatum(metadata, null);
                    }
                }
            }
        }

        private bool SearchMetadata(IEnumerable<string> aValues, IMediaMetadata aMetadata)
        {
            foreach (var value in aValues)
            {
                if (!SearchMetadataToken(value, aMetadata))
                {
                    return (false);
                }
            }

            return (true);
        }

        private bool SearchMetadataToken(string aValue, IMediaMetadata aMetadata)
        {
            if (aValue.EndsWith("*"))
            {
                return (SearchMetadataTokenStartsWith(aValue.Substring(0, aValue.Length - 1), aMetadata));
            }

            foreach (var metadatum in aMetadata)
            {
                if (metadatum.Key.IsSearchable)
                {
                    foreach (var value in metadatum.Value.Values)
                    {
                        var tokens = Tokeniser.Parse(value);

                        foreach (var token in tokens)
                        {
                            if (token.ToLower() == aValue)
                            {
                                return (true);
                            }
                        }
                    }
                }
            }

            return (false);
        }

        private bool SearchMetadataTokenStartsWith(string aValue, IMediaMetadata aMetadata)
        {
            if (aValue.Length == 0)
            {
                return (true);
            }

            foreach (var metadatum in aMetadata)
            {
                if (metadatum.Key.IsSearchable)
                {
                    foreach (var value in metadatum.Value.Values)
                    {
                        var tokens = Tokeniser.Parse(value);

                        foreach (var token in tokens)
                        {
                            if (token.ToLower().StartsWith(aValue))
                            {
                                return (true);
                            }
                        }
                    }
                }
            }

            return (false);
        }

        // IMediaEndpointClient

        public Task<string> Create(CancellationToken aCancellationToken)
        {
            var tcs = new TaskCompletionSource<string>();
            tcs.SetResult(Guid.NewGuid().ToString());
            return (tcs.Task);
        }

        public Task<string> Destroy(CancellationToken aCancellationToken, string aId)
        {
            var tcs = new TaskCompletionSource<string>();
            tcs.SetResult(aId);
            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> Browse(CancellationToken aCancellationToken, string aSession, IMediaDatum aDatum)
        {
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();

            if (aDatum == null)
            {
                tcs.SetResult(new MediaEndpointSnapshotMock(iRoot));
                return (tcs.Task);
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
                        tcs.SetResult(BrowseRootArtists());
                        return (tcs.Task);
                    }

                    if (tag == iNetwork.TagManager.Audio.Album)
                    {
                        tcs.SetResult(BrowseRootAlbums());
                        return (tcs.Task);
                    }

                    if (tag == iNetwork.TagManager.Audio.Genre)
                    {
                        tcs.SetResult(BrowseRootGenres());
                        return (tcs.Task);
                    }

                    Do.Assert(false);
                }

                tcs.SetResult(BrowseRootTracks());
                return (tcs.Task);
            }

            if (aDatum.Type.First() == iNetwork.TagManager.Audio.AlbumArtist)
            {
                // Artist/Album

                var artist = aDatum[iNetwork.TagManager.Audio.AlbumArtist].Value;
                tcs.SetResult(BrowseArtistAlbums(artist));
                return (tcs.Task);
            }

            if (aDatum.Type.First() == iNetwork.TagManager.Audio.Album)
            {
                // Artist/Album

                var album = aDatum[iNetwork.TagManager.Audio.Album].Value;
                tcs.SetResult(BrowseAlbumTracks(album));
                return (tcs.Task);
            }

            Do.Assert(aDatum.Type.First() == iNetwork.TagManager.Audio.Genre);

            // Genre/Tracks

            var genre = aDatum[iNetwork.TagManager.Audio.Genre].Value;
            tcs.SetResult(BrowseGenreTracks(genre));
            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> List(CancellationToken aCancellationToken, string aSession, ITag aTag)
        {
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();
            tcs.SetException(new InvalidOperationException());
            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> Link(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue)
        {
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();

            if (aTag == iNetwork.TagManager.Audio.Artist)
            {
                tcs.SetResult(BrowseArtistAlbums(aValue));
                return (tcs.Task);
            }

            if (aTag == iNetwork.TagManager.Audio.Album)
            {
                tcs.SetResult(BrowseAlbumTracks(aValue));
                return (tcs.Task);
            }

            if (aTag == iNetwork.TagManager.Audio.Genre)
            {
                tcs.SetResult(BrowseGenreTracks(aValue));
                return (tcs.Task);
            }

            tcs.SetException(new InvalidOperationException());

            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> Match(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue)
        {
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();
            tcs.SetResult(new MediaEndpointSnapshotMock(MatchMetadata(aTag, aValue)));
            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> Search(CancellationToken aCancellationToken, string aSession, string aValue)
        {
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();
            tcs.SetResult(new MediaEndpointSnapshotMock(SearchMetadata(aValue)));
            return (tcs.Task);
        }

        public Task<IEnumerable<IMediaDatum>> Read(CancellationToken aCancellationToken, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount)
        {
            var snapshot = aSnapshot as MediaEndpointSnapshotMock;

            return snapshot.Read(aIndex, aCount);
        }

        // IDispose

        public override void Dispose()
        {
            iSupervisor.Cancel();

            base.Dispose();
            
            iSupervisor.Dispose();
        }
    }

    internal class MediaEndpointSnapshotMock : IMediaEndpointClientSnapshot
    {
        private readonly IEnumerable<IMediaDatum> iData;
        private readonly IEnumerable<uint> iAlphaMap;

        public MediaEndpointSnapshotMock(IEnumerable<IMediaDatum> aData)
        {
            iData = aData.ToArray();
            iAlphaMap = null;
        }

        public Task<IEnumerable<IMediaDatum>> Read(uint aIndex, uint aCount)
        {
            var tcs = new TaskCompletionSource<IEnumerable<IMediaDatum>>();
            tcs.SetResult(iData.Skip((int)aIndex).Take((int)aCount));
            return (tcs.Task);
        }

        // IMediaEndpointClientSnapshot

        public uint Total
        {
            get { return ((uint)iData.Count()); }
        }

        public IEnumerable<uint> Alpha
        {
            get { return (iAlphaMap); }
        }
    }

    internal static class ServiceMediaEndpointMockExtensions
    {
        public static void Add(this MediaDatum aDatum, ITag aTag, IMediaMetadata aMetadata)
        {
            var value = aMetadata[aTag];

            if (value != null)
            {
                aDatum.Add(aTag, value);
            }
        }
    }
}
