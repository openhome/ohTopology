using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Net.ControlPoint;
using OpenHome.Os;

namespace OpenHome.Av
{
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

        Task<IVirtualContainer> Browse();

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

        public abstract Task<IVirtualContainer> Browse();
        
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

        public Task<IEnumerable<IMediaDatum>> ReadList(string aIdList)
        {
            Task<IEnumerable<IMediaDatum>> task = Task<IEnumerable<IMediaDatum>>.Factory.StartNew(() =>
            {
                string channelList;
                iService.SyncReadList(aIdList, out channelList);

                List<IMediaDatum> presets = new List<IMediaDatum>();

                XmlDocument document = new XmlDocument();
                document.LoadXml(channelList);

                foreach (uint id in aIdList)
                {
                    if (id > 0)
                    {
                        XmlNode n = document.SelectSingleNode(string.Format("/ChannelList/Entry[Id={0}]/Metadata", id));
                        presets.Add(Network.TagManager.FromDidlLite(n.InnerText));
                    }
                    else
                    {
                        presets.Add(Network.TagManager.FromDidlLite(string.Empty));
                    }
                }

                return presets;
            });
            return task;
        }

        public override Task<IVirtualContainer> Browse()
        {
            Task<IVirtualContainer> task = Task<IVirtualContainer>.Factory.StartNew(() =>
            {
                return iContainer;
            });
            return task;
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

    class RadioContainerNetwork : IVirtualContainer, IDisposable
    {
        private Watchable<IVirtualSnapshot> iSnapshot;
        private ServiceRadioNetwork iRadio;
        private uint iSequence;

        public RadioContainerNetwork(INetwork aNetwork, ServiceRadioNetwork aRadio)
        {
            iRadio = aRadio;
            iSequence = 0;
            iSnapshot = new Watchable<IVirtualSnapshot>(aNetwork, "Snapshot", new RadioSnapshotNetwork(iSequence, new List<uint>(), iRadio));
        }

        public void Dispose()
        {
            iSnapshot.Dispose();
            iSnapshot = null;
            iRadio = null;
        }

        public IWatchable<IVirtualSnapshot> Snapshot
        {
            get
            {
                return iSnapshot;
            }
        }

        public void UpdateSnapshot(IList<uint> aIdArray)
        {
            ++iSequence;
            iSnapshot.Update(new RadioSnapshotNetwork(iSequence, aIdArray, iRadio));
        }
    }

    class RadioSnapshotNetwork : IVirtualSnapshot
    {
        private readonly uint iSequence;
        private readonly IList<uint> iIdArray;
        private readonly ServiceRadioNetwork iRadio;

        public RadioSnapshotNetwork(uint aSequence, IList<uint> aIdArray, ServiceRadioNetwork aRadio)
        {
            iSequence = aSequence;
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

        public uint Sequence
        {
            get
            {
                return iSequence;
            }
        }

        public IEnumerable<uint> AlphaMap
        {
            get
            {
                return null;
            }
        }

        public Task<IVirtualFragment> Read(uint aIndex, uint aCount)
        {
            Task<IVirtualFragment> task = Task<IVirtualFragment>.Factory.StartNew(() =>
            {
                string idList = string.Empty;
                for(uint i = aIndex; i < aIndex + aCount; ++i)
                {
                    idList += string.Format("{0} ", iIdArray[(int)i]);
                }
                return new VirtualFragment(aIndex, iSequence, iRadio.ReadList(idList).Result); 
            });
            return task;
        }
    }

    class ServiceRadioMock : ServiceRadio, IMockable
    {
        private IList<IMediaDatum> iPresets;

        public ServiceRadioMock(INetwork aNetwork, uint aId, IList<IMediaDatum> aPresets, IInfoMetadata aMetadata, string aProtocolInfo, string aTransportState, uint aChannelsMax)
            : base(aNetwork)
        {
            iChannelsMax = aChannelsMax;
            iProtocolInfo = aProtocolInfo;
            iPresets = aPresets;
            
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

        public override Task<IVirtualContainer> Browse()
        {
            Task<IVirtualContainer> task = Task<IVirtualContainer>.Factory.StartNew(() =>
            {
                return new RadioContainerMock(Network, new RadioSnapshotMock(iPresets));
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

    class RadioContainerMock : IVirtualContainer
    {
        public readonly Watchable<IVirtualSnapshot> iSnapshot;

        public RadioContainerMock(INetwork aNetwork, IVirtualSnapshot aSnapshot)
        {
            iSnapshot = new Watchable<IVirtualSnapshot>(aNetwork, "Snapshot", aSnapshot);
        }

        public IWatchable<IVirtualSnapshot> Snapshot
        {
            get 
            {
                return iSnapshot;
            }
        }
    }

    class RadioSnapshotMock : IVirtualSnapshot
    {
        private readonly IEnumerable<IMediaDatum> iData;

        public RadioSnapshotMock(IEnumerable<IMediaDatum> aData)
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

        public uint Sequence
        {
            get
            {
                return 0;
            }
        }

        public IEnumerable<uint> AlphaMap
        {
            get
            {
                return null;
            }
        }

        public Task<IVirtualFragment> Read(uint aIndex, uint aCount)
        {
            Do.Assert(aIndex + aCount <= Total);

            Task<IVirtualFragment> task = Task<IVirtualFragment>.Factory.StartNew(() =>
            {
                return new VirtualFragment(aIndex, 0, iData.Skip((int)aIndex).Take((int)aCount));
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

        public Task<IVirtualContainer> Browse()
        {
            return iService.Browse();
        }
    }
}
