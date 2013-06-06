using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using OpenHome.Os.App;
using OpenHome.MediaServer;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public class ServiceMediaServerUpnp : ServiceMediaServer
    {
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;

        private readonly List<MediaServerSessionUpnp> iSessions;
        
        public ServiceMediaServerUpnp(INetwork aNetwork, IEnumerable<string> aAttributes, 
            string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl,
            string aModelImageUri, string aModelInfo, string aModelName, string aModelUrl,
            string aProductImageUri, string aProductInfo, string aProductName, string aProductUrl,
            CpProxyUpnpOrgContentDirectory1 aUpnpProxy)
            : base(aNetwork, aAttributes,
            aManufacturerImageUri, aManufacturerInfo, aManufacturerName, aManufacturerUrl,
            aModelImageUri, aModelInfo, aModelName, aModelUrl,
            aProductImageUri, aProductInfo, aProductName, aProductUrl)
        {
            iUpnpProxy = aUpnpProxy;
            iSessions = new List<MediaServerSessionUpnp>();
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return (new ProxyMediaServer(aDevice, this));
        }

        public override Task<IMediaServerSession> CreateSession()
        {
            return (Task.Factory.StartNew<IMediaServerSession>(() =>
            {
                var session = new MediaServerSessionUpnp(Network, iUpnpProxy, this);

                lock (iSessions)
                {
                    iSessions.Add(session);
                }

                return (session);
            }));
        }

        internal void Refresh()
        {
            lock (iSessions)
            {
                foreach (var session in iSessions)
                {
                    session.Refresh();
                }
            }
        }

        internal void Destroy(MediaServerSessionUpnp aSession)
        {
            lock (iSessions)
            {
                iSessions.Remove(aSession);
            }
        }

        // IDispose

        public override void Dispose()
        {
            base.Dispose();

            lock (iSessions)
            {
                Do.Assert(iSessions.Count == 0);
            }
        }
    }

    internal class MediaServerSessionUpnp : IMediaServerSession
    {
        private readonly INetwork iNetwork;
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;
        private readonly ServiceMediaServerUpnp iService;

        private readonly object iLock;
        
        private uint iSequence;

        private MediaServerContainerUpnp iContainer;
        
        public MediaServerSessionUpnp(INetwork aNetwork, CpProxyUpnpOrgContentDirectory1 aUpnpProxy, ServiceMediaServerUpnp aService)
        {
            iNetwork = aNetwork;
            iUpnpProxy = aUpnpProxy;
            iService = aService;

            iLock = new object();

            iSequence = 0;
        }

        internal void Refresh()
        {
            uint sequence;
            MediaServerContainerUpnp container;

            lock (iLock)
            {
                sequence = iSequence;
                container = iContainer;
            }

            if (container != null)
            {
                Task.Factory.StartNew(() =>
                {
                    string result;
                    uint numberReturned;
                    uint totalMatches;
                    uint updateId;

                    try
                    {
                        iUpnpProxy.SyncBrowse(container.Id, "BrowseDirectChildren", "", 0, 1, "", out result, out numberReturned, out totalMatches, out updateId);
                    }
                    catch
                    {
                        return;
                    }

                    lock (iLock)
                    {
                        if (iSequence == sequence)
                        {
                            if (container.UpdateId != updateId)
                            {
                                iContainer.Update(updateId, totalMatches);
                            }
                        }
                    }
                });
            }
        }

        private Task<IWatchableContainer<IMediaDatum>> Browse(string aId)
        {
            uint sequence;

            lock (iLock)
            {
                sequence = ++iSequence;

                if (iContainer != null)
                {
                    iContainer.Dispose();
                    iContainer = null;
                }
            }

            return Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                string result;
                uint numberReturned;
                uint totalMatches;
                uint updateId;

                try
                {
                    iUpnpProxy.SyncBrowse(aId, "BrowseDirectChildren", "", 0, 1, "", out result, out numberReturned, out totalMatches, out updateId);
                }
                catch
                {
                    return (null);
                }

                lock (iLock)
                {
                    if (iSequence == sequence)
                    {
                        iContainer = new MediaServerContainerUpnp(iNetwork, iUpnpProxy, aId, updateId, totalMatches);
                        return (iContainer);
                    }

                    return (null);
                }
            });
        }

        // IMediaServerSession

        public Task<IWatchableContainer<IMediaDatum>> Query(string aValue)
        {
            throw new NotImplementedException();
        }

        public Task<IWatchableContainer<IMediaDatum>> Browse(IMediaDatum aDatum)
        {
            if (aDatum == null)
            {
                return (Browse("0"));
            }

            var datum = aDatum as MediaDatumUpnp;

            Do.Assert(datum != null);

            return (Browse(datum.Id));
        }

        // Disposable

        public void Dispose()
        {
            lock (iLock)
            {
                if (iContainer != null)
                {
                    iContainer.Dispose();
                }
            }

            iService.Destroy(this);
        }
    }

    internal class MediaServerContainerUpnp : IWatchableContainer<IMediaDatum>, IDisposable
    {
        private readonly INetwork iNetwork;
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;
        private readonly string iId;
        private uint iUpdateId;

        private uint iSequence;
        private readonly Watchable<IWatchableSnapshot<IMediaDatum>> iWatchable;

        public MediaServerContainerUpnp(INetwork aNetwork, CpProxyUpnpOrgContentDirectory1 aUpnpProxy, string aId, uint aUpdateId, uint aTotal)
        {
            iNetwork = aNetwork;
            iUpnpProxy = aUpnpProxy;
            iId = aId;
            iUpdateId = aUpdateId;

            iSequence = 0;
            iWatchable = new Watchable<IWatchableSnapshot<IMediaDatum>>(aNetwork.WatchableThread, "snapshot", new MediaServerSnapshotUpnp(iNetwork, iUpnpProxy, iId, aTotal, 0));
        }

        internal string Id
        {
            get
            {
                return (iId);
            }
        }

        internal uint UpdateId
        {
            get
            {
                return (iUpdateId);
            }
        }

        internal void Update(uint aUpdateId, uint aTotal)
        {
            iUpdateId = aUpdateId;

            iNetwork.Schedule(() =>
            {
                iWatchable.Update(new MediaServerSnapshotUpnp(iNetwork, iUpnpProxy, iId, aTotal, ++iSequence));
            });
        }

        // IMediaServerContainer

        public IWatchable<IWatchableSnapshot<IMediaDatum>> Snapshot
        {
            get { return (iWatchable); }
        }

        // IDisposable

        public void Dispose()
        {
            iNetwork.Wait();
            iWatchable.Dispose();
        }
    }


    internal class MediaServerSnapshotUpnp : IWatchableSnapshot<IMediaDatum>
    {
        private readonly INetwork iNetwork;
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;
        private readonly string iId;
        private readonly uint iTotal;
        private readonly uint iSequence;

        private readonly IEnumerable<uint> iAlphaMap;

        public MediaServerSnapshotUpnp(INetwork aNetwork, CpProxyUpnpOrgContentDirectory1 aUpnpProxy, string aId, uint aTotal, uint aSequence)
        {
            iNetwork = aNetwork;
            iUpnpProxy = aUpnpProxy;
            iId = aId;
            iTotal = aTotal;
            iSequence = aSequence;

            iAlphaMap = null;
        }

        // IMediaServerSnapshot

        public uint Total
        {
            get { return (iTotal); }
        }

        public uint Sequence
        {
            get { return (iSequence); }
        }

        public IEnumerable<uint> AlphaMap
        {
            get { return (iAlphaMap); }
        }

        public Task<IWatchableFragment<IMediaDatum>> Read(uint aIndex, uint aCount)
        {
            iNetwork.Assert();

            Do.Assert(aIndex + aCount <= iTotal);

            return (Task.Factory.StartNew<IWatchableFragment<IMediaDatum>>(() =>
            {
                string result;
                uint numberReturned;
                uint totalMatches;
                uint updateID;

                try
                {
                    iUpnpProxy.SyncBrowse(iId, "BrowseDirectChildren", "", aIndex, aCount, "", out result, out numberReturned, out totalMatches, out updateID);
                }
                catch
                {
                    return (null);
                }

                return (new MediaServerFragmentUpnp(iNetwork, aIndex, 0, result));
            }));
        }
    }

    internal class MediaServerFragmentUpnp : IWatchableFragment<IMediaDatum>
    {
        private readonly INetwork iNetwork;
        private readonly uint iIndex;
        private readonly uint iSequence;
        private readonly IEnumerable<IMediaDatum> iData;

        public MediaServerFragmentUpnp(INetwork aNetwork, uint aIndex, uint aSequence, string aDidl)
        {
            iNetwork = aNetwork;
            iIndex = aIndex;
            iSequence = aSequence;
            iData = Parse(aDidl);
        }

        private IEnumerable<IMediaDatum> Parse(string aDidl)
        {
            List<IMediaDatum> data = new List<IMediaDatum>();
            data.Add(CreateTestItem("A"));
            data.Add(CreateTestItem("B"));
            data.Add(CreateTestItem("C"));
            data.Add(CreateTestItem("D"));
            return (data);
        }

        private IMediaDatum CreateTestItem(string aTitle)
        {
            var datum = new MediaDatum();
            datum.Add(iNetwork.TagManager.Audio.Title, aTitle);
            return (datum);
        }

        // IWatchableFragment<IMediaDatum>

        public uint Index
        {
            get { return (iIndex); }
        }

        public uint Sequence
        {
            get { return (iSequence); }
        }

        public IEnumerable<IMediaDatum> Data
        {
            get { return (iData); }
        }
    }

    internal class MediaDatumUpnp : MediaDatum
    {
        private string iId;

        public MediaDatumUpnp(string aId, params ITag[] aType)
            : base(aType)
        {
            iId = aId;
        }

        public string Id
        {
            get
            {
                return (iId);
            }
        }
    }

}
