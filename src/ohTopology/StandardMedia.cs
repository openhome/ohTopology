using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardMedia
    {
        IWatchableOrdered<IProxyMediaServer> MediaServers { get; }
        IWatchable<IProxyMediaServer> Local { get; }
        IWatchableUnordered<IProxyMediaServer> Remote { get; }
        void SetLocal(string aName);
    }

    public class StandardMedia : IUnorderedWatcher<IDevice>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly WatchableOrdered<IProxyMediaServer> iWatchableMediaServers;
        private IWatchableUnordered<IDevice> iMediaServers;
        private readonly Dictionary<IDevice, IProxyMediaServer> iMediaServerLookup;
        private readonly Watchable<IProxyMediaServer> iLocal;
        private string iName;
        private bool iDisposed;

        public StandardMedia(INetwork aNetwork)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aNetwork;
            iDisposed = false;
            //iLocal = new Watchable<IProxyMediaServer>(iNetwork, "Local", 
            iWatchableMediaServers = new WatchableOrdered<IProxyMediaServer>(aNetwork);
            iMediaServerLookup = new Dictionary<IDevice, IProxyMediaServer>();

            iNetwork.Schedule(() =>
            {
                iMediaServers = aNetwork.Create<IProxyMediaServer>();
                iMediaServers.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                iMediaServers.RemoveWatcher(this);
            });

            foreach (var kvp in iMediaServerLookup)
            {
                kvp.Value.Dispose();
            }

            iWatchableMediaServers.Dispose();

            iDisposed = true;
        }

        public IWatchableOrdered<IProxyMediaServer> MediaServers
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableMediaServers;
                }
            }
        }

        public IWatchable<IProxyMediaServer> Local
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    throw new NotImplementedException();
                    //return iLocal;
                }
            }
        }

        public IWatchableUnordered<IProxyMediaServer> Remote
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void SetLocal(string aName)
        {
            iNetwork.Schedule(() =>
            {
                iName = aName;
            });
        }

        public void UnorderedOpen() { }

        public void UnorderedInitialised() { }

        public void UnorderedClose() { }

        public void UnorderedAdd(IDevice aItem)
        {
            aItem.Create<IProxyMediaServer>((server) =>
            {
                if (!iDisposed)
                {
                    // calculate where to insert the sender
                    int index = 0;
                    foreach (IProxyMediaServer ms in iWatchableMediaServers.Values)
                    {
                        if (server.ProductName.CompareTo(ms.ProductName) < 0)
                        {
                            break;
                        }
                        ++index;
                    }

                    // insert the sender
                    iMediaServerLookup.Add(aItem, server);
                    iWatchableMediaServers.Add(server, (uint)index);
                }
                else
                {
                    server.Dispose();
                }
            });
        }

        public void UnorderedRemove(IDevice aItem)
        {
            IProxyMediaServer server;
            if (iMediaServerLookup.TryGetValue(aItem, out server))
            {
                // remove the corresponding sender from the watchable collection
                iMediaServerLookup.Remove(aItem);
                iWatchableMediaServers.Remove(server);

                server.Dispose();
            }
        }
    }
}
