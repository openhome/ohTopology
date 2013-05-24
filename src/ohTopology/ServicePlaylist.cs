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
    public interface IProxyPlaylist : IProxy
    {
        IWatchable<uint> Id { get; }
        IWatchable<IList<uint>> IdArray { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<bool> Repeat { get; }
        IWatchable<bool> Shuffle { get; }

        Task Play();
        Task Pause();
        Task Stop();
        Task Previous();
        Task Next();
        Task SeekId(uint aValue);
        Task SeekIndex(uint aValue);
        Task SeekSecondAbsolute(uint aValue);
        Task SeekSecondRelative(int aValue);

        Task<uint> Insert(uint aAfterId, string aUri, string aMetadata);
        Task DeleteId(uint aValue);
        Task DeleteAll();
        Task SetRepeat(bool aValue);
        Task SetShuffle(bool aValue);

        Task<IInfoMetadata> Read(uint aId);
        Task<string> ReadList(string aIdList);

        uint TracksMax { get; }
        string ProtocolInfo { get; }
    }

    public abstract class ServicePlaylist : Service
    {
        protected ServicePlaylist(INetwork aNetwork)
            : base(aNetwork)
        {
            iId = new Watchable<uint>(Network, "Id", 0);
            iIdArray = new Watchable<IList<uint>>(Network, "IdArray", new List<uint>());
            iTransportState = new Watchable<string>(Network, "TransportState", string.Empty);
            iRepeat = new Watchable<bool>(Network, "Repeat", false);
            iShuffle = new Watchable<bool>(Network, "Shuffle", true);
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

            iRepeat.Dispose();
            iRepeat = null;

            iShuffle.Dispose();
            iShuffle = null;
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyPlaylist(aDevice, this);
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

        public abstract Task Play();
        public abstract Task Pause();
        public abstract Task Stop();
        public abstract Task Previous();
        public abstract Task Next();
        public abstract Task SeekId(uint aValue);
        public abstract Task SeekIndex(uint aValue);
        public abstract Task SeekSecondAbsolute(uint aValue);
        public abstract Task SeekSecondRelative(int aValue);
        public abstract Task<uint> Insert(uint aAfterId, string aUri, string aMetadata);
        public abstract Task DeleteId(uint aValue);
        public abstract Task DeleteAll();
        public abstract Task SetRepeat(bool aValue);
        public abstract Task SetShuffle(bool aValue);
        public abstract Task<IInfoMetadata> Read(uint aId);
        public abstract Task<string> ReadList(string aIdList);

        protected uint iTracksMax;
        protected string iProtocolInfo;

        protected IWatchableThread iThread;
        protected Watchable<uint> iId;
        protected Watchable<IList<uint>> iIdArray;
        protected Watchable<string> iTransportState;
        protected Watchable<bool> iRepeat;
        protected Watchable<bool> iShuffle;
    }

    class ServicePlaylistNetwork : ServicePlaylist
    {
        public ServicePlaylistNetwork(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork)
        {
            iSubscribed = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgPlaylist1(aDevice);

            iService.SetPropertyIdChanged(HandleIdChanged);
            iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);
            iService.SetPropertyRepeatChanged(HandleRepeatChanged);
            iService.SetPropertyShuffleChanged(HandleShuffleChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            iSubscribed.Dispose();
            iSubscribed = null;

            iService.Dispose();
            iService = null;

            base.Dispose();
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

        private void HandleInitialEvent()
        {
            iTracksMax = iService.PropertyTracksMax();
            iProtocolInfo = iService.PropertyProtocolInfo();

            iSubscribed.Set();
        }

        protected override void OnUnsubscribe()
        {
            iService.Unsubscribe();
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

        public override Task Previous()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncPrevious();
            });
            return task;
        }

        public override Task Next()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncNext();
            });
            return task;
        }

        public override Task SeekId(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSeekId(aValue);
            });
            return task;
        }

        public override Task SeekIndex(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSeekIndex(aValue);
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

        public override Task<uint> Insert(uint aAfterId, string aUri, string aMetadata)
        {
            Task<uint> task = Task.Factory.StartNew(() =>
            {
                uint newId;
                iService.SyncInsert(aAfterId, aUri, aMetadata, out newId);
                return newId;
            });
            return task;
        }

        public override Task DeleteId(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncDeleteId(aValue);
            });
            return task;
        }

        public override Task DeleteAll()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncDeleteAll();
            });
            return task;
        }

        public override Task SetRepeat(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetRepeat(aValue);
            });
            return task;
        }

        public override Task SetShuffle(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetShuffle(aValue);
            });
            return task;
        }

        public override Task<IInfoMetadata> Read(uint aId)
        {
            Task<IInfoMetadata> task = Task.Factory.StartNew(() =>
            {
                string uri;
                string metadata;
                iService.SyncRead(aId, out uri, out metadata);
                return new InfoMetadata(metadata, uri) as IInfoMetadata;
            });
            return task;
        }

        public override Task<string> ReadList(string aIdList)
        {
            Task<string> task = Task.Factory.StartNew(() =>
            {
                string trackList;
                iService.SyncReadList(aIdList, out trackList);
                return trackList;
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

        private void HandleTransportStateChanged()
        {
            Network.Schedule(() =>
            {
                iTransportState.Update(iService.PropertyTransportState());
            });
        }

        private void HandleRepeatChanged()
        {
            Network.Schedule(() =>
            {
                iRepeat.Update(iService.PropertyRepeat());
            });
        }

        private void HandleShuffleChanged()
        {
            Network.Schedule(() =>
            {
                iShuffle.Update(iService.PropertyShuffle());
            });
        }

        private ManualResetEvent iSubscribed;
        private CpProxyAvOpenhomeOrgPlaylist1 iService;
    }

    class ServicePlaylistMock : ServicePlaylist, IMockable
    {
        public ServicePlaylistMock(INetwork aNetwork, uint aId, IList<uint> aIdArray, bool aRepeat, bool aShuffle, string aTransportState, string aProtocolInfo, uint aTracksMax)
            : base(aNetwork)
        {
            iTracksMax = aTracksMax;
            iProtocolInfo = aProtocolInfo;

            iId.Update(aId);
            iIdArray.Update(aIdArray);
            iTransportState.Update(aTransportState);
            iRepeat.Update(aRepeat);
            iShuffle.Update(aShuffle);
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

        public override Task Pause()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
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
                Network.Schedule(() =>
                {
                    iTransportState.Update("Stopped");
                });
            });
            return task;
        }

        public override Task Previous()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                });
            });
            return task;
        }

        public override Task Next()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                });
            });
            return task;
        }

        public override Task SeekId(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iId.Update(aValue);
                });
            });
            return task;
        }

        public override Task SeekIndex(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                });
            });
            return task;
        }

        public override Task SeekSecondAbsolute(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                });
            });
            return task;
        }

        public override Task SeekSecondRelative(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                });
            });
            return task;
        }

        public override Task<uint> Insert(uint aAfterId, string aUri, string aMetadata)
        {
            Task<uint> task = Task<uint>.Factory.StartNew(() =>
            {
                Network.Execute(() =>
                {
                });
                return 0;
            });
            return task;
        }

        public override Task DeleteId(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                });
            });
            return task;
        }

        public override Task DeleteAll()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                });
            });
            return task;
        }

        public override Task SetRepeat(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iRepeat.Update(aValue);
                });
            });
            return task;
        }

        public override Task SetShuffle(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iShuffle.Update(aValue);
                });
            });
            return task;
        }

        public override Task<IInfoMetadata> Read(uint aId)
        {
            Task<IInfoMetadata> task = Task<IInfoMetadata>.Factory.StartNew(() =>
            {
                Network.Execute(() =>
                {
                });
                return new InfoMetadata();
            });
            return task;
        }

        public override Task<string> ReadList(string aIdList)
        {
            Task<string> task = Task<string>.Factory.StartNew(() =>
            {
                Network.Execute(() =>
                {
                });
                return string.Empty;
            });
            return task;
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "tracksmax")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iTracksMax = uint.Parse(value.First());
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
    }

    public class ProxyPlaylist : Proxy<ServicePlaylist>, IProxyPlaylist
    {
        public ProxyPlaylist(IDevice aDevice, ServicePlaylist aService)
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

        public Task Previous()
        {
            return iService.Previous();
        }

        public Task Next()
        {
            return iService.Next();
        }

        public Task SeekId(uint aValue)
        {
            return iService.SeekId(aValue);
        }

        public Task SeekIndex(uint aValue)
        {
            return iService.SeekIndex(aValue);
        }

        public Task SeekSecondAbsolute(uint aValue)
        {
            return iService.SeekSecondAbsolute(aValue);
        }

        public Task SeekSecondRelative(int aValue)
        {
            return iService.SeekSecondRelative(aValue);
        }

        public Task<uint> Insert(uint aAfterId, string aUri, string aMetadata)
        {
            return iService.Insert(aAfterId, aUri, aMetadata);
        }

        public Task DeleteId(uint aValue)
        {
            return iService.DeleteId(aValue);
        }

        public Task DeleteAll()
        {
            return iService.DeleteAll();
        }

        public Task SetRepeat(bool aValue)
        {
            return iService.SetRepeat(aValue);
        }

        public Task SetShuffle(bool aValue)
        {
            return iService.SetShuffle(aValue);
        }

        public Task<IInfoMetadata> Read(uint aId)
        {
            return iService.Read(aId);
        }

        public Task<string> ReadList(string aIdList)
        {
            return iService.ReadList(aIdList);
        }
    }
}
