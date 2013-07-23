using System;
using System.IO;
using System.Collections.Generic;

using OpenHome.Os;
using OpenHome.Av;
using OpenHome.Os.App;

namespace TestRegistration
{
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

            ResultWatcherFactory factory = new ResultWatcherFactory(runner);

            network.Schedule(() =>
            {
                factory.Create<IEnumerable<ITopology4Registration>>("House", house.Registrations, (v) =>
                {
                    string info = "\nRegistrations begin\n";
                    foreach (ITopology4Registration r in v)
                    {
                        info += r.ManufacturerName + " " + r.ProductId + "\n";
                    }
                    info += "Registrations end";
                    return info;
                });
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
                factory.Dispose();
            });

            house.Dispose();

            mockInjector.Dispose();

            network.Dispose();

            return 0;
        }
    }
}
