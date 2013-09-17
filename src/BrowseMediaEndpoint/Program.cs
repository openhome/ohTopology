using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenHome;
using OpenHome.Net.Core;

namespace BrowseMediaEndpoint
{
    class Program
    {
        static void Main(string[] args)
        {
            var initParams = new OpenHome.Net.Core.InitParams();
            var library = Library.Create(initParams);

            var adapters = new List<NetworkAdapter>();

            using (var subnets = new SubnetList())
            {
                var count = subnets.Size();

                if (count == 0)
                {
                    Console.WriteLine("No network adapter");
                    return;
                }

                var main = new Main(library, subnets.SubnetAt(0));

                main.Run();
            }
        }
    }
}
