using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomController : IDisposable
    {
        string Name { get; }

        IWatchable<bool> Active { get; }

        IWatchable<bool> HasInfoNext { get; }
        IWatchable<IInfoMetadata> InfoNext { get; }

        IWatchable<bool> HasSnapshot { get; }

        IWatchable<bool> HasSourceControl { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<bool> CanPause { get; }
        IWatchable<bool> CanSkip { get; }
        IWatchable<bool> CanSeek { get; }
        IWatchable<bool> HasPlayMode { get; }
        IWatchable<bool> Repeat { get; }
        IWatchable<bool> Shuffle { get; }

        IWatchable<EStandby> Standby { get; }
        void SetStandby(bool aValue);

        // Source Control
        IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot { get; }

        void Play();
        void Pause();
        void Stop();

        void Previous();
        void Next();

        void Seek(uint aSeconds);

        void SetRepeat(bool aValue);
        void SetShuffle(bool aValue);

        // Volume Control
        IWatchable<bool> HasVolume { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Volume { get; }
        IWatchable<uint> VolumeLimit { get; }

        void SetMute(bool aMute);
        void VolumeInc();
        void VolumeDec();
    }

    internal class StandardRoomController : IWatcher<ITopology4Source>, IStandardRoomController, IDisposable
    {
        public StandardRoomController(IStandardRoom aRoom)
        {
            iDisposed = false;
            iNetwork = aRoom.Network;
            iRoom = aRoom;
            iSource = aRoom.Source;

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(iNetwork, "Active", true);

            iVolumeController = aRoom.CreateVolumeController();

            iHasSourceControl = new Watchable<bool>(iNetwork, "HasSourceControl", false);
            iHasInfoNext = new Watchable<bool>(iNetwork, "HasInfoNext", false);
            iInfoNext = new Watchable<IInfoMetadata>(iNetwork, "InfoNext", new RoomMetadata());
            iHasContainer = new Watchable<bool>(iNetwork, "HasQueue", false);

            iCanPause = new Watchable<bool>(iNetwork, "CanPause", false);
            iCanSkip = new Watchable<bool>(iNetwork, "CanSkip", false);
            iCanSeek = new Watchable<bool>(iNetwork, "CanSeek", false);
            iTransportState = new Watchable<string>(iNetwork, "TransportState", "Stopped");

            iHasPlayMode = new Watchable<bool>(iNetwork, "HasPlayMode", false);
            iShuffle = new Watchable<bool>(iNetwork, "Shuffle", false);
            iRepeat = new Watchable<bool>(iNetwork, "Repeat", false);

            iSource.AddWatcher(this);
            iRoom.Join(SetInactive);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("StandardRoomController.Dispose");
            }

            lock (iLock)
            {
                if (iIsActive)
                {
                    iNetwork.Execute(() =>
                    {
                        iSource.RemoveWatcher(this);
                    });
                    iRoom.Unjoin(SetInactive);
                    iIsActive = false;
                }
            }

            iRoom = null;

            iActive.Dispose();
            iActive = null;

            iHasSourceControl.Dispose();
            iHasSourceControl = null;

            iHasInfoNext.Dispose();
            iHasInfoNext = null;

            iInfoNext.Dispose();
            iInfoNext = null;

            iHasContainer.Dispose();
            iHasContainer = null;

            iVolumeController.Dispose();
            iVolumeController = null;

            iTransportState.Dispose();
            iTransportState = null;

            iCanPause.Dispose();
            iCanPause = null;

            iCanSkip.Dispose();
            iCanSkip = null;

            iCanSeek.Dispose();
            iCanSeek = null;

            iHasPlayMode.Dispose();
            iHasPlayMode = null;

            iShuffle.Dispose();
            iShuffle = null;

            iRepeat.Dispose();
            iRepeat = null;

            iDisposed = true;
        }

        private void SetInactive()
        {
            lock (iLock)
            {
                if (iIsActive)
                {
                    iIsActive = false;

                    iActive.Update(false);

                    iSource.RemoveWatcher(this);
                    iRoom.Unjoin(SetInactive);
                }
            }
        }

        public IWatchable<bool> Active
        {
            get
            {
                return iActive;
            }
        }

        public IWatchable<EStandby> Standby
        {
            get
            {
                return iRoom.Standby;
            }
        }

        public void SetStandby(bool aValue)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iRoom.SetStandby(aValue);
                }
            });
        }

        public void SetRepeat(bool aValue)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.SetRepeat(aValue);
                    }
                }
            });
        }

        public void SetShuffle(bool aValue)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.SetShuffle(aValue);
                    }
                }
            });
        }

        public IWatchable<bool> HasVolume
        {
            get
            {
                return iVolumeController.HasVolume;
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                return iVolumeController.Mute;
            }
        }

        public IWatchable<uint> Volume
        {
            get
            {
                return iVolumeController.Volume;
            }
        }

        public IWatchable<uint> VolumeLimit
        {
            get
            {
                return iVolumeController.VolumeLimit;
            }
        }

        public void SetMute(bool aMute)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iVolumeController.SetMute(aMute);
                }
            });
        }

        /*public void SetVolume(uint aVolume)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                   iVolumeController.SetVolume(aVolume);
                }
            });
        }*/

        public void VolumeInc()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iVolumeController.VolumeInc();
                }
            });
        }

        public void VolumeDec()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iVolumeController.VolumeDec();
                }
            });
        }

        public string Name
        {
            get
            {
                return iRoom.Name;
            }
        }

        public IWatchable<bool> HasSourceControl
        {
            get
            {
                return iHasSourceControl;
            }
        }

        public IWatchable<bool> HasInfoNext
        {
            get
            {
                return iHasInfoNext;
            }
        }

        public IWatchable<IInfoMetadata> InfoNext
        {
            get
            {
                return iInfoNext;
            }
        }

        public IWatchable<bool> HasSnapshot
        {
            get
            {
                return iHasContainer;
            }
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                IWatchable<IWatchableSnapshot<IMediaPreset>> snapshot = null;
                iNetwork.Execute(() =>
                {
                    if (iActive.Value)
                    {
                        if (iHasSourceControl.Value)
                        {
                            snapshot = iSourceController.Snapshot;
                        }
                    }
                });
                return snapshot;
            }
        }

        public IWatchable<string> TransportState
        {
            get
            {
                return iTransportState;
            }
        }

        public IWatchable<bool> CanPause
        {
            get
            {
                return iCanPause;
            }
        }

        public void Play()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Play();
                    }
                }
            });
        }

        public void Pause()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Pause();
                    }
                }
            });
        }

        public void Stop()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Stop();
                    }
                }
            });
        }

        public IWatchable<bool> CanSkip
        {
            get
            {
                return iCanSkip;
            }
        }

        public void Previous()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Previous();
                    }
                }
            });
        }

        public void Next()
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Next();
                    }
                }
            });
        }

        public IWatchable<bool> CanSeek
        {
            get
            {
                return iCanSeek;
            }
        }

        public void Seek(uint aSeconds)
        {
            iNetwork.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasSourceControl.Value)
                    {
                        iSourceController.Seek(aSeconds);
                    }
                }
            });
        }

        public IWatchable<bool> HasPlayMode
        {
            get
            {
                return iHasPlayMode;
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

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            iSourceController = SourceController.Create(aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iHasContainer, iTransportState, iCanPause, iCanSkip, iCanSeek, iHasPlayMode, iShuffle, iRepeat);
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if (iSourceController != null)
            {
                iSourceController.Dispose();
                iSourceController = null;
            }

            iSourceController = SourceController.Create(aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iHasContainer, iTransportState, iCanPause, iCanSkip, iCanSeek, iHasPlayMode, iShuffle, iRepeat);
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            if (iSourceController != null)
            {
                iSourceController.Dispose();
                iSourceController = null;
            }
        }

        private bool iDisposed;
        private INetwork iNetwork;
        private IStandardRoom iRoom;
        private IWatchable<ITopology4Source> iSource;

        private object iLock;
        private bool iIsActive;
        private Watchable<bool> iActive;

        private IVolumeController iVolumeController;

        private ISourceController iSourceController;
        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iHasInfoNext;
        private Watchable<IInfoMetadata> iInfoNext;
        private Watchable<bool> iHasContainer;
        private Watchable<string> iTransportState;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSkip;
        private Watchable<bool> iCanSeek;
        private Watchable<bool> iHasPlayMode;
        private Watchable<bool> iShuffle;
        private Watchable<bool> iRepeat;
    }
}
