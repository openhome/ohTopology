using System;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class WatchableProductUnordered : WatchableUnordered<Product>
    {
        public WatchableProductUnordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<Product>();
        }

        public new void Add(Product aValue)
        {
            iList.Add(aValue);

            base.Add(aValue);
        }

        public new void Remove(Product aValue)
        {
            iList.Remove(aValue);

            base.Remove(aValue);
        }

        private List<Product> iList;
    }

    public interface ITopology1
    {
        IWatchableUnordered<Product> Products { get; }
    }

    public class Topology1 : ITopology1, IUnorderedWatcher<IWatchableDevice>, IDisposable
    {
        public Topology1(IWatchableThread aThread, INetwork aNetwork)
        {
            iDisposed = false;

            iNetwork = aNetwork;

            iPendingSubscriptions = new List<IWatchableDevice>();
            iProductLookup = new Dictionary<IWatchableDevice, Product>();
            iProducts = new WatchableProductUnordered(aThread);

            iDevices = iNetwork.GetWatchableDeviceCollection<Product>();
            iDevices.AddWatcher(this);
        }

        public void Dispose()
        {
            iDevices.RemoveWatcher(this);

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

        public IWatchableUnordered<Product> Products
        {
            get
            {
                return iProducts;
            }
        }

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedClose()
        {
        }

        public void UnorderedAdd(IWatchableDevice aItem)
        {
            aItem.Subscribe<Product>(Subscribed);
            iPendingSubscriptions.Add(aItem);
        }

        public void UnorderedRemove(IWatchableDevice aItem)
        {
            Product product;
            if (iProductLookup.TryGetValue(aItem, out product))
            {
                iProducts.Remove(product);
                iProductLookup.Remove(aItem);
            }
        }

        private void Subscribed(IWatchableDevice aDevice, Product aProduct)
        {
            iProducts.Add(aProduct);
            iProductLookup.Add(aDevice, aProduct);
            iPendingSubscriptions.Remove(aDevice);
        }

        private bool iDisposed;

        private INetwork iNetwork;

        private List<IWatchableDevice> iPendingSubscriptions;
        private Dictionary<IWatchableDevice, Product> iProductLookup;
        private WatchableProductUnordered iProducts;
        
        private WatchableDeviceUnordered iDevices;
    }
}
