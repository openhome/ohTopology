using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenHome;
using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Net.Core;

namespace StressMediaEndpoint
{
    class Program
    {
        static NetworkAdapter FindDefaultAdapter(string aToken)
        {
            using (var subnets = new SubnetList())
            {
                var count = subnets.Size();

                if (count == 0)
                {
                    return (null);
                }

                var adapter = subnets.SubnetAt(0);

                adapter.AddRef(aToken);

                return (adapter);
            }
        }

        static void ReportException(Exception aException)
        {
            Console.WriteLine(aException.Message);
            Console.WriteLine(aException.StackTrace);
        }

        static void Main(string[] args)
        {
            const string kUdn = "bf4f32c1e83f7e931c67cfa3ecda7d3f";

            var initParams = new OpenHome.Net.Core.InitParams();

            var library = Library.Create(initParams);

            var adapter = FindDefaultAdapter("main");

            using (var wt = new WatchableThread(ReportException))
            {
                using (var discovery = new Discovery(library, adapter, wt, kUdn))
                {
                    Console.WriteLine("Finding device: {0}", kUdn);

                    if (discovery.Find())
                    {
                        using (var tests = new Tests(wt, discovery.Network, discovery.MediaEndpoint))
                        {
                            Do.AssertThrowsNothing(() =>
                            {
                                tests.Run();
                            });
                        }
                    }
                }
            }

            adapter.RemoveRef("main");

            library.Dispose();
        }
    }
}
