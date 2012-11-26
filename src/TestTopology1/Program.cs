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

        class DeviceWatcher : ICollectionWatcher<IWatchableDevice>
        {

            public void CollectionAdd(IWatchableDevice aItem, uint aIndex)
            {
                Console.WriteLine("Product Added");
                Console.WriteLine("    udn = " + aItem.Udn);
            }

            public void CollectionClose()
            {
            }

            public void CollectionInitialised()
            {
            }

            public void CollectionMove(IWatchableDevice aItem, uint aFrom, uint aTo)
            {
                throw new NotImplementedException();
            }

            public void CollectionOpen()
            {
            }

            public void CollectionRemove(IWatchableDevice aItem, uint aIndex)
            {
                Console.WriteLine("Collection Remove at " + aIndex);
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
            //MockTopology1 topology = new MockTopology1(thread);
            Topology1 topology = new Topology1(thread);

            DeviceWatcher watcher = new DeviceWatcher();
            topology.Devices.AddWatcher(watcher);

            Mockable mocker = new Mockable();
            //mocker.Add("topology", topology);

            MockableStream stream = new MockableStream(Console.In, mocker);
            stream.Start();
            
            topology.Devices.RemoveWatcher(watcher);
            
            topology.Dispose();

            thread.Dispose();

            library.Dispose();
        }
    }
}
