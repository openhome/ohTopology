using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Net.ControlPoint;
using OpenHome.Os;

namespace OpenHome.Av
{
    class MediaPresetPlaylist : IMediaPreset
    {
        private readonly uint iId;
        private readonly IMediaMetadata iMetadata;
        private readonly ServicePlaylist iPlaylist;

        public MediaPresetPlaylist(uint aId, IMediaMetadata aMetadata, ServicePlaylist aPlaylist)
        {
            iId = aId;
            iMetadata = aMetadata;
            iPlaylist = aPlaylist;
        }

        public IMediaMetadata Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public void Play()
        {
            iPlaylist.SeekId(iId);
        }
    }

    public interface IProxyPlaylist : IProxy
    {
        IWatchable<uint> Id { get; }
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

        Task<IWatchableContainer<IMediaPreset>> Container { get; }

        uint TracksMax { get; }
        string ProtocolInfo { get; }
    }

    public abstract class ServicePlaylist : Service
    {
        protected ServicePlaylist(INetwork aNetwork)
            : base(aNetwork)
        {
            iId = new Watchable<uint>(Network, "Id", 0);
            iTransportState = new Watchable<string>(Network, "TransportState", string.Empty);
            iRepeat = new Watchable<bool>(Network, "Repeat", false);
            iShuffle = new Watchable<bool>(Network, "Shuffle", true);
        }

        public override void Dispose()
        {
            base.Dispose();

            iId.Dispose();
            iId = null;

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

        public abstract Task<IWatchableContainer<IMediaPreset>> Container { get; }

        protected uint iTracksMax;
        protected string iProtocolInfo;

        protected IWatchableThread iThread;
        protected Watchable<uint> iId;
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
            iContainer = new PlaylistContainerNetwork(Network, this);

            iService.SetPropertyIdChanged(HandleIdChanged);
            iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);
            iService.SetPropertyRepeatChanged(HandleRepeatChanged);
            iService.SetPropertyShuffleChanged(HandleShuffleChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            iContainer.Dispose();
            iContainer = null;
            
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

        public Task<IEnumerable<IMediaPreset>> ReadList(string aIdList)
        {
            Task<IEnumerable<IMediaPreset>> task = Task<IEnumerable<IMediaPreset>>.Factory.StartNew(() =>
            {
                string trackList;
                iService.SyncReadList(aIdList, out trackList);

                List<IMediaPreset> tracks = new List<IMediaPreset>();

                XmlDocument document = new XmlDocument();
                document.LoadXml(trackList);

                XmlNodeList list = document.SelectNodes("/TrackList/Entry");
                foreach (XmlNode n in list)
                {
                    uint id = uint.Parse(n["Id"].InnerText);
                    IMediaMetadata metadata = Network.TagManager.FromDidlLite(n["Metadata"].InnerText);
                    tracks.Add(new MediaPresetPlaylist(id, metadata, this));
                }

                return tracks;
            });
            return task;
        }

        public override Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                Task<IWatchableContainer<IMediaPreset>> task = Task<IWatchableContainer<IMediaPreset>>.Factory.StartNew(() =>
                {
                    return iContainer;
                });
                return task;
            }
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
                iContainer.UpdateSnapshot(ByteArray.Unpack(iService.PropertyIdArray()));
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
        private PlaylistContainerNetwork iContainer;
    }

    class PlaylistContainerNetwork : IWatchableContainer<IMediaPreset>, IDisposable
    {
        private Watchable<IWatchableSnapshot<IMediaPreset>> iSnapshot;
        private ServicePlaylistNetwork iPlaylist;

        public PlaylistContainerNetwork(INetwork aNetwork, ServicePlaylistNetwork aPlaylist)
        {
            iPlaylist = aPlaylist;
            iSnapshot = new Watchable<IWatchableSnapshot<IMediaPreset>>(aNetwork, "Snapshot", new PlaylistSnapshotNetwork(new List<uint>(), iPlaylist));
        }

