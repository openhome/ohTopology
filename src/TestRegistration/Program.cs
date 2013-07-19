using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os;
using OpenHome.Av;
using OpenHome.Os.App;

namespace TestRegistration
{
    class RoomWatcher : IOrderedWatcher<IStandardRoom>, IDisposable
    {
        public RoomWatcher(ITagManager aTagManager, MockableScriptRunner aRunner)
        {
            iTagManager = aTagManager;
            iRunner = aRunner;
            iFactory = new ResultWatcherFactory(aRunner);
        }

        public void Dispose()
        {
            iFactory.Dispose();
        }

        public void OrderedOpen()
        {
        }

        public void OrderedInitialised()
        {
        }

        public void OrderedClose()
        {
        }

        public void OrderedAdd(IStandardRoom aItem, uint aIndex)
        {
            iRunner.Result(string.Format("Room Added: {0} at {1}", aItem.Name, aIndex));

            iFactory.Create<IEnumerable<ITopology4Registration>>(aItem.Name, aItem.Registrations, (v) =>
            {
                string info = "\nRegistrations begin\n";
                foreach (ITopology4Registration r in v)
                {
                    info += r.ProductId + "\n";
                }
                info += "Registrations end";
                return info;
            });
        }

        public void OrderedMove(IStandardRoom aItem, uint aFrom, uint aTo)
        {
            iRunner.Result(string.Format("Room Moved: {0} from {1} to {2}", aItem.Name, aFrom, aTo));
        }

        public void OrderedRemove(IStandardRoom aItem, uint aIndex)
        {
            iRunner.Result(string.Format("Room Removed: {0} at {1}", aItem.Name, aIndex));
            iFactory.Destroy(aItem.Name);
        }

        private ITagManager iTagManager;
        private MockableScriptRunner iRunner;
        private ResultWatcherFactory iFactory;
    }

    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: TestRegistration.exe <testscript>");
                return 1;
            }

            Mockable mocker = new Mockable();

            Network network = new Network(50);
            DeviceInjectorMock mockInjector = new DeviceInjectorMock(network, Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
            mocker.Add("network", mockInjector);

            StandardHouse house = new StandardHouse(network);
            mocker.Add("house", house);

            MockableScriptRunner runner = new MockableScriptRunner();

            RoomWatcher watcher = new RoomWatcher(network.TagManager, runner);

            network.Schedule(() =>
            {
                house.Rooms.AddWatcher(watcher);
            });

            try
            {
                runner.Run(network, new StringReader(File.ReadAllText(args[0])), mocker);
            }
            catch (MockableScriptRunner.AssertError)
            {
                return 1;
            }

            network.Execute(() =>
            {
                house.Rooms.RemoveWatcher(watcher);
                watcher.Dispose();
            });

            house.Dispose();

            mockInjector.Dispose();

            network.Dispose();

            return 0;
        }
    }
}
