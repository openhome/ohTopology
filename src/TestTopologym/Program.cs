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
                iFactory.Create<ITopologymSender>(aItem.Device.Udn, aItem.Sender, v => 
                {
                    if (v.Enabled)
                    {
                        return "Sender " + v.Enabled + " " + v.Device.Udn;
                    }

                    return "Sender " + v.Enabled;
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

            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread thread = new WatchableThread(reporter);

            Mockable mocker = new Mockable();

            Network network = new Network(thread);
            DeviceInjectorMock mockInjector = new DeviceInjectorMock(network);
            mocker.Add("network", mockInjector);

            Topology1 topology1 = new Topology1(network);
            Topology2 topology2 = new Topology2(topology1);
            Topologym topologym = new Topologym(topology2);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(runner);
            thread.Schedule(() =>
            {
                topologym.Groups.AddWatcher(watcher);
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
                topologym.Groups.RemoveWatcher(watcher);
                watcher.Dispose();
            });

            topologym.Dispose();

            topology2.Dispose();

            topology1.Dispose();

            mockInjector.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
