using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os;
using OpenHome.Av;
using OpenHome.Os.App;

namespace TestTopology4
{
    class Program
    {
        class RootWatcher : IDisposable
        {
            public RootWatcher(MockableScriptRunner aRunner, ITopology4Root aRoot)
            {
                iFactory = new ResultWatcherFactory(aRunner);
                iFactory.Create<ITopology4Source>(aRoot.Name, aRoot.Source, v => 
                {
                    string info = "";
                    info += string.Format("Source {0} {1} {2} {3} {4} {5} {6} {7} Volume",
                        v.Index, v.Group.Name, v.Name, v.Type, v.Visible, v.HasInfo, v.HasTime, v.Device.Udn);
                    foreach (var g in v.Volumes)
                    {
                        info += " " + g.Device.Udn;
                    }
                    return info;
                });
                iFactory.Create<IEnumerable<ITopology4Group>>(aRoot.Name, aRoot.Senders, v =>
                {
                    string info = "\nSenders begin\n";
                    foreach (var g in v)
                    {
                        info += "Sender " + g.Name;
                        info += "\n";
                    }
                    info += "Senders end";
                    return info;
                });
            }

            public void Dispose()
            {
                iFactory.Dispose();
            }

            private ResultWatcherFactory iFactory;
        }

        class RoomWatcher : IWatcher<IEnumerable<ITopology4Root>>, IDisposable
        {
            private MockableScriptRunner iRunner;
            private ITopology4Room iRoom;
            private List<RootWatcher> iWatchers;

            public RoomWatcher(MockableScriptRunner aRunner, ITopology4Room aRoom)
            {
                iRunner = aRunner;
                iRoom = aRoom;

                iWatchers = new List<RootWatcher>();

                iRoom.Roots.AddWatcher(this);
            }

            public void Dispose()
            {
                iRoom.Roots.RemoveWatcher(this);
                iWatchers.ForEach(w => w.Dispose());
            }

            public void ItemOpen(string aId, IEnumerable<ITopology4Root> aValue)
            {
                foreach(var r in aValue)
                {
                    iWatchers.Add(new RootWatcher(iRunner, r));
                }
            }

            public void ItemUpdate(string aId, IEnumerable<ITopology4Root> aValue, IEnumerable<ITopology4Root> aPrevious)
            {
                iWatchers.ForEach(w => w.Dispose());
                iWatchers.Clear();
                foreach (var r in aValue)
                {
                    iWatchers.Add(new RootWatcher(iRunner, r));
                }
            }

            public void ItemClose(string aId, IEnumerable<ITopology4Root> aValue)
            {
            }
        }

        class HouseWatcher : IUnorderedWatcher<ITopology4Room>, IDisposable
        {
            public HouseWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
                iFactory = new ResultWatcherFactory(aRunner);
                iWatcherLookup = new Dictionary<ITopology4Room, RoomWatcher>();
            }

            public void Dispose()
            {
                iFactory.Dispose();
                foreach(var w in iWatcherLookup.Values)
                {
                    w.Dispose();
                }
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

            public void UnorderedAdd(ITopology4Room aItem)
            {
                iRunner.Result("Room Added " + aItem.Name);
                iFactory.Create<EStandby>(aItem.Name, aItem.Standby, v => "Standby " + v);
                iFactory.Create<IEnumerable<ITopology4Source>>(aItem.Name, aItem.Sources, v =>
                {
                    string info = "\nSources begin\n";
                    foreach (var s in v)
                    {
                        info += string.Format("Source {0} {1} {2} {3} {4} {5} {6} {7} Volume",
                            s.Index, s.Group.Name, s.Name, s.Type, s.Visible, s.HasInfo, s.HasTime, s.Device.Udn);
                        foreach (var g in s.Volumes)
                        {
                            info += " " + g.Device.Udn;
                        }
                        info += "\n";
                    }
                    info += "Sources end";
                    return info;
                });
                iWatcherLookup.Add(aItem, new RoomWatcher(iRunner, aItem));
            }

            public void UnorderedRemove(ITopology4Room aItem)
            {
                iRunner.Result("Room Removed " + aItem.Name);
                iFactory.Destroy(aItem.Name);
                iWatcherLookup[aItem].Dispose();
                iWatcherLookup.Remove(aItem);
            }

            private MockableScriptRunner iRunner;
            private ResultWatcherFactory iFactory;
            private Dictionary<ITopology4Room, RoomWatcher> iWatcherLookup;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestTopology4.exe <testscript>");
                return 1;
            }

            Mockable mocker = new Mockable();

            Network network = new Network(50);
            DeviceInjectorMock mockInjector = new DeviceInjectorMock(network, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
            mocker.Add("network", mockInjector);

            Topology1 topology1 = new Topology1(network);
            Topology2 topology2 = new Topology2(topology1);
            Topologym topologym = new Topologym(topology2);
            Topology3 topology3 = new Topology3(topologym);
            Topology4 topology4 = new Topology4(topology3);

            MockableScriptRunner runner = new MockableScriptRunner();

            HouseWatcher watcher = new HouseWatcher(runner);
            
            network.Schedule(() =>
            {
                topology4.Rooms.AddWatcher(watcher);
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
                topology4.Rooms.RemoveWatcher(watcher);
                watcher.Dispose();
            });

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
