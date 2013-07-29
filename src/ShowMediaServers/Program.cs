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
    public class TestMediaServer : IDisposable
    {
        private readonly INetwork iNetwork;
        private readonly IProxyMediaEndpoint iProxy;

        public TestMediaServer(INetwork aNetwork, IProxyMediaEndpoint aProxy)
        {
            iNetwork = aNetwork;
            iProxy = aProxy;

            PrintDetails();
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
            iProxy.Dispose();

            Console.WriteLine("Removed                  : {0}", iProxy.Device.Udn);
        }
    }

    public class Client : IUnorderedWatcher<IDevice>, IDisposable
    {
        private readonly INetwork iNetwork;

        private readonly Dictionary<IDevice, TestMediaServer> iMediaServers;

        private IWatchableUnordered<IDevice> iDevices;

        public Client(INetwork aNetwork)
        {
            iNetwork = aNetwork;

            iMediaServers = new Dictionary<IDevice, TestMediaServer>();

            iNetwork.Execute(() =>
            {
                iDevices = aNetwork.Create<IProxyMediaEndpoint>();
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

            using (var network = new Network(watchableThread, 50))
            {
                using (var mock = new DeviceInjectorMock(network, "."))
                {
                    mock.Execute("medium");

                    using (var real = new DeviceInjectorMediaEndpoint(network))
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
