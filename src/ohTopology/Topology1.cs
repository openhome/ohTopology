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
        IWatchableUnordered<ProxyProduct> Products { get; }
        IWatchableThread WatchableThread { get; }
    }

    public class Topology1 : ITopology1, IUnorderedWatcher<IWatchableDevice>, IDisposable
    {
        public Topology1(INetwork aNetwork)
        {
            iDisposed = false;

            iNetwork = aNetwork;
            iThread = aNetwork.WatchableThread;

            iPendingSubscriptions = new List<IWatchableDevice>();
            iProductLookup = new Dictionary<IWatchableDevice, ProxyProduct>();
            iProducts = new WatchableUnordered<ProxyProduct>(iThread);

            iDevices = iNetwork.Create<ProxyProduct>();
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
            iDevices = null;

            // dispose of all products, which will in turn unsubscribe
            foreach (ProxyProduct p in iProductLookup.Values)
            {
                p.Dispose();
            }
            iProductLookup = null;

            iProducts.Dispose();
            iProducts = null;

            iDisposed = true;
        }

        public IWatchableUnordered<ProxyProduct> Products
        {
            get
            {
                return iProducts;
            }
        }

        public IWatchableThread WatchableThread
        {
            get
            {
                return iThread;
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
            Task<ProxyProduct> task = aItem.Create<ProxyProduct>();
            iThread.Schedule(() =>
            {
                ProxyProduct product = task.Result;

                if (iPendingSubscriptions.Contains(aItem))
                {
                    iProducts.Add(product);
                    iProductLookup.Add(aItem, product);
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

            ProxyProduct product;
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
        private Dictionary<IWatchableDevice, ProxyProduct> iProductLookup;
        private WatchableUnordered<ProxyProduct> iProducts;

        private IWatchableUnordered<IWatchableDevice> iDevices;
    }
}
