using System;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class WatchableDeviceCollection : WatchableCollection<IWatchableDevice>
    {
        public WatchableDeviceCollection(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<IWatchableDevice>();
        }

        internal void Add(IWatchableDevice aValue)
        {
            uint index = (uint)iList.Count;
            iList.Add(aValue);

            CollectionAdd(aValue, index);
        }

        internal void Remove(IWatchableDevice aValue)
        {
            uint index = (uint)iList.IndexOf(aValue);
            iList.Remove(aValue);
             
            CollectionRemove(aValue, index);
        }

        private List<IWatchableDevice> iList;
    }

    public class WatchableProductCollection : WatchableCollection<Product>
    {
        public WatchableProductCollection(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<Product>();
        }

        public void Add(Product aValue)
        {
            uint index = (uint)iList.Count;
            CollectionAdd(aValue, index);
            iList.Add(aValue);
        }

        public void Remove(Product aValue)
        {
            uint index = (uint)iList.IndexOf(aValue);
            iList.Remove(aValue);

            CollectionRemove(aValue, index);
        }

        private List<Product> iList;
    }

    public interface ITopology1
    {
        IWatchableCollection<Product> Products { get; }
    }

    public class Topology1 : ITopology1, ICollectionWatcher<IWatchableDevice>, IDisposable
    {
        public Topology1(IWatchableThread aThread, INetwork aNetwork)
        {
            iDisposed = false;

            iNetwork = aNetwork;

            iPendingSubscriptions = new List<IWatchableDevice>();
            iProductLookup = new Dictionary<IWatchableDevice, Product>();
            iProducts = new WatchableProductCollection(aThread);

            iDevices = iNetwork.GetWatchableDeviceCollection<Product>();
            iDevices.AddWatcher(this);
        }

        public void Dispose()
        {
            iDevices.RemoveWatcher(this);

            lock (iProducts.WatchableThread)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology1.Dispose");
                }

                foreach (IWatchableDevice d in iPendingSubscriptions)
                {
                    d.Unsubscribe<Product>();
                }
                iPendingSubscriptions = null;

                foreach (Product p in iProductLookup.Values)
                {
                    p.Dispose();
                }
                iProductLookup = null;

                iDisposed = true;
            }
        }

        public IWatchableCollection<Product> Products
        {
            get
            {
                return iProducts;
            }
        }

        public void CollectionOpen()
        {
        }

        public void CollectionInitialised()
        {
        }

        public void CollectionClose()
        {
        }

        public void CollectionAdd(IWatchableDevice aItem, uint aIndex)
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology1.CollectionAdd");
            }

            aItem.Subscribe<Product>(Subscribed);
            iPendingSubscriptions.Add(aItem);
        }

        public void CollectionMove(IWatchableDevice aItem, uint aFrom, uint aTo)
        {
            throw new NotSupportedException();
        }

        public void CollectionRemove(IWatchableDevice aItem, uint aIndex)
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology1.CollectionRemove");
            }

            Product product;
            if (iProductLookup.TryGetValue(aItem, out product))
            {
                iProducts.Remove(product);
                iProductLookup.Remove(aItem);
            }
        }

        private void Subscribed(IWatchableDevice aDevice, Product aProduct)
        {
            lock (iProducts.WatchableThread)
            {
                if (iDisposed)
                {
                    return;
                }

                iProducts.Add(aProduct);
                iProductLookup.Add(aDevice, aProduct);
                iPendingSubscriptions.Remove(aDevice);
            }
        }

        private bool iDisposed;

        private INetwork iNetwork;

        private List<IWatchableDevice> iPendingSubscriptions;
        private Dictionary<IWatchableDevice, Product> iProductLookup;
        private WatchableProductCollection iProducts;
        
        private WatchableDeviceCollection iDevices;
    }
}
