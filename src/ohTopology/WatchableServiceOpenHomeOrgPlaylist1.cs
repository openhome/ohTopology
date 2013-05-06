using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgPlaylist1
    {
        IWatchable<uint> Id { get; }
        IWatchable<IList<uint>> IdArray { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<bool> Repeat { get; }
        IWatchable<bool> Shuffle { get; }

        void Play();
        void Pause();
        void Stop();
        void Previous();
        void Next();
        void SeekId(uint aValue);
        void SeekIndex(uint aValue);
        void SeekSecondsAbsolute(uint aValue);
        void SeekSecondsRelative(int aValue);

        uint Insert(uint aAfterId, string aUri, string aMetadata);
        void DeleteId(uint aValue);
        void DeleteAll();
        void SetRepeat(bool aValue);
        void SetShuffle(bool aValue);

        IInfoMetadata Read(uint aId);
        string ReadList(string aIdList);
    }

    public interface IPlaylist : IServiceOpenHomeOrgPlaylist1
    {
        uint TracksMax { get; }
        string ProtocolInfo { get; }
    }

    public abstract class Playlist : IPlaylist, IWatchableService
    {
        public abstract void Dispose();

        public IService Create(IManagableWatchableDevice aDevice)
        {
            return new ServicePlaylist(aDevice, this);
        }

        internal abstract IServiceOpenHomeOrgPlaylist1 Service { get; }

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

        public IWatchable<bool> Repeat
        {
            get
            {
                return Service.Repeat;
            }
        }

        public IWatchable<bool> Shuffle
        {
            get
            {
                return Service.Shuffle;
            }
        }

        public uint TracksMax
        {
            get
            {
                return iTracksMax;
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

        public void Previous()
        {
            Service.Previous();
        }

        public void Next()
        {
            Service.Next();
        }

        public void SeekId(uint aValue)
        {
            Service.SeekId(aValue);
        }

        public void SeekIndex(uint aValue)
        {
            Service.SeekIndex(aValue);
        }

        public void SeekSecondsAbsolute(uint aValue)
        {
            Service.SeekSecondsAbsolute(aValue);
        }

        public void SeekSecondsRelative(int aValue)
        {
            Service.SeekSecondsRelative(aValue);
        }

        public uint Insert(uint aAfterId, string aUri, string aMetadata)
        {
            return Service.Insert(aAfterId, aUri, aMetadata);
        }

        public void DeleteId(uint aValue)
        {
            Service.DeleteId(aValue);
        }

        public void DeleteAll()
        {
            Service.DeleteAll();
        }

        public void SetRepeat(bool aValue)
        {
            Service.SetRepeat(aValue);
        }

        public void SetShuffle(bool aValue)
        {
            Service.SetShuffle(aValue);
        }

        public IInfoMetadata Read(uint aId)
        {
            return Service.Read(aId);
        }

        public string ReadList(string aIdList)
        {
            return Service.ReadList(aIdList);
        }

        protected uint iTracksMax;
        protected string iProtocolInfo;
    }

    public class ServiceOpenHomeOrgPlaylist1 : IServiceOpenHomeOrgPlaylist1
    {
        public ServiceOpenHomeOrgPlaylist1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgPlaylist1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyIdChanged(HandleIdChanged);
                iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
                iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);
                iService.SetPropertyRepeatChanged(HandleRepeatChanged);
                iService.SetPropertyShuffleChanged(HandleShuffleChanged);

                iId = new Watchable<uint>(aThread, string.Format("Id({0})", aId), iService.PropertyId());
                iIdArray = new Watchable<IList<uint>>(aThread, string.Format("IdArray({0})", aId), ByteArray.Unpack(iService.PropertyIdArray()));
                iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aId), iService.PropertyTransportState());
                iRepeat = new Watchable<bool>(aThread, string.Format("Repeat({0})", aId), iService.PropertyRepeat());
                iShuffle = new Watchable<bool>(aThread, string.Format("Shuffle({0})", aId), iService.PropertyShuffle());
            }
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.Dispose");
                }

                iService.Dispose();
                iService = null;

                iId.Dispose();
                iId = null;

                iIdArray.Dispose();
                iIdArray = null;

                iTransportState.Dispose();
                iTransportState = null;

                iRepeat.Dispose();
                iRepeat = null;

                iShuffle.Dispose();
                iShuffle = null;

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

        public IWatchable<bool> Repeat
        {
            get
            {
                return iRepeat;
            }
        }

        public IWatchable<bool> Shuffle
        {
            get
            {
                return iShuffle;
            }
        }

        public void Play()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.Play");
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
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.Pause");
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
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.Stop");
                }

                iService.BeginStop(null);
            }
        }

        public void Previous()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.Previous");
                }

                iService.BeginPrevious(null);
            }
        }

        public void Next()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.Next");
                }

                iService.BeginNext(null);
            }
        }

        public void SeekId(uint aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.SeekId");
                }

                iService.BeginSeekId(aValue, null);
            }
        }

        public void SeekIndex(uint aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.SeekIndex");
                }

                iService.BeginSeekIndex(aValue, null);
            }
        }

        public void SeekSecondsAbsolute(uint aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.SeekSecondsAbsolute");
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
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.SeekSecondsRelative");
                }

                iService.BeginSeekSecondRelative(aValue, null);
            }
        }

        public uint Insert(uint aAfterId, string aUri, string aMetadata)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.Insert");
                }

                uint newId;
                iService.SyncInsert(aAfterId, aUri, aMetadata, out newId);
                return newId;
            }
        }

        public void DeleteId(uint aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.DeleteId");
                }

                iService.BeginDeleteId(aValue, null);
            }
        }

        public void DeleteAll()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.DeleteAll");
                }

                iService.BeginDeleteAll(null);
            }
        }

        public void SetRepeat(bool aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.SetRepeat");
                }

                iService.BeginSetRepeat(aValue, null);
            }
        }

        public void SetShuffle(bool aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.SetShuffle");
                }

                iService.BeginSetShuffle(aValue, null);
            }
        }

        public IInfoMetadata Read(uint aId)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.Read");
                }

                string uri;
                string metadata;
                iService.SyncRead(aId, out uri, out metadata);
                return new InfoMetadata(uri, metadata);
            }
        }

        public string ReadList(string aIdList)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgPlaylist1.ReadList");
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

                iIdArray.Update(ByteArray.Unpack(iService.PropertyIdArray()));
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

        private void HandleRepeatChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iRepeat.Update(iService.PropertyRepeat());
            }
        }

        private void HandleShuffleChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iShuffle.Update(iService.PropertyShuffle());
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgPlaylist1 iService;

        private Watchable<uint> iId;
        private Watchable<IList<uint>> iIdArray;
        private Watchable<string> iTransportState;
        private Watchable<bool> iRepeat;
        private Watchable<bool> iShuffle;
    }

    public class MockServiceOpenHomeOrgPlaylist1 : IServiceOpenHomeOrgPlaylist1, IMockable
    {
        public MockServiceOpenHomeOrgPlaylist1(IWatchableThread aThread, string aServiceId, uint aId, IList<uint> aIdArray, bool aRepeat, bool aShuffle, string aTransportState)
        {
            iId = new Watchable<uint>(aThread, string.Format("Id({0})", aServiceId), aId);
            iIdArray = new Watchable<IList<uint>>(aThread, string.Format("IdArray({0})", aServiceId), aIdArray);
            iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aServiceId), aTransportState);
            iRepeat = new Watchable<bool>(aThread, string.Format("Repeat({0})", aServiceId), aRepeat);
            iShuffle = new Watchable<bool>(aThread, string.Format("Shuffle({0})", aServiceId), aShuffle);
        }

        public void Dispose()
        {
            iId.Dispose();
            iId = null;

            iIdArray.Dispose();
            iIdArray = null;

            iTransportState.Dispose();
            iTransportState = null;

            iRepeat.Dispose();
            iRepeat = null;

            iShuffle.Dispose();
            iShuffle = null;
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

        public IWatchable<bool> Repeat
        {
            get
            {
                return iRepeat;
            }
        }

        public IWatchable<bool> Shuffle
        {
            get
            {
                return iShuffle;
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

        public void Previous()
        {
        }

        public void Next()
        {
        }

        public void SeekId(uint aValue)
        {
        }

        public void SeekIndex(uint aValue)
        {
        }

        public void SeekSecondsAbsolute(uint aValue)
        {
        }

        public void SeekSecondsRelative(int aValue)
        {
        }

        public uint Insert(uint aAfterId, string aUri, string aMetadata)
        {
            return 0;
        }

        public void DeleteId(uint aValue)
        {
        }

        public void DeleteAll()
        {
        }

        public void SetRepeat(bool aValue)
        {
            iRepeat.Update(aValue);
        }

        public void SetShuffle(bool aValue)
        {
            iShuffle.Update(aValue);
        }

        public IInfoMetadata Read(uint aId)
        {
            return new InfoMetadata(string.Empty, string.Empty);
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
            else if (command == "transportstate")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iTransportState.Update(value.First());
            }
            else if (command == "repeat")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iRepeat.Update(bool.Parse(value.First()));
            }
            else if (command == "shuffle")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iShuffle.Update(bool.Parse(value.First()));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private Watchable<uint> iId;
        private Watchable<IList<uint>> iIdArray;
        private Watchable<string> iTransportState;
        private Watchable<bool> iRepeat;
        private Watchable<bool> iShuffle;
    }

    public class WatchablePlaylistFactory : IWatchableServiceFactory
    {
        public WatchablePlaylistFactory(IWatchableThread aThread, IWatchableThread aSubscribeThread)
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
                    iPendingService = new CpProxyAvOpenhomeOrgPlaylist1(d.Device);
                    iPendingService.SetPropertyInitialEvent(delegate
                    {
                        lock (iLock)
                        {
                            if (iPendingService != null)
                            {
                                iService = new WatchablePlaylist(iThread, string.Format("Playlist({0})", aDevice.Udn), iPendingService);
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
        private CpProxyAvOpenhomeOrgPlaylist1 iPendingService;
        private WatchablePlaylist iService;
        private IWatchableThread iThread;
    }

    public class WatchablePlaylist : Playlist
    {
        public WatchablePlaylist(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgPlaylist1 aService)
        {
            iTracksMax = aService.PropertyTracksMax();
            iProtocolInfo = aService.PropertyProtocolInfo();

            iService = new ServiceOpenHomeOrgPlaylist1(aThread, aId, aService);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgPlaylist1 Service
        {
            get
            {
                return iService;
            }
        }

        private ServiceOpenHomeOrgPlaylist1 iService;
    }

    public class MockWatchablePlaylist : Playlist, IMockable
    {
        public MockWatchablePlaylist(IWatchableThread aThread, string aServiceId, uint aId, IList<uint> aIdArray, bool aRepeat, bool aShuffle, string aTransportState, string aProtocolInfo, uint aTracksMax)
        {
            iTracksMax = aTracksMax;
            iProtocolInfo = aProtocolInfo;

            iService = new MockServiceOpenHomeOrgPlaylist1(aThread, aServiceId, aId, aIdArray, aRepeat, aShuffle, aTransportState);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgPlaylist1 Service
        {
            get
            {
                return iService;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "id" || command == "idarray" || command == "transportstate" || command == "repeat" || command == "shuffle")
            {
                iService.Execute(aValue);
            }
            else if (command == "tracksmax")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iTracksMax = uint.Parse(value.First());
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

        private MockServiceOpenHomeOrgPlaylist1 iService;
    }

    public class ServicePlaylist : IPlaylist, IService
    {
        public ServicePlaylist(IManagableWatchableDevice aDevice, IPlaylist aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iDevice.Unsubscribe<ServicePlaylist>();
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

        public IWatchable<bool> Repeat
        {
            get { return iService.Repeat; }
        }

        public IWatchable<bool> Shuffle
        {
            get { return iService.Shuffle; }
        }

        public uint TracksMax
        {
            get { return iService.TracksMax; }
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

        public void Previous()
        {
            iService.Previous();
        }

        public void Next()
        {
            iService.Next();
        }

        public void SeekId(uint aValue)
        {
            iService.SeekId(aValue);
        }

        public void SeekIndex(uint aValue)
        {
            iService.SeekIndex(aValue);
        }

        public void SeekSecondsAbsolute(uint aValue)
        {
            iService.SeekSecondsAbsolute(aValue);
        }

        public void SeekSecondsRelative(int aValue)
        {
            iService.SeekSecondsRelative(aValue);
        }

        public uint Insert(uint aAfterId, string aUri, string aMetadata)
        {
            return iService.Insert(aAfterId, aUri, aMetadata);
        }

        public void DeleteId(uint aValue)
        {
            iService.DeleteId(aValue);
        }

        public void DeleteAll()
        {
            iService.DeleteAll();
        }

        public void SetRepeat(bool aValue)
        {
            iService.SetRepeat(aValue);
        }

        public void SetShuffle(bool aValue)
        {
            iService.SetShuffle(aValue);
        }

        public IInfoMetadata Read(uint aId)
        {
            return iService.Read(aId);
        }

        public string ReadList(string aIdList)
        {
            return iService.ReadList(aIdList);
        }

        private IManagableWatchableDevice iDevice;
        private IPlaylist iService;
    }
}
