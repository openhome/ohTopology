using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using System.Net;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public class ServiceMediaEndpointOpenHome : ServiceMediaEndpoint
    {
        private readonly string iUri;

        private readonly List<MediaEndpointSessionOpenHome> iSessions;
        
        public ServiceMediaEndpointOpenHome(INetwork aNetwork, IDevice aDevice, string aId, string aType, string aName, string aInfo,
            string aUrl, string aArtwork, string aManufacturerName, string aManufacturerInfo, string aManufacturerUrl,
            string aManufacturerArtwork, string aModelName, string aModelInfo, string aModelUrl, string aModelArtwork,
            DateTime aStarted, IEnumerable<string> aAttributes, string aUri)
            : base (aNetwork, aDevice, aId, aType, aName, aInfo, aUrl, aArtwork, aManufacturerName, aManufacturerInfo,
            aManufacturerUrl, aManufacturerArtwork, aModelName, aModelInfo, aModelUrl, aModelArtwork, aStarted, aAttributes)
        {
            iUri = aUri;

            iSessions = new List<MediaEndpointSessionOpenHome>();
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return (new ProxyMediaEndpoint(this));
        }

        public override Task<IMediaEndpointSession> CreateSession()
        {
            return (Task.Factory.StartNew<IMediaEndpointSession>(() =>
            {
                var session = new MediaEndpointSessionOpenHome(Network, this);

                lock (iSessions)
                {
                    iSessions.Add(session);
                }

                return (session);
            }));
        }

        internal string Uri
        {
            get
            {
                return (iUri);
            }
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

        internal void Destroy(MediaEndpointSessionOpenHome aSession)
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

    internal class MediaEndpointSessionOpenHome : IMediaEndpointSession
    {
        private readonly INetwork iNetwork;
        private readonly ServiceMediaEndpointOpenHome iService;

        private readonly object iLock;

        private readonly string iId;
        
        private uint iSequence;

        private MediaEndpointContainerOpenHome iContainer;

        public MediaEndpointSessionOpenHome(INetwork aNetwork, ServiceMediaEndpointOpenHome aService)
        {
            iNetwork = aNetwork;
            iService = aService;

            iLock = new object();

            var client = new WebClient();

            var session = client.DownloadString(iService.Uri + "/create");

            var json = JsonParser.Parse(session) as JsonString;

            iId = json.Value();

            iSequence = 0;
        }

        internal void Refresh()
        {
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

            return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                lock (iLock)
                {
                    if (iSequence == sequence)
                    {
                        iContainer = new MediaEndpointContainerOpenHome(iNetwork, iService, iId, aId);
                        return (iContainer);
                    }

                    return (null);
                }
            }));
        }

        // IMediaEndpointSession

        public Task<IWatchableContainer<IMediaDatum>> Browse(IMediaDatum aDatum)
        {
            if (aDatum == null)
            {
                return (Browse("0"));
            }

            return (Browse(aDatum.Id));
        }

        public Task<IWatchableContainer<IMediaDatum>> Link(ITag aTag, string aValue)
        {
            throw new NotImplementedException();
        }

        public Task<IWatchableContainer<IMediaDatum>> Search(string aValue)
        {
            throw new NotImplementedException();
        }

        public Task<IWatchableContainer<IMediaDatum>> Query(string aValue)
        {
            throw new NotImplementedException();
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

    internal class MediaEndpointContainerOpenHome : IWatchableContainer<IMediaDatum>, IDisposable
    {
        private readonly INetwork iNetwork;
        private readonly ServiceMediaEndpointOpenHome iService;
        private readonly string iSession;
        private readonly string iId;
        private readonly uint iSequence;

        private readonly Watchable<IWatchableSnapshot<IMediaDatum>> iWatchableSnapshot;

        public MediaEndpointContainerOpenHome(INetwork aNetwork, ServiceMediaEndpointOpenHome aService, string aSession, string aId)
        {
            iNetwork = aNetwork;
            iService = aService;
            iSession = aSession;
            iId = aId;

            var uri = string.Format("{0}/browse/{1}/{2}", iService.Uri, iSession, aId);

            var client = new WebClient();

            var session = client.DownloadString(uri);

            var json = JsonParser.Parse(session) as JsonString;

            iSequence = uint.Parse(json.Value());

            iWatchableSnapshot = new Watchable<IWatchableSnapshot<IMediaDatum>>(aNetwork, "snapshot", new MediaEndpointSnapshotOpenHome(iNetwork, iService, iSession, iId, iSequence));
        }

        internal string Id
        {
            get
            {
                return (iId);
            }
        }

        internal uint Sequence
        {
            get
            {
                return (iSequence);
            }
        }

        internal void Update(uint aSequence)
        {
            /*
            iUpdateId = aUpdateId;

            iNetwork.Schedule(() =>
            {
                iWatchable.Update(new MediaEndpointSnapshotOpenHome(iNetwork, iId, aTotal));
            });
            */
        }

        // IMediaServerContainer

        public IWatchable<IWatchableSnapshot<IMediaDatum>> Snapshot
        {
            get { return (iWatchableSnapshot); }
        }

        // IDisposable

        public void Dispose()
        {
            iNetwork.Execute();
            iWatchableSnapshot.Dispose();
        }
    }


    internal class MediaEndpointSnapshotOpenHome : IWatchableSnapshot<IMediaDatum>
    {
        private readonly INetwork iNetwork;
        private readonly ServiceMediaEndpointOpenHome iService;
        private readonly string iSession;
        private readonly string iContainer;
        private readonly uint iSequence;

        private readonly IEnumerable<uint> iAlphaMap;

        private readonly uint iTotal;

        public MediaEndpointSnapshotOpenHome(INetwork aNetwork, ServiceMediaEndpointOpenHome aService, string aSession, string aContainer, uint aSequence)
        {
            iNetwork = aNetwork;
            iService = aService;
            iSession = aSession;
            iContainer = aContainer;
            iSequence = aSequence;

            iAlphaMap = null;


            var uri = string.Format("{0}/snapshot/{1}/{2}/{3}", iService.Uri, iSession, iContainer, iSequence);

            var client = new WebClient();

            var session = client.DownloadString(uri);

            var json = JsonParser.Parse(session) as JsonString;

            iTotal = uint.Parse(json.Value());

        }

        // IMediaServerSnapshot

        public uint Total
        {
            get
            {
                return (iTotal);
            }
        }

        public IEnumerable<uint> AlphaMap
        {
            get
            {
                return (iAlphaMap);
            }
        }

        public Task<IWatchableFragment<IMediaDatum>> Read(uint aIndex, uint aCount)
        {
            iNetwork.Assert();

            Do.Assert(aIndex + aCount <= iTotal);

            return (Task.Factory.StartNew<IWatchableFragment<IMediaDatum>>(() =>
            {
                return (new MediaEndpointFragmentOpenHome(iNetwork, iService, iSession, iContainer, iSequence, aIndex, aCount));
            }));
        }
    }

    internal class MediaEndpointFragmentOpenHome : IWatchableFragment<IMediaDatum>
    {
        private readonly INetwork iNetwork;
        private readonly ServiceMediaEndpointOpenHome iService;
        private readonly string iSession;
        private readonly string iContainer;
        private readonly uint iSequence;

        private readonly uint iIndex;
        private readonly IEnumerable<IMediaDatum> iData;

        public MediaEndpointFragmentOpenHome(INetwork aNetwork, ServiceMediaEndpointOpenHome aService, string aSession, string aContainer, uint aSequence, uint aIndex, uint aCount)
        {
            iNetwork = aNetwork;
            iService = aService;
            iSession = aSession;
            iContainer = aContainer;
            iSequence = aSequence;
            iIndex = aIndex;

            var uri = string.Format("{0}/snapshot/{1}/{2}/{3}?index={4}&count={5}", iService.Uri, iSession, iContainer, iSequence, iIndex, aCount);

            var client = new WebClient();

            var session = client.DownloadString(uri);

            var json = JsonParser.Parse(session);

            iData = GetData(json);
        }

        private IEnumerable<IMediaDatum> GetData(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetDatum(entry));
            }
        }

        private IMediaDatum GetDatum(JsonValue aValue)
        {
            var value = aValue as JsonObject;

            var id = GetValue(value["Id"]);
            var type = GetType(value["Type"]);

            var datum = new MediaDatum(id, type.ToArray());

            foreach (var entry in value["Metadata"] as JsonArray)
            {
                var metadatum = entry as JsonArray;

                datum.Add(GetTag(metadatum.ElementAt(0)), GetValue(metadatum.ElementAt(1)));
            }

            return (datum);
        }

        private string GetValue(JsonValue aValue)
        {
            var value = aValue as JsonString;
            return (value.Value);
        }

        private IEnumerable<ITag> GetType(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetTag(entry));
            }
        }

        private ITag GetTag(JsonValue aValue)
        {
            var value = aValue as JsonString;

            var id = uint.Parse(value.Value);

            return (iNetwork.TagManager[id]);
        }

        // IWatchableFragment<IMediaDatum>

        public uint Index
        {
            get { return (iIndex); }
        }

        public IEnumerable<IMediaDatum> Data
        {
            get { return (iData); }
        }
    }
}
