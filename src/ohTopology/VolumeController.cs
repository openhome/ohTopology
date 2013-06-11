using System;
using System.Threading.Tasks;
using System.Linq;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IVolumeController
    {
        IWatchable<bool> HasVolume { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Volume { get; }
        IWatchable<uint> VolumeLimit { get; }

        void SetMute(bool aMute);
        void SetVolume(uint aVolume);
        void VolumeInc();
        void VolumeDec();
    }

    class StandardVolumeController : IWatcher<ITopology4Source>, IWatcher<bool>, IWatcher<uint>, IDisposable
    {
        public StandardVolumeController(IStandardRoom aRoom)
        {
            iDisposed = false;
            iRoom = aRoom;

            iHasVolume = new Watchable<bool>(aRoom.Network, "HasVolume", false);
            iMute = new Watchable<bool>(aRoom.Network, "Mute", false);
            iValue = new Watchable<uint>(aRoom.Network, "Volume", 0);
            iVolumeLimit = new Watchable<uint>(aRoom.Network, "VolumeLimit", 0);

            aRoom.Source.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("VolumeController.Dispose");
            }

            iRoom.Source.RemoveWatcher(this);

            DestroyProxy();

            iMute.Dispose();
            iMute = null;

            iValue.Dispose();
            iValue = null;

            iVolumeLimit.Dispose();
            iVolumeLimit = null;

            iDisposed = true;
        }

        public string Name
        {
            get
            {
                return iRoom.Name;
            }
        }

        public IDevice Device
        {
            get
            {
                return iDevice;
            }
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

        public void SetMute(bool aValue)
        {
            if (iVolume != null)
            {
                iVolume.SetMute(aValue);
            }
        }

        public void SetVolume(uint aValue)
        {
            if (iVolume != null)
            {
                iVolume.SetVolume(aValue);
            }
        }

        public void VolumeInc()
        {
            if (iVolume != null)
            {
                iVolume.VolumeInc();
            }
        }

        public void VolumeDec()
        {
            if (iVolume != null)
            {
                iVolume.VolumeDec();
            }
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            if (aValue.Volumes.Count() > 0)
            {
                ITopology4Group group = aValue.Volumes.ElementAt(0);
                CreateProxy(group.Device);
            }
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if (aValue.Volumes.Count() > 0)
            {
                ITopology4Group group = aValue.Volumes.ElementAt(0);
                if (iVolume != null)
                {
                    if (group.Device != iDevice)
                    {
                        DestroyProxy();
                        CreateProxy(group.Device);
                    }
                }
                else
                {
                    CreateProxy(group.Device);
                }
            }
            else
            {
                DestroyProxy();
            }
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            DestroyProxy();
        }

        private void CreateProxy(IDevice aDevice)
        {
            iDevice = aDevice;

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

        private void DestroyProxy()
        {
            iHasVolume.Update(false);

            if (iVolume != null)
            {
                iVolume.Mute.RemoveWatcher(this);
                iVolume.Value.RemoveWatcher(this);
                iVolume.VolumeLimit.RemoveWatcher(this);

                iVolume.Dispose();
                iVolume = null;
            }
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
        private IStandardRoom iRoom;
        private IProxyVolume iVolume;

        private IDevice iDevice;
        private Watchable<bool> iHasVolume;
        private Watchable<bool> iMute;
        private Watchable<uint> iValue;
        private Watchable<uint> iVolumeLimit;
    }
}
