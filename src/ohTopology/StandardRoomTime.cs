using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomTime : IDisposable
    {
        string Name { get; }
        IWatchable<bool> Active { get; }
        IWatchable<bool> HasTime { get; }
        IWatchable<uint> Duration { get; }
        IWatchable<uint> Seconds { get; }
    }

    internal class TimeWatcher : IWatcher<uint>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly IDevice iDevice;
        private readonly Watchable<bool> iHasTime;
        private readonly Watchable<uint> iSeconds;
        private readonly Watchable<uint> iDuration;
        private bool iDisposed;
        private IProxyTime iTime;

        public TimeWatcher(INetwork aNetwork, IDevice aDevice, Watchable<bool> aHasTime, Watchable<uint> aSeconds, Watchable<uint> aDuration)
        {
            iDisposeHandler = new DisposeHandler();
            iDevice = aDevice;
            iHasTime = aHasTime;
            iSeconds = aSeconds;
            iDuration = aDuration;
            iDisposed = false;

            iDevice.Create<IProxyTime>((time) =>
            {
                if (!iDisposed)
                {
                    iTime = time;

                    iTime.Seconds.AddWatcher(this);
                    iTime.Duration.AddWatcher(this);

                    iHasTime.Update(true);
                }
                else
                {
                    time.Dispose();
                }
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            if (iTime != null)
            {
                iTime.Seconds.RemoveWatcher(this);
                iTime.Duration.RemoveWatcher(this);

                iTime.Dispose();
                iTime = null;
            }

            iHasTime.Update(false);

            iDisposed = true;
        }

        public void ItemOpen(string aId, uint aValue)
        {
            if (aId == "Seconds")
            {
                iSeconds.Update(aValue);
            }
            if (aId == "Duration")
            {
                iDuration.Update(aValue);
            }
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            if (aId == "Seconds")
            {
                iSeconds.Update(aValue);
            }
            if (aId == "Duration")
            {
                iDuration.Update(aValue);
            }
        }

        public void ItemClose(string aId, uint aValue)
        {
            if (aId == "Seconds")
            {
                iSeconds.Update(0);
            }
            if (aId == "Duration")
            {
                iDuration.Update(0);
            }
        }
    }

    internal class StandardRoomTime : IWatcher<ITopology4Source>, IStandardRoomTime
    {
        public StandardRoomTime(IStandardRoom aRoom)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aRoom.Network;
            iRoom = aRoom;
            iSource = aRoom.Source;

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(iNetwork, "Active", true);

            iHasTime = new Watchable<bool>(iNetwork, "HasTime", false);
            iDuration = new Watchable<uint>(iNetwork, "Duration", 0);
            iSeconds = new Watchable<uint>(iNetwork, "Seconds", 0);

            iSource.AddWatcher(this);
            iRoom.Join(SetInactive);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

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

            Do.Assert(iTimeWatcher == null);

            iHasTime.Dispose();
            iDuration.Dispose();
            iSeconds.Dispose();
            iActive.Dispose();
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
                }
            }
        }

        public string Name
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iRoom.Name;
                }
            }
        }

        public IWatchable<bool> Active
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iActive;
                }
            }
        }

        public IWatchable<bool> HasTime
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iHasTime;
                }
            }
        }

        public IWatchable<uint> Duration
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iDuration;
                }
            }
        }

        public IWatchable<uint> Seconds
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iSeconds;
                }
            }
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            if (aValue.HasTime)
            {
                iTimeWatcher = new TimeWatcher(iNetwork, aValue.Device, iHasTime, iSeconds, iDuration);
            }
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if ((aPrevious.HasTime && !aValue.HasTime) || (aPrevious.HasTime && aValue.HasTime && aPrevious.Device != aValue.Device))
            {
                iTimeWatcher.Dispose();
                iTimeWatcher = null;
            }

            if ((!aPrevious.HasTime && aValue.HasTime) || (aPrevious.HasTime && aValue.HasTime && aPrevious.Device != aValue.Device))
            {
                Do.Assert(iTimeWatcher == null);
                iTimeWatcher = new TimeWatcher(iNetwork, aValue.Device, iHasTime, iSeconds, iDuration);
            }
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            if (aValue.HasTime)
            {
                iTimeWatcher.Dispose();
                iTimeWatcher = null;
            }

            Do.Assert(iTimeWatcher == null);
        }

        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly IStandardRoom iRoom;
        private readonly IWatchable<ITopology4Source> iSource;

        private readonly object iLock;
        private bool iIsActive;
        private readonly Watchable<bool> iActive;

        private TimeWatcher iTimeWatcher;
        private readonly Watchable<bool> iHasTime;
        private readonly Watchable<uint> iDuration;
        private readonly Watchable<uint> iSeconds;
    }
}
