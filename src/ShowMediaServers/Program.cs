using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;

using OpenHome.Net;
using OpenHome.Net.Core;
using OpenHome.Net.ControlPoint;

namespace ShowMediaServers
{
    public class TestMediaEndpointSession : IDisposable
    {
        private readonly INetwork iNetwork;
        private readonly IProxyMediaEndpoint iProxy;

        private List<Task> iTasks;

        private IMediaEndpointSession iSession;

        private IDisposable iWatcher;

        public TestMediaEndpointSession(INetwork aNetwork, IProxyMediaEndpoint aProxy)
        {
            iNetwork = aNetwork;
            iProxy = aProxy;

            iTasks = new List<Task>();

            iTasks.Add(iProxy.CreateSession().ContinueWith(SessionCreated));
        }

        private void SessionCreated(Task<IMediaEndpointSession> aTask)
        {
            iSession = aTask.Result;

            iNetwork.Schedule(() =>
            {
                iSession.Browse(null, ContainerCreated);
            });
        }

        private void ContainerCreated()
        {
            iWatcher = iSession.Snapshot.Read(0, iSession.Snapshot.Total).ContinueWith(FragmentCreated);
        }

        private void FragmentCreated(Task<IWatchableFragment<IMediaDatum>> aTask)
        {
            var fragment = aTask.Result;

            if (fragment != null)
            {
                foreach (var entry in fragment.Data.ToArray())
                {
                    ReportDatum(entry);
                }
            }
        }

        private void ReportDatum(IMediaDatum aDatum)
        {
            if (aDatum.Type.Any())
            {
                ReportContainer(aDatum.Type);
            }
            else
            {
                Console.WriteLine("***** ITEM");
            }

            foreach (var entry in aDatum)
            {
                ReportMetadatum(entry);
            }
        }

        private void ReportContainer(IEnumerable<ITag> aValue)
        {
            Console.WriteLine("***** CONTAINER: {0}", string.Join(", ", aValue.Select(t => t.FullName)));
        }

        private void ReportMetadatum(KeyValuePair<ITag, IMediaValue> aMetadatum)
        {
            Console.WriteLine("***** {0}: {1}", aMetadatum.Key.FullName, string.Join(", ", aMetadatum.Value.Values));
        }

        // IDisposable

        public void Dispose()
        {
            lock (iTasks)
            {
                Task.WaitAll(iTasks.ToArray());
            }

            if (iWatcher != null)
            {
                iWatcher.Dispose();
            }

            iNetwork.Execute(() =>
            {
                iSession.Dispose();
            });
        }
    }

    public class TestMediaEndpoint : IDisposable
    {
        private readonly INetwork iNetwork;
        private readonly IProxyMediaEndpoint iProxy;

        private readonly TestMediaEndpointSession iSession;
        
        public TestMediaEndpoint(INetwork aNetwork, IProxyMediaEndpoint aProxy)
        {
            iNetwork = aNetwork;
            iProxy = aProxy;

            PrintDetails();

            iSession = new TestMediaEndpointSession(iNetwork, iProxy);
        }

        private void PrintDetails()
        {
            Console.WriteLine();
            Console.WriteLine("Added                    : {0}", iProxy.Device.Udn);
            Console.WriteLine("Id                       : {0}", iProxy.Id);
            Console.WriteLine("Type                     : {0}", iProxy.Type);
            Console.WriteLine("Started                  : {0}", iProxy.Started);
            Console.WriteLine("Attributes               : {0}", string.Join(", ", iProxy.Attributes));
            Console.WriteLine("Name                     : {0}", iProxy.Name);
            Console.WriteLine("Info                     : {0}", iProxy.Info);
            Console.WriteLine("Url                      : {0}", iProxy.Url);
            Console.WriteLine("Artwork                  : {0}", iProxy.Artwork);
            Console.WriteLine("ManufacturerName         : {0}", iProxy.ManufacturerName);
            Console.WriteLine("ManufacturerInfo         : {0}", iProxy.ManufacturerInfo);
            Console.WriteLine("ManufacturerUrl          : {0}", iProxy.ManufacturerUrl);
            Console.WriteLine("ManufacturerArtwork      : {0}", iProxy.ManufacturerArtwork);
            Console.WriteLine("ModelName                : {0}", iProxy.ModelName);
            Console.WriteLine("ModelInfo                : {0}", iProxy.ModelInfo);
            Console.WriteLine("ModelUrl                 : {0}", iProxy.ModelUrl);
            Console.WriteLine("ModelArtwork             : {0}", iProxy.ModelArtwork);
            Console.WriteLine();
        }

        // IDisposable

        public void Dispose()
        {
            iSession.Dispose();
            iProxy.Dispose();

            Console.WriteLine("Removed                  : {0}", iProxy.Device.Udn);
        }
    }

    public class Client : IUnorderedWatcher<IDevice>, IDisposable
    {
        private readonly INetwork iNetwork;

        private readonly Dictionary<IDevice, TestMediaEndpoint> iMediaServers;

        private IWatchableUnordered<IDevice> iDevices;

        public Client(INetwork aNetwork)
        {
            iNetwork = aNetwork;

            iMediaServers = new Dictionary<IDevice, TestMediaEndpoint>();

            iNetwork.Execute(() =>
            {
                iDevices = iNetwork.Create<IProxyMediaEndpoint>();
                iDevices.AddWatcher(this);
            });
        }

        public void Run()
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
            aDevice.Create<IProxyMediaEndpoint>((p) =>
            {
                iMediaServers.Add(aDevice, new TestMediaEndpoint(iNetwork, p));
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
                server.Dispose();
            }
        }
    }

    class Program
    {
        static void ReportException(Exception aException)
        {
            Console.WriteLine(aException);
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
            WatchableThread watchableThread = new WatchableThread(ReportException);

            InitParams initParams = new InitParams();
            initParams.LogOutput = new MessageListener(MessageHandlerLog);
            initParams.FatalErrorHandler = new MessageListener(MessageHandlerFatal);

            var library = Library.Create(initParams);

            library.StartCp(0x020a);

            Log log = new Log(new LogConsole());

            using (var network = new Network(watchableThread, 50, log))
            {
                using (var mock = new DeviceInjectorMock(network, ".", log))
                {
                    mock.Execute("medium");

                    using (var real = new DeviceInjectorMediaEndpoint(network, log))
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
                watchableThread.Dispose();
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
