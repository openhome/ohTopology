using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IVolumeController : IDisposable
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

    public class VolumeController
    {
        public static IVolumeController Create(IStandardRoom aRoom)
        {
            return new StandardVolumeController(aRoom);
        }

        public static IVolumeController Create(IZone aZone)
        {
            return new ZoneVolumeController(aZone);
        }
    }

    class StandardVolumeController : IVolumeController, IWatcher<ITopology4Source>, IWatcher<bool>, IWatcher<uint>
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

    class VolumeWatcher : IWatcher<bool>, IDisposable
    {
        private readonly ZoneVolumeController iController;
        private IVolumeController iVolume;

        public VolumeWatcher(ZoneVolumeController aController, IStandardRoom aRoom)
        {
            iController = aController;

            iVolume = VolumeController.Create(aRoom);
            iVolume.HasVolume.AddWatcher(this);
        }

        public void Dispose()
        {
            iVolume.HasVolume.RemoveWatcher(this);
            iVolume.Dispose();
        }

        public void ItemOpen(string aId, bool aValue)
        {
            if (aValue)
            {
                iController.AddVolumeController(iVolume);
            }
        }

        public void ItemUpdate(string aId, bool aValue, bool aPrevious)
        {
            if (aPrevious)
            {
                iController.RemoveVolumeController(iVolume);
            }
            if (aValue)
            {
                iController.AddVolumeController(iVolume);
            }
        }

        public void ItemClose(string aId, bool aValue)
        {
            if (aValue)
            {
                iController.RemoveVolumeController(iVolume);
            }
        }
    }

    class ZoneVolumeController : IVolumeController, IUnorderedWatcher<IStandardRoom>, IWatcher<bool>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly IZone iZone;

        private readonly Watchable<bool> iHasVolume; 
        private readonly Watchable<bool> iMute;
        private readonly Watchable<uint> iValue;
        private readonly Watchable<uint> iVolumeLimit;

        private readonly Dictionary<IStandardRoom, VolumeWatcher> iVolumeLookup;
        private readonly List<IVolumeController> iVolumeControllers;

        private uint iMuteCount;

        public ZoneVolumeController(IZone aZone)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aZone.Room.Network;
            iZone = aZone;

            iMuteCount = 0;
            iVolumeLookup = new Dictionary<IStandardRoom, VolumeWatcher>();
            iVolumeControllers = new List<IVolumeController>();

            iHasVolume = new Watchable<bool>(iNetwork, "HasVolume", false);
            iMute = new Watchable<bool>(iNetwork, "Mute", false);
            iValue = new Watchable<uint>(iNetwork, "Value", 0);
            iVolumeLimit = new Watchable<uint>(iNetwork, "VolumeLimit", 0);

            VolumeWatcher w = new VolumeWatcher(this, aZone.Room);
            iVolumeLookup.Add(aZone.Room, w);
            iZone.Listeners.AddWatcher(this);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iZone.Listeners.RemoveWatcher(this);

            foreach (var kvp in iVolumeLookup)
            {
                kvp.Value.Dispose();
            }

            iHasVolume.Dispose();
            iMute.Dispose();
            iValue.Dispose();
            iVolumeLimit.Dispose();
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
            iNetwork.Schedule(() =>
            {
                iVolumeControllers.ForEach(v => v.SetMute(aMute));
            });
        }

        public void SetVolume(uint aVolume)
        {
            iNetwork.Schedule(() =>
            {
                iVolumeControllers.ForEach(v => v.SetVolume(aVolume));
            });
        }

        public void VolumeInc()
        {
            iNetwork.Schedule(() =>
            {
                iVolumeControllers.ForEach(v => v.VolumeInc());
            });
        }

        public void VolumeDec()
        {
            iNetwork.Schedule(() =>
            {
                iVolumeControllers.ForEach(v => v.VolumeDec());
            });
        }

        internal void AddVolumeController(IVolumeController aController)
        {
            iVolumeControllers.Add(aController);
            aController.Mute.AddWatcher(this);
        }

        internal void RemoveVolumeController(IVolumeController aController)
        {
            iVolumeControllers.Remove(aController);
            aController.Mute.RemoveWatcher(this);
        }

        private void EvaluateMute()
        {
            bool mute = false;

            if (iMuteCount > 0)
            {
                if (iMuteCount == iVolumeControllers.Count)
                {
                    mute = true;
                }
            }

            iMute.Update(mute);
        }

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedClose()
        {
        }

        public void UnorderedAdd(IStandardRoom aItem)
        {
            VolumeWatcher w = new VolumeWatcher(this, aItem);
            iVolumeLookup.Add(aItem, w);
        }

        public void UnorderedRemove(IStandardRoom aItem)
        {
            iVolumeLookup[aItem].Dispose();
            iVolumeLookup.Remove(aItem);
        }

        public void ItemOpen(string aId, bool aValue)
        {
            if (aValue)
            {
                ++iMuteCount;
            }
            EvaluateMute();
        }

        public void ItemUpdate(string aId, bool aValue, bool aPrevious)
        {
            if (aPrevious)
            {
                --iMuteCount;
            }
            if (aValue)
            {
                ++iMuteCount;
            }
            EvaluateMute();
        }

        public void ItemClose(string aId, bool aValue)
        {
            if (aValue)
            {
                --iMuteCount;
            }
            EvaluateMute();
        }
    }
}
