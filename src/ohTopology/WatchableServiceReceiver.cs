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
    public interface IProxyReceiver : IProxy
    {
        string ProtocolInfo { get; }

        IWatchable<IInfoMetadata> Metadata { get; }
        IWatchable<string> TransportState { get; }

        Task Play();
        Task Stop();
        Task SetSender(ISenderMetadata aMetadata);
    }

    public abstract class ServiceReceiver : Service
    {
        protected ServiceReceiver(INetwork aNetwork)
            : base(aNetwork)
        {
            iMetadata = new Watchable<IInfoMetadata>(Network, "Metadata", new InfoMetadata());
            iTransportState = new Watchable<string>(Network, "TransportState", string.Empty);
        }

        public override void Dispose()
        {
            base.Dispose();

            iMetadata.Dispose();
            iMetadata = null;

            iTransportState.Dispose();
            iTransportState = null;
        }

        public override IProxy OnCreate(IWatchableDevice aDevice)
        {
            return new ProxyReceiver(aDevice, this);
        }

        public IWatchable<IInfoMetadata> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<string> TransportState
        {
            get
            {
                return iTransportState;
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
        public abstract Task Stop();
        public abstract Task SetSender(ISenderMetadata aMetadata);

        protected string iProtocolInfo;

        protected Watchable<IInfoMetadata> iMetadata;
        protected Watchable<string> iTransportState;
    }

    public class ServiceReceiverNetwork : ServiceReceiver
    {
        public ServiceReceiverNetwork(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork)
        {
            iSubscribe = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgReceiver1(aDevice);

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

        public override Task Stop()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncStop();
            });
            return task;
        }

        public override Task SetSender(ISenderMetadata aMetadata)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetSender(aMetadata.Uri, aMetadata.ToString());
            });
            return task;
        }

        private void HandleMetadataChanged()
        {
            Network.Schedule(() =>
            {
                iMetadata.Update(new InfoMetadata(iService.PropertyMetadata(), iService.PropertyUri()));
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
        private CpProxyAvOpenhomeOrgReceiver1 iService;
    }

    public class ServiceReceiverMock : ServiceReceiver, IMockable
    {
        public ServiceReceiverMock(INetwork aNetwork, string aMetadata, string aProtocolInfo, string aTransportState, string aUri)
            : base(aNetwork)
        {
            iProtocolInfo = aProtocolInfo;

            iMetadata.Update(new InfoMetadata(aMetadata, aUri));
            iTransportState.Update(aTransportState);
        }

        protected override void OnSubscribe()
        {
            Network.SubscribeThread.Execute(() =>
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
                Network.Schedule(() =>
                {
                    iTransportState.Update("Playing");
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

        public override Task SetSender(ISenderMetadata aMetadata)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iMetadata.Update(new InfoMetadata(aMetadata.ToString(), aMetadata.Uri));
                });
            });
            return task;
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "protocolinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iProtocolInfo = string.Join(" ", value);
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

    public class ProxyReceiver : Proxy<ServiceReceiver>, IProxyReceiver
    {
        public ProxyReceiver(IWatchableDevice aDevice, ServiceReceiver aService)
            : base(aDevice, aService)
        {
        }

        public string ProtocolInfo
        {
            get { return iService.ProtocolInfo; }
        }

        public IWatchable<IInfoMetadata> Metadata
        {
            get { return iService.Metadata; }
        }

        public IWatchable<string> TransportState
        {
            get { return iService.TransportState; }
        }

        public Task Play()
        {
            return iService.Play();
        }

        public Task Stop()
        {
            return iService.Stop();
        }

        public Task SetSender(ISenderMetadata aMetadata)
        {
            return iService.SetSender(aMetadata);
        }
    }
}
