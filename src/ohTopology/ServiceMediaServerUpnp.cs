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

        private readonly List<IMediaServerSession> iSessions;
        
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

            iSessions = new List<IMediaServerSession>();
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
                iSessions.Add(session);
                return (session);
            }));
        }

        internal void Destroy(IMediaServerSession aSession)
        {
            iSessions.Remove(aSession);
        }

        // IDispose

        public override void Dispose()
        {
            base.Dispose();
            Do.Assert(iSessions.Count == 0);
        }
    }

    internal class MediaServerSessionUpnp : IMediaServerSession
    {
        private readonly INetwork iNetwork;
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;
        private readonly ServiceMediaServerUpnp iService;
        
        public MediaServerSessionUpnp(INetwork aNetwork, CpProxyUpnpOrgContentDirectory1 aUpnpProxy, ServiceMediaServerUpnp aService)
        {
            iNetwork = aNetwork;
            iUpnpProxy = aUpnpProxy;
            iService = aService;
        }

        // IMediaServerSession

        public Task<IMediaServerContainer> Query(string aValue)
        {
            throw new NotImplementedException();
        }

        public Task<IMediaServerContainer> Browse(IMediaDatum aDatum)
        {
            throw new NotImplementedException();
        }

        // Disposable

        public void Dispose()
        {
            iService.Destroy(this);
        }
    }

    internal class MediaServerContainerUpnp : IMediaServerContainer
    {
        private readonly Watchable<IMediaServerSnapshot> iSnapshot;

        public MediaServerContainerUpnp(INetwork aNetwork, IMediaServerSnapshot aSnapshot)
        {
            iSnapshot = new Watchable<IMediaServerSnapshot>(aNetwork.WatchableThread, "snapshot", aSnapshot);
        }

        // IMediaServerContainer

        public IWatchable<IMediaServerSnapshot> Snapshot
        {
            get { return (iSnapshot); }
        }
    }


    internal class MediaServerSnapshotUpnp : IMediaServerSnapshot
    {
        private readonly IEnumerable<IMediaDatum> iData;
        private readonly IEnumerable<uint> iAlphaMap;

        public MediaServerSnapshotUpnp(IEnumerable<IMediaDatum> aData)
        {
            iData = aData;
            iAlphaMap = null;
        }

        // IMediaServerSnapshot

        public uint Total
        {
            get { return ((uint)iData.Count()); }
        }

        public uint Sequence
        {
            get { return (0); }
        }

        public IEnumerable<uint> AlphaMap
        {
            get { return (iAlphaMap); }
        }

        public Task<IMediaServerFragment> Read(uint aIndex, uint aCount)
        {
            Do.Assert(aIndex + aCount <= Total);

            return (Task.Factory.StartNew<IMediaServerFragment>(() =>
            {
                return (new MediaServerFragment(aIndex, 0, iData.Skip((int)aIndex).Take((int)aCount)));
            }));
        }
    }
}
