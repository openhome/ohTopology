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
        class ExceptionReporter : IExceptionReporter
        {
            public void ReportException(Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }

        class RoomControllerWatcher : IWatcher<bool>, IWatcher<uint>, IWatcher<string>, IDisposable
        {
            public RoomControllerWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                iRoomControllerLookup = new Dictionary<IStandardRoom, StandardRoomController>();
            }

            public void Dispose()
            {
                List<IStandardRoom> rooms = new List<IStandardRoom>(iRoomControllerLookup.Keys);
                foreach (IStandardRoom r in rooms)
                {
                    Remove(r);
                }
                iRoomControllerLookup = null;
            }

            public void Add(IStandardRoom aRoom)
            {
                StandardRoomController controller = new StandardRoomController(aRoom);

                controller.Active.AddWatcher(this);
                controller.HasVolume.AddWatcher(this);
                controller.HasSourceControl.AddWatcher(this);
                controller.Mute.AddWatcher(this);
                controller.Volume.AddWatcher(this);
                controller.TransportState.AddWatcher(this);

                iRoomControllerLookup.Add(aRoom, controller);
            }

            public void Remove(IStandardRoom aRoom)
            {
                StandardRoomController controller = iRoomControllerLookup[aRoom];
                iRoomControllerLookup.Remove(aRoom);

                controller.Active.RemoveWatcher(this);
                controller.HasVolume.RemoveWatcher(this);
                controller.HasSourceControl.RemoveWatcher(this);
                controller.Mute.RemoveWatcher(this);
                controller.Volume.RemoveWatcher(this);
                controller.TransportState.RemoveWatcher(this);

                controller.Dispose();
            }

            public void ItemOpen(string aId, bool aValue)
            {
                iRunner.Result(string.Format("{0}: {1}", aId, aValue));
            }

            public void ItemUpdate(string aId, bool aValue, bool aPrevious)
            {
                iRunner.Result(string.Format("{0}: {1} -> {2}", aId, aPrevious, aValue));
            }

            public void ItemClose(string aId, bool aValue)
            {
            }

            public void ItemOpen(string aId, uint aValue)
            {
                iRunner.Result(string.Format("{0}: {1}", aId, aValue));
            }

            public void ItemUpdate(string aId, uint aValue, uint aPrevious)
            {
                iRunner.Result(string.Format("{0}: {1} -> {2}", aId, aPrevious, aValue));
            }

            public void ItemClose(string aId, uint aValue)
            {
            }

            public void ItemOpen(string aId, string aValue)
            {
                iRunner.Result(string.Format("{0}: {1}", aId, aValue));
            }

            public void ItemUpdate(string aId, string aValue, string aPrevious)
            {
                iRunner.Result(string.Format("{0}: {1} -> {2}", aId, aPrevious, aValue));
            }

            public void ItemClose(string aId, string aValue)
            {
            }

            private MockableScriptRunner iRunner;

            private Dictionary<IStandardRoom, StandardRoomController> iRoomControllerLookup;
        }

        class RoomWatcher : IOrderedWatcher<IStandardRoom>, IWatcher<EStandby>, IWatcher<bool>, IWatcher<RoomDetails>, IWatcher<RoomMetadata>, IWatcher<RoomMetatext>, IDisposable
        {
            public RoomWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                //iRootWatcher = new RootWatcher(aRunner);
                //iSourceWatcher = new SourceWatcher(aRunner);
                iRoomControllerWatcher = new RoomControllerWatcher(aRunner);

                iList = new List<IStandardRoom>();
            }

            public void Dispose()
            {
                foreach (IStandardRoom r in iList)
                {
                    r.Standby.RemoveWatcher(this);
                    r.Details.RemoveWatcher(this);
                    r.Metadata.RemoveWatcher(this);
                    r.Metatext.RemoveWatcher(this);
                    r.CanSendAudio.RemoveWatcher(this);
                    //r.Roots.RemoveWatcher(iRootWatcher);
                    //r.Sources.RemoveWatcher(iSourceWatcher);
                }

                //iRootWatcher.Dispose();
                //iSourceWatcher.Dispose();
                iRoomControllerWatcher.Dispose();
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

                iList.Insert(aIndex, aItem);
                aItem.Standby.AddWatcher(this);
                aItem.Details.AddWatcher(this);
                aItem.Metadata.AddWatcher(this);
                aItem.Metatext.AddWatcher(this);
                aItem.CanSendAudio.AddWatcher(this);
                //aItem.Roots.AddWatcher(iRootWatcher);
                //aItem.Sources.AddWatcher(iSourceWatcher);
                iRoomControllerWatcher.Add(aItem);
            }

            public void OrderedMove(IStandardRoom aItem, uint aFrom, uint aTo)
            {
                iRunner.Result(string.Format("Room Moved: {0} from {1} to {2}", aItem.Name, aFrom, aTo));

                iList.Remove(aItem);
                iList.Insert(aTo, aItem);
            }

            public void OrderedRemove(IStandardRoom aItem, uint aIndex)
            {
                iRunner.Result(string.Format("Room Removed: {0} at {1}", aItem.Name, aIndex));

                iList.Remove(aItem);
                aItem.Standby.RemoveWatcher(this);
                aItem.Details.RemoveWatcher(this);
                aItem.Metadata.RemoveWatcher(this);
                aItem.Metatext.RemoveWatcher(this);
                aItem.CanSendAudio.RemoveWatcher(this);
                //aItem.Roots.RemoveWatcher(iRootWatcher);
                //aItem.Sources.RemoveWatcher(iSourceWatcher);
                iRoomControllerWatcher.Remove(aItem);
            }

            public void ItemOpen(string aId, EStandby aValue)
            {
                iRunner.Result(string.Format("{0}: {1}", aId, aValue));
            }

            public void ItemUpdate(string aId, EStandby aValue, EStandby aPrevious)
            {
                iRunner.Result(string.Format("{0}: {1} -> {2}", aId, aPrevious, aValue));
            }

            public void ItemClose(string aId, EStandby aValue)
            {
            }

            public void ItemOpen(string aId, bool aValue)
            {
                iRunner.Result(string.Format("{0}: {1}", aId, aValue));
            }

            public void ItemUpdate(string aId, bool aValue, bool aPrevious)
            {
                iRunner.Result(string.Format("{0}: {1} -> {2}", aId, aPrevious, aValue));
            }

            public void ItemClose(string aId, bool aValue)
            {
            }

            public void ItemOpen(string aId, RoomDetails aValue)
            {
                iRunner.Result(string.Format("{0}: Active={1}, BitDepth={2}, BitRate={3}, CodeName={4}, Duration={5}, Lossless={6}, SampleRate={7}",
                    aId, aValue.Enabled, aValue.BitDepth, aValue.BitRate, aValue.CodecName, aValue.Duration, aValue.Lossless, aValue.SampleRate));
            }

            public void ItemUpdate(string aId, RoomDetails aValue, RoomDetails aPrevious)
            {
                iRunner.Result(string.Format("{0} Updated: Active={1}, BitDepth={2}, BitRate={3}, CodeName={4}, Duration={5}, Lossless={6}, SampleRate={7}",
                    aId, aValue.Enabled, aValue.BitDepth, aValue.BitRate, aValue.CodecName, aValue.Duration, aValue.Lossless, aValue.SampleRate));
            }

            public void ItemClose(string aId, RoomDetails aValue)
            {
            }

            public void ItemOpen(string aId, RoomMetadata aValue)
            {
                iRunner.Result(string.Format("{0}: Active={1}, Metadata={2}, Uri={3}", aId, aValue.Enabled, aValue.Metadata, aValue.Uri));
            }

            public void ItemUpdate(string aId, RoomMetadata aValue, RoomMetadata aPrevious)
            {
                iRunner.Result(string.Format("{0} Updated: Active={1}, Metadata={2}, Uri={3}", aId, aValue.Enabled, aValue.Metadata, aValue.Uri));
            }

            public void ItemClose(string aId, RoomMetadata aValue)
            {
            }

            public void ItemOpen(string aId, RoomMetatext aValue)
            {
                iRunner.Result(string.Format("{0}: Active={1}, Metatext={2}", aId, aValue.Enabled, aValue.Metatext));
            }

            public void ItemUpdate(string aId, RoomMetatext aValue, RoomMetatext aPrevious)
            {
                iRunner.Result(string.Format("{0} Updated: Active={1}, Metatext={2}", aId, aValue.Enabled, aValue.Metatext));
            }

            public void ItemClose(string aId, RoomMetatext aValue)
            {
            }

            private MockableScriptRunner iRunner;
            //private RootWatcher iRootWatcher;
            //private SourceWatcher iSourceWatcher;
            private RoomControllerWatcher iRoomControllerWatcher;
            private List<IStandardRoom> iList;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestStandardHouse.exe <testscript>");
                return 1;
            }

            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread thread = new WatchableThread(reporter);
            WatchableThread subscribeThread = new WatchableThread(reporter);

            Mockable mocker = new Mockable();

            MockNetwork network = new FourDsMockNetwork(thread, subscribeThread, mocker);
            mocker.Add("network", network);

            Topology1 topology1 = new Topology1(network);
            Topology2 topology2 = new Topology2(topology1);
            Topology3 topology3 = new Topology3(topology2);
            Topology4 topology4 = new Topology4(topology3);

            StandardHouse house = new StandardHouse(thread, topology4);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(runner);
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

            topology2.Dispose();

            topology1.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
