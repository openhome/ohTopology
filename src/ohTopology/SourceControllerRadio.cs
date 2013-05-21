using System;
using System.Threading.Tasks;

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

            Task<ProxyRadio> task = aSource.Device.Create<ProxyRadio>();
            aThread.Schedule(() =>
            {
                ProxyRadio radio = task.Result;

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

        public void Previous()
        {
            throw new NotImplementedException();
        }

        public void Next()
        {
            throw new NotImplementedException();
        }

        public void Seek(uint aSeconds)
        {
            iRadio.SeekSecondAbsolute(aSeconds);
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

        public void SetRepeat(bool aValue)
        {
            throw new NotImplementedException();
        }

        public void SetShuffle(bool aValue)
        {
            throw new NotImplementedException();
        }

        private object iLock;
        private bool iDisposed;

        private ITopology4Source iSource;
        private ProxyRadio iRadio;

        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<string> iTransportState;
    }
}
