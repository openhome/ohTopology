using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgReceiver1
    {
        IWatchable<IInfoMetadata> Metadata { get; }
        IWatchable<string> TransportState { get; }

        void Play();
        void Stop();
        void SetSender(string aUri, string aMetadata);
    }

    public interface IReceiver : IServiceOpenHomeOrgReceiver1
    {
        string ProtocolInfo{ get; }
    }

    public abstract class Receiver : IReceiver, IWatchableService
    {
        public abstract void Dispose();

        public IService Create(IManagableWatchableDevice aDevice)
        {
            return new ServiceReceiver(aDevice, this);
        }

        internal abstract IServiceOpenHomeOrgReceiver1 Service { get; }

        public IWatchable<IInfoMetadata> Metadata
        {
            get
            {
                return Service.Metadata;
            }
        }

        public IWatchable<string> TransportState
        {
            get
            {
                return Service.TransportState;
            }
        }

        public string ProtocolInfo
        {
            get
            {
                return iProtocolInfo;
            }
        }

        public void Play()
        {
            Service.Play();
        }

        public void Stop()
        {
            Service.Stop();
        }

        public void SetSender(string aUri, string aMetadata)
        {
            Service.SetSender(aUri, aMetadata);
        }

        protected string iProtocolInfo;
    }

    public class ServiceOpenHomeOrgReceiver1 : IServiceOpenHomeOrgReceiver1
    {
        public ServiceOpenHomeOrgReceiver1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgReceiver1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyMetadataChanged(HandleMetadataChanged);
                iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

                iMetadata = new Watchable<IInfoMetadata>(aThread, string.Format("Metadata({0})", aId), new InfoMetadata(iService.PropertyMetadata(), iService.PropertyUri()));
                iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aId), iService.PropertyTransportState());
            }
        }
        
        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgReceiver1.Dispose");
                }

                iService.Dispose();
                iService = null;

                iMetadata.Dispose();
                iMetadata = null;

                iTransportState.Dispose();
                iTransportState = null;

                iDisposed = true;
            }
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

        public void Play()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgReceiver1.Play");
                }

                iService.BeginPlay(null);
            }
        }

        public void Stop()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgReceiver1.Stop");
                }

                iService.BeginStop(null);
            }
        }

        public void SetSender(string aUri, string aMetadata)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgReceiver1.SetSender");
                }

                iService.BeginSetSender(aUri, aMetadata, null);
            }
        }

        private void HandleMetadataChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iMetadata.Update(new InfoMetadata(iService.PropertyMetadata(), iService.PropertyUri()));
            }
        }

        private void HandleTransportStateChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iTransportState.Update(iService.PropertyTransportState());
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgReceiver1 iService;

        private Watchable<IInfoMetadata> iMetadata;
        private Watchable<string> iTransportState;
    }

    public class MockServiceOpenHomeOrgReceiver1 : IServiceOpenHomeOrgReceiver1, IMockable
    {
        public MockServiceOpenHomeOrgReceiver1(IWatchableThread aThread, string aId, string aMetadata, string aTransportState, string aUri)
        {
            iMetadata = new Watchable<IInfoMetadata>(aThread, string.Format("Metadata({0})", aId), new InfoMetadata(aMetadata, aUri));
            iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aId), aTransportState);
        }

        public void Dispose()
        {
            iMetadata.Dispose();
            iMetadata = null;

            iTransportState.Dispose();
            iTransportState = null;
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

        public void Play()
        {
            iTransportState.Update("Playing");
        }

        public void Stop()
        {
            iTransportState.Update("Stopped");
        }

        public void SetSender(string aUri, string aMetadata)
        {
            iMetadata.Update(new InfoMetadata(aMetadata, aUri));
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "metadata")
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

        private Watchable<IInfoMetadata> iMetadata;
        private Watchable<string> iTransportState;
    }

    public class WatchableReceiverFactory : IWatchableServiceFactory
    {
        public WatchableReceiverFactory(IWatchableThread aThread, IWatchableThread aSubscribeThread)
        {
            iLock = new object();
            iThread = aThread;

            iSubscribeThread = aSubscribeThread;
        }

        public void Subscribe(IWatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            iSubscribeThread.Schedule(() =>
            {
                if (iService == null && iPendingService == null)
                {
                    WatchableDevice d = aDevice as WatchableDevice;
                    iPendingService = new CpProxyAvOpenhomeOrgReceiver1(d.Device);
                    iPendingService.SetPropertyInitialEvent(delegate
                    {
                        lock (iLock)
                        {
                            if (iPendingService != null)
                            {
                                iService = new WatchableReceiver(iThread, string.Format("Receiver({0})", aDevice.Udn), iPendingService);
                                iPendingService = null;
                                aCallback(iService);
                            }
                        }
                    });
                    iPendingService.Subscribe();
                }
            });
        }

        public void Unsubscribe()
        {
            iSubscribeThread.Schedule(() =>
            {
                lock (iLock)
                {
                    if (iPendingService != null)
                    {
                        iPendingService.Dispose();
                        iPendingService = null;
                    }

                    if (iService != null)
                    {
                        iService.Dispose();
                        iService = null;
                    }
                }
            });
        }

        private object iLock;
        private IWatchableThread iSubscribeThread;
        private CpProxyAvOpenhomeOrgReceiver1 iPendingService;
        private WatchableReceiver iService;
        private IWatchableThread iThread;
    }

    public class WatchableReceiver : Receiver
    {
        public WatchableReceiver(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgReceiver1 aService)
        {
            iProtocolInfo = aService.PropertyProtocolInfo();

            iService = new ServiceOpenHomeOrgReceiver1(aThread, aId, aService);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgReceiver1 Service
        {
            get
            {
                return iService;
            }
        }

        private ServiceOpenHomeOrgReceiver1 iService;
    }
    
    public class MockWatchableReceiver : Receiver, IMockable
    {
        public MockWatchableReceiver(IWatchableThread aThread, string aId, string aMetadata, string aProtocolInfo, string aTransportState, string aUri)
        {
            iProtocolInfo = aProtocolInfo;

            iService = new MockServiceOpenHomeOrgReceiver1(aThread, aId, aMetadata, aTransportState, aUri);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgReceiver1 Service
        {
            get
            {
                return iService;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if(command == "metadata" || command == "transportstate")
            {
                iService.Execute(aValue);
            }
            else if (command == "protocolinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iProtocolInfo = string.Join(" ", value);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private MockServiceOpenHomeOrgReceiver1 iService;
    }

    public class ServiceReceiver : IReceiver, IService
    {
        public ServiceReceiver(IManagableWatchableDevice aDevice, IReceiver aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iDevice.Unsubscribe<ServiceReceiver>();
            iDevice = null;
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

        public void Play()
        {
            iService.Play();
        }

        public void Stop()
        {
            iService.Stop();
        }

        public void SetSender(string aUri, string aMetadata)
        {
            iService.SetSender(aUri, aMetadata);
        }

        private IManagableWatchableDevice iDevice;
        private IReceiver iService;
    }
}
