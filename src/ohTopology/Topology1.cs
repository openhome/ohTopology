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
        IWatchableUnordered<IProxyProduct> Products { get; }
        INetwork Network { get; }
    }

    public class Topology1 : ITopology1, IUnorderedWatcher<IDevice>, IDisposable
    {
        public Topology1(INetwork aNetwork, ILog aLog)
        {
            iDisposed = false;

            iNetwork = aNetwork;
            iLog = aLog;

            iPendingSubscriptions = new List<IDevice>();
            iProductLookup = new Dictionary<IDevice, IProxyProduct>();
            iProducts = new WatchableUnordered<IProxyProduct>(iNetwork);

            iNetwork.Execute(() =>
            {
                iDevices = iNetwork.Create<IProxyProduct>();
                iDevices.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology1.Dispose");
            }

            iNetwork.Execute(() =>
            {
                iDevices.RemoveWatcher(this);
                iPendingSubscriptions.Clear();
            });
            iDevices = null;

            // dispose of all products, which will in turn unsubscribe
            foreach (var p in iProductLookup.Values)
            {
                p.Dispose();
            }
            iProductLookup = null;

            iProducts.Dispose();
            iProducts = null;

            iDisposed = true;
        }

        public IWatchableUnordered<IProxyProduct> Products
        {
            get
            {
                return iProducts;
            }
        }

        public INetwork Network
        {
            get
            {
                return iNetwork;
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

        public void UnorderedAdd(IDevice aItem)
        {
            iPendingSubscriptions.Add(aItem);
            aItem.Create<IProxyProduct>((product) =>
            {
                if (!iDisposed && iPendingSubscriptions.Contains(aItem))
                {
                    try
                    {
                        iProducts.Add(product);
                    }
                    catch (ServiceNotFoundException)
                    {
                        // NOTE: we need to log the fact that product is not added due to a service not being found
                        product.Dispose();
                        return;
                    }
                    iProductLookup.Add(aItem, product);
                    iPendingSubscriptions.Remove(aItem);
                }
                else
                {
                    product.Dispose();
                }
            });
        }

        public void UnorderedRemove(IDevice aItem)
        {
            if (iPendingSubscriptions.Contains(aItem))
            {
                iPendingSubscriptions.Remove(aItem);
                return;
            }

            IProxyProduct product;
            if (iProductLookup.TryGetValue(aItem, out product))
            {
                // schedule higher layer notification
                iProducts.Remove(product);
                iProductLookup.Remove(aItem);

                product.Dispose();
            }
        }

        private bool iDisposed;

        private INetwork iNetwork;
        private readonly ILog iLog;

        private List<IDevice> iPendingSubscriptions;
        private Dictionary<IDevice, IProxyProduct> iProductLookup;
        private WatchableUnordered<IProxyProduct> iProducts;

        private IWatchableUnordered<IDevice> iDevices;
    }
}
