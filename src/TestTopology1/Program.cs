using System;

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
                Console.WriteLine("Collection Add at " + aIndex);
            }

            public void CollectionClose()
            {
                Console.WriteLine("Collection Close");
            }

            public void CollectionInitialised()
            {
                Console.WriteLine("Collection Initialised");
            }

            public void CollectionMove(IWatchableDevice aItem, uint aFrom, uint aTo)
            {
                throw new NotImplementedException();
            }

            public void CollectionOpen()
            {
                Console.WriteLine("Collection Open");
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
            Topology1 topology = new Topology1(thread);

            DeviceWatcher watcher = new DeviceWatcher();
            topology.Devices.AddWatcher(watcher);

            System.Threading.Thread.Sleep(10000);

            topology.Devices.RemoveWatcher(watcher);
            
            topology.Dispose();

            thread.Dispose();

            library.Dispose();
        }
    }
}
