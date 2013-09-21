using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    class MediaPresetRadio : IMediaPreset, IWatcher<uint>, IWatcher<string>
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly uint iIndex;
        private readonly uint iId;
        private readonly IMediaMetadata iMetadata;
        private readonly string iUri;
        private readonly ServiceRadio iRadio;
        private readonly Watchable<bool> iBuffering;
        private readonly Watchable<bool> iPlaying;
        private uint iCurrentId;
        private string iCurrentTransportState;
        private bool iDisposed;

        public MediaPresetRadio(INetwork aNetwork, uint aIndex, uint aId, IMediaMetadata aMetadata, string aUri, ServiceRadio aRadio)
        {
            iDisposed = false;
            iDisposeHandler = new DisposeHandler();

            iNetwork = aNetwork;
            iIndex = aIndex;
            iId = aId;
            iMetadata = aMetadata;
            iUri = aUri;
            iRadio = aRadio;

            iBuffering = new Watchable<bool>(iNetwork, "Buffering", false);
            iPlaying = new Watchable<bool>(iNetwork, "Playing", false);
            iNetwork.Schedule(() =>
            {
                if (!iDisposed)
                {
                    iRadio.Id.AddWatcher(this);
                    iRadio.TransportState.AddWatcher(this);
                }
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();
            iNetwork.Execute(() =>
            {
                iRadio.Id.RemoveWatcher(this);
                iRadio.TransportState.RemoveWatcher(this);
                iDisposed = true;
            });
            iBuffering.Dispose();
            iPlaying.Dispose();
        }

        public uint Index
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iIndex;
                }
            }
        }

        public uint Id
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iId;
                }
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iMetadata;
                }
            }
        }

        public IWatchable<bool> Buffering
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iBuffering;
                }
            }
        }

        public IWatchable<bool> Playing
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPlaying;
                }
            }
        }

        public void Play()
        {
            using (iDisposeHandler.Lock)
            {
                if (iId > 0)
                {
                    iRadio.SetId(iId, iUri).ContinueWith((t) =>
                    {
                        iRadio.Play();
                    });
                }
            }
        }

        private void EvaluatePlaying()
        {
            iBuffering.Update(iCurrentId == iId && iCurrentTransportState == "Buffering");
            iPlaying.Update(iCurrentId == iId && iCurrentTransportState == "Playing");
        }

        public void ItemOpen(string aId, uint aValue)
        {
            iCurrentId = aValue;
            EvaluatePlaying();
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            iCurrentTransportState = string.Empty;
            iCurrentId = aValue;
            EvaluatePlaying();
        }

        public void ItemClose(string aId, uint aValue)
        {
            iPlaying.Update(false);
        }

        public void ItemOpen(string aId, string aValue)
        {
            iCurrentTransportState = aValue;
            EvaluatePlaying();
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iCurrentTransportState = aValue;
            EvaluatePlaying();
        }

        public void ItemClose(string aId, string aValue)
        {
            iPlaying.Update(false);
        }
    }

    public interface IProxyRadio : IProxy
    {
        IWatchable<uint> Id { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<IInfoMetadata> Metadata { get; }

        Task Play();
        Task Pause();
        Task Stop();
        Task SeekSecondAbsolute(uint aValue);
        Task SeekSecondRelative(int aValue);

        Task SetId(uint aId, string aUri);
        Task SetChannel(string aUri, IMediaMetadata aMetadata);

        Task<IWatchableContainer<IMediaPreset>> Container { get; }

        uint ChannelsMax { get; }
        string ProtocolInfo { get; }
    }

    public abstract class ServiceRadio : Service
    {
        protected ServiceRadio(INetwork aNetwork, IDevice aDevice)
            : base(aNetwork, aDevice)
        {
            iId = new Watchable<uint>(aNetwork, "Id", 0);
            iTransportState = new Watchable<string>(aNetwork, "TransportState", string.Empty);
            iMetadata = new Watchable<IInfoMetadata>(aNetwork, "Metadata", InfoMetadata.Empty);
        }

        public override void Dispose()
        {
            base.Dispose();

            iId.Dispose();
            iId = null;

            iTransportState.Dispose();
            iTransportState = null;

            iMetadata.Dispose();
            iMetadata = null;
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyRadio(this);
        }
        
        public IWatchable<uint> Id
        {
            get
            {
                return iId;
            }
        }
        
        public IWatchable<string> TransportState
        {
            get
            {
                return iTransportState;
            }
        }
        
        public IWatchable<IInfoMetadata> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public uint ChannelsMax
        {
            get
            {
                return iChannelsMax;
            }
        }

        public string ProtocolInfo
        {
            get
            {
                return iProtocolInfo;
            }
        }

        public abstract Task Play();
        public abstract Task Pause();
        public abstract Task Stop();
        public abstract Task SeekSecondAbsolute(uint aValue);
        public abstract Task SeekSecondRelative(int aValue);
        public abstract Task SetId(uint aId, string aUri);
        public abstract Task SetChannel(string aUri, IMediaMetadata aMetadata);

        public abstract Task<IWatchableContainer<IMediaPreset>> Container { get; }
        
        protected uint iChannelsMax;
        protected string iProtocolInfo;

        protected Watchable<uint> iId;
        protected Watchable<string> iTransportState;
        protected Watchable<IInfoMetadata> iMetadata;
    }

    class ServiceRadioNetwork : ServiceRadio
    {
        public ServiceRadioNetwork(INetwork aNetwork, IDevice aDevice, CpDevice aCpDevice)
            : base(aNetwork, aDevice)
        {
            iSubscribed = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgRadio1(aCpDevice);

            iService.SetPropertyIdChanged(HandleIdChanged);
            iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            base.Dispose();

            iSubscribed.Dispose();
            iSubscribed = null;

            Do.Assert(iContainer == null);
            Do.Assert(iCacheSession == null);

            iService.Dispose();
            iService = null;
        }

        protected override Task OnSubscribe()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iCacheSession = Network.IdCache.CreateSession(string.Format("Radio({0})", Device.Udn), ReadList);
                iContainer = new RadioContainer(iNetwork, iCacheSession, this);

                iService.Subscribe();
                iSubscribed.WaitOne();
            });
            return task;
        }

        protected override void OnCancelSubscribe()
        {
            iSubscribed.Set();
        }

        private void HandleInitialEvent()
        {
            iChannelsMax = iService.PropertyChannelsMax();
            iProtocolInfo = iService.PropertyProtocolInfo();

            iSubscribed.Set();
        }

        protected override void OnUnsubscribe()
        {
            if (iService != null)
            {
                iService.Unsubscribe();
            }

            if (iContainer != null)
            {
                iContainer.Dispose();
                iContainer = null;
            }
            if (iCacheSession != null)
            {
                iCacheSession.Dispose();
                iCacheSession = null;
            }

            iSubscribed.Reset();
        }

        public override Task Play()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncPlay();
            });
            return task;
        }

        public override Task Pause()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncPause();
            });
            return task;
        }

        public override Task Stop()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncStop();
            });
            return task;
        }

        public override Task SeekSecondAbsolute(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSeekSecondAbsolute(aValue);
            });
            return task;
        }

        public override Task SeekSecondRelative(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSeekSecondRelative(aValue);
            });
            return task;
        }

        public override Task SetId(uint aId, string aUri)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetId(aId, aUri);
            });
            return task;
        }

        public override Task SetChannel(string aUri, IMediaMetadata aMetadata)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetChannel(aUri, Network.TagManager.ToDidlLite(aMetadata));
            });
            return task;
        }

        private Task<IEnumerable<IIdCacheEntry>> ReadList(IEnumerable<uint> aIdList)
        {
            Task<IEnumerable<IIdCacheEntry>> task = Task<IEnumerable<IIdCacheEntry>>.Factory.StartNew(() =>
            {
                string idList = string.Empty;
                foreach(uint id in aIdList)
                {
                    idList += string.Format("{0} ", id);
                }
                idList.Trim(' ');

                string channelList;
                iService.SyncReadList(idList, out channelList);

                List<IIdCacheEntry> entries = new List<IIdCacheEntry>();

                XmlDocument document = new XmlDocument();
                document.LoadXml(channelList);

                foreach (uint id in aIdList)
                {
                    if (id > 0)
                    {
                        XmlNode n = document.SelectSingleNode(string.Format("/ChannelList/Entry[Id={0}]/Metadata", id));
                        IMediaMetadata metadata = Network.TagManager.FromDidlLite(n.InnerText);
                        string uri = metadata[Network.TagManager.Audio.Uri].Value;
                        entries.Add(new IdCacheEntry(metadata, uri));
                    }
                }

                return entries;
            });
            return task;
        }

        public override Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                Task<IWatchableContainer<IMediaPreset>> task = Task<IWatchableContainer<IMediaPreset>>.Factory.StartNew(() =>
                {
                    return iContainer;
                });
                return task;
            }
        }

        private void HandleIdChanged()
        {
            Network.Schedule(() =>
            {
                iId.Update(iService.PropertyId());
            });
        }

        private void HandleIdArrayChanged()
        {
            Network.Schedule(() =>
            {
                IList<uint> idArray = ByteArray.Unpack(iService.PropertyIdArray());
                iCacheSession.SetValid(idArray.Where(v => v != 0).ToList());
                iContainer.UpdateSnapshot(idArray);
            });
        }

        private void HandleMetadataChanged()
        {
            Network.Schedule(() =>
            {
                iMetadata.Update(
                    new InfoMetadata(
                        Network.TagManager.FromDidlLite(iService.PropertyMetadata()),
                        iService.PropertyUri()
                    ));
            });
        }

        private void HandleTransportStateChanged()
        {
            Network.Schedule(() =>
            {
                iTransportState.Update(iService.PropertyTransportState());
            });
        }

        private ManualResetEvent iSubscribed;
        private CpProxyAvOpenhomeOrgRadio1 iService;
        private RadioContainer iContainer;
        private IIdCacheSession iCacheSession;
    }

    class RadioContainer : IWatchableContainer<IMediaPreset>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly ServiceRadio iRadio;
        private readonly IIdCacheSession iCacheSession;
        private readonly Watchable<IWatchableSnapshot<IMediaPreset>> iSnapshot;

        public RadioContainer(INetwork aNetwork, IIdCacheSession aCacheSession, ServiceRadio aRadio)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aNetwork;
            iRadio = aRadio;
            iCacheSession = aCacheSession;
            iSnapshot = new Watchable<IWatchableSnapshot<IMediaPreset>>(aNetwork, "Snapshot", new RadioSnapshot(iNetwork, iCacheSession, new List<uint>(), iRadio));
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();
            iSnapshot.Dispose();
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iSnapshot;
                }
            }
        }

        internal void UpdateSnapshot(IList<uint> aIdArray)
        {
            using (iDisposeHandler.Lock)
            {
                iSnapshot.Update(new RadioSnapshot(iNetwork, iCacheSession, aIdArray, iRadio));
            }
        }
    }

    class RadioSnapshot : IWatchableSnapshot<IMediaPreset>
    {
        private readonly INetwork iNetwork;
        private readonly IIdCacheSession iCacheSession;
        private readonly IList<uint> iIdArray;
        private readonly ServiceRadio iRadio;

        public RadioSnapshot(INetwork aNetwork, IIdCacheSession aCacheSession, IList<uint> aIdArray, ServiceRadio aRadio)
        {
            iNetwork = aNetwork;
            iCacheSession = aCacheSession;
            iIdArray = aIdArray;
            iRadio = aRadio;
        }

        public uint Total
        {
            get
            {
                return ((uint)iIdArray.Where(v => v != 0).Count());
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                return null;
            }
        }

        public Task<IWatchableFragment<IMediaPreset>> Read(uint aIndex, uint aCount)
        {
            Do.Assert(aIndex + aCount <= Total);

            Task<IWatchableFragment<IMediaPreset>> task = Task<IWatchableFragment<IMediaPreset>>.Factory.StartNew(() =>
            {
                List<uint> idList = new List<uint>();
                for (uint i = aIndex; i < aIndex + aCount; ++i)
                {
                    idList.Add(iIdArray.Where(v => v != 0).ElementAt((int)i));
                }

                List<IMediaPreset> presets = new List<IMediaPreset>();
                IEnumerable<IIdCacheEntry> entries = iCacheSession.Entries(idList).Result;
                uint index = aIndex;
                foreach(IIdCacheEntry e in entries)
                {
                    uint id = iIdArray.Where(v => v != 0).ElementAt((int)index);
                    presets.Add(new MediaPresetRadio(iNetwork, (uint)(iIdArray.IndexOf(id) + 1), id, e.Metadata, e.Uri, iRadio));
                    ++index;
                }

                return new WatchableFragment<IMediaPreset>(aIndex, presets);
            });
            return task;
        }
    }

    class ServiceRadioMock : ServiceRadio, IMockable
    {
        private IIdCacheSession iCacheSession;
        private RadioContainer iContainer;
        private IList<IMediaMetadata> iPresets;
        private List<uint> iIdArray;

        public ServiceRadioMock(INetwork aNetwork, IDevice aDevice, uint aId, IList<IMediaMetadata> aPresets, IInfoMetadata aMetadata, string aProtocolInfo, string aTransportState, uint aChannelsMax)
            : base(aNetwork, aDevice)
        {
            iChannelsMax = aChannelsMax;
            iProtocolInfo = aProtocolInfo;

            iIdArray = new List<uint>();
            uint id = 1;
            foreach (IMediaMetadata m in aPresets)
            {
                if (m == null)
                {
                    iIdArray.Add(0);
                }
                else
                {
                    iIdArray.Add(id);
                }
                ++id;
            }
            iPresets = aPresets;
            
            iId.Update(aId);
            iMetadata.Update(aMetadata);
            iTransportState.Update(aTransportState);
        }

        public override void Dispose()
        {
            if (iContainer != null)
            {
                iContainer.Dispose();
                iContainer = null;
            }

            if (iCacheSession != null)
            {
                iCacheSession.Dispose();
                iCacheSession = null;
            }

            base.Dispose();
        }

        protected override Task OnSubscribe()
        {
            iCacheSession = Network.IdCache.CreateSession(string.Format("Radio({0})", Device.Udn), ReadList);
            iCacheSession.SetValid(iIdArray.Where(v => v != 0).ToList());
            iContainer = new RadioContainer(iNetwork, iCacheSession, this);
            iContainer.UpdateSnapshot(iIdArray);

            return base.OnSubscribe();
        }

        protected override void OnUnsubscribe()
        {
            if (iCacheSession != null)
            {
                iCacheSession.Dispose();
                iCacheSession = null;
            }
            if (iContainer != null)
            {
                iContainer.Dispose();
                iContainer = null;
            }

            base.OnUnsubscribe();
        }

        public override Task Play()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iTransportState.Update("Playing");
                });
            });
            return task;
        }

        public override Task Pause()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iTransportState.Update("Paused");
                });
            });
            return task;
        }

        public override Task Stop()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iTransportState.Update("Stopped");
                });
            });
            return task;
        }

        public override Task SeekSecondAbsolute(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
            });
            return task;
        }

        public override Task SeekSecondRelative(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
            });
            return task;
        }

        public override Task SetId(uint aId, string aUri)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iId.Update(aId);
                });
            });
            return task;
        }

        public override Task SetChannel(string aUri, IMediaMetadata aMetadata)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iMetadata.Update(new InfoMetadata(aMetadata, aUri));
                });
            });
            return task;
        }

        public override Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                Task<IWatchableContainer<IMediaPreset>> task = Task<IWatchableContainer<IMediaPreset>>.Factory.StartNew(() =>
                {
                    return iContainer;
                });
                return task;
            }
        }

        private Task<IEnumerable<IIdCacheEntry>> ReadList(IEnumerable<uint> aIdList)
        {
            Task<IEnumerable<IIdCacheEntry>> task = Task<IEnumerable<IIdCacheEntry>>.Factory.StartNew(() =>
            {
                List<IdCacheEntry> entries = new List<IdCacheEntry>();
                lock(iIdArray)
                {
                    foreach (uint id in aIdList)
                    {
                        IMediaMetadata metadata = iPresets[iIdArray.IndexOf(id)];
                        entries.Add(new IdCacheEntry(metadata, metadata[Network.TagManager.Audio.Uri].Value)); 
                    }
                }
                return entries;
            });
            return task;
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "channelsmax")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iChannelsMax = uint.Parse(value.First());
            }
            else if (command == "protocolinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iProtocolInfo = string.Join(" ", value);
            }
            else if (command == "id")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iId.Update(uint.Parse(value.First()));
            }
            else if (command == "presets")
            {
                throw new NotImplementedException();
            }
            else if (command == "metadata")
            {
                IEnumerable<string> value = aValue.Skip(1);
                if (value.Count() != 2)
                {
                    throw new NotSupportedException();
                }

                /*XmlDocument document = new XmlDocument();
                XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
                nsManager.AddNamespace("didl", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
                nsManager.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
                nsManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
                nsManager.AddNamespace("ldl", "urn:linn-co-uk/DIDL-Lite");

                XmlNode didl = document.CreateElement("DIDL-Lite", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");

                XmlNode item = document.CreateElement("item", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");

                XmlNode title = document.CreateElement("dc:title", "http://purl.org/dc/elements/1.1/");
                title.AppendChild(document.CreateTextNode(value.ElementAt(0)));
                item.AppendChild(title);

                XmlNode c = document.CreateElement("upnp:class", "urn:schemas-upnp-org:metadata-1-0/upnp/");
                c.AppendChild(document.CreateTextNode("object.item.audioItem"));
                item.AppendChild(c);

                XmlNode res = document.CreateElement("res", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
                res.AppendChild(document.CreateTextNode(value.ElementAt(1)));
                item.AppendChild(res);

                didl.AppendChild(item);

                document.AppendChild(didl);*/

                IInfoMetadata metadata = new InfoMetadata(Network.TagManager.FromDidlLite(value.ElementAt(0)), value.ElementAt(1));
                iMetadata.Update(metadata);
            }
            else if (command == "transportstate")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iTransportState.Update(value.First());
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public class ProxyRadio : Proxy<ServiceRadio>, IProxyRadio
    {
        public ProxyRadio(ServiceRadio aService)
            : base(aService)
        {
        }

        public IWatchable<uint> Id
        {
            get { return iService.Id; }
        }

        public IWatchable<string> TransportState
        {
            get { return iService.TransportState; }
        }

        public IWatchable<IInfoMetadata> Metadata
        {
            get { return iService.Metadata; }
        }

        public uint ChannelsMax
        {
            get { return iService.ChannelsMax; }
        }

        public string ProtocolInfo
        {
            get { return iService.ProtocolInfo; }
        }

        public Task Play()
        {
            return iService.Play();
        }

        public Task Pause()
        {
            return iService.Pause();
        }

        public Task Stop()
        {
            return iService.Stop();
        }

        public Task SeekSecondAbsolute(uint aValue)
        {
            return iService.SeekSecondAbsolute(aValue);
        }

        public Task SeekSecondRelative(int aValue)
        {
            return iService.SeekSecondRelative(aValue);
        }

        public Task SetId(uint aId, string aUri)
        {
            return iService.SetId(aId, aUri);
        }

        public Task SetChannel(string aUri, IMediaMetadata aMetadata)
        {
            return iService.SetChannel(aUri, aMetadata);
        }

        public Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                return iService.Container;
            }
        }
    }
}
