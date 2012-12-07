using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Av;

namespace TestTopology
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

        class ProductWatcher : IUnorderedWatcher<Product>
        {
            public ProductWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
            }

            public void UnorderedOpen()
            {
            }

            public void UnorderedClose()
            {
            }

            public void UnorderedInitialised()
            {
            }

            public void UnorderedAdd(Product aItem)
            {
                iRunner.Result("Product Added");
                iRunner.Result("    udn = " + aItem.Id);
            }

            public void UnorderedRemove(Product aItem)
            {
                iRunner.Result("Product Removed");
                iRunner.Result("    udn = " + aItem.Id);
            }

            private MockableScriptRunner iRunner;
        }

        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestTopology1.exe <testscript>");
                return 1;
            }

            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread thread = new WatchableThread(reporter);

            Mockable mocker = new Mockable();

            MockNetwork network = new FourDsMockNetwork(thread, mocker);
            mocker.Add("network", network);

            Topology1 topology = new Topology1(thread, network);

            thread.WaitComplete();

            MockableScriptRunner runner = new MockableScriptRunner();

            ProductWatcher watcher = new ProductWatcher(runner);
            topology.Products.AddWatcher(watcher);

            try
            {
                runner.Run(thread, new StreamReader(args[0]), mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            topology.Products.RemoveWatcher(watcher);

            topology.Dispose();

            network.Dispose();

            thread.Dispose();

            return 0;
        }
    }
}
