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
        class ExceptionReporter : IExceptionReporter
        {
            public void ReportException(Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }

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

            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread thread = new WatchableThread(reporter);

            Mockable mocker = new Mockable();

            Network network = new Network(thread);
            MockDeviceInjector mockInjector = new MockDeviceInjector(network);
            mocker.Add("network", mockInjector);

            Topology1 topology1 = new Topology1(network);
            Topology2 topology2 = new Topology2(topology1);
            Topologym topologym = new Topologym(topology2);
            Topology3 topology3 = new Topology3(topologym);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(runner);
            thread.Schedule(() =>
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

            thread.Execute(() =>
            {
                topology3.Rooms.RemoveWatcher(watcher);
                watcher.Dispose();
            });

            topology3.Dispose();

            topologym.Dispose();

            topology2.Dispose();

            topology1.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
