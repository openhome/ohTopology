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
    public interface IRadioPreset
    {
        string Metadata { get; }
    }

    public class RadioPreset : IRadioPreset
    {
        public static readonly RadioPreset Empty = new RadioPreset();
        private string iMetadata;

        private RadioPreset()
        {
            iMetadata = "null";
        }

        public RadioPreset(string aMetadata)
        {
            iMetadata = aMetadata;
        }

        public string Metadata
        {
            get
            {
                return iMetadata;
            }
        }
    }

    public interface IProxyRadio : IProxy
    {
        IWatchable<uint> Id { get; }
        IWatchable<IEnumerable<IRadioPreset>> Presets { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<IInfoMetadata> Metadata { get; }

        Task Play();
        Task Pause();
        Task Stop();
        Task SeekSecondAbsolute(uint aValue);
        Task SeekSecondRelative(int aValue);

        Task SetId(uint aId, string aUri);
        Task SetChannel(string aUri, string aMetadata);

        uint ChannelsMax { get; }
        string ProtocolInfo { get; }
    }

    public abstract class ServiceRadio : Service
    {
        protected ServiceRadio(INetwork aNetwork)
            : base(aNetwork)
        {
            iId = new Watchable<uint>(aNetwork, "Id", 0);
            iPresets = new Watchable<IEnumerable<IRadioPreset>>(aNetwork, "Presets", new List<IRadioPreset>());
            iTransportState = new Watchable<string>(aNetwork, "TransportState", string.Empty);
            iMetadata = new Watchable<IInfoMetadata>(aNetwork, "Metadata", new InfoMetadata());
        }

        public override void Dispose()
        {
            base.Dispose();

            iId.Dispose();
            iId = null;

            iPresets.Dispose();
            iPresets = null;

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

        public IWatchable<IEnumerable<IRadioPreset>> Presets
        {
            get
            {
                return iPresets;
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
        public abstract Task SetChannel(string aUri, string aMetadata);
        
        protected uint iChannelsMax;
        protected string iProtocolInfo;

        protected Watchable<uint> iId;
        protected Watchable<IEnumerable<IRadioPreset>> iPresets;
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

            iService.SetPropertyIdChanged(HandleIdChanged);
            iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
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

        public override Task SetChannel(string aUri, string aMetadata)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetChannel(aUri, aMetadata);
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
            IList<uint> idArray = ByteArray.Unpack(iService.PropertyIdArray());

            string idList = string.Empty;
            foreach (uint id in idArray)
            {
                idList += id.ToString() + " ";
            }

            iService.BeginReadList(idList, (IntPtr ptr) =>
            {
                string channelList;
                iService.EndReadList(ptr, out channelList);

                List<IRadioPreset> presets = new List<IRadioPreset>();

                XmlDocument document = new XmlDocument();
                document.LoadXml(channelList);

                foreach (uint id in idArray)
                {
                    if (id > 0)
                    {
                        XmlNode n = document.SelectSingleNode(string.Format("/ChannelList/Entry[Id={0}]/Metadata", id));
                        presets.Add(new RadioPreset(n.InnerText));
                    }
                    else
                    {
                        presets.Add(RadioPreset.Empty);
                    }
                }

                Network.Schedule(() =>
                {
                    iPresets.Update(presets);
                });
            });
        }

        private void HandleMetadataChanged()
        {
            Network.Schedule(() =>
            {
                iMetadata.Update(
                    new InfoMetadata(
                        iService.PropertyMetadata(),
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
    }

    class ServiceRadioMock : ServiceRadio, IMockable
    {
        public ServiceRadioMock(INetwork aNetwork, uint aId, IList<IRadioPreset> aPresets, IInfoMetadata aMetadata, string aProtocolInfo, string aTransportState, uint aChannelsMax)
            : base(aNetwork)
        {
            iChannelsMax = aChannelsMax;
            iProtocolInfo = aProtocolInfo;
            
            iId.Update(aId);
            iPresets.Update(aPresets);
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

        public override Task SetChannel(string aUri, string aMetadata)
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
                IInfoMetadata metadata = new InfoMetadata(value.ElementAt(0), value.ElementAt(1));
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
        public ProxyRadio(IDevice aDevice, ServiceRadio aService)
            : base(aDevice, aService)
        {
        }

        public IWatchable<uint> Id
        {
            get { return iService.Id; }
        }

        public IWatchable<IEnumerable<IRadioPreset>> Presets
        {
            get { return iService.Presets; }
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

        public Task SetChannel(string aUri, string aMetadata)
        {
            return iService.SetChannel(aUri, aMetadata);
        }
    }
}
