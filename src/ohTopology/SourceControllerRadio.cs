using System;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class SourceControllerRadio : IWatcher<string>, ISourceController
    {
        public SourceControllerRadio(IWatchableThread aThread, ITopology4Source aSource, Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<string> aTransportState, Watchable<bool> aCanPause, Watchable<bool> aCanSkip, Watchable<bool> aCanSeek)
        {
            iLock = new object();
            iDisposed = false;

            iSource = aSource;

            iCanPause = aCanPause;
            iCanSeek = aCanSeek;
            iTransportState = aTransportState;

            aSource.Device.Create<ServiceRadio>((IWatchableDevice device, ServiceRadio radio) =>
            {
                lock (iLock)
                {
                    if (!iDisposed)
                    {
                        iRadio = radio;

                        aHasInfoNext.Update(false);
                        aCanSkip.Update(false);
                        iCanPause.Update(false);
                        iCanSeek.Update(false);
                        iRadio.TransportState.AddWatcher(this);
                    }
                    else
                    {
                        radio.Dispose();
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
                    throw new ObjectDisposedException("SourceControllerRadio.Dispose");
                }

                if (iRadio != null)
                {
                    iRadio.TransportState.RemoveWatcher(this);

                    iRadio.Dispose();
                    iRadio = null;
                }

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
                return false;
            }
        }

        public IWatchable<IInfoMetadata> InfoNext
        {
            get { throw new NotSupportedException(); }
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
            iRadio.Play();
        }

        public void Pause()
        {
            iRadio.Pause();
        }

        public void Stop()
        {
            iRadio.Stop();
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
            throw new NotImplementedException();
        }

        public void Next()
        {
            throw new NotImplementedException();
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
            iRadio.SeekSecondsAbsolute(aSeconds);
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
        private ServiceRadio iRadio;

        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<string> iTransportState;
    }
}
