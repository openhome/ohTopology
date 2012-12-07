using System;
using System.IO;

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

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestTopology3.exe <testscript>");
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

            thread.WaitComplete();
            thread.WaitComplete();
            thread.WaitComplete();

            try
            {
                runner.Run(thread, new StringReader(File.ReadAllText(args[0])), mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            topology3.Dispose();

            topology2.Dispose();

            topology1.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
