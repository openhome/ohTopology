using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;

namespace TestMediaServer
{
    public class Client
    {
        private readonly INetwork iNetwork;
        
        private IProxyMediaEndpoint iProxy;

        private AutoResetEvent iReady;

        public Client(INetwork aNetwork, IProxyMediaEndpoint aProxy)
        {
            iNetwork = aNetwork;
            iProxy = aProxy;
            
            iReady = new AutoResetEvent(false);
        }

        public void Run()
        {
            Do.Assert(iProxy.SupportsBrowse());
            Do.Assert(iProxy.SupportsLink());
            Do.Assert(iProxy.SupportsLink(iNetwork.TagManager.Audio.Artist));
            Do.Assert(iProxy.SupportsLink(iNetwork.TagManager.Audio.Album));
            Do.Assert(iProxy.SupportsLink(iNetwork.TagManager.Audio.Genre));
            Do.Assert(iProxy.SupportsSearch());

            IMediaEndpointSession session = null;

            iNetwork.Execute(() =>
            {
                session = iProxy.CreateSession().Result;
            });

            iNetwork.Execute(() =>
            {
                session.Browse(null,  () => iReady.Set());
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 4);

            IWatchableFragment<IMediaDatum> rootFragment = null;
            
            iNetwork.Execute(() =>
            {
                rootFragment = session.Snapshot.Read(0, 4).Result;
            });

            Do.Assert(rootFragment.Index == 0);
            Do.Assert(rootFragment.Data.Count() == 4);

            Do.Assert(rootFragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(rootFragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(rootFragment.Data.ElementAt(0)[iNetwork.TagManager.Container.Title].Value == "Tracks");

            Do.Assert(rootFragment.Data.ElementAt(1).Type.Count() == 3);
            Do.Assert(rootFragment.Data.ElementAt(1).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(rootFragment.Data.ElementAt(1).Type.ElementAt(1) == iNetwork.TagManager.Audio.Artist);
            Do.Assert(rootFragment.Data.ElementAt(1).Type.ElementAt(2) == iNetwork.TagManager.Audio.Album);
            Do.Assert(rootFragment.Data.ElementAt(1)[iNetwork.TagManager.Container.Title].Value == "Artist");

            Do.Assert(rootFragment.Data.ElementAt(2).Type.Count() == 2);
            Do.Assert(rootFragment.Data.ElementAt(2).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(rootFragment.Data.ElementAt(2).Type.ElementAt(1) == iNetwork.TagManager.Audio.Album);
            Do.Assert(rootFragment.Data.ElementAt(2)[iNetwork.TagManager.Container.Title].Value == "Album");

            Do.Assert(rootFragment.Data.ElementAt(3).Type.Count() == 2);
            Do.Assert(rootFragment.Data.ElementAt(3).Type.ElementAt(0) == iNetwork.TagManager.Container.Title);
            Do.Assert(rootFragment.Data.ElementAt(3).Type.ElementAt(1) == iNetwork.TagManager.Audio.Genre);
            Do.Assert(rootFragment.Data.ElementAt(3)[iNetwork.TagManager.Container.Title].Value == "Genre");

            iNetwork.Execute(() =>
            {
                session.Browse(rootFragment.Data.ElementAt(0), UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 12521);

            iNetwork.Execute(() =>
            {
                session.Browse(rootFragment.Data.ElementAt(1), UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 650);

            IWatchableFragment<IMediaDatum> fragment = null;
                
            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(100, 1).Result;
            });

            Do.Assert(fragment.Index == 100);
            Do.Assert(fragment.Data.Count() == 1);

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 2);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.AlbumArtist);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(1) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Cecilia Bartoli & Bryn Terfel");

            iNetwork.Execute(() =>
            {
                session.Browse(fragment.Data.ElementAt(0), UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 1);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(0, 1).Result;
            });

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Cecilia Bartoli & Bryn Terfel");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "Cecilia & Bryn: Duets");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Cecilia Bartoli & Bryn Terfel");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");

            iNetwork.Execute(() =>
            {
                session.Browse(fragment.Data.ElementAt(0), UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 18);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(0, 1).Result;
            });

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Cecilia Bartoli");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "Cecilia & Bryn: Duets");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Cecilia Bartoli & Bryn Terfel");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "Le nozze di Figaro: Duet: Cinque...dieci... venti... trenta...");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "160");

            iNetwork.Execute(() =>
            {
                session.Browse(rootFragment.Data.ElementAt(2), UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 1000);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(100, 1).Result;
            });

            Do.Assert(fragment.Index == 100);
            Do.Assert(fragment.Data.Count() == 1);

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Blur");

            iNetwork.Execute(() =>
            {
                session.Browse(fragment.Data.ElementAt(0), UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 18);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(0, 1).Result;
            });

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Blur");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "The Best Of");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Blur");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "Beetlebum");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "305");

