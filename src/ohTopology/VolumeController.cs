using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IVolumeController : IDisposable
    {
        string Name { get; }

        IWatchable<bool> HasVolume { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Volume { get; }
        IWatchable<uint> VolumeLimit { get; }

        void SetMute(bool aMute);
        void VolumeInc();
        void VolumeDec();
    }

    class StandardVolumeController : IVolumeController, IWatcher<ITopology4Source>, IWatcher<bool>, IWatcher<uint>
    {
        public StandardVolumeController(IStandardRoom aRoom)
        {
            iDisposeHandler = new DisposeHandler();
            iDisposed = false;
            iRoom = aRoom;
            iNetwork = aRoom.Network;
            iSource = aRoom.Source;

            iHasVolume = new Watchable<bool>(iNetwork, "HasVolume", false);
            iMute = new Watchable<bool>(iNetwork, "Mute", false);
            iValue = new Watchable<uint>(iNetwork, "Volume", 0);
            iVolumeLimit = new Watchable<uint>(iNetwork, "VolumeLimit", 0);

            iSource.AddWatcher(this);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                iSource.RemoveWatcher(this);
            });

            DestroyProxy();

            iMute.Dispose();
            iValue.Dispose();
            iVolumeLimit.Dispose();
            iHasVolume.Dispose();

            iDisposed = true;
        }

        public string Name
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iRoom.Name;
                }
            }
        }

        public IDevice Device
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iDevice;
                }
            }
        }

        public IWatchable<bool> HasVolume
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iHasVolume;
                }
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iMute;
                }
            }
        }

        public IWatchable<uint> Volume
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iValue;
                }
            }
        }

        public IWatchable<uint> VolumeLimit
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iVolumeLimit;
                }
            }
        }

        public void SetMute(bool aValue)
        {
            using (iDisposeHandler.Lock)
            {
                if (iVolume != null)
                {
                    iVolume.SetMute(aValue);
                }
            }
        }

        /*public void SetVolume(uint aValue)
        {
            if (iVolume != null)
            {
                iVolume.SetVolume(aValue);
            }
        }*/

        public void VolumeInc()
        {
            using (iDisposeHandler.Lock)
            {
                if (iVolume != null)
                {
                    iVolume.VolumeInc();
                }
            }
        }

        public void VolumeDec()
        {
            using (iDisposeHandler.Lock)
            {
                if (iVolume != null)
                {
                    iVolume.VolumeDec();
                }
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

        private readonly DisposeHandler iDisposeHandler;
        private readonly IStandardRoom iRoom;
        private readonly IWatchable<ITopology4Source> iSource;
        private readonly INetwork iNetwork;
        
        private bool iDisposed;
        private IProxyVolume iVolume;
        private IDevice iDevice;

        private readonly Watchable<bool> iHasVolume;
        private readonly Watchable<bool> iMute;
        private readonly Watchable<uint> iValue;
        private readonly Watchable<uint> iVolumeLimit;
    }

    class VolumeWatcher : IWatcher<bool>, IWatcher<uint>, IDisposable
    {
        private readonly ZoneVolumeController iController;
        private readonly IVolumeController iVolume;
        private uint iValue;
        private uint iVolumeLimit;
        private Watchable<uint> iWatchableValue;

        public VolumeWatcher(ZoneVolumeController aController, IStandardRoom aRoom)
        {
            iController = aController;

            iWatchableValue = new Watchable<uint>(aRoom.Network, "Value", iValue);

            iVolume = aRoom.CreateVolumeController();
            iVolume.Volume.AddWatcher(this);
            iVolume.VolumeLimit.AddWatcher(this);
            iVolume.HasVolume.AddWatcher(this);
        }

        public void Dispose()
        {
            iVolume.HasVolume.RemoveWatcher(this);
            iVolume.VolumeLimit.RemoveWatcher(this);
            iVolume.Volume.RemoveWatcher(this);
            iVolume.Dispose();

            iWatchableValue.Dispose();
        }

        public IWatchable<uint> Value
        {
            get
            {
                return iWatchableValue;
            }
        }

        public void ItemOpen(string aId, bool aValue)
        {
            if (aValue)
            {
                iController.AddVolumeController(iVolume, this);
            }
        }

        public void ItemUpdate(string aId, bool aValue, bool aPrevious)
        {
            if (aPrevious)
            {
                iController.RemoveVolumeController(iVolume, this);
            }
            if (aValue)
            {
                iController.AddVolumeController(iVolume, this);
            }
        }

        public void ItemClose(string aId, bool aValue)
        {
            if (aValue)
            {
                iController.RemoveVolumeController(iVolume, this);
            }
        }

        public void ItemOpen(string aId, uint aValue)
        {
            if (aId == "VolumeLimit")
            {
                iVolumeLimit = aValue;
            }
            if (aId == "Volume")
            {
                iValue = aValue;
            }
            EvaluateValue();
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            if (aId == "VolumeLimit")
            {
                iVolumeLimit = aValue;
            }
            if (aId == "Volume")
            {
                iValue = aValue;
            }
            EvaluateValue();
        }

        public void ItemClose(string aId, uint aValue)
        {
        }

        private void EvaluateValue()
        {
            if (iVolumeLimit == 0)
            {
                iWatchableValue.Update(0);
            }
            else
            {
                iWatchableValue.Update((uint)(((float)iValue / (float)iVolumeLimit) * 100));
            }
        }
    }

    class ZoneVolumeController : IVolumeController, IOrderedWatcher<IStandardRoom>, IWatcher<bool>, IWatcher<uint>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly IZoneSender iZone;

        private readonly Watchable<bool> iHasVolume; 
        private readonly Watchable<bool> iMute;
        private readonly Watchable<uint> iValue;
        private readonly Watchable<uint> iVolumeLimit;

        private readonly Dictionary<IStandardRoom, VolumeWatcher> iVolumeLookup;
        private readonly List<IVolumeController> iVolumeControllers;

        private uint iTotalValue;
        private uint iMuteCount;

        public ZoneVolumeController(IZoneSender aZone)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aZone.Room.Network;
            iZone = aZone;

            iTotalValue = 0;
            iMuteCount = 0;
            iVolumeLookup = new Dictionary<IStandardRoom, VolumeWatcher>();
            iVolumeControllers = new List<IVolumeController>();

            iHasVolume = new Watchable<bool>(iNetwork, "HasVolume", false);
            iMute = new Watchable<bool>(iNetwork, "Mute", false);
            iValue = new Watchable<uint>(iNetwork, "Value", 0);
            iVolumeLimit = new Watchable<uint>(iNetwork, "VolumeLimit", 100);

            VolumeWatcher w = new VolumeWatcher(this, aZone.Room);
            iVolumeLookup.Add(aZone.Room, w);
            iZone.Listeners.AddWatcher(this);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                iZone.Listeners.RemoveWatcher(this);

                foreach (var kvp in iVolumeLookup)
                {
                    kvp.Value.Dispose();
                }
            });
            iVolumeLookup.Clear();

            iHasVolume.Dispose();
            iMute.Dispose();
            iValue.Dispose();
            iVolumeLimit.Dispose();
        }

        public string Name
        {
            get
            {
                return iZone.Room.Name;
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

        public void SetMute(bool aMute)
        {
            iNetwork.Schedule(() =>
            {
                iVolumeControllers.ForEach(v => v.SetMute(aMute));
            });
        }

        /*public void SetVolume(uint aVolume)
        {
            iNetwork.Schedule(() =>
            {
                iVolumeControllers.ForEach(v => v.SetVolume(aVolume));
            });
        }*/

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

        internal void AddVolumeController(IVolumeController aController, VolumeWatcher aWatcher)
        {
            iVolumeControllers.Add(aController);
            aController.Mute.AddWatcher(this);
            aWatcher.Value.AddWatcher(this);
            iHasVolume.Update(iVolumeControllers.Count() > 0);
        }

        internal void RemoveVolumeController(IVolumeController aController, VolumeWatcher aWatcher)
        {
            iVolumeControllers.Remove(aController);
            aController.Mute.RemoveWatcher(this);
            aWatcher.Value.RemoveWatcher(this);
            iHasVolume.Update(iVolumeControllers.Count() > 0);
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

        public void OrderedOpen()
        {
        }

        public void OrderedInitialised()
        {
        }

        public void OrderedClose()
        {
        }

        public void OrderedAdd(IStandardRoom aItem, uint aIndex)
        {
            VolumeWatcher w = new VolumeWatcher(this, aItem);
            iVolumeLookup.Add(aItem, w);
        }

        public void OrderedMove(IStandardRoom aItem, uint aFrom, uint aTo) { }

        public void OrderedRemove(IStandardRoom aItem, uint aIndex)
        {
            VolumeWatcher watcher;
            if (iVolumeLookup.TryGetValue(aItem, out watcher))
            {
                watcher.Dispose();
                iVolumeLookup.Remove(aItem);
            }
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

        public void ItemOpen(string aId, uint aValue)
        {
            iTotalValue += aValue;
            iValue.Update(iTotalValue / (uint)iVolumeControllers.Count());
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            iTotalValue -= aPrevious;
            iTotalValue += aValue;
            iValue.Update(iTotalValue / (uint)iVolumeControllers.Count());
        }

        public void ItemClose(string aId, uint aValue)
        {
            iTotalValue -= aValue;
            if (iVolumeControllers.Count() > 0)
            {
                iValue.Update(iTotalValue / (uint)iVolumeControllers.Count());
            }
            else
            {
                iValue.Update(0);
            }
        }
    }
}
