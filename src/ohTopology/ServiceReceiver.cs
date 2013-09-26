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
        protected ServiceReceiver(INetwork aNetwork, IDevice aDevice)
            : base(aNetwork, aDevice)
        {
            iMetadata = new Watchable<IInfoMetadata>(Network, "Metadata", InfoMetadata.Empty);
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

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyReceiver(this);
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

    class ServiceReceiverNetwork : ServiceReceiver
    {
        public ServiceReceiverNetwork(INetwork aNetwork, IDevice aDevice, CpDevice aCpDevice)
            : base(aNetwork, aDevice)
        {
            iSubscribed = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgReceiver1(aCpDevice);

            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            base.Dispose();

            iSubscribed.Dispose();
            iSubscribed = null;

            iService.Dispose();
            iService = null;
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

        protected override void OnCancelSubscribe()
        {
            iSubscribed.Set();
        }

        private void HandleInitialEvent()
        {
            iProtocolInfo = iService.PropertyProtocolInfo();

            iSubscribed.Set();
        }

        protected override void OnUnsubscribe()
        {
            if (iService != null)
            {
                iService.Unsubscribe();
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
            IMediaMetadata metadata = iNetwork.TagManager.FromDidlLite(iService.PropertyMetadata());
            string uri = iService.PropertyUri();
            Network.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iMetadata.Update(new InfoMetadata(metadata, uri));
                });
            });
        }

        private void HandleTransportStateChanged()
        {
            string transportState = iService.PropertyTransportState();
            Network.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iTransportState.Update(transportState);
                });
            });
        }

        private ManualResetEvent iSubscribed;
        private CpProxyAvOpenhomeOrgReceiver1 iService;
    }

    class ServiceReceiverMock : ServiceReceiver, IMockable
    {
        public ServiceReceiverMock(INetwork aNetwork, IDevice aDevice, string aMetadata, string aProtocolInfo, string aTransportState, string aUri)
            : base(aNetwork, aDevice)
        {
            iProtocolInfo = aProtocolInfo;

            iMetadata.Update(new InfoMetadata(aNetwork.TagManager.FromDidlLite(aMetadata), aUri));
            iTransportState.Update(aTransportState);
        }

        public override Task Play()
        {
            return Start(() =>
            {
                iTransportState.Update("Playing");
            });
        }

        public override Task Stop()
        {
            return Start(() =>
            {
                iTransportState.Update("Stopped");
            });
        }

        public override Task SetSender(ISenderMetadata aMetadata)
        {
            return Start(() =>
            {
                iMetadata.Update(new InfoMetadata(Network.TagManager.FromDidlLite(aMetadata.ToString()), aMetadata.Uri));
            });
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
                if (value.Count() < 2)
                {
                    throw new NotSupportedException();
                }
                IInfoMetadata metadata = new InfoMetadata(Network.TagManager.FromDidlLite(string.Join(" ", value.Take(value.Count() - 1))), value.Last());
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
        public ProxyReceiver(ServiceReceiver aService)
            : base(aService)
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
