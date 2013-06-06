using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomController : ISourceController
    {
        string Name { get; }

        IWatchable<bool> Active { get; }

        IWatchable<bool> HasInfoNext { get; }
        IWatchable<IInfoMetadata> InfoNext { get; }

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

        IWatchable<bool> HasVolume { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Volume { get; }
        IWatchable<uint> VolumeLimit { get; }
        void SetMute(bool aMute);
        void SetVolume(uint aVolume);
        void VolumeInc();
        void VolumeDec();
    }

    public class StandardRoomController : IWatcher<ITopology4Source>, IStandardRoomController, IDisposable
    {
        public StandardRoomController(IStandardRoom aRoom)
        {
            iThread = aRoom.Network;
            iRoom = aRoom;

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(iThread, "Active", true);

            iHasVolume = new Watchable<bool>(iThread, "HasVolume", false);
            iMute = new Watchable<bool>(iThread, "Mute", false);
            iValue = new Watchable<uint>(iThread, "Volume", 0);
            iVolumeLimit = new Watchable<uint>(iThread, "VolumeLimit", 0);

            iHasSourceControl = new Watchable<bool>(iThread, "HasSourceControl", false);
            iHasInfoNext = new Watchable<bool>(iThread, "HasInfoNext", false);
            iInfoNext = new Watchable<IInfoMetadata>(iThread, "InfoNext", new RoomMetadata());

            iCanPause = new Watchable<bool>(iThread, "CanPause", false);
            iCanSkip = new Watchable<bool>(iThread, "CanSkip", false);
            iCanSeek = new Watchable<bool>(iThread, "CanSeek", false);
            iTransportState = new Watchable<string>(iThread, "TransportState", string.Empty);

            iHasPlayMode = new Watchable<bool>(iThread, "HasPlayMode", false);
            iShuffle = new Watchable<bool>(iThread, "Shuffle", false);
            iRepeat = new Watchable<bool>(iThread, "Repeat", false);

            iRoom.Source.AddWatcher(this);

            iRoom.Join(SetInactive);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iIsActive)
                {
                    iThread.Execute(() =>
                    {
                        iRoom.Source.RemoveWatcher(this);
                    });
                    iRoom.UnJoin(SetInactive);
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

            iHasVolume.Dispose();
            iHasVolume = null;

            iMute.Dispose();
            iMute = null;

            iValue.Dispose();
            iValue = null;

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
        }

        private void SetInactive()
        {
            lock (iLock)
            {
                iIsActive = false;

                iActive.Update(false);

                iRoom.Source.RemoveWatcher(this);
                iRoom.UnJoin(SetInactive);
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
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iRoom.SetStandby(aValue);
                }
            });
        }

        public void SetRepeat(bool aValue)
        {
            iThread.Schedule(() =>
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
            iThread.Schedule(() =>
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
                return iHasVolume;
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                return iMute;
            }
        }

        public IWatchable<uint> Volume
        {
            get
            {
                return iValue;
            }
        }

        public IWatchable<uint> VolumeLimit
        {
            get
            {
                return iVolumeLimit;
            }
        }

        public void SetMute(bool aMute)
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasVolume.Value)
                    {
                        iVolumeController.SetMute(aMute);
                    }
                }
            });
        }

        public void SetVolume(uint aVolume)
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasVolume.Value)
                    {
                        iVolumeController.SetVolume(aVolume);
                    }
                }
            });
        }

        public void VolumeInc()
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasVolume.Value)
                    {
                        iVolumeController.VolumeInc();
                    }
                }
            });
        }

        public void VolumeDec()
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasVolume.Value)
                    {
                        iVolumeController.VolumeDec();
                    }
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
            iThread.Schedule(() =>
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
            iThread.Schedule(() =>
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
            iThread.Schedule(() =>
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
            iThread.Schedule(() =>
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
            iThread.Schedule(() =>
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
            iThread.Schedule(() =>
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
            iSourceController = SourceController.Create(iThread, aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iTransportState, iCanPause, iCanSkip, iCanSeek, iHasPlayMode, iShuffle, iRepeat);

            if (aValue.Volumes.Count() > 0)
            {
                ITopology4Group group = aValue.Volumes.ElementAt(0);
                iVolumeController = new VolumeController(iThread, group.Device, iHasVolume, iMute, iValue, iVolumeLimit);
            }
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if (iSourceController != null)
            {
                iSourceController.Dispose();
                iSourceController = null;
            }

            iSourceController = SourceController.Create(iThread, aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iTransportState, iCanPause, iCanSkip, iCanSeek, iHasPlayMode, iShuffle, iRepeat);

            if (aValue.Volumes.Count() > 0)
            {
                ITopology4Group group = aValue.Volumes.ElementAt(0);
                if (iVolumeController != null)
                {
                    if (group.Device != iVolumeController.Device)
                    {
                        iVolumeController.Dispose();
                        iVolumeController = new VolumeController(iThread, group.Device, iHasVolume, iMute, iValue, iVolumeLimit);
                    }
                }
                else
                {
                    iVolumeController = new VolumeController(iThread, group.Device, iHasVolume, iMute, iValue, iVolumeLimit);
                }
            }
            else
            {
                if (iVolumeController != null)
                {
                    iVolumeController.Dispose();
                    iVolumeController = null;
                }
            }
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            if (iSourceController != null)
            {
                iSourceController.Dispose();
                iSourceController = null;
            }

            if (iVolumeController != null)
            {
                iVolumeController.Dispose();
                iVolumeController = null;
            }
        }

        private IWatchableThread iThread;
        private IStandardRoom iRoom;

        private object iLock;
        private bool iIsActive;
        private Watchable<bool> iActive;

        private VolumeController iVolumeController;
        private Watchable<bool> iHasVolume;
        private Watchable<bool> iMute;
        private Watchable<uint> iValue;
        private Watchable<uint> iVolumeLimit;

        private ISourceController iSourceController;
        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iHasInfoNext;
        private Watchable<IInfoMetadata> iInfoNext;
        private Watchable<string> iTransportState;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSkip;
        private Watchable<bool> iCanSeek;
        private Watchable<bool> iHasPlayMode;
        private Watchable<bool> iShuffle;
        private Watchable<bool> iRepeat;
    }
}
