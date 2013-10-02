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

        static void Main(string[] args)
        {
            var initParams = new OpenHome.Net.Core.InitParams();
            
            var library = Library.Create(initParams);

            var adapter = FindDefaultAdapter("main");

            using (var main = new Main(library, adapter))
            {
                main.Run();
            }

            adapter.RemoveRef("main");

            library.Dispose();
        }
    }
}
