using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;

namespace TestMediaServer
{
    public class Client : IUnorderedWatcher<IDevice>, IWatcher<IWatchableSnapshot<IMediaDatum>>, IDisposable
    {
        private readonly INetwork iNetwork;
        
        private IWatchableUnordered<IDevice> iMediaServers;
        
        private IProxyMediaEndpoint iProxy;
        private IWatchableSnapshot<IMediaDatum> iSnaphot;

        public Client(INetwork aNetwork)
        {
            iNetwork = aNetwork;

            iNetwork.Execute(() =>
            {
                iMediaServers = aNetwork.Create<IProxyMediaEndpoint>();
                iMediaServers.AddWatcher(this);
            });

            iNetwork.Wait();
        }

        public void Run()
        {
            Do.Assert(iProxy.SupportsBrowse());
            Do.Assert(iProxy.SupportsLink());
            Do.Assert(iProxy.SupportsLink(iNetwork.TagManager.Audio.Artist));
            Do.Assert(iProxy.SupportsLink(iNetwork.TagManager.Audio.Album));
            Do.Assert(iProxy.SupportsLink(iNetwork.TagManager.Audio.Genre));
            Do.Assert(iProxy.SupportsSearch());

            var session = iProxy.CreateSession().Result;
            
            var root = session.Browse(null).Result;

            iNetwork.Execute(() =>
            {
                root.Snapshot.AddWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 4);

            var rootFragment = iSnaphot.Read(0, 4).Result;

            Do.Assert(rootFragment.Index == 0);
            Do.Assert(rootFragment.Data.Count() == 4);

            Do.Assert(rootFragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(rootFragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(rootFragment.Data.ElementAt(0)[iNetwork.TagManager.Container.Title].Value == "Tracks");

            Do.Assert(rootFragment.Data.ElementAt(1).Type.Count() == 3);
            Do.Assert(rootFragment.Data.ElementAt(1).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(rootFragment.Data.ElementAt(1).Type.ElementAt(1) == iNetwork.TagManager.Audio.Artist);
            Do.Assert(rootFragment.Data.ElementAt(1).Type.ElementAt(2) == iNetwork.TagManager.Audio.Album);
            Do.Assert(rootFragment.Data.ElementAt(1)[iNetwork.TagManager.Container.Title].Value == "Artists");

            Do.Assert(rootFragment.Data.ElementAt(2).Type.Count() == 2);
            Do.Assert(rootFragment.Data.ElementAt(2).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(rootFragment.Data.ElementAt(2).Type.ElementAt(1) == iNetwork.TagManager.Audio.Album);
            Do.Assert(rootFragment.Data.ElementAt(2)[iNetwork.TagManager.Container.Title].Value == "Albums");

            Do.Assert(rootFragment.Data.ElementAt(3).Type.Count() == 2);
            Do.Assert(rootFragment.Data.ElementAt(3).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(rootFragment.Data.ElementAt(3).Type.ElementAt(1) == iNetwork.TagManager.Audio.Genre);
            Do.Assert(rootFragment.Data.ElementAt(3)[iNetwork.TagManager.Container.Title].Value == "Genres");

            var tracks = session.Browse(rootFragment.Data.ElementAt(0)).Result;

            iNetwork.Execute(() =>
            {
                tracks.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                tracks.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 12521);

            var artists = session.Browse(rootFragment.Data.ElementAt(1)).Result;

            iNetwork.Execute(() =>
            {
                artists.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                artists.Snapshot.RemoveWatcher(this);
            });


            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 882);

            var fragment = iSnaphot.Read(100, 1).Result;

            Do.Assert(fragment.Index == 100);
            Do.Assert(fragment.Data.Count() == 1);

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 2);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Artist);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(1) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Bon Jovi");

            var artistAlbums = session.Browse(fragment.Data.ElementAt(0)).Result;

            iNetwork.Execute(() =>
            {
                artistAlbums.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                artistAlbums.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 2);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "Crossroad");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");

            var artistAlbumTracks = session.Browse(fragment.Data.ElementAt(0)).Result;

            iNetwork.Execute(() =>
            {
                artistAlbumTracks.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                artistAlbumTracks.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 15);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "Crossroad");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "Livin' on a prayer");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "251");

            var albums = session.Browse(rootFragment.Data.ElementAt(2)).Result;

            iNetwork.Execute(() =>
            {
                albums.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                albums.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 1000);

            fragment = iSnaphot.Read(100, 1).Result;

            Do.Assert(fragment.Index == 100);
            Do.Assert(fragment.Data.Count() == 1);

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Blur");

            var albumTracks = session.Browse(fragment.Data.ElementAt(0)).Result;

            iNetwork.Execute(() =>
            {
                albumTracks.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                albumTracks.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 18);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Blur");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "The Best Of");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Blur");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "Beetlebum");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "305");


            var genres = session.Browse(rootFragment.Data.ElementAt(3)).Result;

            iNetwork.Execute(() =>
            {
                genres.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                genres.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 124);

            fragment = iSnaphot.Read(100, 1).Result;

            Do.Assert(fragment.Index == 100);
            Do.Assert(fragment.Data.Count() == 1);

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Genre);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Genre].Value == "Rap/R & B");

