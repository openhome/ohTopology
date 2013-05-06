using System;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class SourceControllerPlaylist : IWatcher<string>, ISourceController
    {
        public SourceControllerPlaylist(IWatchableThread aThread, ITopology4Source aSource, Watchable<bool> aHasInfoNext, Watchable<IInfoNext> aInfoNext, Watchable<string> aTransportState, Watchable<bool> aCanPause, Watchable<bool> aCanSkip, Watchable<bool> aCanSeek)
        {
            iLock = new object();
            iDisposed = false;

            iSource = aSource;

            iInfoNext = aInfoNext;
            iCanPause = aCanPause;
            iCanSeek = aCanSeek;
            iTransportState = aTransportState;

            aSource.Device.Create<ServicePlaylist>((IWatchableDevice device, ServicePlaylist playlist) =>
            {
                lock (iLock)
                {
                    if (!iDisposed)
                    {
                        iPlaylist = playlist;

                        aHasInfoNext.Update(true);
                        aCanSkip.Update(true);
                        iPlaylist.TransportState.AddWatcher(this);
                    }
                    else
                    {
                        playlist.Dispose();
                    }
                }
            });
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("SourceControllerPlaylist.Dispose");
                }

                if (iPlaylist != null)
                {
                    iPlaylist.TransportState.RemoveWatcher(this);

                    iPlaylist.Dispose();
                    iPlaylist = null;
                }

                iInfoNext = null;
                iCanPause = null;
                iCanSeek = null;

                iDisposed = true;
            }
        }

        public string Name
        {
            get
            {
                return iSource.Name;
            }
        }

        public bool HasInfoNext
        {
            get
            {
                return true;
            }
        }

        public IWatchable<IInfoNext> InfoNext
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
            iPlaylist.Play();
        }

        public void Pause()
        {
            iPlaylist.Pause();
        }

        public void Stop()
        {
            iPlaylist.Stop();
        }

        public bool CanSkip
        {
            get
            {
                return true;
            }
        }

        public void Previous()
        {
            iPlaylist.Previous();
        }

        public void Next()
        {
            iPlaylist.Next();
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
            iPlaylist.SeekSecondsAbsolute(aSeconds);
        }

        public void ItemOpen(string aId, string aValue)
        {
            iTransportState.Update(aValue);
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iTransportState.Update(aValue);
        }

        public void ItemClose(string aId, string aValue)
        {
        }

        private object iLock;
        private bool iDisposed;

        private ITopology4Source iSource;
        private ServicePlaylist iPlaylist;

        private Watchable<IInfoNext> iInfoNext;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<string> iTransportState;
    }
}
