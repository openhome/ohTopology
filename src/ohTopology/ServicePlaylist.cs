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
    class MediaPresetPlaylist : IMediaPreset, IWatcher<uint>, IWatcher<string>
    {
        private readonly uint iIndex;
        private readonly uint iId;
        private readonly IMediaMetadata iMetadata;
        private readonly ServicePlaylist iPlaylist;
        private readonly Watchable<bool> iBuffering;
        private readonly Watchable<bool> iPlaying;
        private readonly Watchable<bool> iSelected;

        private uint iCurrentId;
        private string iCurrentTransportState;

        public MediaPresetPlaylist(IWatchableThread aThread, uint aIndex, uint aId, IMediaMetadata aMetadata, ServicePlaylist aPlaylist)
        {
            iIndex = aIndex;
            iId = aId;
            iMetadata = aMetadata;
            iPlaylist = aPlaylist;

            iBuffering = new Watchable<bool>(aThread, "Buffering", false);
            iPlaying = new Watchable<bool>(aThread, "Playing", false);
            iSelected = new Watchable<bool>(aThread, "Selected", false);

            iPlaylist.Id.AddWatcher(this);
            iPlaylist.TransportState.AddWatcher(this);
        }

        public void Dispose()
        {
            iPlaylist.Id.RemoveWatcher(this);
            iPlaylist.TransportState.RemoveWatcher(this);
            iBuffering.Dispose();
            iPlaying.Dispose();
            iSelected.Dispose();
        }

        public uint Index
        {
            get
            {
                return iIndex;
            }
        }

        public uint Id
        {
            get
            {
                return iId;
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<bool> Buffering
        {
            get
            {
                return iBuffering;
            }
        }

        public IWatchable<bool> Playing
        {
            get
            {
                return iPlaying;
            }
        }

        public IWatchable<bool> Selected {
            get {
                return iSelected;
            }
        }

        public void Play()
        {
            iBuffering.Update(iCurrentTransportState == "Buffering");
            iPlaylist.SeekId(iId);
        }

        private void EvaluatePlaying()
        {
            iBuffering.Update(iCurrentId == iId && iCurrentTransportState == "Buffering");
            iPlaying.Update(iCurrentId == iId && iCurrentTransportState == "Playing");
            iSelected.Update(iCurrentId == iId);
        }

        public void ItemOpen(string aId, uint aValue)
        {
            iCurrentId = aValue;
            EvaluatePlaying();
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            iCurrentId = aValue;
            EvaluatePlaying();
        }

        public void ItemClose(string aId, uint aValue)
        {
            iPlaying.Update(false);
        }

        public void ItemOpen(string aId, string aValue)
        {
            iCurrentTransportState = aValue;
            EvaluatePlaying();
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iCurrentTransportState = aValue;
            EvaluatePlaying();
        }

        public void ItemClose(string aId, string aValue)
        {
            iPlaying.Update(false);
        }
    }

    public interface IProxyPlaylist : IProxy
    {
        IWatchable<uint> Id { get; }
        IWatchable<IInfoMetadata> InfoNext { get; }
        IWatchable<int> InfoCurrentIndex { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<bool> Repeat { get; }
        IWatchable<bool> Shuffle { get; }

        Task Play();
        Task Pause();
        Task Stop();
        Task Previous();
        Task Next();
        Task SeekId(uint aValue);
        Task SeekSecondAbsolute(uint aValue);
        Task SeekSecondRelative(int aValue);

        Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata);
        Task<uint> InsertNext(string aUri, IMediaMetadata aMetadata);
        Task<uint> InsertEnd(string aUri, IMediaMetadata aMetadata);
        Task MakeRoomForInsert(uint aCount);
        Task Delete(IMediaPreset aValue);
        Task DeleteAll();
        Task SetRepeat(bool aValue);
        Task SetShuffle(bool aValue);

        IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot { get; }

        uint TracksMax { get; }
        string ProtocolInfo { get; }
    }

    public abstract class ServicePlaylist : Service
    {
        public const string kCacheIdFormat = "Playlist({0})";

        protected ServicePlaylist(INetwork aNetwork, IInjectorDevice aDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iId = new Watchable<uint>(aNetwork, "Id", 0);
            iInfoCurrentIndex = new Watchable<int>(aNetwork, "CurrentIndex", -1);
            iInfoNext = new Watchable<IInfoMetadata>(aNetwork, "InfoNext", InfoMetadata.Empty);
            iTransportState = new Watchable<string>(aNetwork, "TransportState", string.Empty);
            iRepeat = new Watchable<bool>(aNetwork, "Repeat", false);
            iShuffle = new Watchable<bool>(aNetwork, "Shuffle", true);
        }

        public override void Dispose()
        {
            base.Dispose();

            iId.Dispose();
            iId = null;

            iInfoNext.Dispose();
            iInfoNext = null;

            iTransportState.Dispose();
            iTransportState = null;

            iRepeat.Dispose();
            iRepeat = null;

            iShuffle.Dispose();
            iShuffle = null;
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyPlaylist(this, aDevice);
        }

        public IWatchable<uint> Id
        {
            get
            {
                return iId;
            }
        }

        public IWatchable<int> InfoCurrentIndex
        {
            get
            {
                return iInfoCurrentIndex;
            }
        }

        public IWatchable<IInfoMetadata> InfoNext
        {
            get
            {
                return iInfoNext;
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

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iMediaSupervisor.Snapshot;
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
        public abstract Task SeekSecondAbsolute(uint aValue);
        public abstract Task SeekSecondRelative(int aValue);
        public abstract Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata);
        public abstract Task<uint> InsertNext(string aUri, IMediaMetadata aMetadata);
        public abstract Task<uint> InsertEnd(string aUri, IMediaMetadata aMetadata);
        public abstract Task MakeRoomForInsert(uint aCount);
        public abstract Task Delete(IMediaPreset aValue);
        public abstract Task DeleteAll();
        public abstract Task SetRepeat(bool aValue);
        public abstract Task SetShuffle(bool aValue);

        protected uint iTracksMax;
        protected string iProtocolInfo;

        protected IWatchableThread iThread;
        protected Watchable<uint> iId;
        protected Watchable<int> iInfoCurrentIndex;
        protected Watchable<IInfoMetadata> iInfoNext;
        protected Watchable<string> iTransportState;
        protected Watchable<bool> iRepeat;
        protected Watchable<bool> iShuffle;
        protected MediaSupervisor<IMediaPreset> iMediaSupervisor;
    }

    class ServicePlaylistNetwork : ServicePlaylist
    {
        public ServicePlaylistNetwork(INetwork aNetwork, IInjectorDevice aDevice, CpDevice aCpDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iCpDevice = aCpDevice;
            iCpDevice.AddRef();

            iService = new CpProxyAvOpenhomeOrgPlaylist1(aCpDevice);

            iService.SetPropertyIdChanged(HandleIdChanged);
            iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);
            iService.SetPropertyRepeatChanged(HandleRepeatChanged);
            iService.SetPropertyShuffleChanged(HandleShuffleChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            base.Dispose();

            Do.Assert(iCacheSession == null);

            iService.Dispose();
            iService = null;

            iCpDevice.RemoveRef();
        }

        protected override Task OnSubscribe()
        {
            Do.Assert(iSubscribedSource == null);

            iSubscribedSource = new TaskCompletionSource<bool>();

            iCacheSession = iNetwork.IdCache.CreateSession(string.Format(ServicePlaylist.kCacheIdFormat, Device.Udn), ReadList);

            iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new PlaylistSnapshot(iNetwork, iCacheSession, new List<uint>(), this));

            iService.Subscribe();

            iSubscribed = true;

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
            iTracksMax = iService.PropertyTracksMax();
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

            if (iMediaSupervisor != null)
            {
                iMediaSupervisor.Dispose();
                iMediaSupervisor = null;
            }

            if (iCacheSession != null)
            {
                iCacheSession.Dispose();
                iCacheSession = null;
            }

            iSubscribedSource = null;

            iSubscribed = false;
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
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task Pause()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginPause((ptr) =>
            {
                try
                {
                    iService.EndPause(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
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
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task Previous()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginPrevious((ptr) =>
            {
                try
                {
                    iService.EndPrevious(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task Next()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginNext((ptr) =>
            {
                try
                {
                    iService.EndNext(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task SeekId(uint aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSeekId(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSeekId(ptr);
                    taskSource.SetResult(true);
                }
                catch (ProxyError e)
                {
                    if (e.Code == 800)      // id not found (silently handle)
                    {
                        taskSource.SetResult(true);
                        return;
                    }
                    taskSource.SetException(e);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task SeekSecondAbsolute(uint aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSeekSecondAbsolute(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSeekSecondAbsolute(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task SeekSecondRelative(int aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSeekSecondRelative(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSeekSecondRelative(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata)
        {
            TaskCompletionSource<uint> taskSource = new TaskCompletionSource<uint>();
            iService.BeginInsert(aAfterId, aUri, iNetwork.TagManager.ToDidlLite(aMetadata), (ptr) =>
            {
                try
                {
                    uint newId;
                    iService.EndInsert(ptr, out newId);
                    taskSource.SetResult(newId);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task<uint> InsertNext(string aUri, IMediaMetadata aMetadata)
        {
            uint id = iService.PropertyId();

            TaskCompletionSource<uint> taskSource = new TaskCompletionSource<uint>();
            iService.BeginInsert(id, aUri, iNetwork.TagManager.ToDidlLite(aMetadata), (ptr) =>
            {
                try
                {
                    uint newId;
                    iService.EndInsert(ptr, out newId);
                    taskSource.SetResult(newId);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task<uint> InsertEnd(string aUri, IMediaMetadata aMetadata)
        {
            uint id = 0;

            IList<uint> idArray = ByteArray.Unpack(iService.PropertyIdArray());
            if (idArray.Count > 0)
            {
                id = idArray.Last();
            }

            TaskCompletionSource<uint> taskSource = new TaskCompletionSource<uint>();
            iService.BeginInsert(id, aUri, iNetwork.TagManager.ToDidlLite(aMetadata), (ptr) =>
            {
                try
                {
                    uint newId;
                    iService.EndInsert(ptr, out newId);
                    taskSource.SetResult(newId);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task MakeRoomForInsert(uint aCount)
        {
            IList<uint> idArray = ByteArray.Unpack(iService.PropertyIdArray());
            IEnumerable<uint> ids = idArray.Take((int)aCount);

            return Task.Factory.ContinueWhenAll(Delete(ids).ToArray(), (tasks) => { Task.WaitAll(tasks); });
        }

        private IList<Task> Delete(IEnumerable<uint> aIds)
        {
            IList<Task> tasks = new List<Task>();
            foreach (uint id in aIds)
            {
                tasks.Add(Delete(id));
            }
            return tasks;
        }

        public override Task Delete(IMediaPreset aValue)
        {
            Do.Assert(aValue is MediaPresetPlaylist);
            uint id = (aValue as MediaPresetPlaylist).Id;

            return Delete(id);
        }

        private Task Delete(uint aId)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginDeleteId(aId, (ptr) =>
            {
                try
                {
                    iService.EndDeleteId(ptr);
                    taskSource.SetResult(true);
                }
                catch (ProxyError e)
                {
                    if (e.Code == 800)      // id not found (silently handle)
                    {
                        taskSource.SetResult(true);
                        return;
                    }
                    taskSource.SetException(e);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task DeleteAll()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginDeleteAll((ptr) =>
            {
                try
                {
                    iService.EndDeleteAll(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task SetRepeat(bool aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetRepeat(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSetRepeat(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task SetShuffle(bool aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetShuffle(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSetShuffle(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        private Task<IEnumerable<IIdCacheEntry>> ReadList(IEnumerable<uint> aIdList)
        {
            TaskCompletionSource<IEnumerable<IIdCacheEntry>> taskSource = new TaskCompletionSource<IEnumerable<IIdCacheEntry>>();

            string idList = string.Empty;
            foreach (uint id in aIdList)
            {
                idList += string.Format("{0} ", id);
            }
            idList.Trim(' ');

            iService.BeginReadList(idList, (ptr) =>
            {
                try
                {
                    string trackList;
                    iService.EndReadList(ptr, out trackList);

                    List<IIdCacheEntry> entries = new List<IIdCacheEntry>();

                    XmlDocument document = new XmlDocument();
                    document.LoadXml(trackList);

                    XmlNodeList list = document.SelectNodes("/TrackList/Entry");
                    foreach (XmlNode n in list)
                    {
                        IMediaMetadata metadata = iNetwork.TagManager.FromDidlLite(n["Metadata"].InnerText);
                        string uri = n["Uri"].InnerText;
                        entries.Add(new IdCacheEntry(metadata, uri));
                    }

                    taskSource.SetResult(entries);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });

            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        private void HandleIdChanged()
        {
            IList<uint> idArray = ByteArray.Unpack(iService.PropertyIdArray());
            uint id = iService.PropertyId();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    if (iSubscribed)
                    {
                        iId.Update(id);
                        EvaluateInfoCurrentIndex(id, idArray);
                        EvaluateInfoNext(id, idArray);
                    }
                });
            });
        }

        private void HandleIdArrayChanged()
        {
            IList<uint> idArray = ByteArray.Unpack(iService.PropertyIdArray());
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    if (iSubscribed)
                    {
                        iCacheSession.SetValid(idArray);
                        iMediaSupervisor.Update(new PlaylistSnapshot(iNetwork, iCacheSession, idArray, this));
                        EvaluateInfoCurrentIndex(iId.Value, idArray);
                        EvaluateInfoNext(iId.Value, idArray);
                    }
                });
            });
        }

        private void EvaluateInfoCurrentIndex(uint aId, IList<uint> aIdArray)
        {
            iInfoCurrentIndex.Update(aIdArray.IndexOf(aId));
        }

        private void EvaluateInfoNext(uint aId, IList<uint> aIdArray)
        {
            int index = aIdArray.IndexOf(aId);
            if (!iShuffle.Value && (index > -1) && (index < aIdArray.Count - 1) && (aIdArray.Count > 1))
            {
                iCacheSession.Entries(new uint[] { aIdArray.ElementAt(index + 1) }).ContinueWith((t) =>
                {
                    iNetwork.Schedule(() =>
                    {
                        iDisposeHandler.WhenNotDisposed(() =>
                        {
                            try
                            {
                                IIdCacheEntry entry = t.Result.ElementAt(0);
                                iInfoNext.Update(new InfoMetadata(entry.Metadata, entry.Uri));
                            }
                            catch
                            {
                                iInfoNext.Update(InfoMetadata.Empty);
                            }
                        });
                    });
                });
            }
            else
            {
                if (!iShuffle.Value && iRepeat.Value && (index > -1) && index == aIdArray.Count - 1)
                {
                    iCacheSession.Entries(new uint[] { aIdArray.ElementAt((uint)0) }).ContinueWith((t) =>
                    {
                        iNetwork.Schedule(() =>
                        {
                            iDisposeHandler.WhenNotDisposed(() =>
                            {
                                try
                                {
                                    IIdCacheEntry entry = t.Result.ElementAt(0);
                                    iInfoNext.Update(new InfoMetadata(entry.Metadata, entry.Uri));
                                }
                                catch
                                {
                                    iInfoNext.Update(InfoMetadata.Empty);
                                }
                            });
                        });
                    });
                }
                else
                {
                    iInfoNext.Update(InfoMetadata.Empty);
                }
            }
        }

        private void HandleTransportStateChanged()
        {
            string transportState = iService.PropertyTransportState();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    if (iSubscribed)
                    {
                        iTransportState.Update(transportState);
                    }
                });
            });
        }

        private void HandleRepeatChanged()
        {
            IList<uint> idArray = ByteArray.Unpack(iService.PropertyIdArray());
            bool repeat = iService.PropertyRepeat();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    if (iSubscribed)
                    {
                        iRepeat.Update(repeat);
                        EvaluateInfoNext(iId.Value, idArray);
                    }
                });
            });
        }

        private void HandleShuffleChanged()
        {
            IList<uint> idArray = ByteArray.Unpack(iService.PropertyIdArray());
            bool shuffle = iService.PropertyShuffle();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    if (iSubscribed)
                    {
                        iShuffle.Update(shuffle);
                        EvaluateInfoNext(iId.Value, idArray);
                    }
                });
            });
        }

        private readonly CpDevice iCpDevice;
        private bool iSubscribed;
        private TaskCompletionSource<bool> iSubscribedSource;
        private CpProxyAvOpenhomeOrgPlaylist1 iService;
        private IIdCacheSession iCacheSession;
    }

    class PlaylistSnapshot : IMediaClientSnapshot<IMediaPreset>
    {
        private readonly INetwork iNetwork;
        private readonly IIdCacheSession iCacheSession;
        private readonly IList<uint> iIdArray;
        private readonly ServicePlaylist iPlaylist;

        public PlaylistSnapshot(INetwork aNetwork, IIdCacheSession aCacheSession, IList<uint> aIdArray, ServicePlaylist aPlaylist)
        {
            iNetwork = aNetwork;
            iCacheSession = aCacheSession;
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

        public IEnumerable<uint> Alpha
        {
            get
            {
                return null;
            }
        }

        public void Read(CancellationToken aCancellationToken, uint aIndex, uint aCount, Action<IEnumerable<IMediaPreset>> aCallback)
        {
            Do.Assert(aIndex + aCount <= Total);

            List<uint> idList = new List<uint>();
            for (uint i = aIndex; i < aIndex + aCount; ++i)
            {
                idList.Add(iIdArray.ElementAt((int)i));
            }

            List<IMediaPreset> tracks = new List<IMediaPreset>();
            IEnumerable<IIdCacheEntry> entries = new List<IIdCacheEntry>();
            try
            {
                entries = iCacheSession.Entries(idList).Result;
            }
            catch
            {
            }

            iNetwork.Schedule(() =>
            {
                if (!aCancellationToken.IsCancellationRequested)
                {
                    uint index = aIndex;
                    foreach (IIdCacheEntry e in entries)
                    {
                        uint id = iIdArray.ElementAt((int)index);
                        tracks.Add(new MediaPresetPlaylist(iNetwork, (uint)(iIdArray.IndexOf(id) + 1), id, e.Metadata, iPlaylist));
                        ++index;
                    }

                    aCallback(tracks);
                }
            });
        }
    }

    class ServicePlaylistMock : ServicePlaylist, IMockable
    {
        private uint iIdFactory;
        private IIdCacheSession iCacheSession;
        private MediaSnapshot<IMediaPreset> iMediaSnapshot;
        private List<TrackMock> iTracks;
        private List<uint> iIdArray;

        private class TrackMock
        {
            private readonly string iUri;
            private readonly IMediaMetadata iMetadata;

            public TrackMock(string aUri, IMediaMetadata aMetadata)
            {
                iUri = aUri;
                iMetadata = aMetadata;
            }

            public string Uri
            {
                get
                {
                    return iUri;
                }
            }

            public IMediaMetadata Metadata
            {
                get
                {
                    return iMetadata;
                }
            }
        }

        public ServicePlaylistMock(INetwork aNetwork, IInjectorDevice aDevice, uint aId, IList<IMediaMetadata> aTracks, bool aRepeat, bool aShuffle,
            string aTransportState, string aProtocolInfo, uint aTracksMax, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iIdFactory = 1;
            iTracksMax = aTracksMax;
            iProtocolInfo = aProtocolInfo;

            iIdArray = new List<uint>();
            iTracks = new List<TrackMock>();
            foreach (IMediaMetadata m in aTracks)
            {
                iIdArray.Add(iIdFactory);
                iTracks.Add(new TrackMock(m[iNetwork.TagManager.Audio.Uri].Value, m));
                ++iIdFactory;
            }

            iId.Update(aId);
            iTransportState.Update(aTransportState);
            iRepeat.Update(aRepeat);
            iShuffle.Update(aShuffle);
        }

        public override void Dispose()
        {
            if (iMediaSnapshot != null)
            {
                iMediaSnapshot.Dispose();
                iMediaSnapshot = null;
            }

            if (iCacheSession != null)
            {
                iCacheSession.Dispose();
                iCacheSession = null;
            }

            base.Dispose();
        }

        protected override Task OnSubscribe()
        {
            iCacheSession = iNetwork.IdCache.CreateSession(string.Format("Playlist({0})", Device.Udn), ReadList);
            iCacheSession.SetValid(iIdArray);

            iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new PlaylistSnapshot(iNetwork, iCacheSession, iIdArray, this));

            return base.OnSubscribe();
        }

        protected override void OnUnsubscribe()
        {
            if (iMediaSupervisor != null)
            {
                iMediaSupervisor.Dispose();
                iMediaSupervisor = null;
            }

            if (iCacheSession != null)
            {
                iCacheSession.Dispose();
                iCacheSession = null;
            }

            base.OnUnsubscribe();
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

        public override Task Pause()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
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
                iNetwork.Schedule(() =>
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
                iNetwork.Schedule(() =>
                {
                    int index = iIdArray.IndexOf(iId.Value);
                    if (index > 0)
                    {
                        iId.Update(iIdArray.ElementAt(index - 1));
                    }
                });
            });
            return task;
        }

        public override Task Next()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    int index = iIdArray.IndexOf(iId.Value);
                    if (index < iIdArray.Count - 1)
                    {
                        iId.Update(iIdArray.ElementAt(index + 1));
                    }
                });
            });
            return task;
        }

        public override Task SeekId(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    iId.Update(aValue);
                });
            });
            return task;
        }

        public override Task SeekSecondAbsolute(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                });
            });
            return task;
        }

        public override Task SeekSecondRelative(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                });
            });
            return task;
        }

        public override Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata)
        {
            Task<uint> task = Task<uint>.Factory.StartNew(() =>
            {
                uint newId = 0;
                iNetwork.Execute(() =>
                {
                    int index = iIdArray.IndexOf(aAfterId);
                    if (index == -1)
                    {
                        throw new Exception("Id not found");
                    }
                    newId = iIdFactory;
                    iIdArray.Insert(index + 1, newId);
                    iTracks.Insert(index + 1, new TrackMock(aUri, aMetadata));
                    ++iIdFactory;

                    iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new PlaylistSnapshot(iNetwork, iCacheSession, iIdArray, this));
                });
                return newId;
            });
            return task;
        }

        public override Task<uint> InsertNext(string aUri, IMediaMetadata aMetadata)
        {
            Task<uint> task = Task<uint>.Factory.StartNew(() =>
            {
                uint newId = 0;
                iNetwork.Execute(() =>
                {
                    int index = iIdArray.IndexOf(iId.Value);
                    if (index == -1)
                    {
                        index = 0;
                    }
                    newId = iIdFactory;
                    iIdArray.Insert(index + 1, newId);
                    iTracks.Insert(index + 1, new TrackMock(aUri, aMetadata));
                    ++iIdFactory;

                    iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new PlaylistSnapshot(iNetwork, iCacheSession, iIdArray, this));
                });
                return newId;
            });
            return task;
        }

        public override Task<uint> InsertEnd(string aUri, IMediaMetadata aMetadata)
        {
            Task<uint> task = Task<uint>.Factory.StartNew(() =>
            {
                uint newId = 0;
                iNetwork.Execute(() =>
                {
                    int index = iIdArray.Count - 1;
                    if (index == -1)
                    {
                        index = 0;
                    }
                    newId = iIdFactory;
                    iIdArray.Insert(index + 1, newId);
                    iTracks.Insert(index + 1, new TrackMock(aUri, aMetadata));
                    ++iIdFactory;

                    iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new PlaylistSnapshot(iNetwork, iCacheSession, iIdArray, this));
                });
                return newId;
            });
            return task;
        }

        public override Task MakeRoomForInsert(uint aCount)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    iIdArray.RemoveRange(0, (int)aCount);
                });
            });
            return task;
        }

        public override Task Delete(IMediaPreset aValue)
        {
            uint id = (aValue as MediaPresetPlaylist).Id;
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    int index = iIdArray.IndexOf(id);
                    iIdArray.Remove(id);
                    if (index < iIdArray.Count)
                    {
                        iId.Update(iIdArray.ElementAt(index));
                    }

                    iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new PlaylistSnapshot(iNetwork, iCacheSession, iIdArray, this));
                });
            });
            return task;
        }

        public override Task DeleteAll()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    iIdArray.Clear();
                    iId.Update(0);

                    iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new PlaylistSnapshot(iNetwork, iCacheSession, iIdArray, this));
                });
            });
            return task;
        }

        public override Task SetRepeat(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
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
                iNetwork.Schedule(() =>
                {
                    iShuffle.Update(aValue);
                });
            });
            return task;
        }

        private Task<IEnumerable<IIdCacheEntry>> ReadList(IEnumerable<uint> aIdList)
        {
            Task<IEnumerable<IIdCacheEntry>> task = Task<IEnumerable<IIdCacheEntry>>.Factory.StartNew(() =>
            {
                List<IdCacheEntry> entries = new List<IdCacheEntry>();

                lock (iIdArray)
                {
                    foreach (uint id in aIdList)
                    {
                        TrackMock track = iTracks[iIdArray.IndexOf(id)];
                        entries.Add(new IdCacheEntry(track.Metadata, track.Uri));
                    }
                }
                return entries;
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

    public class ProxyPlaylist : Proxy<ServicePlaylist>, IProxyPlaylist
    {
        public ProxyPlaylist(ServicePlaylist aService, IDevice aDevice)
            : base(aService, aDevice)
        {
        }

        public IWatchable<uint> Id
        {
            get { return iService.Id; }
        }

        public IWatchable<int> InfoCurrentIndex
        {
            get
            {
                return iService.InfoCurrentIndex;
            }
        }

        public IWatchable<IInfoMetadata> InfoNext
        {
            get { return iService.InfoNext; }
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

        public Task SeekSecondAbsolute(uint aValue)
        {
            return iService.SeekSecondAbsolute(aValue);
        }

        public Task SeekSecondRelative(int aValue)
        {
            return iService.SeekSecondRelative(aValue);
        }

        public Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata)
        {
            return iService.Insert(aAfterId, aUri, aMetadata);
        }

        public Task<uint> InsertNext(string aUri, IMediaMetadata aMetadata)
        {
            return iService.InsertNext(aUri, aMetadata);
        }

        public Task<uint> InsertEnd(string aUri, IMediaMetadata aMetadata)
        {
            return iService.InsertEnd(aUri, aMetadata);
        }

        public Task MakeRoomForInsert(uint aCount)
        {
            return iService.MakeRoomForInsert(aCount);
        }

        public Task Delete(IMediaPreset aValue)
        {
            return iService.Delete(aValue);
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

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iService.Snapshot;
            }
        }
    }
}
