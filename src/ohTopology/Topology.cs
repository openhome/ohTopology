using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    // Interface for objects that are passed through the topology layers. Implements the
    // IDisposable interface - the Dispose() method should dispose of all watchables created
    // for this object. At the point that this method is called, the upper layers should
    // have been notified that this object is about to be removed.
    public interface ITopologyObject : IDisposable
    {
        // This method is intended to detach the object from its lower layer dependencies
        // which will primarily involve removing watchers from the lower layer's watchables
        void Detach();
    }

    // This class should be used to proxy a watchable in a lower layer up to a higher layer.
    // For example, if a L2 object has a Watchable that is not directly required in L3 but
    // needs to be passed up to L4, the a WatchableProxy should be used in L3 to achieve this,
    // rather than making the L2 Watchable object accessible to L4
    public class WatchableProxy<T> : IWatchable<T>, IWatcher<T>, IDisposable
    {
        public WatchableProxy(IWatchable<T> aWatchable)
        {
            iWatchable = aWatchable;
            aWatchable.AddWatcher(this);
        }

        public void Detach()
        {
            // detach this proxy from its wrapped IWatchable<T>
            iWatchable.RemoveWatcher(this);
        }

        public void Dispose()
        {
            if (iProxy == null)
            {
                throw new ObjectDisposedException("WatchableProxy<T>.Dispose");
            }

            iProxy.Dispose();
            iProxy = null;
        }

        // IWatchable<T>

        public string Id
        {
            get { return iProxy.Id; }
        }

        public T Value
        {
            get { return iProxy.Value; }
        }

        public void AddWatcher(IWatcher<T> aWatcher)
        {
            iProxy.AddWatcher(aWatcher);
        }

        public void RemoveWatcher(IWatcher<T> aWatcher)
        {
            iProxy.RemoveWatcher(aWatcher);
        }

        // IWatcher<T>

        public void ItemOpen(string aId, T aValue)
        {
            iProxy = new Watchable<T>(iWatchable.WatchableThread, iWatchable.Id + "(Proxy)", aValue);
        }

        public void ItemUpdate(string aId, T aValue, T aPrevious)
        {
            iProxy.Update(aValue);
        }

        public void ItemClose(string aId, T aValue)
        {
        }

        public IWatchableThread WatchableThread
        {
            get
            {
                return iProxy.WatchableThread;
            }
        }

        private IWatchable<T> iWatchable;
        private Watchable<T> iProxy;
    }

    public enum EStandby
    {
        eOn,
        eMixed,
        eOff
    }
}
