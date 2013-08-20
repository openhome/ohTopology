using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IMusicEndpoint
    {
        bool Enabled { get; }
        IProxyMediaEndpoint Endpoint { get; }
    }

    class MusicEndpoint : IMusicEndpoint
    {
        private bool iEnabled;
        private IProxyMediaEndpoint iEndpoint;

        public MusicEndpoint()
        {
            iEnabled = false;
            iEndpoint = null;
        }

        public MusicEndpoint(IProxyMediaEndpoint aEndpoint)
        {
            iEnabled = true;
            iEndpoint = aEndpoint;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
        }

        public IProxyMediaEndpoint Endpoint
        {
            get
            {
                return iEndpoint;
            }
        }
    }

    public interface IStandardMedia
    {
        IWatchableOrdered<IProxyMediaEndpoint> MusicEndpoints { get; }
        IWatchable<IProxyMediaEndpoint> MusicEndpoint { get; }
        IWatchableUnordered<IProxyMediaEndpoint> OtherEndpoints { get; }
        void SetMusicEndpoint(string aId);
    }

    public class StandardMedia : IUnorderedWatcher<IDevice>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private IWatchableUnordered<IDevice> iMediaEndpoints;
        private readonly Dictionary<IDevice, IProxyMediaEndpoint> iEndpointLookup;
        private readonly Dictionary<string, List<IProxyMediaEndpoint>> iOtherEndpointLookup;
        private readonly WatchableOrdered<IProxyMediaEndpoint> iMusicEndpoints;
        private readonly WatchableUnordered<IProxyMediaEndpoint> iOtherEndpoints;
        private readonly Watchable<IMusicEndpoint> iMusicEndpoint;
        private string iId;
        private bool iDisposed;

        public StandardMedia(INetwork aNetwork)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aNetwork;
            iDisposed = false;
            iMusicEndpoint = new Watchable<IMusicEndpoint>(iNetwork, "MusicEndpoint", new MusicEndpoint());
            iOtherEndpoints = new WatchableUnordered<IProxyMediaEndpoint>(iNetwork);
            iMusicEndpoints = new WatchableOrdered<IProxyMediaEndpoint>(aNetwork);
            iEndpointLookup = new Dictionary<IDevice, IProxyMediaEndpoint>();
            iOtherEndpointLookup = new Dictionary<string, List<IProxyMediaEndpoint>>();

            iNetwork.Schedule(() =>
            {
                iMediaEndpoints = iNetwork.Create<IProxyMediaEndpoint>();
                iMediaEndpoints.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                iMediaEndpoints.RemoveWatcher(this);
            });

            foreach (var kvp in iEndpointLookup)
            {
                kvp.Value.Dispose();
            }
            iEndpointLookup.Clear();
            iOtherEndpointLookup.Clear();

            iMusicEndpoints.Dispose();
            iOtherEndpoints.Dispose();
            iMusicEndpoint.Dispose();

            iDisposed = true;
        }

        public IWatchableOrdered<IProxyMediaEndpoint> MusicEndpoints
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iMusicEndpoints;
                }
            }
        }

        public IWatchable<IMusicEndpoint> MusicEndpoint
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iMusicEndpoint;
                }
            }
        }

        public IWatchableUnordered<IProxyMediaEndpoint> OtherEndpoints
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iOtherEndpoints;
                }
            }
        }

        public void SetLocal(string aId)
        {
            iNetwork.Schedule(() =>
            {
                iId = aId;

                foreach (var e in iMusicEndpoints.Values)
                {
                    if (iId == e.Id)
                    {
                        iMusicEndpoint.Update(new MusicEndpoint(e));
                    }
                }
            });
        }

        public void UnorderedOpen() { }

        public void UnorderedInitialised() { }

        public void UnorderedClose() { }

        public void UnorderedAdd(IDevice aItem)
        {
            aItem.Create<IProxyMediaEndpoint>((endpoint) =>
            {
                if (!iDisposed)
                {
                    if (endpoint.Type == "Music")
                    {
                        // calculate where to insert the endpoint
                        int index = 0;
                        foreach (IProxyMediaEndpoint ep in iMusicEndpoints.Values)
                        {
                            if (endpoint.Name.CompareTo(ep.Name) < 0)
                            {
                                break;
                            }
                            ++index;
                        }

                        // insert the music endpoint
                        iMusicEndpoints.Add(endpoint, (uint)index);

                        if (iId == endpoint.Id)
                        {
                            iMusicEndpoint.Update(new MusicEndpoint(endpoint));
                        }
                    }
                    else
                    {
                        List<IProxyMediaEndpoint> list;
                        if(!iOtherEndpointLookup.TryGetValue(endpoint.Type, out list))
                        {
                            list = new List<IProxyMediaEndpoint>();
                            iOtherEndpointLookup.Add(endpoint.Type, list);
                        }

                        IProxyMediaEndpoint current = (list.Count() > 0) ? list[0] : null;

                        bool inserted = false;
                        for (int i = 0; i < list.Count(); ++i)
                        {
                            IProxyMediaEndpoint ep = list[i];
                            if (endpoint.Attributes.Contains("Local") && ep.Attributes.Contains("Cloud"))
                            {
                                list.Insert(i, endpoint);
                                inserted = true;
                                break;
                            }
                            else if (endpoint.Attributes.Contains("Local") && ep.Attributes.Contains("Local") ||
                                endpoint.Attributes.Contains("Cloud") && ep.Attributes.Contains("Cloud"))
                            {
                                if (endpoint.Started < ep.Started)
                                {
                                    list.Insert(i, endpoint);
                                    inserted = true;
                                    break;
                                }
                            }
                        }
                        if (!inserted)
                        {
                            list.Add(endpoint);
                        }
                        // update other media endpoints according to the following
                        // only one of each Type should appear
                        // if Types are the same prefer Local to Cloud
                        // if both are Local/Cloud prefer earliest Started
                        if(current != list[0])
                        {
                            if (current != null)
                            {
                                iOtherEndpoints.Remove(current);
                            }
                            iOtherEndpoints.Add(list[0]);
                        }
                    }

                    iEndpointLookup.Add(aItem, endpoint);
                }
                else
                {
                    endpoint.Dispose();
                }
            });
        }

        public void UnorderedRemove(IDevice aItem)
        {
            IProxyMediaEndpoint endpoint;

            if (iEndpointLookup.TryGetValue(aItem, out endpoint))
            {
                // remove the corresponding endpoint from the watchable collections
                if (endpoint.Type == "Music")
                {
                    iMusicEndpoints.Remove(endpoint);
                }
                else
                {
                    List<IProxyMediaEndpoint> list = iOtherEndpointLookup[endpoint.Type];
                    if (list[0] == endpoint)
                    {
                        iOtherEndpoints.Remove(list[0]);
                        list.Remove(endpoint);
                        if (list.Count() > 0)
                        {
                            iOtherEndpoints.Add(list[0]);
                        }
                    }
                }
                iEndpointLookup.Remove(aItem);
                endpoint.Dispose();
            }
        }
    }
}
