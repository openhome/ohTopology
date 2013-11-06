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
    class MediaPresetSdp : IMediaPreset, IWatcher<uint>, IWatcher<string>
    {
        private readonly INetwork iNetwork;
        private readonly uint iIndex;
        private readonly uint iId;
        private readonly IMediaMetadata iMetadata;
        private readonly ServiceSdp iSdp;
        private readonly Watchable<bool> iBuffering;
        private readonly Watchable<bool> iPlaying;

        private uint iCurrentId;
        private string iCurrentTransportState;

        public MediaPresetSdp(INetwork aNetwork, uint aIndex, uint aId, IMediaMetadata aMetadata, ServiceSdp aSdp)
        {
            iNetwork = aNetwork;
            iIndex = aIndex;
            iId = aId;
            iMetadata = aMetadata;
            iSdp = aSdp;

            iBuffering = new Watchable<bool>(iNetwork, "Buffering", false);
            iPlaying = new Watchable<bool>(iNetwork, "Playing", false);
            iSdp.Id.AddWatcher(this);
            iSdp.TransportState.AddWatcher(this);
        }

        public void Dispose()
        {
            iSdp.Id.RemoveWatcher(this);
            iSdp.TransportState.RemoveWatcher(this);
            iBuffering.Dispose();
            iPlaying.Dispose();
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

        public void Play()
        {
            iSdp.SeekId(iId);
        }

        private void EvaluatePlaying()
        {
            iBuffering.Update(iCurrentId == iId && iCurrentTransportState == "Buffering");
            iPlaying.Update(iCurrentId == iId && iCurrentTransportState == "Playing");
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

    public interface IProxySdp : IProxy
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
        Task Eject();
        Task SeekId(uint aValue);
        Task SeekSecondAbsolute(uint aValue);
        Task SeekSecondRelative(int aValue);

        Task SetRepeat(bool aValue);
        Task SetShuffle(bool aValue);

        IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot { get; }
    }

    public abstract class ServiceSdp : Service
    {
        protected ServiceSdp(INetwork aNetwork, IInjectorDevice aDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
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
            return new ProxySdp(this, aDevice);
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

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iMediaSupervisor.Snapshot;
            }
        }

        public abstract Task Play();
        public abstract Task Pause();
        public abstract Task Stop();
        public abstract Task Previous();
        public abstract Task Next();
        public abstract Task Eject();
        public abstract Task SeekId(uint aValue);
        public abstract Task SeekSecondAbsolute(uint aValue);
        public abstract Task SeekSecondRelative(int aValue);
        public abstract Task SetRepeat(bool aValue);
        public abstract Task SetShuffle(bool aValue);

        protected IWatchableThread iThread;
        protected Watchable<uint> iId;
        protected Watchable<string> iTransportState;
        protected Watchable<bool> iRepeat;
        protected Watchable<bool> iShuffle;
        protected MediaSupervisor<IMediaPreset> iMediaSupervisor;
    }

    class ServiceSdpNetwork : ServiceSdp
    {
        public ServiceSdpNetwork(INetwork aNetwork, IInjectorDevice aDevice, CpDevice aCpDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iCpDevice = aCpDevice;
            iCpDevice.AddRef();

            iService = new CpProxyLinnCoUkSdp1(aCpDevice);

            iService.SetPropertyTrackChanged(HandleIdChanged);
            iService.SetPropertyPlayStateChanged(HandleTransportStateChanged);
            iService.SetPropertyRepeatModeChanged(HandleRepeatChanged);
            iService.SetPropertyProgramModeChanged(HandleShuffleChanged);
            iService.SetPropertyTotalTracksChanged(HandleTotalTracksChanged);
            iService.SetPropertyTrayStateChanged(HandleTrayStateChanged);

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

            iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new SdpSnapshot(iNetwork, 0, this));

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

        public override Task Previous()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetPrev("SkipTrack", (ptr) =>
            {
                try
                {
                    iService.EndSetPrev(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task Next()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetNext("SkipTrack", (ptr) =>
            {
                try
                {
                    iService.EndSetNext(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task Eject()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            if (iTrayState == "Tray Closed" || iTrayState == "Tray Closing")
            {
                iService.BeginOpen((ptr) =>
                {
                    try
                    {
                        iService.EndOpen(ptr);
                        taskSource.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        taskSource.SetException(e);
                    }
                });
            }
            else if (iTrayState == "Tray Open" || iTrayState == "Tray Opening")
            {
                iService.BeginClose((ptr) =>
                {
                    try
                    {
                        iService.EndClose(ptr);
                        taskSource.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        taskSource.SetException(e);
                    }
                });
            }
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task SeekId(uint aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetTrack((int)aValue, 0, (ptr) =>
            {
                try
                {
                    iService.EndSetTrack(ptr);
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

        public override Task SeekSecondAbsolute(uint aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetTime(aValue.ToString() + ".00", 0, (ptr) =>
            {
                try
                {
                    iService.EndSetTime(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task SeekSecondRelative(int aValue)
        {
            throw new NotSupportedException();
        }

        public override Task SetRepeat(bool aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            if (aValue)
            {
                iService.BeginSetRepeatAll((ptr) =>
                {
                    try
                    {
                        iService.EndSetRepeatAll(ptr);
                        taskSource.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        taskSource.SetException(e);
                    }
                });
            }
            else
            {
                iService.BeginSetRepeatOff((ptr) =>
                {
                    try
                    {
                        iService.EndSetRepeatOff(ptr);
                        taskSource.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        taskSource.SetException(e);
                    }
                });
            }
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task SetShuffle(bool aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            if (aValue)
            {
                iService.BeginSetProgramShuffle((ptr) =>
                {
                    try
                    {
                        iService.EndSetProgramShuffle(ptr);
                        taskSource.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        taskSource.SetException(e);
                    }
                });
            }
            else
            {
                iService.BeginSetProgramOff((ptr) =>
                {
                    try
                    {
                        iService.EndSetProgramOff(ptr);
                        taskSource.SetResult(true);
                    }
                    catch (Exception e)
                    {
                        taskSource.SetException(e);
                    }
                });
            }
            return taskSource.Task.ContinueWith((t) => { });
        }

        private void HandleIdChanged()
        {
            int id = iService.PropertyTrack();
            if (id < 0)
            {
                id = 0;
            }
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iId.Update((uint)id);
                });
            });
        }

        private void HandleTransportStateChanged()
        {
            string transportState = iService.PropertyPlayState();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iTransportState.Update(transportState);
                });
            });
        }

        private void HandleRepeatChanged()
        {
            string repeat = iService.PropertyRepeatMode();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iRepeat.Update(repeat != "Off");
                });
            });
        }

        private void HandleShuffleChanged()
        {
            string shuffle = iService.PropertyProgramMode();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iShuffle.Update(shuffle == "Shuffle");
                });
            });
        }

        private void HandleTotalTracksChanged()
        {
            int tracks = iService.PropertyTotalTracks();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iMediaSupervisor.Update(new SdpSnapshot(iNetwork, (uint)tracks, this));
                });
            });
        }

        private void HandleTrayStateChanged()
        {
            string trayState = iService.PropertyTrayState();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iTrayState = trayState;
                });
            });
        }

        private readonly CpDevice iCpDevice;
        private TaskCompletionSource<bool> iSubscribedSource;
        private CpProxyLinnCoUkSdp1 iService;
        private string iTrayState;
    }

    class SdpSnapshot : IMediaClientSnapshot<IMediaPreset>
    {
        private readonly INetwork iNetwork;
        private readonly uint iTrackCount;
        private readonly ServiceSdp iSdp;

        public SdpSnapshot(INetwork aNetwork, uint aTrackCount, ServiceSdp aSdp)
        {
            iNetwork = aNetwork;
            iTrackCount = aTrackCount;
            iSdp = aSdp;
        }

        public uint Total
        {
            get
            {
                return iTrackCount;
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

            iNetwork.Schedule(() =>
            {
                List<IMediaPreset> tracks = new List<IMediaPreset>();
                for (uint i = aIndex; i < aIndex + aCount; ++i)
                {
                    MediaMetadata metadata = new MediaMetadata();
                    metadata.Add(iNetwork.TagManager.Audio.Title, "Track " + (i + 1));
                    tracks.Add(new MediaPresetSdp(iNetwork, i, (i + 1), metadata, iSdp));
                }

                aCallback(tracks);
            });
        }
    }

    class ServiceSdpMock : ServiceSdp, IMockable
    {
        private MediaSnapshot<IMediaPreset> iMediaSnapshot;
        private uint iTrackCount;

        public ServiceSdpMock(INetwork aNetwork, IInjectorDevice aDevice, uint aId, uint aTrackCount, bool aRepeat,
            bool aShuffle, string aTransportState, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iTrackCount = aTrackCount;

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

            base.Dispose();
        }

        protected override Task OnSubscribe()
        {
            iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new SdpSnapshot(iNetwork, iTrackCount, this));

            return base.OnSubscribe();
        }

        protected override void OnUnsubscribe()
        {
            if (iMediaSupervisor != null)
            {
                iMediaSupervisor.Dispose();
                iMediaSupervisor = null;
            }

            base.OnUnsubscribe();
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
                    int index = (int)iId.Value;
                    if (index > 0)
                    {
                        iId.Update((uint)(index - 1));
                    }
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
                    int index = (int)iId.Value;
                    if (index < iTrackCount - 1)
                    {
                        iId.Update((uint)(index + 1));
                    }
                });
            });
            return task;
        }

        public override Task Eject()
        {
            Task task = Task.Factory.StartNew(() =>
            {
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
            throw new NotSupportedException();
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

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "id")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iId.Update(uint.Parse(value.First()));
            }
            else if (command == "trackcount")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iTrackCount = uint.Parse(value.First());
                iMediaSupervisor.Update(new SdpSnapshot(Network, iTrackCount, this));

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

    public class ProxySdp : Proxy<ServiceSdp>, IProxySdp
    {
        public ProxySdp(ServiceSdp aService, IDevice aDevice)
            : base(aService, aDevice)
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

        public Task Eject()
        {
            return iService.Eject();
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
