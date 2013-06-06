using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;
using OpenHome.MediaServer;

using OpenHome.Net;
using OpenHome.Net.Core;
using OpenHome.Net.ControlPoint;

namespace ShowMediaServers
{
    public class TestMediaServer : IWatcher<IWatchableSnapshot<IMediaDatum>>, IDisposable
    {
        private readonly INetwork iNetwork;
        private readonly IProxyMediaServer iProxy;
        private readonly Queue<IMediaDatum> iContainers;

        private IMediaServerSession iSession;
        private IWatchableContainer<IMediaDatum> iContainer;

        public TestMediaServer(INetwork aNetwork, IProxyMediaServer aProxy)
        {
            iNetwork = aNetwork;
            iProxy = aProxy;
            iContainers = new Queue<IMediaDatum>();
            Console.WriteLine("Added    : {0}", this);
            iProxy.CreateSession().ContinueWith(GetSession);
        }

        private void GetSession(Task<IMediaServerSession> aTask)
        {
            iSession = aTask.Result;
            iContainers.Enqueue(null);
            ProcessNextContainer();
        }

        private void ProcessNextContainer()
        {
            if (iContainers.Count > 0)
            {
                iSession.Browse(iContainers.Dequeue()).ContinueWith((t) =>
                {
                    iContainer = t.Result;

                    if (iContainer != null)
                    {
                        iNetwork.Schedule(() =>
                        {
                            iContainer.Snapshot.AddWatcher(this);
                        });
                    }
                });
            }
        }

        public override string ToString()
        {
            return string.Join(", ", iProxy.Device.Udn, iProxy.ProductName);
        }

        // IWatcher<IWatchableSnapshot<IMediaDatum>>

        public void ItemOpen(string aId, IWatchableSnapshot<IMediaDatum> aValue)
        {
            Console.WriteLine("Snapshot : {0} Items = {1} Alpha Map = {2}", this, aValue.Total, aValue.AlphaMap != null);

            aValue.Read(0, aValue.Total).ContinueWith((t) =>
            {
                var fragment = t.Result;

                if (fragment != null)
                {
                    foreach (var datum in fragment.Data)
                    {
                        if (datum.Type.Any())
                        {
                            iContainers.Enqueue(datum);
                        }
                    }
                }

                iNetwork.Schedule(() =>
                {
                    iContainer.Snapshot.RemoveWatcher(this);
                });
            });
        }

        public void ItemUpdate(string aId, IWatchableSnapshot<IMediaDatum> aValue, IWatchableSnapshot<IMediaDatum> aPrevious)
        {
        }

        public void ItemClose(string aId, IWatchableSnapshot<IMediaDatum> aValue)
        {
            ProcessNextContainer();
        }

        // IDisposable

        public void Dispose()
        {
            Console.WriteLine("Removed  : {0}", this);

            if (iContainer != null)
            {
                iContainer.Snapshot.RemoveWatcher(this);
            }

            if (iSession != null)
            {
                iSession.Dispose();
            }

            iProxy.Dispose();
        }
    }

    public class Client : IUnorderedWatcher<IDevice>, IWatcher<IWatchableSnapshot<IMediaDatum>>, IDisposable
    {
        private readonly INetwork iNetwork;

        private readonly Dictionary<IDevice, TestMediaServer> iMediaServers;

        private IWatchableUnordered<IDevice> iDevices;

        private IWatchableSnapshot<IMediaDatum> iSnaphot;

        public Client(INetwork aNetwork)
        {
            iNetwork = aNetwork;

            iMediaServers = new Dictionary<IDevice, TestMediaServer>();

            iNetwork.Execute(() =>
            {
                iDevices = aNetwork.Create<IProxyMediaServer>();
                iDevices.AddWatcher(this);
            });

            iNetwork.Wait();
        }

        public void Run()
        {
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
            aDevice.Create<IProxyMediaServer>((p) =>
            {
                iMediaServers.Add(aDevice, new TestMediaServer(iNetwork, p));
            });
        }

        public void UnorderedRemove(IDevice aDevice)
        {
            var server = iMediaServers[aDevice];
            iMediaServers.Remove(aDevice);
            server.Dispose();
        }

        public void UnorderedClose()
        {
        }

        // IDisposable

        public void Dispose()
        {
            iNetwork.Execute(() =>
            {
                iDevices.RemoveWatcher(this);

                foreach (var entry in iMediaServers)
                {
                    entry.Value.Dispose();
                }
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

        static void MessageHandlerLog(string aMessage)
        {
            Console.WriteLine("LOG:   {0}", aMessage);
        }

        static void MessageHandlerFatal(string aMessage)
        {
            Console.WriteLine("FATAL: {0}", aMessage);
        }

        static void Main(string[] args)
        {
            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread watchableThread = new WatchableThread(reporter);
            WatchableThread subscribeThread = new WatchableThread(reporter);

            InitParams initParams = new InitParams();
            initParams.LogOutput = new MessageListener(MessageHandlerLog);
            initParams.FatalErrorHandler = new MessageListener(MessageHandlerFatal);

            var library = Library.Create(initParams);

            library.StartCp(0x020a);

            using (var network = new Network(watchableThread, subscribeThread))
            {
                network.Execute(() =>
                {
                    network.Execute("medium");
                });

                using (var injector = new DeviceInjectorContentDirectory(network))
                {
                    using (var client = new Client(network))
                    {
                        client.Run();
                        Console.ReadKey();
                    }
                }

                network.Execute(() =>
                {
                    network.Execute("medium");
                });
            }

            try
            {
                library.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadKey();
        }
    }
}
