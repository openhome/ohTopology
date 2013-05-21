using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;

namespace TestMediaServer
{
    public class Client : IUnorderedWatcher<IWatchableDevice>, IWatcher<IMediaServerSnapshot>, IDisposable
    {
        private readonly IWatchableThread iWatchableThread;
        private readonly IWatchableUnordered<IWatchableDevice> iMediaServers;
        
        private IProxyMediaServer iProxy;
        private IMediaServerSnapshot iSnaphot;

        public Client(IWatchableThread aWatchableThread, INetwork aNetwork)
        {
            iWatchableThread = aWatchableThread;

            iMediaServers = aNetwork.Create<IProxyMediaServer>();

            iWatchableThread.Execute(() =>
            {
                iMediaServers.AddWatcher(this);
            });

            iWatchableThread.Wait();
        }

        public void Run()
        {
            Do.Assert(iProxy.Attributes.Contains("query"));
            Do.Assert(iProxy.Attributes.Contains("browse"));

            var session = iProxy.CreateSession().Result;
            
            var root = session.Browse(null).Result;

            iWatchableThread.Execute(() =>
            {
                root.Snapshot.AddWatcher(this);
            });

            Do.Assert(iSnaphot.AlphaMap == null);
            Do.Assert(iSnaphot.Sequence == 0);
            Do.Assert(iSnaphot.Total == 4);

            var fragment = iSnaphot.Read(0, 4).Result;

            Do.Assert(fragment.Index == 0);
            Do.Assert(fragment.Sequence == 0);

            IMediaDatum item = null;
            
            item = fragment.Data.ElementAt(0);

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
            iWatchableThread.Execute(() =>
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

                using (var client = new Client(watchableThread, network))
                {
                    client.Run();
                }
            }
        }
    }
}
