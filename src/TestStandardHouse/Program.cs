using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os;
using OpenHome.Av;
using OpenHome.Os.App;

namespace TestLinnHouse
{
    class Program
    {
        class RoomControllerWatcher : IDisposable
        {
            private ITagManager iTagManager;
            private IStandardHouse iHouse;
            private ResultWatcherFactory iFactory;
            private IStandardRoomController iController;
            private IStandardRoomTime iTime;
            private StandardRoomWatcherExternal iWatcherExternal;
            private StandardRoomWatcherRadio iWatcherRadio;
            private StandardRoomWatcherMusic iWatcherMusic;

            public RoomControllerWatcher(ITagManager aTagManager, IStandardHouse aHouse, MockableScriptRunner aRunner, IStandardRoom aRoom)
            {
                iTagManager = aTagManager;
                iHouse = aHouse;
                iFactory = new ResultWatcherFactory(aRunner);
                iController = new StandardRoomController(aRoom);
                iTime = new StandardRoomTime(aRoom);
                iWatcherExternal = new StandardRoomWatcherExternal(aRoom);
                iWatcherRadio = new StandardRoomWatcherRadio(aRoom);
                iWatcherMusic = new StandardRoomWatcherMusic(aRoom);

                iFactory.Create<bool>(iController.Name, iController.Active, v => "Controller Active " + v);
                iFactory.Create<bool>(iController.Name, iController.HasVolume, v => "HasVolume " + v);
                iFactory.Create<bool>(iController.Name, iController.HasSourceControl, v => "HasSourceControl " + v);
                iFactory.Create<bool>(iController.Name, iController.Mute, v => "Mute " + v);
                iFactory.Create<uint>(iController.Name, iController.Volume, v => "Volume " + v);
                iFactory.Create<string>(iController.Name, iController.TransportState, v => "TransportState " + v);

                iFactory.Create<bool>(iTime.Name, iTime.Active, v => "Time Active " + v);
                iFactory.Create<bool>(iTime.Name, iTime.HasTime, v => "HasTime " + v);
                iFactory.Create<uint>(iTime.Name, iTime.Duration, v => "Duration " + v);
                iFactory.Create<uint>(iTime.Name, iTime.Seconds, v => "Seconds " + v);

                iFactory.Create<IWatchableSnapshot<IMediaPreset>>(iWatcherExternal.Room.Name, iWatcherExternal.Unconfigured.Snapshot, v =>
                {
                    string info = "\nUnconfigured source begin\n";
                    IWatchableFragment<IMediaPreset> fragment = v.Read(0, v.Total).Result;
                    foreach (IMediaPreset p in fragment.Data)
                    {
                        info += p.Metadata[iTagManager.Audio.Title].Value + "\n";
                    }
                    info += "Unconfigured source end";
                    return info;
                });
                iFactory.Create<IWatchableSnapshot<IMediaPreset>>(iWatcherExternal.Room.Name, iWatcherExternal.Configured.Snapshot, v =>
                {
                    string info = "\nConfigured source begin\n";
                    IWatchableFragment<IMediaPreset> fragment = v.Read(0, v.Total).Result;
                    foreach (IMediaPreset p in fragment.Data)
                    {
                        info += p.Metadata[iTagManager.Audio.Title].Value + "\n";
                    }
                    info += "Configured source end";
                    return info;
                });

                iFactory.Create<bool>(iWatcherRadio.Room.Name, iWatcherRadio.Enabled, v =>
                {
                    if (v)
                    {
                        IWatchableContainer<IMediaPreset> container = iWatcherRadio.Container.Result;
                        iFactory.Create<IWatchableSnapshot<IMediaPreset>>(iWatcherRadio.Room.Name, container.Snapshot, w =>
                        {
                            string info = "\nPresets begin\n";
                            IWatchableFragment<IMediaPreset> fragment = w.Read(0, w.Total).Result;
                            foreach (IMediaPreset m in fragment.Data)
                            {
                                info += m.Index + " ";
                                string didl = iTagManager.ToDidlLite(m.Metadata);
                                info += didl;
                                info += "\n";
                            }
                            info += "Presets end";
                            return info;
                        });
                    }

                    return "Presets Enabled " + v;
                });

                iFactory.Create<bool>(iWatcherMusic.Room.Name, iWatcherMusic.Enabled, v =>
                {
                    return "Music Enabled " + v;
                });
            }

            public void Dispose()
            {
                iFactory.Dispose();
                iController.Dispose();
                iTime.Dispose();
                iWatcherExternal.Dispose();
                iWatcherRadio.Dispose();
                iWatcherMusic.Dispose();
            }
        }

        class RoomWatcher : IOrderedWatcher<IStandardRoom>, IDisposable
        {
            public RoomWatcher(ITagManager aTagManager, IStandardHouse aHouse, MockableScriptRunner aRunner)
            {
                iTagManager = aTagManager;
                iHouse = aHouse;
                iRunner = aRunner;
                iFactory = new ResultWatcherFactory(aRunner);

                iWatcherLookup = new Dictionary<IStandardRoom, RoomControllerWatcher>();
            }

            public void Dispose()
            {
                foreach(var kvp in iWatcherLookup.Values)
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
                iFactory.Create<EStandby>(aItem.Name, aItem.Standby, v => "Standby " + v);
                iFactory.Create<RoomDetails>(aItem.Name, aItem.Details, v => "Details " + v.Enabled + " " + v.BitDepth + " " + v.BitRate + " " + v.CodecName + " " + v.Duration + " " + v.Lossless + " " + v.SampleRate);
                iFactory.Create<RoomMetadata>(aItem.Name, aItem.Metadata, v => "Metadata " + v.Enabled + " " + iTagManager.ToDidlLite(v.Metadata) + " " + v.Uri);
                iFactory.Create<RoomMetatext>(aItem.Name, aItem.Metatext, v => "Metatext " + v.Enabled + " " + v.Metatext);
                iFactory.Create<IZone>(aItem.Name, aItem.Zone, v => "Zone " + v.Active + " " + ((v.Sender == null) ? "" : v.Sender.Udn));

                iWatcherLookup.Add(aItem, new RoomControllerWatcher(iTagManager, iHouse, iRunner, aItem));
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
            private IStandardHouse iHouse;
            private MockableScriptRunner iRunner;
            private ResultWatcherFactory iFactory;
            private Dictionary<IStandardRoom, RoomControllerWatcher> iWatcherLookup;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestStandardHouse.exe <testscript>");
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

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(network.TagManager, house, runner);

            network.Schedule(() =>
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
