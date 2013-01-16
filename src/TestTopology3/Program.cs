using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Av;
using OpenHome.Os.App;

namespace TestTopology3
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

        class GroupWatcher : IUnorderedWatcher<ITopology2Group>, IWatcher<string>, IDisposable
        {
            public GroupWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                iStringLookup = new Dictionary<string, string>();
            }

            public void Dispose()
            {
                iStringLookup = null;
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

            public void UnorderedAdd(ITopology2Group aItem)
            {
                aItem.Room.AddWatcher(this);
                aItem.Name.AddWatcher(this);

                iRunner.Result(string.Format("Group Added {0}:{1}", iStringLookup[string.Format("Room({0})", aItem.Id)], iStringLookup[string.Format("Name({0})", aItem.Id)]));
            }

            public void UnorderedRemove(ITopology2Group aItem)
            {
                aItem.Room.RemoveWatcher(this);
                aItem.Name.RemoveWatcher(this);

                iRunner.Result(string.Format("Group Removed {0}:{1}", iStringLookup[string.Format("Room({0})", aItem.Id)], iStringLookup[string.Format("Name({0})", aItem.Id)]));

                iStringLookup.Remove(string.Format("Room({0})", aItem.Id));
                iStringLookup.Remove(string.Format("Name({0})", aItem.Id));
            }

            public void ItemOpen(string aId, string aValue)
            {
                iStringLookup.Add(aId, aValue);
            }

            public void ItemUpdate(string aId, string aValue, string aPrevious)
            {
                // ignore name changes
            }

            public void ItemClose(string aId, string aValue)
            {
                // key pair removed in UnorderedRemove
            }

            private MockableScriptRunner iRunner;
            private Dictionary<string, string> iStringLookup;
        }

        class RoomWatcher : IUnorderedWatcher<ITopology3Room>, IDisposable
        {
            public RoomWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                iWatcher = new GroupWatcher(aRunner);

                iList = new List<ITopology3Room>();
            }

            public void Dispose()
            {
                foreach (ITopology3Room r in iList)
                {
                    r.Groups.RemoveWatcher(iWatcher);
                }

                iWatcher.Dispose();
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

            public void UnorderedAdd(ITopology3Room aItem)
            {
                iRunner.Result("Room Added " + aItem.Name);

                iList.Add(aItem);
                aItem.AddRef();
                aItem.Groups.AddWatcher(iWatcher);
            }

            public void UnorderedRemove(ITopology3Room aItem)
            {
                iRunner.Result("Room Removed " + aItem.Name);

                iList.Remove(aItem);
                aItem.Groups.RemoveWatcher(iWatcher);
                aItem.RemoveRef();
            }

            private MockableScriptRunner iRunner;
            private GroupWatcher iWatcher;
            private List<ITopology3Room> iList;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestTopology3.exe <testscript>");
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

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(runner);
            topology3.Rooms.AddWatcher(watcher);

            thread.WaitComplete();
            thread.WaitComplete();
            thread.WaitComplete();

            try
            {
                runner.Run(thread, new StreamReader(args[0]), mocker);
                //runner.Run(thread, Console.In, mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            topology3.Rooms.RemoveWatcher(watcher);
            watcher.Dispose();

            topology3.Dispose();

            topology2.Dispose();

            topology1.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
