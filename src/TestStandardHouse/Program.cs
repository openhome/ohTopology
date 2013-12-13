using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using OpenHome.Os;
using OpenHome.Av;
using OpenHome.Os.App;

namespace TestStandardHouse
{
    class Program
    {
        class RoomControllerWatcher : IDisposable
        {
            private ITagManager iTagManager;
            private ResultWatcherFactory iFactory;
            private ResultWatcherFactory iFactoryRadioPresets;
            private ResultWatcherFactory iFactoryRadioPresetsPlaying;
            private ResultWatcherFactory iFactorySendersPresets;
            private ResultWatcherFactory iFactorySendersPresetsPlaying;
            private IStandardRoomController iController;
            private IStandardRoomTime iTime;
            private IStandardRoomInfo iInfo;
            private StandardRoomWatcherExternal iWatcherExternal;
            private StandardRoomWatcherRadio iWatcherRadio;
            private StandardRoomWatcherMusic iWatcherMusic;
            private StandardRoomWatcherSenders iWatcherSenders;
            private IWatchableFragment<IMediaPreset> iRadioPresets;
            private IWatchableFragment<IMediaPreset> iSendersPresets;

            public RoomControllerWatcher(ITagManager aTagManager, MockableScriptRunner aRunner, IStandardHouse aHouse, IStandardRoom aRoom)
            {
                iTagManager = aTagManager;
                iFactory = new ResultWatcherFactory(aRunner);
                iFactoryRadioPresets = new ResultWatcherFactory(aRunner);
                iFactoryRadioPresetsPlaying = new ResultWatcherFactory(aRunner);
                iFactorySendersPresets = new ResultWatcherFactory(aRunner);
                iFactorySendersPresetsPlaying = new ResultWatcherFactory(aRunner);
                iController = aRoom.CreateController();
                iTime = aRoom.CreateTimeController();
                iInfo = aRoom.CreateInfoController();
                iWatcherExternal = new StandardRoomWatcherExternal(aRoom);
                iWatcherRadio = new StandardRoomWatcherRadio(aRoom);
                iWatcherMusic = new StandardRoomWatcherMusic(aRoom);
                iWatcherSenders = new StandardRoomWatcherSenders(aHouse, aRoom);

                iFactory.Create<RoomDetails>(iInfo.Name, iInfo.Details, (v, w) => w("Details " + v.Enabled + " " + v.BitDepth + " " + v.BitRate + " " + v.CodecName + " " + v.Duration + " " + v.Lossless + " " + v.SampleRate));
                iFactory.Create<RoomMetadata>(iInfo.Name, iInfo.Metadata, (v, w) => w("Metadata " + v.Enabled + " " + iTagManager.ToDidlLite(v.Metadata) + " " + v.Uri));
                iFactory.Create<RoomMetatext>(iInfo.Name, iInfo.Metatext, (v, w) => w("Metatext " + v.Enabled + " " + iTagManager.ToDidlLite(v.Metatext)));

                iFactory.Create<bool>(iController.Name, iController.Active, (v, w) => w("Controller Active " + v));
                iFactory.Create<bool>(iController.Name, iController.HasVolume, (v, w) => w("HasVolume " + v));
                iFactory.Create<bool>(iController.Name, iController.HasSourceControl, (v, w) => w("HasSourceControl " + v));
                iFactory.Create<bool>(iController.Name, iController.Mute, (v, w) => w("Mute " + v));
                iFactory.Create<uint>(iController.Name, iController.Volume, (v, w) => w("Volume " + v));
                iFactory.Create<string>(iController.Name, iController.TransportState, (v, w) => w("TransportState " + v));

                iFactory.Create<bool>(iTime.Name, iTime.Active, (v, w) => w("Time Active " + v));
                iFactory.Create<bool>(iTime.Name, iTime.HasTime, (v, w) => w("HasTime " + v));
                iFactory.Create<uint>(iTime.Name, iTime.Duration, (v, w) => w("Duration " + v));
                iFactory.Create<uint>(iTime.Name, iTime.Seconds, (v, w) => w("Seconds " + v));

                iFactory.Create<bool>(iWatcherExternal.Room.Name, iWatcherExternal.Enabled, (x, y) =>
                {
                    if (x)
                    {
                        iFactory.Create<IWatchableSnapshot<IMediaPreset>>(iWatcherExternal.Room.Name, iWatcherExternal.Unconfigured, (v, w) =>
                        {
                            v.Read(0, v.Total, (f) =>
                            {
                                string info = "\nUnconfigured source begin\n";
                                foreach (IMediaPreset p in f.Data)
                                {
                                    info += p.Metadata[iTagManager.Audio.Title].Value + "\n";
                                    p.Dispose();
                                }
                                info += "Unconfigured source end";
                                w(info);
                            });
                        });
                        iFactory.Create<IWatchableSnapshot<IMediaPreset>>(iWatcherExternal.Room.Name, iWatcherExternal.Configured, (v, w) =>
                        {
                            v.Read(0, v.Total, (f) =>
                            {
                                string info = "\nConfigured source begin\n";
                                foreach (IMediaPreset p in f.Data)
                                {
                                    info += p.Metadata[iTagManager.Audio.Title].Value + "\n";
                                    p.Dispose();
                                }
                                info += "Configured source end";
                                w(info);
                            });
                        });
                    }
                    y("External Enabled " + x);
                });

                iFactory.Create<bool>(iWatcherRadio.Room.Name, iWatcherRadio.Enabled, (x, y) =>
                {
                    if (x)
                    {
                        iFactoryRadioPresets.Create<IWatchableSnapshot<IMediaPreset>>(iWatcherRadio.Room.Name, iWatcherRadio.Snapshot, (v, w) =>
                        {
                            if (iRadioPresets != null)
                            {
                                iFactoryRadioPresetsPlaying.Destroy(iWatcherRadio.Room.Name);
                                foreach (IMediaPreset p in iRadioPresets.Data)
                                {
                                    p.Dispose();
                                }
                                iRadioPresets = null;
                            }

                            v.Read(0, v.Total, (f) =>
                            {
                                string info = "\nPresets begin\n";
                                iRadioPresets = f;
                                foreach (IMediaPreset p in f.Data)
                                {
                                    CreateResultWatcherPreset(iFactoryRadioPresetsPlaying, p);

                                    info += p.Index + " ";
                                    string didl = iTagManager.ToDidlLite(p.Metadata);
                                    info += didl;
                                    info += "\n";
                                }
                                info += "Presets end";
                                w(info);
                            });
                        });
                    }
                    else
                    {
                        if (iRadioPresets != null)
                        {
                            iFactoryRadioPresetsPlaying.Destroy(iWatcherRadio.Room.Name);
                            foreach (IMediaPreset p in iRadioPresets.Data)
                            {
                                p.Dispose();
                            }
                            iRadioPresets = null;
                            iFactoryRadioPresets.Destroy(iWatcherRadio.Room.Name);
                        }
                    }

                    y("Presets Enabled " + x);
                });

                iFactory.Create<bool>(iWatcherMusic.Room.Name, iWatcherMusic.Enabled, (v, w) =>
                {
                    w("Music Enabled " + v);
                });

                iFactory.Create<bool>(iWatcherSenders.Room.Name, iWatcherSenders.Enabled, (x, y) =>
                {
                    /*if (x)
                    {
                        IWatchableContainer<IMediaPreset> container = iWatcherSenders.Container.Result;
                        iFactorySendersPresets.Create<IWatchableSnapshot<IMediaPreset>>(iWatcherSenders.Room.Name, container.Snapshot, (v, w) =>
                        {
                            if (iSendersPresets != null)
                            {
                                iFactorySendersPresetsPlaying.Destroy(iWatcherSenders.Room.Name);
                                foreach (IMediaPreset p in iSendersPresets.Data)
                                {
                                    p.Dispose();
                                }
                                iSendersPresets = null;
                            }

                            string info = "\nSenders begin\n";
                            IWatchableFragment<IMediaPreset> fragment = w.Read(0, w.Total).Result;
                            iSendersPresets = fragment;
                            foreach (IMediaPreset p in fragment.Data)
                            {
                                CreateResultWatcherPreset(iFactorySendersPresetsPlaying, p);

                                info += p.Index + " ";
                                string didl = iTagManager.ToDidlLite(p.Metadata);
                                info += didl;
                                info += "\n";
                            }
                            info += "Senders end";
                            return info;
                        });
                    }
                    else
                    {
                        if (iSendersPresets != null)
                        {
                            iFactorySendersPresetsPlaying.Destroy(iWatcherSenders.Room.Name);
                            foreach (IMediaPreset p in iSendersPresets.Data)
                            {
                                p.Dispose();
                            }
                            iSendersPresets = null;
                            iFactorySendersPresets.Destroy(iWatcherSenders.Room.Name);
                        }
                    }*/
                    y("Senders Enabled " + x);
                });
            }

