using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class MockWatchableMediaServer : MockWatchableDevice
    {
        public MockWatchableMediaServer(IWatchableThread aThread, string aUdn)
            : base(aThread, aUdn)
        {
            // content directory service
            //MockWatchableContentDirectory contentDirectory = new MockWatchableContentDirectory(aThread, aUdn, 0, "");
            //Add<ContentDirectory>(contentDirectory);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            base.Execute(aValue);

            Type key = typeof(Product);

            /*
            string command = aValue.First().ToLowerInvariant();

            if (command == "contentdirectory")
            {
                foreach (KeyValuePair<Type, IWatchableService> s in iServices)
                {
                    if (s.Key == key)
                    {
                        MockWatchableContentDirectory p = s.Value as MockWatchableContentDirectory;
                        p.Execute(aValue.Skip(1));
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
            */

            throw new NotSupportedException();
        }
    }
}
