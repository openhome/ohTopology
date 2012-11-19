using System;

namespace OpenHome.Net.ControlPoint
{
    public interface IWatchableServiceOpenHomeOrgProduct1 : IDisposable
    {
    }

    public class WatchableServiceOpenHomeOrgProduct1 : IWatchableServiceOpenHomeOrgProduct1
    {
        public WatchableServiceOpenHomeOrgProduct1(CpDevice aDevice)
        {
        }

        public void Dispose()
        {
        }
    }

    public class MoqServiceOpenHomeOrgProduct1 : IWatchableServiceOpenHomeOrgProduct1
    {
        public void Dispose()
        {
        }
    }
}
