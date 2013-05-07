using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            iDevices = iNetwork.GetWatchableDeviceCollection<ServiceProduct>();
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

            iThread.Execute(() =>
            {
                iDevices.RemoveWatcher(this);
                iPendingSubscriptions.Clear();
            });
            iDevices.Dispose();
            iDevices = null;

            // dispose of all products, which will in turn unsubscribe
            foreach (ServiceProduct p in iProductLookup.Values)
            {
                p.Dispose();
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
            iPendingSubscriptions.Add(aItem);
            aItem.Create<ServiceProduct>((IWatchableDevice device, ServiceProduct product) =>
            {
                if (iPendingSubscriptions.Contains(aItem))
                {
                    iProducts.Add(product);
                    iProductLookup.Add(product.Device, product);
                    iPendingSubscriptions.Remove(aItem);
                }
                else
                {
                    product.Dispose();
                }
            });
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

        private bool iDisposed;

        private INetwork iNetwork;
        private IWatchableThread iThread;

        private List<IWatchableDevice> iPendingSubscriptions;
        private Dictionary<IWatchableDevice, ServiceProduct> iProductLookup;
        private WatchableUnordered<ServiceProduct> iProducts;
        
        private WatchableDeviceUnordered iDevices;
    }
}
