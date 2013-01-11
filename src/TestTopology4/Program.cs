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

        class SourceWatcher : IWatcher<IEnumerable<ITopology4Source>>, IDisposable
        {
            public SourceWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                //iStringLookup = new Dictionary<string, string>();
            }

            public void Dispose()
            {
                //iStringLookup = null;
            }            

            public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
            {
                foreach (ITopology4Source s in aValue)
                {
                    Console.WriteLine(string.Format("Added: {0}: Name={1}, Type={2}, Visible={3}", s.Index, s.Name, s.Type, s.Visible));
                }
            }

            public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
            {
                foreach (ITopology4Source s in aValue)
                {
                    Console.WriteLine(string.Format("Updated: {0}: Name={1}, Type={2}, Visible={3}", s.Index, s.Name, s.Type, s.Visible));
                }
            }

            public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
            {
                foreach (ITopology4Source s in aValue)
                {
                    Console.WriteLine(string.Format("Removed: {0}: Name={1}, Type={2}, Visible={3}", s.Index, s.Name, s.Type, s.Visible));
                }
            }

            private MockableScriptRunner iRunner;
        }

        class RoomWatcher : IUnorderedWatcher<ITopology4Room>, IDisposable
        {
            public RoomWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                iWatcher = new SourceWatcher(aRunner);

                iList = new List<ITopology3Room>();
            }

            public void Dispose()
            {
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

            public void UnorderedAdd(ITopology4Room aItem)
            {
                aItem.Sources.AddWatcher(iWatcher);

                Console.WriteLine("Room Added " + aItem.Name);
            }

            public void UnorderedRemove(ITopology4Room aItem)
            {
                aItem.Sources.RemoveWatcher(iWatcher);

                Console.WriteLine("Room Removed " + aItem.Name);
            }

            private MockableScriptRunner iRunner;
            private SourceWatcher iWatcher;
            private List<ITopology3Room> iList;
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
            thread.WaitComplete();
            thread.WaitComplete();

            try
            {
                //runner.Run(thread, new StringReader(File.ReadAllText(args[0])), mocker);
                runner.Run(thread, Console.In, mocker);
                //MockableStream stream = new MockableStream(Console.In, mocker);
                //stream.Start();
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
