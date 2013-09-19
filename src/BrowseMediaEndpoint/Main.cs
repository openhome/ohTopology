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

namespace BrowseMediaEndpoint
{
    public class Main
    {
        private readonly Library iLibrary;
        private readonly NetworkAdapter iAdapter;

        private readonly WatchableThread iWatchableThread;
        private readonly Network iNetwork;
        private readonly DeviceInjectorMediaEndpoint iDeviceInjectorMediaEndpoint;
        private readonly IWatchableUnordered<IDevice> iDevices;
        private IProxyMediaEndpoint iMediaEndpoint;
        private IMediaEndpointSession iMediaEndpointSession;

        private void ReportException(Exception aException)
        {
            Console.WriteLine(aException);
        }

        public Main(Library aLibrary, NetworkAdapter aAdapter)
        {
            iLibrary = aLibrary;
            iAdapter = aAdapter;

            iLibrary.StartCp(iAdapter.Subnet());

            iWatchableThread = new WatchableThread(ReportException);

            iNetwork = new Network(iWatchableThread, 5000);

            iDeviceInjectorMediaEndpoint = new DeviceInjectorMediaEndpoint(iNetwork);

            iDevices = iNetwork.Create<IProxyMediaEndpoint>();
        }

        public void Run()
        {
            while (true)
            {
                var command = Console.ReadLine();

                if (command == null)
                {
                    break;
                }

                var tokens = Tokeniser.Parse(command);

                if (tokens.Any())
                {
                    var token = tokens.First().ToLowerInvariant();

                    switch (token)
                    {
                        case "q":
                        case "x":
                            return;
                        case "l":
                            iWatchableThread.Schedule(List);
                            break;
                        case "s":
                            Search(tokens.Skip(1));
                            break;
                        default:
                            Select(tokens);
                            break;
                    }
                }
                else
                {
                    iWatchableThread.Schedule(List);
                }
            }
        }

        private void Search(IEnumerable<string> aTokens)
        {
            if (iMediaEndpoint != null)
            {
                if (aTokens.Any())
                {
                    iWatchableThread.Schedule(() =>
                    {
                        Search(aTokens.First());
                    });
                }
            }
        }

        private void Search(string aValue)
        {
            try
            {
                var sw = new Stopwatch();

                sw.Start();

                iMediaEndpointSession.Search(aValue, (s) =>
                {
                    sw.Stop();

                    Console.WriteLine("{0} items in {1}ms", s.Total, sw.Milliseconds);
                });
            }
            catch
            {
                Console.WriteLine("Operation failed");
            }
        }

        private void Select(IEnumerable<string> aTokens)
        {
            if (aTokens.Any())
            {
                try
                {
                    uint index = uint.Parse(aTokens.First());

                    iWatchableThread.Schedule(() =>
                    {
                        Select(index);
                    });
                }
                catch
                {
                }
            }
            else
            {
                iWatchableThread.Schedule(Select);
            }
        }

        private void Select()
        {
            if (iMediaEndpoint == null)
            {
                Console.WriteLine("No media endpoint selected");
            }
            else
            {
                Console.WriteLine("Selected: {0}:{1}", iMediaEndpoint.Type, iMediaEndpoint.Name);
            }
        }

        private void Select(uint aIndex)
        {
            uint index = 0;

            foreach (var entry in iDevices.Values)
            {
                entry.Create<IProxyMediaEndpoint>((me) =>
                {
                    if (me.Attributes.Contains("Search"))
                    {
                        if (index++ == aIndex)
                        {
                            iMediaEndpoint = me;
                            iMediaEndpointSession = iMediaEndpoint.CreateSession().Result;
                            Select();
                        }
                    }
                });
            }
        }

        private void List()
        {
            uint index = 0;

            foreach (var entry in iDevices.Values)
            {
                entry.Create<IProxyMediaEndpoint>((me) =>
                {
                    if (me.Attributes.Contains("Search"))
                    {
                        Console.WriteLine("{0}. {1}:{2}", index++, me.Type, me.Name);
                    }
                });
            }
        }
    }
}