            private void CreateResultWatcherPreset(ResultWatcherFactory aFactory, IMediaPreset aPreset)
            {
                aFactory.Create<bool>(iWatcherRadio.Room.Name, aPreset.Playing, (v, w) =>
                {
                    w("Playing " + aPreset.Index + " " + v);
                });
            }

            public void Dispose()
            {
                iFactory.Dispose();
                iFactoryRadioPresets.Dispose();
                iFactoryRadioPresetsPlaying.Dispose();
                if (iRadioPresets != null)
                {
                    foreach (IMediaPreset p in iRadioPresets.Data)
                    {
                        p.Dispose();
                    }
                    iRadioPresets = null;
                }
                iFactorySendersPresets.Dispose();
                iFactorySendersPresetsPlaying.Dispose();
                if (iSendersPresets != null)
                {
                    foreach (IMediaPreset p in iSendersPresets.Data)
                    {
                        p.Dispose();
                    }
                    iSendersPresets = null;
                }
                iController.Dispose();
                iTime.Dispose();
                iWatcherExternal.Dispose();
                iWatcherRadio.Dispose();
                iWatcherMusic.Dispose();
                iWatcherSenders.Dispose();
            }
        }

        class RoomWatcher : IOrderedWatcher<IStandardRoom>, IDisposable
        {
            public RoomWatcher(ITagManager aTagManager, MockableScriptRunner aRunner, IStandardHouse aHouse)
            {
                iTagManager = aTagManager;
                iRunner = aRunner;
                iHouse = aHouse;
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
                iFactory.Create<EStandby>(aItem.Name, aItem.Standby, (v, w) => w("Standby " + v));
                iFactory.Create<IZoneSender>(aItem.Name, aItem.ZoneSender, (v, w) => w("Zone " + v.Enabled + " " + ((v.Sender == null) ? "" : v.Sender.Udn)));

                iWatcherLookup.Add(aItem, new RoomControllerWatcher(iTagManager, iRunner, iHouse, aItem));
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
            private IStandardHouse iHouse;
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

            Log log = new Log(new LogConsole());

            Network network = new Network(50, log);
            InjectorMock mockInjector = new InjectorMock(network, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), log);
            mocker.Add("network", mockInjector);

            StandardHouse house = new StandardHouse(network, log);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(network.TagManager, runner, house);

            network.Schedule(() =>
            {
                house.Rooms.AddWatcher(watcher);
            });

            try
            {
                runner.Run(network.Wait, new StringReader(File.ReadAllText(args[0])), mocker);
                //runner.Run(thread.Wait, Console.In, mocker);
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
