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

            Mockable mocker = new Mockable();

            Log log = new Log(new LogConsole());

            Network network = new Network(50, log);
            DeviceInjectorMock mockInjector = new DeviceInjectorMock(network, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), log);
            mocker.Add("network", mockInjector);

            Topology1 topology1 = new Topology1(network, log);
            Topology2 topology2 = new Topology2(topology1, log);

            MockableScriptRunner runner = new MockableScriptRunner();

            GroupWatcher watcher = new GroupWatcher(runner);
            
            network.Schedule(() =>
            {
                topology2.Groups.AddWatcher(watcher);
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
                topology2.Groups.RemoveWatcher(watcher);
                watcher.Dispose();
            });

            topology2.Dispose();

            topology1.Dispose();

            mockInjector.Dispose();

            network.Dispose();

            return 0;
        }
    }
}
