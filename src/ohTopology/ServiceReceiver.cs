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
        Task Play(ISenderMetadata aMetadata);
        Task Stop();
    }

    public abstract class ServiceReceiver : Service
    {
        protected ServiceReceiver(INetwork aNetwork, IInjectorDevice aDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iMetadata = new Watchable<IInfoMetadata>(aNetwork, "Metadata", InfoMetadata.Empty);
            iTransportState = new Watchable<string>(aNetwork, "TransportState", string.Empty);
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
        public abstract Task Play(ISenderMetadata aMetadata);
        public abstract Task Stop();

        protected string iProtocolInfo;

        protected Watchable<IInfoMetadata> iMetadata;
        protected Watchable<string> iTransportState;
    }

    class ServiceReceiverNetwork : ServiceReceiver
    {
        public ServiceReceiverNetwork(INetwork aNetwork, IInjectorDevice aDevice, CpDevice aCpDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iCpDevice = aCpDevice;
            iCpDevice.AddRef();

            iService = new CpProxyAvOpenhomeOrgReceiver1(aCpDevice);

            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            base.Dispose();

            iService.Dispose();
            iService = null;

            iCpDevice.RemoveRef();
        }

        protected override Task OnSubscribe()
        {
            Do.Assert(iSubscribedSource == null);

            iSubscribedSource = new TaskCompletionSource<bool>();

            iService.Subscribe();

            return iSubscribedSource.Task.ContinueWith((t) => { });
        }

        protected override void OnCancelSubscribe()
        {
            if (iSubscribedSource != null)
            {
                iSubscribedSource.TrySetCanceled();
            }
        }

        private void HandleInitialEvent()
        {
            iProtocolInfo = iService.PropertyProtocolInfo();

            if (!iSubscribedSource.Task.IsCanceled)
            {
                iSubscribedSource.SetResult(true);
            }
        }

        protected override void OnUnsubscribe()
        {
            if (iService != null)
            {
                iService.Unsubscribe();
            }

            iSubscribedSource = null;
        }

        public override Task Play()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginPlay((ptr) =>
            {
                try
                {
                    iService.EndPlay(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task Play(ISenderMetadata aMetadata)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetSender(aMetadata.Uri, aMetadata.ToString(), (ptr1) =>
            {
                try
                {
                    iService.EndSetSender(ptr1);
                    iService.BeginPlay((ptr2) =>
                    {
                        try
                        {
                            iService.EndPlay(ptr2);
                            taskSource.SetResult(true);
                        }
                        catch (Exception e)
                        {
                            taskSource.SetException(e);
                        }
                    });
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task Stop()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginStop((ptr) =>
            {
                try
                {
                    iService.EndStop(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        private void HandleMetadataChanged()
        {
            IMediaMetadata metadata = iNetwork.TagManager.FromDidlLite(iService.PropertyMetadata());
            string uri = iService.PropertyUri();
            iNetwork.Schedule(() =>
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
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iTransportState.Update(transportState);
                });
            });
        }

        private readonly CpDevice iCpDevice;
        private TaskCompletionSource<bool> iSubscribedSource;
        private CpProxyAvOpenhomeOrgReceiver1 iService;
    }

    class ServiceReceiverMock : ServiceReceiver, IMockable
    {
        public ServiceReceiverMock(INetwork aNetwork, IInjectorDevice aDevice, string aMetadata, string aProtocolInfo,
            string aTransportState, string aUri, ILog aLog)
            : base(aNetwork, aDevice, aLog)
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

        public override Task Play(ISenderMetadata aMetadata)
        {
            return Start(() =>
            {
                iMetadata.Update(new InfoMetadata(iNetwork.TagManager.FromDidlLite(aMetadata.ToString()), aMetadata.Uri));
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
                IInfoMetadata metadata = new InfoMetadata(iNetwork.TagManager.FromDidlLite(string.Join(" ", value.Take(value.Count() - 1))), value.Last());
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
        public ProxyReceiver(ServiceReceiver aService, IDevice aDevice)
            : base(aService, aDevice)
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

        public Task Play(ISenderMetadata aMetadata)
        {
            return iService.Play(aMetadata);
        }

        public Task Stop()
        {
            return iService.Stop();
        }
    }
}