            var genreTracks = session.Browse(fragment.Data.ElementAt(0)).Result;

            iNetwork.Execute(() =>
            {
                genreTracks.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                genreTracks.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 16);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "15");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "All My Love");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "201");

            // check artist link

            var linkArtistAlbums = session.Link(iNetwork.TagManager.Audio.Artist, "Bon Jovi").Result;

            iNetwork.Execute(() =>
            {
                linkArtistAlbums.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                linkArtistAlbums.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 2);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "Crossroad");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");

            var linkArtistAlbumTracks = session.Browse(fragment.Data.ElementAt(0)).Result;

            iNetwork.Execute(() =>
            {
                linkArtistAlbumTracks.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                linkArtistAlbumTracks.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 15);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "Crossroad");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "Livin' on a prayer");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "251");

            // check album link

            var linkAlbumTracks = session.Link(iNetwork.TagManager.Audio.Album, "4207").Result;

            iNetwork.Execute(() =>
            {
                linkAlbumTracks.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                linkAlbumTracks.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 18);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Blur");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "The Best Of");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Blur");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "Beetlebum");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "305");

            // check genre link

            var linkGenreTracks = session.Link(iNetwork.TagManager.Audio.Genre, "Rap/R & B").Result;

            iNetwork.Execute(() =>
            {
                linkGenreTracks.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                linkGenreTracks.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 16);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "15");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "All My Love");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "201");

            // check search

            var search = session.Search("Love").Result;

            iNetwork.Execute(() =>
            {
                search.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                search.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Total == 556);

            session.Dispose();
        }

        // IWatcher<IWatchableSnapshot<IMediaDatum>>

        public void ItemOpen(string aId, IWatchableSnapshot<IMediaDatum> aValue)
        {
            iSnaphot = aValue;
        }

        public void ItemUpdate(string aId, IWatchableSnapshot<IMediaDatum> aValue, IWatchableSnapshot<IMediaDatum> aPrevious)
        {
        }

        public void ItemClose(string aId, IWatchableSnapshot<IMediaDatum> aValue)
        {
        }

        // IUnorderedWatcher<IWatchableDevice>

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedAdd(IDevice aDevice)
        {
            aDevice.Create<IProxyMediaEndpoint>((t) =>
            {
                iProxy = t;
            });
        }

        public void UnorderedClose()
        {
        }

        public void UnorderedRemove(IDevice aItem)
        {
        }

        // IDisposable

        public void Dispose()
        {
            iNetwork.Execute(() =>
            {
                iProxy.Dispose();
                iMediaServers.RemoveWatcher(this);
            });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var network = new Network(50))
            {
                using (DeviceInjectorMock mockInjector = new DeviceInjectorMock(network, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)))
                {
                    network.Execute(() =>
                    {
                        mockInjector.Execute("medium");
                    });

                    using (var client = new Client(network))
                    {
                        client.Run();
                    }

                    Console.WriteLine("Test completed successfully ... Press key to continue");
                    Console.ReadKey();
                }
            }
        }

        static void ReportException(Exception e)
        {
            Console.WriteLine(e);
            Environment.Exit(-1);
        }
    }
}
