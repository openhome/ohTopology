using System;
using System.IO;
using System.Collections.Generic;

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

        class RootWatcher : IWatcher<IEnumerable<ITopology4Group>>, IWatcher<ITopology4Source>, IDisposable
        {
            public RootWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
            }

            public void Dispose()
            {
            }

            public void ItemOpen(string aId, IEnumerable<ITopology4Group> aValue)
            {
                foreach (ITopology4Group g in aValue)
                {
                    g.Source.AddWatcher(this);
                }
            }

            public void ItemUpdate(string aId, IEnumerable<ITopology4Group> aValue, IEnumerable<ITopology4Group> aPrevious)
            {
                List<ITopology4Group> removed = new List<ITopology4Group>();
                foreach (ITopology4Group g1 in aPrevious)
                {
                    bool found = false;
                    foreach (ITopology4Group g2 in aValue)
                    {
                        if (g1 == g2)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        removed.Add(g1);
                    }
                }

                List<ITopology4Group> added = new List<ITopology4Group>();
                foreach (ITopology4Group g1 in aValue)
                {
                    bool found = false;
                    foreach (ITopology4Group g2 in aPrevious)
                    {
                        if (g1 == g2)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        added.Add(g1);
                    }
                }

                foreach (ITopology4Group g in removed)
                {
                    g.Source.RemoveWatcher(this);
                }

                foreach (ITopology4Group g in added)
                {
                    g.Source.AddWatcher(this);
                }
            }

            public void ItemClose(string aId, IEnumerable<ITopology4Group> aValue)
            {
                foreach (ITopology4Group g in aValue)
                {
                    g.Source.RemoveWatcher(this);
                }
            }

            public void ItemOpen(string aId, ITopology4Source aValue)
            {
                iRunner.Result(string.Format("Current: {0}: Group={1}, Name={2}, Type={3}, Visible={4}", aValue.Index, aValue.Group, aValue.Name, aValue.Type, aValue.Visible));
            }

            public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
            {
                iRunner.Result(string.Format("Current Updated: {0}: Group={1}, Name={2}, Type={3}, Visible={4}", aValue.Index, aValue.Group, aValue.Name, aValue.Type, aValue.Visible));
            }

            public void ItemClose(string aId, ITopology4Source aValue)
            {
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
                    iRunner.Result(string.Format("Added: {0}: Group={1}, Name={2}, Type={3}, Visible={4}", s.Index, s.Group, s.Name, s.Type, s.Visible));
                }
            }

            public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
            {
                foreach (ITopology4Source s in aValue)
                {
                    iRunner.Result(string.Format("Updated: {0}: Group={1}, Name={2}, Type={3}, Visible={4}", s.Index, s.Group, s.Name, s.Type, s.Visible));
                }
            }

            public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
            {
                foreach (ITopology4Source s in aValue)
                {
                    iRunner.Result(string.Format("Removed: {0}: Group={1}, Name={2}, Type={3}, Visible={4}", s.Index, s.Group, s.Name, s.Type, s.Visible));
                }
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

            Mockable mocker = new Mockable();

            MockNetwork network = new FourDsMockNetwork(thread, mocker);
            mocker.Add("network", network);

            Topology1 topology1 = new Topology1(thread, network);
            Topology2 topology2 = new Topology2(thread, topology1);
            Topology3 topology3 = new Topology3(thread, topology2);
            Topology4 topology4 = new Topology4(thread, topology3);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(runner);
            topology4.Rooms.AddWatcher(watcher);

            thread.WaitComplete();

            try
            {
                runner.Run(thread, new StringReader(File.ReadAllText(args[0])), mocker);
                //runner.Run(thread, Console.In, mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            topology4.Rooms.RemoveWatcher(watcher);
            watcher.Dispose();

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
