using System;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Av;
using OpenHome.Net.Core;

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

        class ProductWatcher : ICollectionWatcher<Product>
        {

            public void CollectionAdd(Product aItem, uint aIndex)
            {
                Console.WriteLine("Product Added");
                Console.WriteLine("    udn = " + aItem.Id);
            }

            public void CollectionClose()
            {
            }

            public void CollectionInitialised()
            {
            }

            public void CollectionMove(Product aItem, uint aFrom, uint aTo)
            {
                throw new NotImplementedException();
            }

            public void CollectionOpen()
            {
            }

            public void CollectionRemove(Product aItem, uint aIndex)
            {
                Console.WriteLine("Product Removed");
                Console.WriteLine("    udn = " + aItem.Id);
            }
        }

        static void Main(string[] args)
        {
            InitParams initParams = new InitParams();
            Library library = Library.Create(initParams);

            SubnetList subnets = new SubnetList();
            library.StartCp(subnets.SubnetAt(0).Subnet());
            subnets.Dispose();

            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread thread = new  WatchableThread(reporter);

            Mockable mocker = new Mockable();

            /*MockNetwork network = new MockNetwork(thread, mocker);

            mocker.Add("network", network);

            MockWatchableDs ds = new MockWatchableDs(thread, "45");

            network.AddDevice(ds);*/

            MockNetwork network = new FourDsMockNetwork(thread, mocker);

            //Network network = new Network(thread);

            Topology1 topology = new Topology1(thread, network);

            ProductWatcher watcher = new ProductWatcher();

            topology.Products.AddWatcher(watcher);

            MockableStream stream = new MockableStream(Console.In, mocker);
            stream.Start();

            topology.Products.RemoveWatcher(watcher);

            topology.Dispose();

            network.Dispose();

            thread.Dispose();

            library.Dispose();
        }
    }
}
