using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class StandardRoomTime : IDisposable
    {
        public StandardRoomTime(IStandardRoom aRoom)
        {
            iLock = new object();
            iDisposed = false;
            iThread = aRoom.WatchableThread;
            iRoom = aRoom;

            iRoom.Join(SetInactive);

            Task<ProxyTime> task = null;// iRoom.Time.Create<ProxyTime>();
            iThread.Schedule(() =>
            {
                ProxyTime time = task.Result;

                lock(iLock)
                {
                    if (!iDisposed)
                    {
                        //iTime = time;
                    }
                    else
                    {
                        time.Dispose();
                    }
                }
            });
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iIsActive)
                {
                    iDuration.Detach();
                    iSeconds.Detach();

                    iRoom.UnJoin(SetInactive);
                }

                iRoom = null;

                iActive.Dispose();
                iActive = null;

                iDisposed = true;
            }
        }

        private void SetInactive()
        {
            lock (iLock)
            {
                iIsActive = false;

                iActive.Update(false);

                iDuration.Detach();
                iSeconds.Detach();

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

        private bool iDisposed;

        private IWatchableThread iThread;
        private IStandardRoom iRoom;

        private object iLock;
        private bool iIsActive;
        private Watchable<bool> iActive;

        private WatchableProxy<uint> iDuration;
        private WatchableProxy<uint> iSeconds;
    }
}
