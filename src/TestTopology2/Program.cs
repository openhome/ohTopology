using System;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Av;
using OpenHome.Net.Core;

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
            public SourceWatcher()
            {
            }

            public void Dispose()
            {
            }

            public void ItemOpen(string aId, ITopology2Source aValue)
            {
                Console.WriteLine(string.Format("{0}. {1} {2} {3}", aId, aValue.Name, aValue.Type, aValue.Visible));
            }

            public void ItemClose(string aId, ITopology2Source aValue)
            {
            }

            public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
            {
                Console.WriteLine(string.Format("{0}. {1} {2} {3} -> {4} {5} {6}", aId, aPrevious.Name, aPrevious.Type, aPrevious.Visible, aValue.Name, aValue.Type, aValue.Visible));
                Console.WriteLine("");
            }
        }

        class GroupWatcher : ICollectionWatcher<ITopology2Group>, IWatcher<string>, IWatcher<uint>, IWatcher<bool>, IDisposable
        {
            public GroupWatcher()
            {
                iLock = new object();
                iDisposed = false;

                iWatcher = new SourceWatcher();

                iStringLookup = new Dictionary<string, string>();
                iList = new List<ITopology2Group>();
            }

            public void Dispose()
            {
                lock (iLock)
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

                    iDisposed = true;
                }
            }

            public void CollectionOpen()
            {
            }

            public void CollectionClose()
            {
            }

            public void CollectionInitialised()
            {
            }

            public void CollectionAdd(ITopology2Group aItem, uint aIndex)
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        return;
                    }

                    aItem.Room.AddWatcher(this);
                    aItem.Name.AddWatcher(this);
                    aItem.SourceIndex.AddWatcher(this);
                    aItem.Standby.AddWatcher(this);
                    iList.Add(aItem);

                    Console.WriteLine(string.Format("Group Added\t\t{0}:{1}", iStringLookup[string.Format("Room({0})", aItem.Id)], iStringLookup[string.Format("Name({0})", aItem.Id)]));
                    Console.WriteLine("===============================================");

                    foreach (IWatchable<ITopology2Source> s in aItem.Sources)
                    {
                        s.AddWatcher(iWatcher);
                    }

                    Console.WriteLine("===============================================");
                    Console.WriteLine("");
                    Console.WriteLine("");
                }
            }

            public void CollectionMove(ITopology2Group aItem, uint aFrom, uint aTo)
            {
                throw new NotImplementedException();
            }

            public void CollectionRemove(ITopology2Group aItem, uint aIndex)
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        return;
                    }

                    aItem.Room.RemoveWatcher(this);
                    aItem.Name.RemoveWatcher(this);
                    aItem.SourceIndex.RemoveWatcher(this);
                    aItem.Standby.RemoveWatcher(this);
                    iList.Remove(aItem);

                    Console.WriteLine(string.Format("Group Removed\t\t{0}:{1}", iStringLookup[string.Format("Room({0})", aItem.Id)], iStringLookup[string.Format("Name({0})", aItem.Id)]));
                    iStringLookup.Remove(string.Format("Room({0})", aItem.Id));
                    iStringLookup.Remove(string.Format("Name({0})", aItem.Id));
                }
            }

            public void ItemOpen(string aId, string aValue)
            {
                iStringLookup.Add(aId, aValue);
            }

            public void ItemClose(string aId, string aValue)
            {
                // key pair removed in CollectionRemove
            }

            public void ItemUpdate(string aId, string aValue, string aPrevious)
            {
                iStringLookup[aId] = aValue;

                Console.WriteLine(string.Format("{0} changed from {1} to {2}", aId, aPrevious, aValue));
            }

            public void ItemOpen(string aId, uint aValue)
            {
            }

            public void ItemClose(string aId, uint aValue)
            {
            }

            public void ItemUpdate(string aId, uint aValue, uint aPrevious)
            {
                Console.WriteLine(string.Format("{0} changed from {1} to {2}", aId, aPrevious, aValue));
            }

            public void ItemOpen(string aId, bool aValue)
            {
            }

            public void ItemClose(string aId, bool aValue)
            {
            }

            public void ItemUpdate(string aId, bool aValue, bool aPrevious)
            {
                Console.WriteLine(string.Format("{0} changed from {1} to {2}", aId, aPrevious, aValue));
            }

            private object iLock;
            private bool iDisposed;

            private SourceWatcher iWatcher;
            private List<ITopology2Group> iList;
            private Dictionary<string, string> iStringLookup;
        }

        static int Main(string[] args)
        {
            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread thread = new WatchableThread(reporter);

            Mockable mocker = new Mockable();

            MockNetwork network = new FourDsMockNetwork(thread, mocker);
            mocker.Add("network", network);

            //Network network = new Network(thread);

            Topology1 topology1 = new Topology1(thread, network);
            Topology2 topology2 = new Topology2(thread, topology1);

            GroupWatcher watcher = new GroupWatcher();
            topology2.Groups.AddWatcher(watcher);

            MockableStream stream = new MockableStream(Console.In, mocker);
            stream.Start();

            topology2.Groups.RemoveWatcher(watcher);

            topology2.Dispose();

            topology1.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
