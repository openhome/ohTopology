using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomTime : IDisposable
    {
        IWatchable<bool> Active { get; }
        IWatchable<bool> HasTime { get; }
        IWatchable<uint> Duration { get; }
        IWatchable<uint> Seconds { get; }
    }

    public class StandardRoomTime : IWatcher<ITopology4Source>, IWatcher<uint>
    {
        public StandardRoomTime(IStandardRoom aRoom)
        {
            iThread = aRoom.WatchableThread;
            iRoom = aRoom;

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(iThread, string.Format("Active({0})", aRoom.Name), true);

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

        public IWatchable<bool> HasTime
        {
            get
            {
                return iHasTime;
            }
        }

        public IWatchable<uint> Duration
        {
            get
            {
                return iDuration;
            }
        }

        public IWatchable<uint> Seconds
        {
            get
            {
                return iSeconds;
            }
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            iHasTime = new Watchable<bool>(iThread, string.Format("HasTime({0})", iRoom.Name), false);
            iDuration = new Watchable<uint>(iThread, string.Format("Duration({0})", iRoom.Name), 0);
            iSeconds = new Watchable<uint>(iThread, string.Format("Seconds({0})", iRoom.Name), 0);

            if (aValue.HasTime)
            {
                Subscribe(aValue.Device);
            }
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if (aPrevious.HasTime && !aValue.HasTime || aPrevious.HasTime && aValue.HasTime && aPrevious.Device != aValue.Device)
            {
                Unsubscribe();
            }

            if (!aPrevious.HasTime && aValue.HasTime || aPrevious.HasTime && aValue.HasTime && aPrevious.Device != aValue.Device)
            {
                Subscribe(aValue.Device);
            }
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            if (aValue.HasTime)
            {
                Unsubscribe();
            }

            iHasTime.Dispose();
            iHasTime = null;

            iDuration.Dispose();
            iDuration = null;

            iSeconds.Dispose();
            iSeconds = null;
        }

        private void Subscribe(IWatchableDevice aDevice)
        {
            Task<ProxyTime> task = aDevice.Create<ProxyTime>();
            iThread.Schedule(() =>
            {
                ProxyTime time = task.Result;

                iTime = time;
                iTime.Duration.AddWatcher(this);
                iTime.Seconds.AddWatcher(this);

                iHasTime.Update(true);
            });
        }

        private void Unsubscribe()
        {
            iTime.Duration.RemoveWatcher(this);
            iTime.Seconds.RemoveWatcher(this);

            iTime.Dispose();
            iTime = null;

            iHasTime.Update(false);
        }

        public void ItemOpen(string aId, uint aValue)
        {
            if (aId == string.Format("Duration(ServiceTime({0}))", iTime.Device.Udn))
            {
                iDuration.Update(aValue);
            }
            if (aId == string.Format("Seconds(ServiceTime({0}))", iTime.Device.Udn))
            {
                iSeconds.Update(aValue);
            }
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            if (aId == string.Format("Duration(ServiceTime({0}))", iTime.Device.Udn))
            {
                iDuration.Update(aValue);
            }
            if (aId == string.Format("Seconds(ServiceTime({0}))", iTime.Device.Udn))
            {
                iSeconds.Update(aValue);
            }
        }

        public void ItemClose(string aId, uint aValue)
        {
        }

        private IWatchableThread iThread;
        private IStandardRoom iRoom;

        private object iLock;
        private bool iIsActive;
        private Watchable<bool> iActive;

        private ProxyTime iTime;
        private Watchable<bool> iHasTime;
        private Watchable<uint> iDuration;
        private Watchable<uint> iSeconds;
    }
}
