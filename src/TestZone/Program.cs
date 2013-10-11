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
        class RoomControllerWatcher : IWatcher<IZoneSender>, IDisposable
        {
            private ResultWatcherFactory iFactory;
            private IStandardRoom iRoom;
            private IVolumeController iController;

            public RoomControllerWatcher(ITagManager aTagManager, MockableScriptRunner aRunner, IStandardRoom aRoom)
            {
                iFactory = new ResultWatcherFactory(aRunner);
                iRoom = aRoom;

                iRoom.ZoneSender.AddWatcher(this);
            }

            public void Dispose()
            {
                iRoom.ZoneSender.RemoveWatcher(this);

                iFactory.Dispose();
            }

            private void CreateController(IZoneSender aZone)
            {
                iFactory.Create<bool>(iRoom.Name, aZone.HasListeners, (v) =>
                {
                    return "HasListeners " + v;
                });

                iFactory.Create<IStandardRoom>(iRoom.Name, aZone.Listeners, (v) =>
                {
                    return "Listener " + v.Name;
                });


                iController = aZone.CreateVolumeController();
                iFactory.Create<bool>(iRoom.Name, iController.HasVolume, v => "Zone HasVolume " + v);
                iFactory.Create<bool>(iRoom.Name, iController.Mute, v => "Zone Mute " + v);
                iFactory.Create<uint>(iRoom.Name, iController.Volume, v => "Zone Volume " + v);
            }

            private void DestroyController()
            {
                iFactory.Destroy(iRoom.Name);
                iController.Dispose();
            }

            public void ItemOpen(string aId, IZoneSender aValue)
            {
                if (aValue.Enabled)
                {
                    CreateController(aValue);
                }
            }

            public void ItemUpdate(string aId, IZoneSender aValue, IZoneSender aPrevious)
            {
                if (aPrevious.Enabled)
                {
                    DestroyController();
                }
                if (aValue.Enabled)
                {
                    CreateController(aValue);
                }
            }

            public void ItemClose(string aId, IZoneSender aValue)
            {
                if (aValue.Enabled)
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

                iFactory.Create<IZoneSender>(aItem.Name, aItem.ZoneSender, (v) =>
                {
                    return "ZoneSender " + v.Enabled + " " + (v.Enabled ? v.Sender.Udn : "") + " " + v.Room.Name;
                });
                iFactory.Create<IZoneReceiver>(aItem.Name, aItem.ZoneReceiver, (v) =>
                {
                    return "ZoneReceiver " + v.Enabled + " " + (v.ZoneSender != null ? v.ZoneSender.Room.Name : "");
                });
                iFactory.Create<IStandardRoom>(aItem.Name, aItem.Satellites, (v) =>
                {
                    return "Satellite " + v.Name;
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

            Log log = new Log(new LogConsole());

            Network network = new Network(50, log);
            DeviceInjectorMock mockInjector = new DeviceInjectorMock(network, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), log);
            mocker.Add("network", mockInjector);

            StandardHouse house = new StandardHouse(network, log);
            mocker.Add("house", house);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(network.TagManager, runner);

            network.Schedule(() =>
            {
                house.Rooms.AddWatcher(watcher);
            });

            try
            {
                runner.Run(network.Wait, new StringReader(File.ReadAllText(args[0])), mocker);
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

            mockInjector.Dispose();

            network.Dispose();

            return 0;
        }
    }
}
