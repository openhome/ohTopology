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
        private readonly TaskCompletionSource<bool> iComplete;

        private bool iClose;
        private IMediaServerSession iSession;
        private IWatchableContainer<IMediaDatum> iContainer;

        public TestMediaServer(INetwork aNetwork, IProxyMediaServer aProxy)
        {
            iNetwork = aNetwork;
            iProxy = aProxy;
            iContainers = new Queue<IMediaDatum>();
            iComplete = new TaskCompletionSource<bool>();
            iClose = false;
            Console.WriteLine("Added    : {0}", this);
            iProxy.CreateSession().ContinueWith(GetSession);
        }

        public void Close()
        {
            iNetwork.Execute(() =>
            {
                Console.WriteLine("Close    : {0}", this);
                iClose = true;
            });
        }

        private void GetSession(Task<IMediaServerSession> aTask)
        {
            iNetwork.Schedule(() =>
            {
                iSession = aTask.Result;
                iContainers.Enqueue(null);
                ProcessNextContainer();
            });
        }

        // ProcessNextContainer always called in the watchable thread

        private void ProcessNextContainer()
        {
            if (iContainer != null)
            {
                iContainer.Snapshot.RemoveWatcher(this);
                iContainer = null;
            }

            if (!iClose)
            {
                if (iContainers.Count > 0)
                {
                    iSession.Browse(iContainers.Dequeue()).ContinueWith((t) =>
                    {
                        iNetwork.Schedule(() =>
                        {
                            iContainer = t.Result;

                            if (iContainer != null)
                            {
                                iContainer.Snapshot.AddWatcher(this);
                            }
                            else
                            {
                                ProcessNextContainer();
                            }
                        });
                    });

                    return;
                }
            }

            Console.WriteLine("Closing  : {0}", this);
            iSession.Dispose();
            iComplete.SetResult(true);
            Console.WriteLine("Closed   : {0}", this);
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
                    ProcessNextContainer();
                });
            });
        }

        public void ItemUpdate(string aId, IWatchableSnapshot<IMediaDatum> aValue, IWatchableSnapshot<IMediaDatum> aPrevious)
        {
        }

        public void ItemClose(string aId, IWatchableSnapshot<IMediaDatum> aValue)
        {
        }

        // IDisposable

        public void Dispose()
        {
            Console.WriteLine("Removing : {0}", this);

            iComplete.Task.Wait();

            iProxy.Dispose();

            Console.WriteLine("Removed  : {0}", this);
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
            });

            foreach (var server in iMediaServers.Values)
            {
                server.Close();
            }

            foreach (var server in iMediaServers.Values)
            {
                server.Dispose();
            }
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

            InitParams initParams = new InitParams();
            initParams.LogOutput = new MessageListener(MessageHandlerLog);
            initParams.FatalErrorHandler = new MessageListener(MessageHandlerFatal);

            var library = Library.Create(initParams);

            library.StartCp(0x020a);

            using (var network = new Network(watchableThread))
            {
                using (var mock = new DeviceInjectorMock(network))
                {
                    mock.Execute("medium");

                    using (var real = new DeviceInjectorContentDirectory(network))
                    {
                        using (var client = new Client(network))
                        {
                            client.Run();
                            Console.ReadKey();
                        }
                    }
                }
            }

            try
            {
                library.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Closed");

            Console.ReadKey();
        }
    }
}
