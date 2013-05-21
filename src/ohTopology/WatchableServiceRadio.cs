using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Net.ControlPoint;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IProxyRadio : IProxy
    {
        IWatchable<uint> Id { get; }
        IWatchable<IList<uint>> IdArray { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<IInfoMetadata> Metadata { get; }

        Task Play();
        Task Pause();
        Task Stop();
        Task SeekSecondAbsolute(uint aValue);
        Task SeekSecondRelative(int aValue);

        Task SetId(uint aId, string aUri);
        Task SetChannel(string aUri, string aMetadata);

        Task<string> Read(uint aId);
        Task<string> ReadList(string aIdList);

        uint ChannelsMax { get; }
        string ProtocolInfo { get; }
    }

    public abstract class ServiceRadio : Service
    {
        protected ServiceRadio(INetwork aNetwork)
            : base(aNetwork)
        {
            iId = new Watchable<uint>(aNetwork, "Id", 0);
            iIdArray = new Watchable<IList<uint>>(aNetwork, "IdArray", new List<uint>());
            iTransportState = new Watchable<string>(aNetwork, "TransportState", string.Empty);
            iMetadata = new Watchable<IInfoMetadata>(aNetwork, "Metadata", new InfoMetadata());
        }

        public override void Dispose()
        {
            base.Dispose();

            iId.Dispose();
            iId = null;

            iIdArray.Dispose();
            iIdArray = null;

            iTransportState.Dispose();
            iTransportState = null;

            iMetadata.Dispose();
            iMetadata = null;
        }

        public override IProxy OnCreate(IWatchableDevice aDevice)
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
        
        public IWatchable<IList<uint>> IdArray
        {
            get
            {
                return iIdArray;
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
        public abstract Task<string> Read(uint aId);
        public abstract Task<string> ReadList(string aIdList);
        
        protected uint iChannelsMax;
        protected string iProtocolInfo;

        protected Watchable<uint> iId;
        protected Watchable<IList<uint>> iIdArray;
        protected Watchable<string> iTransportState;
        protected Watchable<IInfoMetadata> iMetadata;
    }

    public class ServiceRadioNetwork : ServiceRadio
    {
        public ServiceRadioNetwork(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork)
        {
            iSubscribe = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgRadio1(aDevice);

            iService.SetPropertyIdChanged(HandleIdChanged);
            iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            iSubscribe.Dispose();
            iSubscribe = null;

            iService.Dispose();
            iService = null;

            base.Dispose();
        }

        protected override void OnSubscribe()
        {
            iSubscribe.Reset();
            iService.Subscribe();
            iSubscribe.WaitOne();
        }

        private void HandleInitialEvent()
        {
            iChannelsMax = iService.PropertyChannelsMax();
            iProtocolInfo = iService.PropertyProtocolInfo();

            iSubscribe.Set();
        }

        protected override void OnUnsubscribe()
        {
            iService.Unsubscribe();
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

        public override Task<string> Read(uint aId)
        {
            Task<string> task = Task.Factory.StartNew(() =>
            {
                string metadata;
                iService.SyncRead(aId, out metadata);
                return metadata;
            });
            return task;
        }

        public override Task<string> ReadList(string aIdList)
        {
            Task<string> task = Task.Factory.StartNew(() =>
            {
                string channelList;
                iService.SyncReadList(aIdList, out channelList);
                return channelList;
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
                iIdArray.Update(ByteArray.Unpack(iService.PropertyIdArray()));
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

        private ManualResetEvent iSubscribe;
        private CpProxyAvOpenhomeOrgRadio1 iService;
    }

    public class ServiceRadioMock : ServiceRadio, IMockable
    {
        public ServiceRadioMock(INetwork aNetwork, uint aId, IList<uint> aIdArray, IInfoMetadata aMetadata, string aProtocolInfo, string aTransportState, uint aChannelsMax)
            : base(aNetwork)
        {
            iNetwork = aNetwork;

            iChannelsMax = aChannelsMax;
            iProtocolInfo = aProtocolInfo;

            iId.Update(aId);
            iIdArray.Update(aIdArray);
            iMetadata.Update(aMetadata);
            iTransportState.Update(aTransportState);
        }

        protected override void OnSubscribe()
        {
            iNetwork.SubscribeThread.Execute(() =>
            {
            });
        }

        protected override void OnUnsubscribe()
        {
        }

        public override Task Play()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
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
                iNetwork.Schedule(() =>
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
                iNetwork.Schedule(() =>
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
                iNetwork.Schedule(() =>
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
                iNetwork.Schedule(() =>
                {
                    iMetadata.Update(new InfoMetadata(aMetadata, aUri));
                });
            });
            return task;
        }

        public override Task<string> Read(uint aId)
        {
            Task<string> task = Task.Factory.StartNew(() =>
            {
                return string.Empty;
            });
            return task;
        }

        public override Task<string> ReadList(string aIdList)
        {
            Task<string> task = Task.Factory.StartNew(() =>
            {
                return string.Empty;
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
            else if (command == "idarray")
            {
                List<uint> ids = new List<uint>();
                IList<string> values = aValue.ToList();
                foreach (string s in values)
                {
                    ids.Add(uint.Parse(s));
                }
                iIdArray.Update(ids);
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

        private INetwork iNetwork;
    }

    public class ProxyRadio : Proxy<ServiceRadio>, IProxyRadio
    {
        public ProxyRadio(IWatchableDevice aDevice, ServiceRadio aService)
            : base(aDevice, aService)
        {
        }

        public IWatchable<uint> Id
        {
            get { return iService.Id; }
        }

        public IWatchable<IList<uint>> IdArray
        {
            get { return iService.IdArray; }
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

        public Task<string> Read(uint aId)
        {
            return iService.Read(aId);
        }

        public Task<string> ReadList(string aIdList)
        {
            return iService.ReadList(aIdList);
        }
    }
}
