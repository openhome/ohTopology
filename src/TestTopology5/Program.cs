using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Av;
using OpenHome.Os.App;

namespace TestTopology5
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

        class SourceWatcher : IUnorderedWatcher<ITopology4Source>, IDisposable
        {
            public SourceWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
            }

            public void Dispose()
            {
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

            public void UnorderedAdd(ITopology4Source aItem)
            {
                iRunner.Result(string.Format("Added: {0}: Group={1}, Name={2}, Type={3}, Visible={4}", aItem.Index, aItem.Group, aItem.Name, aItem.Type, aItem.Visible));
            }

            public void UnorderedRemove(ITopology4Source aItem)
            {
                iRunner.Result(string.Format("Removed: {0}: Group={1}, Name={2}, Type={3}, Visible={4}", aItem.Index, aItem.Group, aItem.Name, aItem.Type, aItem.Visible));
            }

            private MockableScriptRunner iRunner;
        }

        class RootWatcher : IUnorderedWatcher<ITopology4Group>, IDisposable
        {
            public RootWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
            }

            public void Dispose()
            {
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

            public void UnorderedAdd(ITopology4Group aItem)
            {
                iRunner.Result(string.Format("Root Added: Name={0}", aItem.Name));
            }

            public void UnorderedRemove(ITopology4Group aItem)
            {
                iRunner.Result(string.Format("Root Removed: Name={0}", aItem.Name));
            }

            private MockableScriptRunner iRunner;
        }

        class RoomWatcher : IUnorderedWatcher<ITopology5Room>, IWatcher<EStandby>, IDisposable
        {
            public RoomWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                iRootWatcher = new RootWatcher(aRunner);
                iSourceWatcher = new SourceWatcher(aRunner);

                iList = new List<ITopology5Room>();
            }

            public void Dispose()
            {
                foreach (ITopology5Room r in iList)
                {
                    r.Standby.RemoveWatcher(this);
                    r.Roots.RemoveWatcher(iRootWatcher);
                    r.Sources.RemoveWatcher(iSourceWatcher);
                }

                iSourceWatcher.Dispose();
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

            public void UnorderedAdd(ITopology5Room aItem)
            {
                iRunner.Result("Room Added " + aItem.Name);

                iList.Add(aItem);
                aItem.Standby.AddWatcher(this);
                aItem.Roots.AddWatcher(iRootWatcher);
                aItem.Sources.AddWatcher(iSourceWatcher);
            }

            public void UnorderedRemove(ITopology5Room aItem)
            {
                iRunner.Result("Room Removed " + aItem.Name);

                iList.Remove(aItem);
                aItem.Standby.RemoveWatcher(this);
                aItem.Roots.RemoveWatcher(iRootWatcher);
                aItem.Sources.RemoveWatcher(iSourceWatcher);
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
            private RootWatcher iRootWatcher;
            private SourceWatcher iSourceWatcher;
            private List<ITopology5Room> iList;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestTopology5.exe <testscript>");
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
            Topology5 topology5 = new Topology5(thread, topology4);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(runner);
            topology5.Rooms.AddWatcher(watcher);

            thread.WaitComplete();

            try
            {
                //runner.Run(thread, new StringReader(File.ReadAllText(args[0])), mocker);
                runner.Run(thread, Console.In, mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            topology5.Rooms.RemoveWatcher(watcher);
            watcher.Dispose();

            topology5.Dispose();

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
