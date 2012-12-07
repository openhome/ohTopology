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

        class GroupWatcher : IUnorderedWatcher<ITopology3Group>, IDisposable
        {
            public GroupWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;

                //iStringLookup = new Dictionary<string, string>();
                iList = new List<ITopology3Group>();
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

            public void UnorderedAdd(ITopology3Group aItem)
            {
                Console.WriteLine("Added: " + aItem);
            }

            public void UnorderedRemove(ITopology3Group aItem)
            {
                Console.WriteLine("Removed: " + aItem);
            }

            private MockableScriptRunner iRunner;
            private List<ITopology3Group> iList;
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

            GroupWatcher watcher = new GroupWatcher(runner);
            topology3.Groups.AddWatcher(watcher);

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

            topology3.Groups.RemoveWatcher(watcher);

            topology3.Dispose();

            topology2.Dispose();

            topology1.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
