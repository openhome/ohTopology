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
    class MediaPresetRadio : IMediaPreset
    {
        private readonly uint iIndex;
        private readonly uint iId;
        private readonly IMediaMetadata iMetadata;
        private readonly string iUri;
        private readonly ServiceRadio iRadio;

        public MediaPresetRadio(uint aIndex, uint aId, IMediaMetadata aMetadata, string aUri, ServiceRadio aRadio)
        {
            iIndex = aIndex;
            iId = aId;
            iMetadata = aMetadata;
            iUri = aUri;
            iRadio = aRadio;
        }

        public uint Index
        {
            get
            {
                return iIndex;
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public void Play()
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
        protected ServiceRadio(INetwork aNetwork)
            : base(aNetwork)
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
            return new ProxyRadio(aDevice, this);
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
        public ServiceRadioNetwork(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork)
        {
            iSubscribed = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgRadio1(aDevice);
            iContainer = new RadioContainerNetwork(Network, this);

            iService.SetPropertyIdChanged(HandleIdChanged);
            iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            iContainer.Dispose();
            iContainer = null;

            iSubscribed.Dispose();
            iSubscribed = null;

            iService.Dispose();
            iService = null;

            base.Dispose();
        }

        protected override Task OnSubscribe()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.Subscribe();
                iSubscribed.WaitOne();
            });
            return task;
        }

        private void HandleInitialEvent()
        {
            iChannelsMax = iService.PropertyChannelsMax();
            iProtocolInfo = iService.PropertyProtocolInfo();

            iSubscribed.Set();
        }

        protected override void OnUnsubscribe()
        {
            iService.Unsubscribe();
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

        public Task<IEnumerable<IMediaPreset>> ReadList(string aIdList)
        {
            Task<IEnumerable<IMediaPreset>> task = Task<IEnumerable<IMediaPreset>>.Factory.StartNew(() =>
            {
                string channelList;
                iService.SyncReadList(aIdList, out channelList);

                List<IMediaPreset> presets = new List<IMediaPreset>();

                XmlDocument document = new XmlDocument();
                document.LoadXml(channelList);

                string[] ids = aIdList.Split(' ');
                uint index = 1;
                foreach (string i in ids)
                {
                    uint id = uint.Parse(i);
                    if (id > 0)
                    {
                        XmlNode n = document.SelectSingleNode(string.Format("/ChannelList/Entry[Id={0}]/Metadata", id));
                        IMediaMetadata metadata = Network.TagManager.FromDidlLite(n.InnerText);
                        string uri = metadata[Network.TagManager.Audio.Uri].Value;
                        presets.Add(new MediaPresetRadio(index, id, metadata, uri, this));
                    }
                    ++index;
                }

                return presets;
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
                iContainer.UpdateSnapshot(ByteArray.Unpack(iService.PropertyIdArray()));
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
        private RadioContainerNetwork iContainer;
    }

    class RadioContainerNetwork : IWatchableContainer<IMediaPreset>, IDisposable
    {
        private Watchable<IWatchableSnapshot<IMediaPreset>> iSnapshot;
        private ServiceRadioNetwork iRadio;

        public RadioContainerNetwork(INetwork aNetwork, ServiceRadioNetwork aRadio)
        {
            iRadio = aRadio;
            iSnapshot = new Watchable<IWatchableSnapshot<IMediaPreset>>(aNetwork, "Snapshot", new RadioSnapshotNetwork(new List<uint>(), iRadio));
        }

        public void Dispose()
        {
            iSnapshot.Dispose();
            iSnapshot = null;
            iRadio = null;
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iSnapshot;
            }
        }

        public void UpdateSnapshot(IList<uint> aIdArray)
        {
            iSnapshot.Update(new RadioSnapshotNetwork(aIdArray, iRadio));
        }
    }

    class RadioSnapshotNetwork : IWatchableSnapshot<IMediaPreset>
    {
        private readonly IList<uint> iIdArray;
        private readonly ServiceRadioNetwork iRadio;

        public RadioSnapshotNetwork(IList<uint> aIdArray, ServiceRadioNetwork aRadio)
        {
            iIdArray = aIdArray;
            iRadio = aRadio;
        }

        public uint Total
        {
            get
            {
                return ((uint)iIdArray.Count());
            }
        }

        public IEnumerable<uint> AlphaMap
        {
            get
            {
                return null;
            }
        }

        public Task<IWatchableFragment<IMediaPreset>> Read(uint aIndex, uint aCount)
        {
            Task<IWatchableFragment<IMediaPreset>> task = Task<IWatchableFragment<IMediaPreset>>.Factory.StartNew(() =>
            {
                string idList = string.Empty;
                for (uint i = aIndex; i < aIndex + aCount; ++i)
                {
                    idList += string.Format("{0} ", iIdArray[(int)i]);
                }
                return new WatchableFragment<IMediaPreset>(aIndex, iRadio.ReadList(idList.TrimEnd(' ')).Result);
            });
            return task;
        }
    }

    class ServiceRadioMock : ServiceRadio, IMockable
    {
        private IList<IMediaPreset> iPresets;

        public ServiceRadioMock(INetwork aNetwork, uint aId, IList<IMediaMetadata> aPresets, IInfoMetadata aMetadata, string aProtocolInfo, string aTransportState, uint aChannelsMax)
            : base(aNetwork)
        {
            iChannelsMax = aChannelsMax;
            iProtocolInfo = aProtocolInfo;

            iPresets = new List<IMediaPreset>();
            uint index = 1;
            uint id = 1;
            foreach (IMediaMetadata m in aPresets)
            {
                if (m != null)
                {
                    iPresets.Add(new MediaPresetRadio(index, id, m, m[Network.TagManager.Audio.Uri].Value, this));
                    ++id;
                }
                ++index;
            }
            
            iId.Update(aId);
            iMetadata.Update(aMetadata);
            iTransportState.Update(aTransportState);
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
                    return new RadioContainerMock(Network, new RadioSnapshotMock(iPresets));
                });
                return task;
            }
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

    class RadioContainerMock : IWatchableContainer<IMediaPreset>
    {
        public readonly Watchable<IWatchableSnapshot<IMediaPreset>> iSnapshot;

        public RadioContainerMock(INetwork aNetwork, IWatchableSnapshot<IMediaPreset> aSnapshot)
        {
            iSnapshot = new Watchable<IWatchableSnapshot<IMediaPreset>>(aNetwork, "Snapshot", aSnapshot);
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get 
            {
                return iSnapshot;
            }
        }
    }

    class RadioSnapshotMock : IWatchableSnapshot<IMediaPreset>
    {
        private readonly IEnumerable<IMediaPreset> iData;

        public RadioSnapshotMock(IEnumerable<IMediaPreset> aData)
        {
            iData = aData;
        }

        public uint Total
        {
            get
            {
                return ((uint)iData.Count());
            }
        }

        public IEnumerable<uint> AlphaMap
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
                return new WatchableFragment<IMediaPreset>(aIndex, iData.Skip((int)aIndex).Take((int)aCount));
            });
            return task;
        }
    }

    public class ProxyRadio : Proxy<ServiceRadio>, IProxyRadio
    {
        public ProxyRadio(IDevice aDevice, ServiceRadio aService)
            : base(aDevice, aService)
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