        public void Dispose()
        {
            iSnapshot.Dispose();
            iSnapshot = null;
            iPlaylist = null;
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iSnapshot;
            }
        }

        public void UpdateSnapshot(IList<uint> aIdArray)
        {
            iSnapshot.Update(new PlaylistSnapshotNetwork(aIdArray, iPlaylist));
        }
    }

    class PlaylistSnapshotNetwork : IWatchableSnapshot<IMediaPreset>
    {
        private readonly IList<uint> iIdArray;
        private readonly ServicePlaylistNetwork iPlaylist;

        public PlaylistSnapshotNetwork(IList<uint> aIdArray, ServicePlaylistNetwork aPlaylist)
        {
            iIdArray = aIdArray;
            iPlaylist = aPlaylist;
        }

        public uint Total
        {
            get
            {
                return ((uint)iIdArray.Count());
            }
        }

        public IEnumerable<uint> AlphaMap
        {
            get
            {
                return null;
            }
        }

        public Task<IWatchableFragment<IMediaPreset>> Read(uint aIndex, uint aCount)
        {
            Task<IWatchableFragment<IMediaPreset>> task = Task<IWatchableFragment<IMediaPreset>>.Factory.StartNew(() =>
            {
                string idList = string.Empty;
                for (uint i = aIndex; i < aIndex + aCount; ++i)
                {
                    idList += string.Format("{0} ", iIdArray[(int)i]);
                }
                return new WatchableFragment<IMediaPreset>(aIndex, iPlaylist.ReadList(idList.TrimEnd(' ')).Result);
            });
            return task;
        }
    }

    class ServicePlaylistMock : ServicePlaylist, IMockable
    {
        private IList<IMediaPreset> iTracks;

        public ServicePlaylistMock(INetwork aNetwork, uint aId, IList<IMediaMetadata> aTracks, bool aRepeat, bool aShuffle, string aTransportState, string aProtocolInfo, uint aTracksMax)
            : base(aNetwork)
        {
            iTracksMax = aTracksMax;
            iProtocolInfo = aProtocolInfo;

            iTracks = new List<IMediaPreset>();

            iId.Update(aId);
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

        public override Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                Task<IWatchableContainer<IMediaPreset>> task = Task<IWatchableContainer<IMediaPreset>>.Factory.StartNew(() =>
                {
                    return new PlaylistContainerMock(Network, new PlaylistSnapshotMock(iTracks));
                });
                return task;
            }
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
            else if (command == "tracks")
            {
                throw new NotImplementedException();
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

    class PlaylistContainerMock : IWatchableContainer<IMediaPreset>
    {
        public readonly Watchable<IWatchableSnapshot<IMediaPreset>> iSnapshot;

        public PlaylistContainerMock(INetwork aNetwork, IWatchableSnapshot<IMediaPreset> aSnapshot)
        {
            iSnapshot = new Watchable<IWatchableSnapshot<IMediaPreset>>(aNetwork, "Snapshot", aSnapshot);
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iSnapshot;
            }
        }
    }

    class PlaylistSnapshotMock : IWatchableSnapshot<IMediaPreset>
    {
        private readonly IEnumerable<IMediaPreset> iData;

        public PlaylistSnapshotMock(IEnumerable<IMediaPreset> aData)
        {
            iData = aData;
        }

        public uint Total
        {
            get
            {
                return ((uint)iData.Count());
            }
        }

        public uint Sequence
        {
            get
            {
                return 0;
            }
        }

        public IEnumerable<uint> AlphaMap
        {
            get
            {
                return null;
            }
        }

        public Task<IWatchableFragment<IMediaPreset>> Read(uint aIndex, uint aCount)
        {
            Do.Assert(aIndex + aCount <= Total);

            Task<IWatchableFragment<IMediaPreset>> task = Task<IWatchableFragment<IMediaPreset>>.Factory.StartNew(() =>
            {
                return new WatchableFragment<IMediaPreset>(aIndex, iData.Skip((int)aIndex).Take((int)aCount));
            });
            return task;
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

        public Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                return iService.Container;
            }
        }
    }
}
