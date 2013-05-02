using System;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
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
            iThread = aThread;

            iPendingSubscriptions = new List<IWatchableDevice>();
            iProductLookup = new Dictionary<IWatchableDevice, Product>();
            iProducts = new WatchableUnordered<Product>(aThread);

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
            if (iPendingSubscriptions.Contains(aItem))
            {
                aItem.Unsubscribe<Product>();
                iPendingSubscriptions.Remove(aItem);
                return;
            }

            Product product;
            if (iProductLookup.TryGetValue(aItem, out product))
            {
                // schedule higher layer notification
                iProducts.Remove(product);
                iProductLookup.Remove(aItem);

                // schedule Product disposal
                iThread.Schedule(() =>
                {
                    aItem.Unsubscribe<Product>();
                });
            }
        }

        private void Subscribed(IWatchableDevice aDevice, Product aProduct)
        {
            if (iPendingSubscriptions.Contains(aDevice))
            {
                iProducts.Add(aProduct);
                iProductLookup.Add(aDevice, aProduct);
                iPendingSubscriptions.Remove(aDevice);
            }
            else
            {
                aDevice.Unsubscribe<Product>();
            }
        }

        private bool iDisposed;

        private INetwork iNetwork;
        private IWatchableThread iThread;

        private List<IWatchableDevice> iPendingSubscriptions;
        private Dictionary<IWatchableDevice, Product> iProductLookup;
        private WatchableUnordered<Product> iProducts;
        
        private WatchableDeviceUnordered iDevices;
    }
}