            iNetwork.Execute(() =>
            {
                session.Browse(rootFragment.Data.ElementAt(3), UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 124);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(100, 1).Result;
            });

            Do.Assert(fragment.Index == 100);
            Do.Assert(fragment.Data.Count() == 1);

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Genre);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Genre].Value == "Rap/R & B");

            iNetwork.Execute(() =>
            {
                session.Browse(fragment.Data.ElementAt(0), UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 16);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(0, 1).Result;
            });

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "15");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "All My Love");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "201");

            // check artist link

            iNetwork.Execute(() =>
            {
                session.Link(iNetwork.TagManager.Audio.Artist, "Bon Jovi", UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 2);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(0, 1).Result;
            });

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 1);
            Do.Assert(fragment.Data.ElementAt(0).Type.ElementAt(0) == iNetwork.TagManager.Audio.Album);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "Crossroad");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");

            iNetwork.Execute(() =>
            {
                session.Browse(fragment.Data.ElementAt(0), UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 15);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(0, 1).Result;
            });

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "Crossroad");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Bon Jovi");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "Livin' on a prayer");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "251");

            // check album link

            iNetwork.Execute(() =>
            {
                session.Link(iNetwork.TagManager.Audio.Album, "4207", UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 18);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(0, 1).Result;
            });

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "Blur");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "The Best Of");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "Blur");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "Beetlebum");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "305");

            // check genre link

            iNetwork.Execute(() =>
            {
                session.Link(iNetwork.TagManager.Audio.Genre, "Rap/R & B", UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 16);

            iNetwork.Execute(() =>
            {
                fragment = session.Snapshot.Read(0, 1).Result;
            });

            Do.Assert(fragment.Data.ElementAt(0).Type.Count() == 0);
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Artist].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumTitle].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumArtist].Value == "House of Pain");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.AlbumDiscs].Value == "1");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Track].Value == "15");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Title].Value == "All My Love");
            Do.Assert(fragment.Data.ElementAt(0)[iNetwork.TagManager.Audio.Duration].Value == "201");

            // check search

            iNetwork.Execute(() =>
            {
                session.Search("Love", UpdateSnapshot);
            });

            iReady.WaitOne();

            Do.Assert(session.Snapshot.Alpha == null);
            Do.Assert(session.Snapshot.Total == 556);

            iNetwork.Execute(() =>
            {
                session.Dispose();
            });
        }

        private void UpdateSnapshot()
        {
            iReady.Set();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var network = new Network(50, new Log(new LogConsole())))
            {
                Log log = new Log(new LogConsole());

                var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

                var device = DeviceFactory.CreateMediaServer(network, "4c494e4e-0026-0f99-0000-000000000000", path, log);
                var device2 = new Device(device);

                IProxyMediaEndpoint proxy =null;

                using (var ready = new ManualResetEvent(false))
                {
                    network.Schedule(() =>
                    {
                        device.Create<IProxyMediaEndpoint>((p) =>
                        {
                            proxy = p;
                            ready.Set();
                        }, device2);
                    });

                    ready.WaitOne();
                }

                var client = new Client(network, proxy);

                client.Run();

                network.Execute(() =>
                {
                    proxy.Dispose();
                    device2.Dispose();
                });

                Console.WriteLine("Test completed successfully");
            }
        }

        static void ReportException(Exception e)
        {
            Console.WriteLine(e);
            Environment.Exit(-1);
        }
    }
}
