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
    public interface IServiceReceiver
    {
        IWatchable<IInfoMetadata> Metadata { get; }
        IWatchable<string> TransportState { get; }

        Task Play();
        Task Stop();
        Task SetSender(ISenderMetadata aMetadata);
    }

    public interface IReceiver : IServiceReceiver
    {
        string ProtocolInfo{ get; }
    }

    public abstract class ServiceReceiver : Service, IReceiver
    {
        protected ServiceReceiver(INetwork aNetwork, string aId)
        {
            iMetadata = new Watchable<IInfoMetadata>(aNetwork, string.Format("Metadata({0})", aId), new InfoMetadata());
            iTransportState = new Watchable<string>(aNetwork, string.Format("TransportState({0})", aId), string.Empty);
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
            return new ProxyReceiver(this, aDevice);
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
        public ServiceReceiverNetwork(INetwork aNetwork, string aId, CpDevice aDevice)
            : base(aNetwork, aId)
        {
            iNetwork = aNetwork;
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
            iNetwork.Schedule(() =>
            {
                iMetadata.Update(new InfoMetadata(iService.PropertyMetadata(), iService.PropertyUri()));
            });
        }

        private void HandleTransportStateChanged()
        {
            iNetwork.Schedule(() =>
            {
                iTransportState.Update(iService.PropertyTransportState());
            });
        }

        private IWatchableThread iNetwork;
        private ManualResetEvent iSubscribe;
        private CpProxyAvOpenhomeOrgReceiver1 iService;
    }

    public class ServiceReceiverMock : ServiceReceiver, IMockable
    {
        public ServiceReceiverMock(INetwork aNetwork, string aId, string aMetadata, string aProtocolInfo, string aTransportState, string aUri)
            : base(aNetwork, aId)
        {
            iNetwork = aNetwork;

            iProtocolInfo = aProtocolInfo;

            iMetadata.Update(new InfoMetadata(aMetadata, aUri));
            iTransportState.Update(aTransportState);
        }

        protected override void OnSubscribe()
        {
            iNetwork.WatchableSubscribeThread.Execute(() =>
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

        public override Task SetSender(ISenderMetadata aMetadata)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    iMetadata.Update(new InfoMetadata(aMetadata.ToString(), aMetadata.Uri));
                });
            });
            return task;
        }

        public void Execute(IEnumerable<string> aValue)
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

        private INetwork iNetwork;
    }

    public class ProxyReceiver : IReceiver, IProxy
    {
        public ProxyReceiver(ServiceReceiver aService, IWatchableDevice aDevice)
        {
            iService = aService;
            iDevice = aDevice;
        }

        public void Dispose()
        {
            iService.Unsubscribe();
            iService = null;
        }

        public IWatchableDevice Device
        {
            get { return iDevice; }
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

        private ServiceReceiver iService;
        private IWatchableDevice iDevice;
    }
}
