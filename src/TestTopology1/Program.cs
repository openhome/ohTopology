using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;

namespace TestTopology
{
    class Program
    {
        class ProductWatcher : IUnorderedWatcher<IProxyProduct>
        {
            public ProductWatcher(MockableScriptRunner aRunner)
            {
                iRunner = aRunner;
            }

            public void UnorderedOpen()
            {
            }

            public void UnorderedInitialised()
            {
            }

            public void UnorderedAdd(IProxyProduct aItem)
            {
                iRunner.Result("product added " + aItem.Device.Udn);
            }

            public void UnorderedRemove(IProxyProduct aItem)
            {
                iRunner.Result("product removed " + aItem.Device.Udn);
            }

            public void UnorderedClose()
            {
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

            Mockable mocker = new Mockable();

            Log log = new Log(new LogConsole());

            Network network = new Network(50, log);
            DeviceInjectorMock mockInjector = new DeviceInjectorMock(network, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
            mocker.Add("network", mockInjector);

            Topology1 topology = new Topology1(network, log);

            MockableScriptRunner runner = new MockableScriptRunner();

            ProductWatcher watcher = new ProductWatcher(runner);
            
            network.Schedule(() =>
            {
                topology.Products.AddWatcher(watcher);
            });

            try
            {
                runner.Run(network.Wait, new StreamReader(args[0]), mocker);
            }
            catch(MockableScriptRunner.AssertError)
            {
                return 1;
            }

            network.Execute(() =>
            {
                topology.Products.RemoveWatcher(watcher);
            });

            topology.Dispose();

            mockInjector.Dispose();

            network.Dispose();

            return 0;
        }
    }
}
