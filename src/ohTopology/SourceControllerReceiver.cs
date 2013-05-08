using System;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class SourceControllerReceiver : IWatcher<string>, ISourceController
    {
        public SourceControllerReceiver(IWatchableThread aThread, ITopology4Source aSource, Watchable<bool> aHasSourceControl, Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<string> aTransportState, Watchable<bool> aCanPause, Watchable<bool> aCanSkip, Watchable<bool> aCanSeek)
        {
            iDisposed = false;

            iSource = aSource;

            iHasSourceControl = aHasSourceControl;
            iCanPause = aCanPause;
            iCanSeek = aCanSeek;
            iTransportState = aTransportState;

            aSource.Device.Create<ServiceReceiver>((IWatchableDevice device, ServiceReceiver receiver) =>
            {
                if (!iDisposed)
                {
                    iReceiver = receiver;

                    aHasInfoNext.Update(false);
                    aCanSkip.Update(false);
                    iCanPause.Update(false);
                    iCanSeek.Update(false);

                    iReceiver.TransportState.AddWatcher(this);

                    iHasSourceControl.Update(true);
                }
                else
                {
                    receiver.Dispose();
                }
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("SourceControllerReceiver.Dispose");
            }

            if (iReceiver != null)
            {
                iHasSourceControl.Update(false);

                iReceiver.TransportState.RemoveWatcher(this);

                iReceiver.Dispose();
                iReceiver = null;
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
            iReceiver.Play(null);
        }

        public void Pause()
        {
            throw new NotSupportedException();
        }

        public void Stop()
        {
            iReceiver.Stop(null);
        }

        public bool CanSkip
        {
            get
            {
                return false;
            }
        }

        public void Previous()
        {
            throw new NotSupportedException();
        }

        public void Next()
        {
            throw new NotSupportedException();
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
            throw new NotSupportedException();
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

        private bool iDisposed;

        private ITopology4Source iSource;
        private ServiceReceiver iReceiver;

        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<string> iTransportState;
    }
}
