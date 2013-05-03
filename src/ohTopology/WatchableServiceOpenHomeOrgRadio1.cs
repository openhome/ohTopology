using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgRadio1
    {
        IWatchable<uint> Id { get; }
        IWatchable<byte[]> IdArray { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<IInfoMetadata> Metadata { get; }

        void Play();
        void Pause();
        void Stop();
        void SeekSecondsAbsolute(uint aValue);
        void SeekSecondsRelative(int aValue);

        void SetId(uint aId, string aUri);
        void SetChannel(string aUri, string aMetadata);

        string Read(uint aId);
        string ReadList(string aIdList);
    }

    public interface IRadio : IServiceOpenHomeOrgRadio1
    {
        uint ChannelsMax { get; }
        string ProtocolInfo { get; }
    }

    public abstract class Radio : IRadio, IWatchableService
    {
        public abstract void Dispose();

        public IService Create(IManagableWatchableDevice aDevice)
        {
            return new ServiceRadio(aDevice, this);
        }

        internal abstract IServiceOpenHomeOrgRadio1 Service { get; }

        public IWatchable<uint> Id
        {
            get
            {
                return Service.Id;
            }
        }
        
        public IWatchable<byte[]> IdArray
        {
            get
            {
                return Service.IdArray;
            }
        }
        
        public IWatchable<string> TransportState
        {
            get
            {
                return Service.TransportState;
            }
        }
        
        public IWatchable<IInfoMetadata> Metadata
        {
            get
            {
                return Service.Metadata;
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

        public void Play()
        {
            Service.Play();
        }

        public void Pause()
        {
            Service.Pause();
        }

        public void Stop()
        {
            Service.Stop();
        }

        public void SeekSecondsAbsolute(uint aValue)
        {
            Service.SeekSecondsAbsolute(aValue);
        }

        public void SeekSecondsRelative(int aValue)
        {
            Service.SeekSecondsRelative(aValue);
        }

        public void SetId(uint aId, string aUri)
        {
            Service.SetId(aId, aUri);
        }

        public void SetChannel(string aUri, string aMetadata)
        {
            Service.SetChannel(aUri, aMetadata);
        }

        public string Read(uint aId)
        {
            return Service.Read(aId);
        }

        public string ReadList(string aIdList)
        {
            return Service.ReadList(aIdList);
        }

        protected uint iChannelsMax;
        protected string iProtocolInfo;
    }

    public class ServiceOpenHomeOrgRadio1 : IServiceOpenHomeOrgRadio1
    {
        public ServiceOpenHomeOrgRadio1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgRadio1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyIdChanged(HandleIdChanged);
                iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
                iService.SetPropertyMetadataChanged(HandleMetadataChanged);
                iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

                iId = new Watchable<uint>(aThread, string.Format("Id({0})", aId), iService.PropertyId());
                iIdArray = new Watchable<byte[]>(aThread, string.Format("IdArray({0})", aId), iService.PropertyIdArray());
                iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aId), iService.PropertyTransportState());
                iMetadata = new Watchable<IInfoMetadata>(aThread, string.Format("Metadata({0})", aId), new InfoMetadata(iService.PropertyMetadata(), iService.PropertyUri()));
            }
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.Dispose");
                }

                iService.Dispose();
                iService = null;

                iId.Dispose();
                iId = null;

                iIdArray.Dispose();
                iIdArray = null;

                iTransportState.Dispose();
                iTransportState = null;

                iMetadata.Dispose();
                iMetadata = null;

                iDisposed = true;
            }
        }

        public IWatchable<uint> Id
        {
            get
            {
                return iId;
            }
        }

        public IWatchable<byte[]> IdArray
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

        public void Play()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.Play");
                }

                iService.BeginPlay(null);
            }
        }

        public void Pause()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.Pause");
                }

                iService.BeginPause(null);
            }
        }

        public void Stop()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.Stop");
                }

                iService.BeginStop(null);
            }
        }

        public void SeekSecondsAbsolute(uint aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.SeekSecondsAbsolute");
                }

                iService.BeginSeekSecondAbsolute(aValue, null);
            }
        }

        public void SeekSecondsRelative(int aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.SeekSecondsRelative");
                }

                iService.BeginSeekSecondRelative(aValue, null);
            }
        }

        public void SetId(uint aId, string aUri)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.SetId");
                }

                iService.BeginSetId(aId, aUri, null);
            }
        }

        public void SetChannel(string aUri, string aMetadata)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.SetChannel");
                }

                iService.BeginSetChannel(aUri, aMetadata, null);
            }
        }

        public string Read(uint aId)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.Read");
                }

                string metadata;
                iService.SyncRead(aId, out metadata);
                return metadata;
            }
        }

        public string ReadList(string aIdList)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.ReadList");
                }

                string metadata;
                iService.SyncReadList(aIdList, out metadata);
                return metadata;
            }
        }

        private void HandleIdChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iId.Update(iService.PropertyId());
            }
        }

        private void HandleIdArrayChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iIdArray.Update(iService.PropertyIdArray());
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

                iMetadata.Update(
                    new InfoMetadata(
                        iService.PropertyMetadata(),
                        iService.PropertyUri()
                    ));
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

        private CpProxyAvOpenhomeOrgRadio1 iService;

        private Watchable<uint> iId;
        private Watchable<byte[]> iIdArray;
        private Watchable<string> iTransportState;
        private Watchable<IInfoMetadata> iMetadata;
    }

    public class MockServiceOpenHomeOrgRadio1 : IServiceOpenHomeOrgRadio1, IMockable
    {
        public MockServiceOpenHomeOrgRadio1(IWatchableThread aThread, string aServiceId, uint aId, byte[] aIdArray, IInfoMetadata aMetadata, string aTransportState)
        {
            iId = new Watchable<uint>(aThread, string.Format("Id({0})", aServiceId), aId);
            iIdArray = new Watchable<byte[]>(aThread, string.Format("IdArray({0})", aServiceId), aIdArray);
            iMetadata = new Watchable<IInfoMetadata>(aThread, string.Format("Metadata({0})", aServiceId), aMetadata);
            iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aServiceId), aTransportState);
        }

        public void Dispose()
        {
            iId.Dispose();
            iId = null;

            iIdArray.Dispose();
            iIdArray = null;

            iTransportState.Dispose();
            iTransportState = null;

            iMetadata.Dispose();
            iMetadata = null;
        }

        public IWatchable<uint> Id
        {
            get
            {
                return iId;
            }
        }

        public IWatchable<byte[]> IdArray
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

        public void Play()
        {
            iTransportState.Update("Playing");
        }

        public void Pause()
        {
            iTransportState.Update("Paused");
        }

        public void Stop()
        {
            iTransportState.Update("Stopped");
        }

        public void SeekSecondsAbsolute(uint aValue)
        {
        }

        public void SeekSecondsRelative(int aValue)
        {
        }

        public void SetId(uint aId, string aUri)
        {
            iId.Update(aId);
        }

        public void SetChannel(string aUri, string aMetadata)
        {
            iMetadata.Update(new InfoMetadata(aMetadata, aUri));
        }

        public string Read(uint aId)
        {
            return string.Empty;
        }

        public string ReadList(string aIdList)
        {
            return string.Empty;
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "id")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iId.Update(uint.Parse(value.First()));
            }
            /*else if (command == "idarray")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iIdArray.Update();
            }*/
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

        private Watchable<uint> iId;
        private Watchable<byte[]> iIdArray;
        private Watchable<IInfoMetadata> iMetadata;
        private Watchable<string> iTransportState;
    }

    public class WatchableRadioFactory : IWatchableServiceFactory
    {
        public WatchableRadioFactory(IWatchableThread aThread, IWatchableThread aSubscribeThread)
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
                    iPendingService = new CpProxyAvOpenhomeOrgRadio1(d.Device);
                    iPendingService.SetPropertyInitialEvent(delegate
                    {
                        lock (iLock)
                        {
                            if (iPendingService != null)
                            {
                                iService = new WatchableRadio(iThread, string.Format("Radio({0})", aDevice.Udn), iPendingService);
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
        private CpProxyAvOpenhomeOrgRadio1 iPendingService;
        private WatchableRadio iService;
        private IWatchableThread iThread;
    }

    public class WatchableRadio : Radio
    {
        public WatchableRadio(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgRadio1 aService)
        {
            iProtocolInfo = aService.PropertyProtocolInfo();

            iService = new ServiceOpenHomeOrgRadio1(aThread, aId, aService);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgRadio1 Service
        {
            get
            {
                return iService;
            }
        }

        private ServiceOpenHomeOrgRadio1 iService;
    }

    public class MockWatchableRadio : Radio, IMockable
    {
        public MockWatchableRadio(IWatchableThread aThread, string aServiceId, uint aId, byte[] aIdArray, string aMetadata, string aProtocolInfo, string aTransportState, string aUri)
        {
            iProtocolInfo = aProtocolInfo;

            iService = new MockServiceOpenHomeOrgRadio1(aThread, aServiceId, aId, aIdArray, new InfoMetadata(aMetadata, aUri), aTransportState);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgRadio1 Service
        {
            get
            {
                return iService;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "id" || command == "idarray" || command == "metadata" || command == "transportstate" || command == "uri")
            {
                iService.Execute(aValue);
            }
            else if (command == "channelmax")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iChannelsMax = uint.Parse(value.First());
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

        private MockServiceOpenHomeOrgRadio1 iService;
    }

    public class ServiceRadio : IRadio, IService
    {
        public ServiceRadio(IManagableWatchableDevice aDevice, IRadio aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iDevice.Unsubscribe<ServiceRadio>();
            iDevice = null;
        }

        public IWatchableDevice Device
        {
            get { return iDevice; }
        }

        public IWatchable<uint> Id
        {
            get { return iService.Id; }
        }

        public IWatchable<byte[]> IdArray
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

        public void Play()
        {
            iService.Play();
        }

        public void Pause()
        {
            iService.Pause();
        }

        public void Stop()
        {
            iService.Stop();
        }

        public void SeekSecondsAbsolute(uint aValue)
        {
            iService.SeekSecondsAbsolute(aValue);
        }

        public void SeekSecondsRelative(int aValue)
        {
            iService.SeekSecondsRelative(aValue);
        }

        public void SetId(uint aId, string aUri)
        {
            iService.SetId(aId, aUri);
        }

        public void SetChannel(string aUri, string aMetadata)
        {
            iService.SetChannel(aUri, aMetadata);
        }

        public string Read(uint aId)
        {
            return iService.Read(aId);
        }

        public string ReadList(string aIdList)
        {
            return iService.ReadList(aIdList);
        }

        private IManagableWatchableDevice iDevice;
        private IRadio iService;
    }
}
