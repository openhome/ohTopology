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
        class ExceptionReporter : IExceptionReporter
        {
            public void ReportException(Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }

        class RootWatcher : IWatcher<IEnumerable<ITopology4Root>>, IWatcher<ITopology4Source>, IDisposable
        {
            public RootWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
            }

            public void Dispose()
            {
            }

            public void ItemOpen(string aId, IEnumerable<ITopology4Root> aValue)
            {
                foreach (ITopology4Root g in aValue)
                {
                    g.Source.AddWatcher(this);
                }
            }

            public void ItemUpdate(string aId, IEnumerable<ITopology4Root> aValue, IEnumerable<ITopology4Root> aPrevious)
            {
                List<ITopology4Root> removed = new List<ITopology4Root>();
                foreach (ITopology4Root r1 in aPrevious)
                {
                    bool found = false;
                    foreach (ITopology4Root r2 in aValue)
                    {
                        if (r1 == r2)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        removed.Add(r1);
                    }
                }

                List<ITopology4Root> added = new List<ITopology4Root>();
                foreach (ITopology4Root r1 in aValue)
                {
                    bool found = false;
                    foreach (ITopology4Root r2 in aPrevious)
                    {
                        if (r1 == r2)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        added.Add(r1);
                    }
                }

                foreach (ITopology4Root r in removed)
                {
                    r.Source.RemoveWatcher(this);
                }

                foreach (ITopology4Root r in added)
                {
                    r.Source.AddWatcher(this);
                }
            }

            public void ItemClose(string aId, IEnumerable<ITopology4Root> aValue)
            {
                foreach (ITopology4Root r in aValue)
                {
                    r.Source.RemoveWatcher(this);
                }
            }

            public void ItemOpen(string aId, ITopology4Source aValue)
            {
                iRunner.Result(string.Format("Current: {0}", SourceInfo(aValue)));
            }

            public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
            {
                iRunner.Result(string.Format("Current Updated: {0}", SourceInfo(aValue)));
            }

            public void ItemClose(string aId, ITopology4Source aValue)
            {
            }

            private string SourceInfo(ITopology4Source aSource)
            {
                string info = string.Format("{0}: Group={1}, Name={2}, Type={3}, Visible={4}, HasInfo={5}, HasTime={6}, Device={7}, Volume=",
                                            aSource.Index, aSource.Group, aSource.Name, aSource.Type, aSource.Visible, aSource.HasInfo, aSource.HasTime, aSource.Device.Udn);

                foreach (IWatchableDevice d in aSource.VolumeDevices)
                {
                    info += d.Udn + " ";
                }

                return info;
            }

            private MockableScriptRunner iRunner;
        }

        class SourceWatcher : IWatcher<IEnumerable<ITopology4Source>>, IDisposable
        {
            public SourceWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
            }

            public void Dispose()
            {
            }            

            public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
            {
                foreach (ITopology4Source s in aValue)
                {
                    iRunner.Result(string.Format("Added: {0}", SourceInfo(s)));
                }
            }

            public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
            {
                foreach (ITopology4Source s in aValue)
                {
                    iRunner.Result(string.Format("Updated: {0}", SourceInfo(s)));
                }
            }

            public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
            {
                foreach (ITopology4Source s in aValue)
                {
                    iRunner.Result(string.Format("Removed: {0}", SourceInfo(s)));
                }
            }

            private string SourceInfo(ITopology4Source aSource)
            {
                string info = string.Format("{0}: Group={1}, Name={2}, Type={3}, Visible={4}, HasInfo={5}, HasTime={6}, Device={7}, Volume=",
                                            aSource.Index, aSource.Group, aSource.Name, aSource.Type, aSource.Visible, aSource.HasInfo, aSource.HasTime, aSource.Device.Udn);

                foreach (IWatchableDevice d in aSource.VolumeDevices)
                {
                    info += d.Udn + " ";
                }

                return info;
            }

            private MockableScriptRunner iRunner;

        }

        class RoomWatcher : IUnorderedWatcher<ITopology4Room>, IWatcher<EStandby>, IDisposable
        {
            public RoomWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                iSourceWatcher = new SourceWatcher(aRunner);
                iRootWatcher = new RootWatcher(aRunner);

                iList = new List<ITopology4Room>();
            }

            public void Dispose()
            {
                foreach (ITopology4Room r in iList)
                {
                    r.Standby.RemoveWatcher(this);
                    r.Sources.RemoveWatcher(iSourceWatcher);
                    r.Roots.RemoveWatcher(iRootWatcher);
                }

                iSourceWatcher.Dispose();
                iRootWatcher.Dispose();
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

                iList.Add(aItem);
                aItem.Standby.AddWatcher(this);
                aItem.Sources.AddWatcher(iSourceWatcher);
                aItem.Roots.AddWatcher(iRootWatcher);
            }

            public void UnorderedRemove(ITopology4Room aItem)
            {
                iRunner.Result("Room Removed " + aItem.Name);

                iList.Remove(aItem);
                aItem.Standby.RemoveWatcher(this);
                aItem.Sources.RemoveWatcher(iSourceWatcher);
                aItem.Roots.RemoveWatcher(iRootWatcher);
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

            private MockableScriptRunner iRunner;
            private SourceWatcher iSourceWatcher;
            private RootWatcher iRootWatcher;
            private List<ITopology4Room> iList;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestTopology4.exe <testscript>");
                return 1;
            }

            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread thread = new WatchableThread(reporter);
            WatchableThread subscribeThread = new WatchableThread(reporter);

            Mockable mocker = new Mockable();

            MockNetwork network = new FourDsMockNetwork(thread, subscribeThread, mocker);
            mocker.Add("network", network);

            Topology1 topology1 = new Topology1(thread, network);
            Topology2 topology2 = new Topology2(thread, topology1);
            Topology3 topology3 = new Topology3(thread, topology2);
            Topology4 topology4 = new Topology4(thread, topology3);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(runner);
            thread.Schedule(() =>
            {
                topology4.Rooms.AddWatcher(watcher);
            });

            network.Start();

            try
            {
                runner.Run(network, new StringReader(File.ReadAllText(args[0])), mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            thread.Execute(() =>
            {
                topology4.Rooms.RemoveWatcher(watcher);
                watcher.Dispose();
            });

            topology4.Dispose();

            topology3.Dispose();

            topology2.Dispose();

            topology1.Dispose();

            network.Stop();
            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
