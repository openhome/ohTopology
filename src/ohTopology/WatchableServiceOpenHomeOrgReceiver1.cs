using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgReceiver1 : IWatchableService
    {
        IWatchable<string> Metadata { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<string> Uri { get; }
    }

    public class Receiver : IServiceOpenHomeOrgReceiver1
    {
        protected Receiver(string aId, IWatchableDevice aDevice, IServiceOpenHomeOrgReceiver1 aService)
        {
            iId = aId;
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        public string Id
        {
            get
            {
                return iId;
            }
        }

        public IWatchableDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public IWatchable<string> Metadata
        {
            get
            {
                return iService.Metadata;
            }
        }

        public IWatchable<string> TransportState
        {
            get
            {
                return iService.TransportState;
            }
        }

        public IWatchable<string> Uri
        {
            get
            {
                return iService.Uri;
            }
        }

        public string ProtocolInfo
        {
            get
            {
                return iProtocolInfo;
            }
        }

        private string iId;
        private IWatchableDevice iDevice;

        protected IServiceOpenHomeOrgReceiver1 iService;
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
                iService.SetPropertyUriChanged(HandleUriChanged);

                iMetadata = new Watchable<string>(aThread, string.Format("Metadata({0})", aId), iService.PropertyMetadata());
                iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aId), iService.PropertyTransportState());
                iUri = new Watchable<string>(aThread, string.Format("Uri({0})", aId), iService.PropertyUri());
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

                iDisposed = true;
            }
        }

        public IWatchable<string> Metadata
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

        public IWatchable<string> Uri
        {
            get
            {
                return iUri;
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

                iMetadata.Update(iService.PropertyMetadata());
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

        private void HandleUriChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iUri.Update(iService.PropertyUri());
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgReceiver1 iService;

        private Watchable<string> iMetadata;
        private Watchable<string> iTransportState;
        private Watchable<string> iUri;
    }

    public class MockServiceOpenHomeOrgReceiver1 : IServiceOpenHomeOrgReceiver1, IMockable
    {
        public MockServiceOpenHomeOrgReceiver1(IWatchableThread aThread, string aId, string aMetadata, string aTransportState, string aUri)
        {
            iMetadata = new Watchable<string>(aThread, string.Format("Metadata({0})", aId), aMetadata);
            iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aId), aTransportState);
            iUri = new Watchable<string>(aThread, string.Format("Uri({0})", aId), aUri);
        }

        public void Dispose()
        {
        }

        public IWatchable<string> Metadata
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

        public IWatchable<string> Uri
        {
            get
            {
                return iUri;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "metadata")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iMetadata.Update(value.First());
            }
            else if (command == "transportstate")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iTransportState.Update(value.First());
            }
            else if (command == "uri")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iUri.Update(value.First());
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private Watchable<string> iMetadata;
        private Watchable<string> iTransportState;
        private Watchable<string> iUri;
    }

    public class WatchableReceiverFactory : IWatchableServiceFactory
    {
        public WatchableReceiverFactory(IWatchableThread aThread)
        {
            iThread = aThread;

            iService = null;
        }

        public Type Type
        {
            get
            {
                return typeof(Receiver);
            }
        }

        public void Subscribe(IWatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            if (iService == null && iPendingService == null)
            {
                WatchableDevice d = aDevice as WatchableDevice;
                iPendingService = new CpProxyAvOpenhomeOrgReceiver1(d.Device);
                iPendingService.SetPropertyInitialEvent(delegate
                {
                    iThread.Schedule(() =>
                    {
                        iService = new WatchableReceiver(iThread, string.Format("Receiver({0})", aDevice.Udn), aDevice, iPendingService);
                        iPendingService = null;
                        aCallback(iService);
                    });
                });
                iPendingService.Subscribe();
            }
        }

        public void Unsubscribe()
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

        private CpProxyAvOpenhomeOrgReceiver1 iPendingService;
        private WatchableReceiver iService;
        private IWatchableThread iThread;
    }

    public class WatchableReceiver : Receiver
    {
        public WatchableReceiver(IWatchableThread aThread, string aId, IWatchableDevice aDevice, CpProxyAvOpenhomeOrgReceiver1 aService)
            : base(aId, aDevice, new ServiceOpenHomeOrgReceiver1(aThread, aId, aService))
        {
            iProtocolInfo = aService.PropertyProtocolInfo();
        }
    }

    public class MockWatchableReceiverFactory : IWatchableServiceFactory
    {
        public MockWatchableReceiverFactory(IWatchableThread aThread, string aMetadata, string aProtocolInfo, string aTransportState, string aUri)
        {
            iThread = aThread;

            iPendingService = false;
            iService = null;

            iMetadata = aMetadata;
            iProtocolInfo = aProtocolInfo;
            iTransportState = aTransportState;
            iUri = aUri;
        }

        public Type Type
        {
            get
            {
                return typeof(Receiver);
            }
        }

        public void Subscribe(IWatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            if (iService == null && iPendingService == false)
            {
                iPendingService = true;
                iThread.Schedule(() =>
                {
                    if (iPendingService)
                    {
                        iService = new MockWatchableReceiver(iThread, string.Format("Receiver({0})", aDevice.Udn), aDevice, iMetadata, iProtocolInfo, iTransportState, iUri);
                        iPendingService = false;
                        aCallback(iService);
                    }
                });
            }
        }

        public void Unsubscribe()
        {
            iPendingService = false;

            if (iService != null)
            {
                iService.Dispose();
                iService = null;
            }
        }

        private bool iPendingService;
        private MockWatchableReceiver iService;
        private IWatchableThread iThread;

        private string iMetadata;
        private string iProtocolInfo;
        private string iTransportState;
        private string iUri;
    }

    public class MockWatchableReceiver : Receiver, IMockable
    {
        public MockWatchableReceiver(IWatchableThread aThread, string aId, IWatchableDevice aDevice, string aMetadata, string aProtocolInfo, string aTransportState, string aUri)
            : base(aId, aDevice, new MockServiceOpenHomeOrgReceiver1(aThread, aId, aMetadata, aTransportState, aUri))
        {
            iProtocolInfo = aProtocolInfo;
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if(command == "metadata" || command == "transportstate" || command == "uri")
            {
                MockServiceOpenHomeOrgReceiver1 r = iService as MockServiceOpenHomeOrgReceiver1;
                r.Execute(aValue);
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
    }
}
