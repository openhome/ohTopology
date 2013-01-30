using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;

namespace TestTopology2
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

        class SourceWatcher : IWatcher<ITopology2Source>, IDisposable
        {
            public SourceWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
            }

            public void Dispose()
            {
            }

            public void ItemOpen(string aId, ITopology2Source aValue)
            {
                iRunner.Result(string.Format("{0}. {1} {2} {3}", aValue.Index, aValue.Name, aValue.Type, aValue.Visible));
            }

            public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
            {
                iRunner.Result(string.Format("{0}. {1} {2} {3} -> {4} {5} {6}", aValue.Index, aPrevious.Name, aPrevious.Type, aPrevious.Visible, aValue.Name, aValue.Type, aValue.Visible));
            }

            public void ItemClose(string aId, ITopology2Source aValue)
            {
            }

            private MockableScriptRunner iRunner;
        }

        class GroupWatcher : IUnorderedWatcher<ITopology2Group>, IWatcher<string>, IWatcher<uint>, IWatcher<bool>, IDisposable
        {
            public GroupWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                iWatcher = new SourceWatcher(aRunner);

                iStringLookup = new Dictionary<string, string>();
                iList = new List<ITopology2Group>();
            }

            public void Dispose()
            {
                foreach (ITopology2Group g in iList)
                {
                    foreach (IWatchable<ITopology2Source> s in g.Sources)
                    {
                        s.RemoveWatcher(iWatcher);
                    }

                    g.Room.RemoveWatcher(this);
                    g.Name.RemoveWatcher(this);
                }
                iList = null;

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
                aItem.SourceIndex.AddWatcher(this);
                aItem.Standby.AddWatcher(this);
                iList.Add(aItem);

                iRunner.Result(string.Format("Group Added\t\t{0}:{1}", iStringLookup[string.Format("Room({0})", aItem.Id)], iStringLookup[string.Format("Name({0})", aItem.Id)]));
                iRunner.Result("===============================================");

                foreach (IWatchable<ITopology2Source> s in aItem.Sources)
                {
                    s.AddWatcher(iWatcher);
                }

                iRunner.Result("===============================================");
            }

            public void UnorderedRemove(ITopology2Group aItem)
            {
                aItem.Room.RemoveWatcher(this);
                aItem.Name.RemoveWatcher(this);
                aItem.SourceIndex.RemoveWatcher(this);
                aItem.Standby.RemoveWatcher(this);
                iList.Remove(aItem);

                foreach (IWatchable<ITopology2Source> s in aItem.Sources)
                {
                    s.RemoveWatcher(iWatcher);
                }

                iRunner.Result(string.Format("Group Removed\t\t{0}:{1}", iStringLookup[string.Format("Room({0})", aItem.Id)], iStringLookup[string.Format("Name({0})", aItem.Id)]));
                iStringLookup.Remove(string.Format("Room({0})", aItem.Id));
                iStringLookup.Remove(string.Format("Name({0})", aItem.Id));
            }

            public void ItemOpen(string aId, string aValue)
            {
                iStringLookup.Add(aId, aValue);
            }

            public void ItemUpdate(string aId, string aValue, string aPrevious)
            {
                iStringLookup[aId] = aValue;

                iRunner.Result(string.Format("{0} changed from {1} to {2}", aId, aPrevious, aValue));
            }

            public void ItemClose(string aId, string aValue)
            {
                // key pair removed in UnorderedRemove
            }

            public void ItemOpen(string aId, uint aValue)
            {
            }

            public void ItemUpdate(string aId, uint aValue, uint aPrevious)
            {
                iRunner.Result(string.Format("{0} changed from {1} to {2}", aId, aPrevious, aValue));
            }

            public void ItemClose(string aId, uint aValue)
            {
            }

            public void ItemOpen(string aId, bool aValue)
            {
            }

            public void ItemUpdate(string aId, bool aValue, bool aPrevious)
            {
                iRunner.Result(string.Format("{0} changed from {1} to {2}", aId, aPrevious, aValue));
            }

            public void ItemClose(string aId, bool aValue)
            {
            }

            private MockableScriptRunner iRunner;

            private SourceWatcher iWatcher;
            private List<ITopology2Group> iList;
            private Dictionary<string, string> iStringLookup;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestTopology2.exe <testscript>");
                return 1;
            }

            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread thread = new WatchableThread(reporter);

            Mockable mocker = new Mockable();

            MockNetwork network = new FourDsMockNetwork(thread, mocker);
            mocker.Add("network", network);

            Topology1 topology1 = new Topology1(thread, network);
            Topology2 topology2 = new Topology2(thread, topology1);

            MockableScriptRunner runner = new MockableScriptRunner();

            GroupWatcher watcher = new GroupWatcher(runner);
            topology2.Groups.AddWatcher(watcher);

            thread.WaitComplete();

            try
            {
            	runner.Run(thread, new StreamReader(args[0]), mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            topology2.Groups.RemoveWatcher(watcher);
            watcher.Dispose();

            topology2.Dispose();

            topology1.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
