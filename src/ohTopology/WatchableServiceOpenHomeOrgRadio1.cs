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
        IWatchable<IList<uint>> IdArray { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<IInfoMetadata> Metadata { get; }

        void Play(Action aAction);
        void Pause(Action aAction);
        void Stop(Action aAction);
        void SeekSecondsAbsolute(uint aValue, Action aAction);
        void SeekSecondsRelative(int aValue, Action aAction);

        void SetId(uint aId, string aUri, Action aAction);
        void SetChannel(string aUri, string aMetadata, Action aAction);

        void Read(uint aId, Action<string> aAction);
        void ReadList(string aIdList, Action<string> aAction);
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
        
        public IWatchable<IList<uint>> IdArray
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

        public void Play(Action aAction)
        {
            Service.Play(aAction);
        }

        public void Pause(Action aAction)
        {
            Service.Pause(aAction);
        }

        public void Stop(Action aAction)
        {
            Service.Stop(aAction);
        }

        public void SeekSecondsAbsolute(uint aValue, Action aAction)
        {
            Service.SeekSecondsAbsolute(aValue, aAction);
        }

        public void SeekSecondsRelative(int aValue, Action aAction)
        {
            Service.SeekSecondsRelative(aValue, aAction);
        }

        public void SetId(uint aId, string aUri, Action aAction)
        {
            Service.SetId(aId, aUri, aAction);
        }

        public void SetChannel(string aUri, string aMetadata, Action aAction)
        {
            Service.SetChannel(aUri, aMetadata, aAction);
        }

        public void Read(uint aId, Action<string> aAction)
        {
            Service.Read(aId, aAction);
        }

        public void ReadList(string aIdList, Action<string> aAction)
        {
            Service.ReadList(aIdList, aAction);
        }

        protected uint iChannelsMax;
        protected string iProtocolInfo;
    }

    public class ServiceOpenHomeOrgRadio1 : IServiceOpenHomeOrgRadio1
    {
        public ServiceOpenHomeOrgRadio1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgRadio1 aService)
        {
            iThread = aThread;

            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyIdChanged(HandleIdChanged);
                iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
                iService.SetPropertyMetadataChanged(HandleMetadataChanged);
                iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

                iId = new Watchable<uint>(iThread, string.Format("Id({0})", aId), iService.PropertyId());
                iIdArray = new Watchable<IList<uint>>(iThread, string.Format("IdArray({0})", aId), ByteArray.Unpack(iService.PropertyIdArray()));
                iTransportState = new Watchable<string>(iThread, string.Format("TransportState({0})", aId), iService.PropertyTransportState());
                iMetadata = new Watchable<IInfoMetadata>(iThread, string.Format("Metadata({0})", aId), new InfoMetadata(iService.PropertyMetadata(), iService.PropertyUri()));
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

        public void Play(Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.Play");
                }

                iService.BeginPlay((IntPtr ptr) =>
                {
                    iService.EndPlay(ptr);

                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void Pause(Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.Pause");
                }

                iService.BeginPause((IntPtr ptr) =>
                {
                    iService.EndPause(ptr);

                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void Stop(Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.Stop");
                }

                iService.BeginStop((IntPtr ptr) =>
                {
                    iService.EndStop(ptr);

                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void SeekSecondsAbsolute(uint aValue, Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.SeekSecondsAbsolute");
                }

                iService.BeginSeekSecondAbsolute(aValue, (IntPtr ptr) =>
                {
                    iService.EndSeekSecondAbsolute(ptr);

                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void SeekSecondsRelative(int aValue, Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.SeekSecondsRelative");
                }

                iService.BeginSeekSecondRelative(aValue, (IntPtr ptr) =>
                {
                    iService.EndSeekSecondRelative(ptr);

                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void SetId(uint aId, string aUri, Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.SetId");
                }

                iService.BeginSetId(aId, aUri, (IntPtr ptr) =>
                {
                    iService.EndSetId(ptr);

                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void SetChannel(string aUri, string aMetadata, Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.SetChannel");
                }

                iService.BeginSetChannel(aUri, aMetadata, (IntPtr ptr) =>
                {
                    iService.EndSetChannel(ptr);

                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void Read(uint aId, Action<string> aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.Read");
                }

                iService.BeginRead(aId, (IntPtr ptr) =>
                {
                    string metadata;
                    iService.EndRead(ptr, out metadata);

                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction(metadata);
                        }
                    });
                });
            }
        }

        public void ReadList(string aIdList, Action<string> aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgRadio1.ReadList");
                }

                iService.BeginReadList(aIdList, (IntPtr ptr) =>
                {
                    string channelList;
                    iService.EndReadList(ptr, out channelList);

                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction(channelList);
                        }
                    });
                });
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

                iThread.Schedule(() =>
                {
                    iId.Update(iService.PropertyId());
                });
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

                iThread.Schedule(() =>
                {
                    iIdArray.Update(ByteArray.Unpack(iService.PropertyIdArray()));
                });
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

                iThread.Schedule(() =>
                {
                    iMetadata.Update(
                        new InfoMetadata(
                            iService.PropertyMetadata(),
                            iService.PropertyUri()
                        ));
                });
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

                iThread.Schedule(() =>
                {
                    iTransportState.Update(iService.PropertyTransportState());
                });
            }
        }

        private object iLock;
        private bool iDisposed;

        private IWatchableThread iThread;
        private CpProxyAvOpenhomeOrgRadio1 iService;

        private Watchable<uint> iId;
        private Watchable<IList<uint>> iIdArray;
        private Watchable<string> iTransportState;
        private Watchable<IInfoMetadata> iMetadata;
    }

    public class MockServiceOpenHomeOrgRadio1 : IServiceOpenHomeOrgRadio1, IMockable
    {
        public MockServiceOpenHomeOrgRadio1(IWatchableThread aThread, string aServiceId, uint aId, IList<uint> aIdArray, IInfoMetadata aMetadata, string aTransportState)
        {
            iThread = aThread;

            iId = new Watchable<uint>(iThread, string.Format("Id({0})", aServiceId), aId);
            iIdArray = new Watchable<IList<uint>>(iThread, string.Format("IdArray({0})", aServiceId), aIdArray);
            iMetadata = new Watchable<IInfoMetadata>(iThread, string.Format("Metadata({0})", aServiceId), aMetadata);
            iTransportState = new Watchable<string>(iThread, string.Format("TransportState({0})", aServiceId), aTransportState);
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

        public void Play(Action aAction)
        {
            iTransportState.Update("Playing");
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void Pause(Action aAction)
        {
            iTransportState.Update("Paused");
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void Stop(Action aAction)
        {
            iTransportState.Update("Stopped");
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void SeekSecondsAbsolute(uint aValue, Action aAction)
        {
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void SeekSecondsRelative(int aValue, Action aAction)
        {
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void SetId(uint aId, string aUri, Action aAction)
        {
            iId.Update(aId);
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void SetChannel(string aUri, string aMetadata, Action aAction)
        {
            iMetadata.Update(new InfoMetadata(aMetadata, aUri));
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void Read(uint aId, Action<string> aAction)
        {
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction(string.Empty);
                }
            });
        }

        public void ReadList(string aIdList, Action<string> aAction)
        {
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction(string.Empty);
                }
            });
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "id")
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

        private IWatchableThread iThread;

        private Watchable<uint> iId;
        private Watchable<IList<uint>> iIdArray;
        private Watchable<IInfoMetadata> iMetadata;
        private Watchable<string> iTransportState;
    }

    public class WatchableRadioFactory : IWatchableServiceFactory
    {
        public WatchableRadioFactory(IWatchableThread aThread, IWatchableThread aSubscribeThread)
        {
            iLock = new object();
            iDisposed = false;
            iPendingSubscribes = new List<Action<IWatchableService>>();

            iThread = aThread;
            iSubscribeThread = aSubscribeThread;
        }

        public void Dispose()
        {
            iSubscribeThread.Execute(() =>
            {
                Unsubscribe();
                iDisposed = true;
            });
        }

        public void Subscribe(IWatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            iSubscribeThread.Schedule(() =>
            {
                lock (iLock)
                {
                    if (!iDisposed)
                    {
                        if (iPendingService == null)
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
                                        foreach (Action<IWatchableService> c in iPendingSubscribes)
                                        {
                                            c(iService);
                                        }
                                        iPendingSubscribes.Clear();
                                    }
                                }
                            });
                            iPendingService.Subscribe();
                        }
                        else
                        {
                            iPendingSubscribes.Add(aCallback);
                        }
                    }
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
                }
            });
        }

        private object iLock;
        private bool iDisposed;
        private IWatchableThread iSubscribeThread;
        private CpProxyAvOpenhomeOrgRadio1 iPendingService;
        private WatchableRadio iService;
        private IWatchableThread iThread;
        private List<Action<IWatchableService>> iPendingSubscribes;
    }

    public class WatchableRadio : Radio
    {
        public WatchableRadio(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgRadio1 aService)
        {
            iChannelsMax = aService.PropertyChannelsMax();
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
        public MockWatchableRadio(IWatchableThread aThread, string aServiceId, uint aId, IList<uint> aIdArray, string aMetadata, string aProtocolInfo, string aTransportState, string aUri, uint aChannelsMax)
        {
            iChannelsMax = aChannelsMax;
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
            else if (command == "channelsmax")
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

        public void Play(Action aAction)
        {
            iService.Play(aAction);
        }

        public void Pause(Action aAction)
        {
            iService.Pause(aAction);
        }

        public void Stop(Action aAction)
        {
            iService.Stop(aAction);
        }

        public void SeekSecondsAbsolute(uint aValue, Action aAction)
        {
            iService.SeekSecondsAbsolute(aValue, aAction);
        }

        public void SeekSecondsRelative(int aValue, Action aAction)
        {
            iService.SeekSecondsRelative(aValue, aAction);
        }

        public void SetId(uint aId, string aUri, Action aAction)
        {
            iService.SetId(aId, aUri, aAction);
        }

        public void SetChannel(string aUri, string aMetadata, Action aAction)
        {
            iService.SetChannel(aUri, aMetadata, aAction);
        }

        public void Read(uint aId, Action<string> aAction)
        {
            iService.Read(aId, aAction);
        }

        public void ReadList(string aIdList, Action<string> aAction)
        {
            iService.ReadList(aIdList, aAction);
        }

        private IManagableWatchableDevice iDevice;
        private IRadio iService;
    }
}
