using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os;
using OpenHome.Av;
using OpenHome.Os.App;
using OpenHome.MediaServer;

namespace TestZone
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

        class RoomControllerWatcher : IDisposable
        {
            private ResultWatcherFactory iFactory;

            public RoomControllerWatcher(ITagManager aTagManager, MockableScriptRunner aRunner, IStandardRoom aRoom)
            {
                iFactory = new ResultWatcherFactory(aRunner);
                iFactory.Create<RoomMetadata>(aRoom.Name, aRoom.Metadata, (v) =>
                {
                    return "Metadata " + v.Enabled + " " + aTagManager.ToDidlLite(v.Metadata) + " " + v.Uri;
                });
            }

            public void Dispose()
            {
                iFactory.Dispose();
            }
        }

        class RoomWatcher : IOrderedWatcher<IStandardRoom>, IDisposable
        {
            public RoomWatcher(ITagManager aTagManager, MockableScriptRunner aRunner)
            {
                iTagManager = aTagManager;
                iRunner = aRunner;
                iFactory = new ResultWatcherFactory(aRunner);

                iWatcherLookup = new Dictionary<IStandardRoom, RoomControllerWatcher>();
            }

            public void Dispose()
            {
                foreach (var kvp in iWatcherLookup.Values)
                {
                    kvp.Dispose();
                }

                iFactory.Dispose();
            }

            public void OrderedOpen()
            {
            }

            public void OrderedInitialised()
            {
            }

            public void OrderedClose()
            {
            }

            public void OrderedAdd(IStandardRoom aItem, uint aIndex)
            {
                iRunner.Result(string.Format("Room Added: {0} at {1}", aItem.Name, aIndex));

                iFactory.Create<IZone>(aItem.Name, aItem.Zone, (v) =>
                {
                    iFactory.Create<IStandardRoom>(aItem.Name, v.Listeners, (w) =>
                    {
                        return "Listener " + w.Name;
                    });

                    return "Zone " + v.Active + " " + (v.Active ? v.Sender.Udn : "") + " " + v.Room.Name;
                });

                iWatcherLookup.Add(aItem, new RoomControllerWatcher(iTagManager, iRunner, aItem));
            }

            public void OrderedMove(IStandardRoom aItem, uint aFrom, uint aTo)
            {
                iRunner.Result(string.Format("Room Moved: {0} from {1} to {2}", aItem.Name, aFrom, aTo));
            }

            public void OrderedRemove(IStandardRoom aItem, uint aIndex)
            {
                iRunner.Result(string.Format("Room Removed: {0} at {1}", aItem.Name, aIndex));
                iFactory.Destroy(aItem.Name);

                iWatcherLookup[aItem].Dispose();
                iWatcherLookup.Remove(aItem);
            }

            private ITagManager iTagManager;
            private MockableScriptRunner iRunner;
            private ResultWatcherFactory iFactory;
            private Dictionary<IStandardRoom, RoomControllerWatcher> iWatcherLookup;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestZone.exe <testscript>");
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
            Topology3 topology3 = new Topology3(topologym);
            Topology4 topology4 = new Topology4(topology3);

            StandardHouse house = new StandardHouse(network, topology4);
            mocker.Add("house", house);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(network.TagManager, runner);
            thread.Schedule(() =>
            {
                house.Rooms.AddWatcher(watcher);
            });

            try
            {
                runner.Run(network, new StringReader(File.ReadAllText(args[0])), mocker);
                //runner.Run(thread, Console.In, mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            thread.Execute(() =>
            {
                house.Rooms.RemoveWatcher(watcher);
                watcher.Dispose();
            });

            house.Dispose();

            topology4.Dispose();

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
