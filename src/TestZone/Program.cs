using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os;
using OpenHome.Av;
using OpenHome.Os.App;

namespace TestZone
{
    class Program
    {
        class RoomControllerWatcher : IWatcher<IZone>, IDisposable
        {
            private ResultWatcherFactory iFactory;
            private IStandardRoom iRoom;
            private IVolumeController iController;

            public RoomControllerWatcher(ITagManager aTagManager, MockableScriptRunner aRunner, IStandardRoom aRoom)
            {
                iFactory = new ResultWatcherFactory(aRunner);
                iRoom = aRoom;

                iRoom.Zone.AddWatcher(this);
            }

            public void Dispose()
            {
                iRoom.Zone.RemoveWatcher(this);

                iFactory.Dispose();
            }

            private void CreateController(IZone aZone)
            {
                iController = VolumeController.Create(aZone);
                iFactory.Create<bool>(iRoom.Name, iController.Mute, v => "Zone Mute " + v);
            }

            private void DestroyController()
            {
                iFactory.Destroy(iRoom.Name);
                iController.Dispose();
            }

            public void ItemOpen(string aId, IZone aValue)
            {
                if (aValue.Active)
                {
                    CreateController(aValue);
                }
            }

            public void ItemUpdate(string aId, IZone aValue, IZone aPrevious)
            {
                if (aPrevious.Active)
                {
                    DestroyController();
                }
                if (aValue.Active)
                {
                    CreateController(aValue);
                }
            }

            public void ItemClose(string aId, IZone aValue)
            {
                if (aValue.Active)
                {
                    DestroyController();
                }
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
                iFactory.Create<RoomMetadata>(aItem.Name, aItem.Metadata, (v) =>
                {
                    return "Metadata " + v.Enabled + " " + iTagManager.ToDidlLite(v.Metadata) + " " + v.Uri;
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

            Mockable mocker = new Mockable();

            Network network = new Network();
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
            
            network.Schedule(() =>
            {
                house.Rooms.AddWatcher(watcher);
            });

            try
            {
                runner.Run(network, new StringReader(File.ReadAllText(args[0])), mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            network.Execute(() =>
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

            mockInjector.Dispose();

            network.Dispose();

            return 0;
        }
    }
}
