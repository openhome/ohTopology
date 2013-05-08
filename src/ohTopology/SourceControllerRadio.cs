using System;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class SourceControllerRadio : IWatcher<string>, ISourceController
    {
        public SourceControllerRadio(IWatchableThread aThread, ITopology4Source aSource, Watchable<bool> aHasSourceControl, Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<string> aTransportState, Watchable<bool> aCanPause, Watchable<bool> aCanSkip, Watchable<bool> aCanSeek)
        {
            iLock = new object();
            iDisposed = false;

            iSource = aSource;

            iHasSourceControl = aHasSourceControl;
            iCanPause = aCanPause;
            iCanSeek = aCanSeek;
            iTransportState = aTransportState;

            aSource.Device.Create<ServiceRadio>((IWatchableDevice device, ServiceRadio radio) =>
            {
                if (!iDisposed)
                {
                    iRadio = radio;

                    aHasInfoNext.Update(false);
                    aCanSkip.Update(false);
                    iCanPause.Update(false);
                    iCanSeek.Update(false);

                    iRadio.TransportState.AddWatcher(this);
                        
                    iHasSourceControl.Update(true);
                }
                else
                {
                    radio.Dispose();
                }
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("SourceControllerRadio.Dispose");
            }

            if (iRadio != null)
            {
                iHasSourceControl.Update(false);

                iRadio.TransportState.RemoveWatcher(this);

                iRadio.Dispose();
                iRadio = null;
            }

            iHasSourceControl = null;
            iCanPause = null;
            iCanSeek = null;
            iTransportState = null;

            iDisposed = true;
        }

        public string Name
        {
            get
            {
                return iSource.Name;
            }
        }

        public IWatchable<bool> HasSourceControl
        {
            get
            {
                return iHasSourceControl;
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
            iRadio.Play(null);
        }

        public void Pause()
        {
            iRadio.Pause(null);
        }

        public void Stop()
        {
            iRadio.Stop(null);
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
            iRadio.SeekSecondsAbsolute(aSeconds, null);
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

        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<string> iTransportState;
    }
}
