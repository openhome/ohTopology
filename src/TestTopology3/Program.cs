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
        class RoomWatcher : IUnorderedWatcher<ITopology3Room>, IDisposable
        {
            private readonly MockableScriptRunner iRunner;
            private readonly ResultWatcherFactory iFactory;

            public RoomWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
                iFactory = new ResultWatcherFactory(iRunner);
            }

            // IUnorderedWatcher<ITopology3Room>

            public void UnorderedOpen()
            {
            }

            public void UnorderedInitialised()
            {
            }

            public void UnorderedClose()
            {
            }

            public void UnorderedAdd(ITopology3Room aItem)
            {
                iRunner.Result("Room Added " + aItem.Name);
                iFactory.Create<ITopologymGroup>(aItem.Name, aItem.Groups, v => v.Id);
            }

            public void UnorderedRemove(ITopology3Room aItem)
            {
                iFactory.Destroy(aItem.Name);
                iRunner.Result("Room Removed " + aItem.Name);
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

            Network network = new Network();
            DeviceInjectorMock mockInjector = new DeviceInjectorMock(network);
            mocker.Add("network", mockInjector);

            Topology1 topology1 = new Topology1(network);
            Topology2 topology2 = new Topology2(topology1);
            Topologym topologym = new Topologym(topology2);
            Topology3 topology3 = new Topology3(topologym);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(runner);
            
            network.Schedule(() =>
            {
                topology3.Rooms.AddWatcher(watcher);
            });

            try
            {
                runner.Run(network, new StreamReader(args[0]), mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            network.Execute(() =>
            {
                topology3.Rooms.RemoveWatcher(watcher);
                watcher.Dispose();
            });

            topology3.Dispose();

            topologym.Dispose();

            topology2.Dispose();

            topology1.Dispose();

            mockInjector.Dispose();

            network.Dispose();

            return 0;
        }
    }
}
