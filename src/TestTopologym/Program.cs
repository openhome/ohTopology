using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os;
using OpenHome.Av;
using OpenHome.Os.App;

namespace TestTopology3
{
    class Program
    {
        class RoomWatcher : IUnorderedWatcher<ITopologymGroup>, IDisposable
        {
            private readonly MockableScriptRunner iRunner;
            private readonly ResultWatcherFactory iFactory;

            public RoomWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
                iFactory = new ResultWatcherFactory(iRunner);
            }

            // IUnorderedWatcher<ITopologymGroup>

            public void UnorderedOpen()
            {
            }

            public void UnorderedInitialised()
            {
            }

            public void UnorderedClose()
            {
            }

            public void UnorderedAdd(ITopologymGroup aItem)
            {
                iRunner.Result(aItem.Device.Udn + " Group Added");
                iFactory.Create<ITopologymSender>(aItem.Device.Udn, aItem.Sender, (v, w) => 
                {
                    if (v.Enabled)
                    {
                        w("Sender " + v.Enabled + " " + v.Device.Udn);
                    }
                    else
                    {
                        w("Sender " + v.Enabled);
                    }
                });
            }

            public void UnorderedRemove(ITopologymGroup aItem)
            {
                iFactory.Destroy(aItem.Device.Udn);
                iRunner.Result(aItem.Device.Udn + " Group Removed");
            }

            // IDisposable

            public void Dispose()
            {
                iFactory.Dispose();
            }
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestTopology3.exe <testscript>");
                return 1;
            }

            Mockable mocker = new Mockable();

            Log log = new Log(new LogConsole());

            Network network = new Network(50, log);
            DeviceInjectorMock mockInjector = new DeviceInjectorMock(network, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), log);
            mocker.Add("network", mockInjector);

            Topology1 topology1 = new Topology1(network, log);
            Topology2 topology2 = new Topology2(topology1, log);
            Topologym topologym = new Topologym(topology2, log);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(runner);
            
            network.Schedule(() =>
            {
                topologym.Groups.AddWatcher(watcher);
            });

            try
            {
                runner.Run(network.Wait, new StreamReader(args[0]), mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            network.Execute(() =>
            {
                topologym.Groups.RemoveWatcher(watcher);
                watcher.Dispose();
            });

            topologym.Dispose();

            topology2.Dispose();

            topology1.Dispose();

            mockInjector.Dispose();

            network.Dispose();

            return 0;
        }
    }
}
