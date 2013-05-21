using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public class MockWatchableMediaServer : WatchableDevice
    {
        public MockWatchableMediaServer(INetwork aNetwork, string aUdn, string aAppRoot)
            : base(aUdn)
        {
            Add<IProxyMediaServer>(new ServiceMediaServerMock(aNetwork, new string[] {"browse", "query"},
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "."));

            // content directory service
            //MockWatchableContentDirectory contentDirectory = new MockWatchableContentDirectory(aThread, aUdn, 0, "");
            //Add<ContentDirectory>(contentDirectory);
        }
    }
}
