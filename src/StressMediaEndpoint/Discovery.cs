using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenHome;
using OpenHome.Net.Core;
using OpenHome.Av;
using OpenHome.Os;
using OpenHome.Os.App;

namespace StressMediaEndpoint
{
    public class Discovery : IUnorderedWatcher<IDevice>, IDisposable
    {
        private const int kTimeoutMs = 10000;

        private readonly Library iLibrary;
        private readonly NetworkAdapter iAdapter;
        private readonly IWatchableThread iWatchableThread;
        private readonly string iUdn;

        
        private readonly Network iNetwork;

        private InjectorMediaEndpoint iInjectorMediaEndpoint;

        private IWatchableUnordered<IDevice> iDevices;

        private readonly WatchableTimer iFindingTimer;

        private readonly ManualResetEvent iFound;

        private IProxyMediaEndpoint iMediaEndpoint;

        public Discovery(Library aLibrary, NetworkAdapter aAdapter, IWatchableThread aWatchableThread, string aUdn)
        {
            iLibrary = aLibrary;
            iAdapter = aAdapter;
            iWatchableThread = aWatchableThread;
            iUdn = aUdn;

            iLibrary.StartCp(iAdapter.Subnet());

            Log log = new Log(new LogConsole());

            iNetwork = new Network(iWatchableThread, 5000, log);

            iFindingTimer = new WatchableTimer(iWatchableThread, FindingTimerExpired);

            iFound = new ManualResetEvent(false);

            iNetwork.Execute(() =>
            {
                iInjectorMediaEndpoint = new InjectorMediaEndpoint(iNetwork, log);
                iDevices = iNetwork.Create<IProxyMediaEndpoint>();
                iDevices.AddWatcher(this);
            });
        }

        public INetwork Network
        {
            get
            {
                return (iNetwork);
            }
        }

        public IProxyMediaEndpoint MediaEndpoint
        {
            get
            {
                return (iMediaEndpoint);
            }
        }

        private void FindingTimerExpired()
        {
            iDevices.RemoveWatcher(this);
            iFound.Set();
        }

        private void PrintDetails(IProxyMediaEndpoint aProxyMediaEndpoint)
        {
            Console.WriteLine("Udn                      : {0}", aProxyMediaEndpoint.Device.Udn);
            Console.WriteLine("Id                       : {0}", aProxyMediaEndpoint.Id);
            Console.WriteLine("Type                     : {0}", aProxyMediaEndpoint.Type);
            Console.WriteLine("Started                  : {0}", aProxyMediaEndpoint.Started);
            Console.WriteLine("Attributes               : {0}", string.Join(", ", aProxyMediaEndpoint.Attributes));
            Console.WriteLine("Name                     : {0}", aProxyMediaEndpoint.Name);
            Console.WriteLine("Info                     : {0}", aProxyMediaEndpoint.Info);
            Console.WriteLine("Url                      : {0}", aProxyMediaEndpoint.Url);
            Console.WriteLine("Artwork                  : {0}", aProxyMediaEndpoint.Artwork);
            Console.WriteLine("ManufacturerName         : {0}", aProxyMediaEndpoint.ManufacturerName);
            Console.WriteLine("ManufacturerInfo         : {0}", aProxyMediaEndpoint.ManufacturerInfo);
            Console.WriteLine("ManufacturerUrl          : {0}", aProxyMediaEndpoint.ManufacturerUrl);
            Console.WriteLine("ManufacturerArtwork      : {0}", aProxyMediaEndpoint.ManufacturerArtwork);
            Console.WriteLine("ModelName                : {0}", aProxyMediaEndpoint.ModelName);
            Console.WriteLine("ModelInfo                : {0}", aProxyMediaEndpoint.ModelInfo);
            Console.WriteLine("ModelUrl                 : {0}", aProxyMediaEndpoint.ModelUrl);
            Console.WriteLine("ModelArtwork             : {0}", aProxyMediaEndpoint.ModelArtwork);
        }

        public bool Find()
        {
            iFound.WaitOne();

            lock (iFindingTimer)
            {
                return (iMediaEndpoint != null);
            }
        }

        // IUnorderedWatcher<IDevice>

        void IUnorderedWatcher<IDevice>.UnorderedInitialised()
        {
        }

        void IUnorderedWatcher<IDevice>.UnorderedOpen()
        {
        }

        void IUnorderedWatcher<IDevice>.UnorderedAdd(IDevice aItem)
        {
            if (aItem.Udn == iUdn)
            {
                aItem.Create<IProxyMediaEndpoint>((me) =>
                {
                    try
                    {
                        lock (iFindingTimer)
                        {
                            iMediaEndpoint = me;
                        }

                        iFindingTimer.Cancel();
                        iDevices.RemoveWatcher(this);
                        iFound.Set();
                    }
                    catch
                    {
                    }
                });
            }
        }

        void IUnorderedWatcher<IDevice>.UnorderedRemove(IDevice aItem)
        {
        }

        void IUnorderedWatcher<IDevice>.UnorderedClose()
        {
        }

        // IDisposable

        public void Dispose()
        {
            if (iMediaEndpoint != null)
            {
                iWatchableThread.Execute(() =>
                {
                    iMediaEndpoint.Dispose();
                });
            }

            iInjectorMediaEndpoint.Dispose();

            iNetwork.Dispose();
        }
    }
}
