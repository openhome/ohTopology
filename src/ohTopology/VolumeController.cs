using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class VolumeController : IWatcher<bool>, IWatcher<uint>, IDisposable
    {
        public VolumeController(IDevice aDevice, Watchable<bool> aHasVolume, Watchable<bool> aMute, Watchable<uint> aValue, Watchable<uint> aVolumeLimit)
        {
            iDisposed = false;

            iDevice = aDevice;

            iHasVolume = aHasVolume;
            iMute = aMute;
            iValue = aValue;
            iVolumeLimit = aVolumeLimit;

            iDevice.Create<IProxyVolume>((volume) =>
            {
                if (!iDisposed)
                {
                    iVolume = volume;

                    iVolume.Mute.AddWatcher(this);
                    iVolume.Value.AddWatcher(this);
                    iVolume.VolumeLimit.AddWatcher(this);

                    iHasVolume.Update(true);
                }
                else
                {
                    volume.Dispose();
                }
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("VolumeController.Dispose");
            }

            if (iVolume != null)
            {
                iHasVolume.Update(false);

                iVolume.Mute.RemoveWatcher(this);
                iVolume.Value.RemoveWatcher(this);
                iVolume.VolumeLimit.RemoveWatcher(this);

                iVolume.Dispose();
                iVolume = null;
            }

            iDisposed = true;
        }

        public IDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public void SetMute(bool aValue)
        {
            iVolume.SetMute(aValue);
        }

        public void SetVolume(uint aValue)
        {
            iVolume.SetVolume(aValue);
        }

        public void VolumeInc()
        {
            iVolume.VolumeInc();
        }

        public void VolumeDec()
        {
            iVolume.VolumeDec();
        }

        public void ItemOpen(string aId, bool aValue)
        {
            iMute.Update(aValue);
        }

        public void ItemUpdate(string aId, bool aValue, bool aPrevious)
        {
            iMute.Update(aValue);
        }

        public void ItemClose(string aId, bool aValue)
        {
        }

        public void ItemOpen(string aId, uint aValue)
        {
            if (aId == "Value")
            {
                iValue.Update(aValue);
            }
            if (aId == "VolumeLimit")
            {
                iVolumeLimit.Update(aValue);
            }
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            if (aId == "Value")
            {
                iValue.Update(aValue);
            }
            if (aId == "VolumeLimit")
            {
                iVolumeLimit.Update(aValue);
            }
        }

        public void ItemClose(string aId, uint aValue)
        {
        }

        private bool iDisposed;
        private IProxyVolume iVolume;

        private IDevice iDevice;
        private Watchable<bool> iHasVolume;
        private Watchable<bool> iMute;
        private Watchable<uint> iValue;
        private Watchable<uint> iVolumeLimit;
    }
}
