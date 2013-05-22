using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;
using OpenHome.MediaServer;

namespace TestMediaServer
{
    public class Client : IUnorderedWatcher<IWatchableDevice>, IWatcher<IMediaServerSnapshot>, IDisposable
    {
        private readonly INetwork iNetwork;
        
        private IWatchableUnordered<IWatchableDevice> iMediaServers;
        
        private IProxyMediaServer iProxy;
        private IMediaServerSnapshot iSnaphot;

        public Client(INetwork aNetwork)
        {
            iNetwork = aNetwork;

            iNetwork.Execute(() =>
            {
                iMediaServers = aNetwork.Create<IProxyMediaServer>();
                iMediaServers.AddWatcher(this);
            });

            iNetwork.Wait();
        }

        public void Run()
        {
            Do.Assert(iProxy.Attributes.Contains("query"));
            Do.Assert(iProxy.Attributes.Contains("browse"));

            var session = iProxy.CreateSession().Result;
            
            var root = session.Browse(null).Result;

            iNetwork.Execute(() =>
            {
                root.Snapshot.AddWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Sequence == 0);
            Do.Assert(iSnaphot.Total == 4);

            var fragment = iSnaphot.Read(0, 4).Result;

            Do.Assert(fragment.Index == 0);
            Do.Assert(fragment.Sequence == 0);
            Do.Assert(fragment.Data.Count() == 4);

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Container.Title].Value == "Tracks");

            Do.Assert(fragment.Data.ElementAt(1).Type.Count() == 3);
            Do.Assert(fragment.Data.ElementAt(1).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(fragment.Data.ElementAt(1).Type.ElementAt(1) == iNetwork.TagManager.Audio.Artist);
            Do.Assert(fragment.Data.ElementAt(1).Type.ElementAt(2) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(1)[iNetwork.TagManager.Container.Title].Value == "Artists");

            Do.Assert(fragment.Data.ElementAt(2).Type.Count() == 2);
            Do.Assert(fragment.Data.ElementAt(2).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(fragment.Data.ElementAt(2).Type.ElementAt(1) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(2)[iNetwork.TagManager.Container.Title].Value == "Albums");

            Do.Assert(fragment.Data.ElementAt(3).Type.Count() == 2);
            Do.Assert(fragment.Data.ElementAt(3).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(fragment.Data.ElementAt(3).Type.ElementAt(1) == iNetwork.TagManager.Audio.Genre);
            Do.Assert(fragment.Data.ElementAt(3)[iNetwork.TagManager.Container.Title].Value == "Genres");

            var tracks = session.Browse(fragment.Data.ElementAt(0)).Result;

            iNetwork.Execute(() =>
            {
                tracks.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                tracks.Snapshot.RemoveWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Sequence == 0);
            Do.Assert(iSnaphot.Total == 12529);

            var artists = session.Browse(fragment.Data.ElementAt(1)).Result;

            iNetwork.Execute(() =>
            {
                artists.Snapshot.AddWatcher(this);
            });

            iNetwork.Execute(() =>
            {
                artists.Snapshot.RemoveWatcher(this);
            });


            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Sequence == 0);
            Do.Assert(iSnaphot.Total == 817);

            fragment = iSnaphot.Read(100, 1).Result;

            Do.Assert(fragment.Index == 100);
            Do.Assert(fragment.Sequence == 0);
            Do.Assert(fragment.Data.Count() == 1);

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 2);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Artist);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(1) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Boo Radleys");

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
            Do.Assert(iSnaphot.Sequence == 0);
            Do.Assert(iSnaphot.Total == 1);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Boo Radleys");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "C'Mon Kids");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Boo Radleys");
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
            Do.Assert(iSnaphot.Sequence == 0);
            Do.Assert(iSnaphot.Total == 13);

            fragment = iSnaphot.Read(0, 1).Result;

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Boo Radleys");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "C'Mon Kids");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Boo Radleys");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "C'Mon Kids");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "252");

        }

        // IWatcher<IMediaServerSnapshot>

        public void ItemOpen(string aId, IMediaServerSnapshot aValue)
        {
            iSnaphot = aValue;
        }

        public void ItemUpdate(string aId, IMediaServerSnapshot aValue, IMediaServerSnapshot aPrevious)
        {
        }

        public void ItemClose(string aId, IMediaServerSnapshot aValue)
        {
        }

        // IUnorderedWatcher<IWatchableDevice>

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedAdd(IWatchableDevice aDevice)
        {
            aDevice.Create<IProxyMediaServer>().ContinueWith((t) =>
            {
                iProxy = t.Result;
            });
        }

        public void UnorderedClose()
        {
        }

        public void UnorderedRemove(IWatchableDevice aItem)
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
        class ExceptionReporter : IExceptionReporter
        {
            public void ReportException(Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }

        static void Main(string[] args)
        {
            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread watchableThread = new WatchableThread(reporter);
            WatchableThread subscribeThread = new WatchableThread(reporter);

            using (var network = new Network(watchableThread, subscribeThread))
            {
                network.Add(new MockWatchableMediaServer(network, "4c494e4e-0026-0f99-1111-111111111111", "."));

                using (var client = new Client(network))
                {
                    client.Run();
                }
            }
        }
    }
}
