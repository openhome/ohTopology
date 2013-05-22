using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;

namespace TestTopology2
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

        class GroupWatcher : IUnorderedWatcher<ITopology2Group>, IDisposable
        {
            public GroupWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
                iFactory = new ResultWatcherFactory(aRunner);
            }

            public void Dispose()
            {
                iFactory.Dispose();
            }

            public void UnorderedOpen()
            {
            }

            public void UnorderedInitialised()
            {
            }

            public void UnorderedClose()
            {
            }

            public void UnorderedAdd(ITopology2Group aItem)
            {
                iRunner.Result(aItem.Device.Udn + " Group Added");
                iFactory.Create<string>(aItem.Device.Udn, aItem.Room, v => "Room " + v);
                iFactory.Create<string>(aItem.Device.Udn, aItem.Name, v => "Name " + v);
                iFactory.Create<uint>(aItem.Device.Udn, aItem.SourceIndex, v => "SourceIndex " + v);
                iFactory.Create<bool>(aItem.Device.Udn, aItem.Standby, v => "Standby " + v);

                foreach (IWatchable<ITopology2Source> s in aItem.Sources)
                {
                    iFactory.Create<ITopology2Source>(aItem.Device.Udn, s, v => "Source " + v.Index + " " + v.Name + " " + v.Type + " " + v.Visible);
                }
            }

            public void UnorderedRemove(ITopology2Group aItem)
            {
                iFactory.Destroy(aItem.Device.Udn);
                iRunner.Result(aItem.Device.Udn + " Group Removed");
            }

            private readonly MockableScriptRunner iRunner;
            private readonly ResultWatcherFactory iFactory;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestTopology2.exe <testscript>");
                return 1;
            }

            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread thread = new WatchableThread(reporter);
            WatchableThread subscribeThread = new WatchableThread(reporter);

            Mockable mocker = new Mockable();

            Network network = new Network(thread, subscribeThread);
            mocker.Add("network", network);

            Topology1 topology1 = new Topology1(network);
            Topology2 topology2 = new Topology2(topology1);

            MockableScriptRunner runner = new MockableScriptRunner();

            GroupWatcher watcher = new GroupWatcher(runner);
            thread.Schedule(() =>
            {
                topology2.Groups.AddWatcher(watcher);
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
                topology2.Groups.RemoveWatcher(watcher);
                watcher.Dispose();
            });

            topology2.Dispose();

            topology1.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
