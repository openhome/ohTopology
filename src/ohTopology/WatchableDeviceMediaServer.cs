using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public class MockWatchableMediaServer : MockWatchableDevice
    {
        public MockWatchableMediaServer(IWatchableThread aThread, IWatchableThread aSubscribeThread, string aUdn, string aAppRoot)
            : base(aSubscribeThread, aThread, aUdn)
        {
            Add<IProxyMediaServer>(new ServiceMediaServerMock(aThread, new string[] {"browse", "query"},
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "."));

            // content directory service
            //MockWatchableContentDirectory contentDirectory = new MockWatchableContentDirectory(aThread, aUdn, 0, "");
            //Add<ContentDirectory>(contentDirectory);
        }

        // IMockable

        public override void Execute(IEnumerable<string> aValue)
        {
        }
    }
}
