using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomController : ISourceController
    {
        IWatchable<bool> Active { get; }

        IWatchable<EStandby> Standby { get; }
        void SetStandby(bool aValue);

        IWatchable<bool> HasVolume { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Volume { get; }
        void SetMute(bool aMute);
        void SetVolume(uint aVolume);
        void VolumeInc();
        void VolumeDec();
    }

    public class StandardRoomController : IWatcher<ITopology4Source>, IDisposable
    {
        internal StandardRoomController(IWatchableThread aThread, StandardRoom aRoom)
        {
            iThread = aThread;
            iRoom = aRoom;

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(aThread, string.Format("Active({0})", aRoom.Name), true);

            iStandby = new WatchableProxy<EStandby>(aRoom.Standby);

            iHasVolume = new Watchable<bool>(aThread, string.Format("HasVolume({0})", aRoom.Name), false);
            iMute = new Watchable<bool>(aThread, string.Format("Mute({0})", aRoom.Name), false);
            iValue = new Watchable<uint>(aThread, string.Format("Volume({0})", aRoom.Name), 0);

            iHasSourceControl = new Watchable<bool>(aThread, string.Format("HasSourceControl({0})", aRoom.Name), false);
            iHasInfoNext = new Watchable<bool>(aThread, string.Format("HasInfoNext({0})", aRoom.Name), false);
            iInfoNext = new Watchable<IInfoMetadata>(iThread, string.Format("InfoNext({0})", iRoom.Name), new RoomMetadata());

            iCanPause = new Watchable<bool>(aThread, string.Format("CanPause({0})", aRoom.Name), false);
            iCanSkip = new Watchable<bool>(aThread, string.Format("CanSkip({0})", aRoom.Name), false);
            iCanSeek = new Watchable<bool>(aThread, string.Format("CanSeek({0})", aRoom.Name), false);
            iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aRoom.Name), string.Empty);

            iRoom.Source.AddWatcher(this);

            iRoom.AddController(this);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iIsActive)
                {
                    iStandby.Detach();

                    iRoom.Source.RemoveWatcher(this);
                    iRoom.RemoveController(this);
                }
            }

            iRoom = null;

            iActive.Dispose();
            iActive = null;

            iStandby.Dispose();
            iStandby = null;

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
        }

        internal void SetInactive()
        {
            lock (iLock)
            {
                iIsActive = false;

                iActive.Update(false);

                iStandby.Detach();

                iRoom.Source.RemoveWatcher(this);
                iRoom.RemoveController(this);
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
                return iStandby;
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

        public void SetMute(bool aMute)
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iVolumeController.SetMute(aMute);
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

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            iSourceController = SourceController.Create(iThread, aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iTransportState, iCanPause, iCanSkip, iCanSeek);

            if (aValue.Volumes.Count() > 0)
            {
                ITopology4Group group = aValue.Volumes.ElementAt(0);
                iVolumeController = new VolumeController(iThread, group.Device, iHasVolume, iMute, iValue);
            }
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if (iSourceController != null)
            {
                iSourceController.Dispose();
                iSourceController = null;
            }

            iSourceController = SourceController.Create(iThread, aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iTransportState, iCanPause, iCanSkip, iCanSeek);

            if (aValue.Volumes.Count() > 0)
            {
                ITopology4Group group = aValue.Volumes.ElementAt(0);
                if (iVolumeController != null)
                {
                    if (group.Device != iVolumeController.Device)
                    {
                        iVolumeController.Dispose();
                        iVolumeController = new VolumeController(iThread, group.Device, iHasVolume, iMute, iValue);
                    }
                }
                else
                {
                    iVolumeController = new VolumeController(iThread, group.Device, iHasVolume, iMute, iValue);
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
        private StandardRoom iRoom;

        private object iLock;
        private bool iIsActive;
        private Watchable<bool> iActive;
        private WatchableProxy<EStandby> iStandby;

        private VolumeController iVolumeController;
        private Watchable<bool> iHasVolume;
        private Watchable<bool> iMute;
        private Watchable<uint> iValue;

        private ISourceController iSourceController;
        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iHasInfoNext;
        private Watchable<IInfoMetadata> iInfoNext;
        private Watchable<string> iTransportState;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSkip;
        private Watchable<bool> iCanSeek;
    }
}
