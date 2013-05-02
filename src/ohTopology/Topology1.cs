using System;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ITopology1
    {
        IWatchableUnordered<ServiceProduct> Products { get; }
    }

    public class Topology1 : ITopology1, IUnorderedWatcher<IWatchableDevice>, IDisposable
    {
        public Topology1(IWatchableThread aThread, INetwork aNetwork)
        {
            iDisposed = false;

            iNetwork = aNetwork;
            iThread = aThread;

            iPendingSubscriptions = new List<IWatchableDevice>();
            iProductLookup = new Dictionary<IWatchableDevice, ServiceProduct>();
            iProducts = new WatchableUnordered<ServiceProduct>(aThread);

            iDevices = iNetwork.GetWatchableDeviceCollection<Product>();
            iThread.Schedule(() =>
            {
                iDevices.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology1.Dispose");
            }

            iThread.Wait(() =>
            {
                iDevices.RemoveWatcher(this);
            });
            iDevices.Dispose();
            iDevices = null;

            // stop subscriptions for all products that are outstanding
            foreach (IWatchableDevice d in iPendingSubscriptions)
            {
                d.Unsubscribe<Product>();
            }
            iPendingSubscriptions = null;

            // dispose of all products, which will in turn unsubscribe
            foreach (IWatchableDevice d in iProductLookup.Keys)
            {
                d.Unsubscribe<Product>();
            }
            iProductLookup = null;

            iProducts.Dispose();
            iProducts = null;

            iDisposed = true;
        }

        public IWatchableUnordered<ServiceProduct> Products
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
            if (iPendingSubscriptions.Contains(aItem))
            {
                iPendingSubscriptions.Remove(aItem);
                return;
            }

            ServiceProduct product;
            if (iProductLookup.TryGetValue(aItem, out product))
            {
                // schedule higher layer notification
                iProducts.Remove(product);
                iProductLookup.Remove(aItem);

                // schedule Product disposal
                iThread.Schedule(() =>
                {
                    product.Dispose();
                });
            }
        }

        private void Subscribed(IWatchableDevice aDevice, Product aProduct)
        {
            ServiceProduct product = new ServiceProduct(aDevice, aProduct);
            if (iPendingSubscriptions.Contains(aDevice))
            {
                iProducts.Add(product);
                iProductLookup.Add(aDevice, product);
                iPendingSubscriptions.Remove(aDevice);
            }
            else
            {
                product.Dispose();
            }
        }

        private bool iDisposed;

        private INetwork iNetwork;
        private IWatchableThread iThread;

        private List<IWatchableDevice> iPendingSubscriptions;
        private Dictionary<IWatchableDevice, ServiceProduct> iProductLookup;
        private WatchableUnordered<ServiceProduct> iProducts;
        
        private WatchableDeviceUnordered iDevices;
    }
}
