using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class SourceControllerReceiver : IWatcher<string>, ISourceController
    {
        public SourceControllerReceiver(ITopology4Source aSource, Watchable<bool> aHasSourceControl,
            Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<bool> aHasContainer, Watchable<string> aTransportState, Watchable<bool> aCanPause,
            Watchable<bool> aCanSkip, Watchable<bool> aCanSeek, Watchable<bool> aHasPlayMode, Watchable<bool> aShuffle, Watchable<bool> aRepeat)
        {
            iDisposed = false;

            iHasSourceControl = aHasSourceControl;
            iCanPause = aCanPause;
            iTransportState = aTransportState;

            aSource.Device.Create<IProxyReceiver>((receiver) =>
            {
                if (!iDisposed)
                {
                    iReceiver = receiver;

                    aHasInfoNext.Update(false);
                    aHasContainer.Update(false);
                    iCanPause.Update(true);
                    aCanSkip.Update(false);
                    aCanSeek.Update(false);
                    aHasPlayMode.Update(false);

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
                iReceiver.TransportState.RemoveWatcher(this);

                iReceiver.Dispose();
                iReceiver = null;
            }

            iCanPause.Update(false);
            iHasSourceControl.Update(false);

            iHasSourceControl = null;
            iTransportState = null;

            iDisposed = true;
        }

        public Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public void Play()
        {
            iReceiver.Play();
        }

        public void Pause()
        {
            throw new NotSupportedException();
        }

        public void Stop()
        {
            iReceiver.Stop();
        }

        public void Previous()
        {
            throw new NotSupportedException();
        }

        public void Next()
        {
            throw new NotSupportedException();
        }

        public void Seek(uint aSeconds)
        {
            throw new NotSupportedException();
        }

        public void SetShuffle(bool aValue)
        {
            throw new NotSupportedException();
        }

        public void SetRepeat(bool aValue)
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

        private IProxyReceiver iReceiver;

        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iCanPause;
        private Watchable<string> iTransportState;
    }
}
