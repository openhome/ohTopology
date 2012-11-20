using System;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public class Topology2 : ICollectionWatcher<IWatchableDevice>, IDisposable
    {
        public Topology2(WatchableThread aThread, ITopology1 aTopology1)
        {
            iDisposed = false;
            iLock = new object();

            iThread = aThread;
            iTopology1 = aTopology1;

            iTopology1.Devices.AddWatcher(this);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2.Dispose");
                }

                iTopology1.Dispose();

                iDisposed = true;
            }
        }

        public void CollectionAdd(IWatchableDevice aItem, uint aIndex)
        {
            throw new NotImplementedException();
        }

        public void CollectionClose()
        {
            throw new NotImplementedException();
        }

        public void CollectionInitialised()
        {
            throw new NotImplementedException();
        }

        public void CollectionMove(IWatchableDevice aItem, uint aFrom, uint aTo)
        {
            throw new NotSupportedException();
        }

        public void CollectionOpen()
        {
            throw new NotImplementedException();
        }

        public void CollectionRemove(IWatchableDevice aItem, uint aIndex)
        {
            throw new NotImplementedException();
        }

        private object iLock;
        private bool iDisposed;

        private WatchableThread iThread;
        private ITopology1 iTopology1;
    }
}
