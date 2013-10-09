using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class SourceControllerDisc : IWatcher<string>, IWatcher<IInfoDetails>, ISourceController
    {
        public SourceControllerDisc(ITopology4Source aSource, Watchable<bool> aHasSourceControl,
            Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<bool> aHasContainer, Watchable<string> aTransportState, Watchable<bool> aCanPause,
            Watchable<bool> aCanSkip, Watchable<bool> aCanSeek, Watchable<bool> aHasPlayMode, Watchable<bool> aShuffle, Watchable<bool> aRepeat)
        {
            iDisposed = false;

            iHasSourceControl = aHasSourceControl;
            iHasContainer = aHasContainer;
            iCanPause = aCanPause;
            iCanSeek = aCanSeek;
            iTransportState = aTransportState;

            aSource.Device.Create<IProxySdp>((sdp) =>
            {
                if (!iDisposed)
                {
                    iSdp = sdp;

                    aHasInfoNext.Update(false);
                    iHasContainer.Update(true);
                    iCanPause.Update(false);
                    iCanSeek.Update(true);
                    aCanSkip.Update(true);
                    aHasPlayMode.Update(true);

                    iSdp.TransportState.AddWatcher(this);

                    iHasSourceControl.Update(true);
                }
                else
                {
                    sdp.Dispose();
                }
            });
            aSource.Device.Create<IProxyInfo>((info) =>
            {
                if (!iDisposed)
                {
                    iInfo = info;

                    iInfo.Details.AddWatcher(this);
                }
                else
                {
                    info.Dispose();
                }
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("SourceControllerDisc.Dispose");
            }

            if (iSdp != null)
            {
                iSdp.TransportState.RemoveWatcher(this);

                iSdp.Dispose();
                iSdp = null;
            }

            if (iInfo != null)
            {
                iInfo.Details.RemoveWatcher(this);

                iInfo.Dispose();
                iInfo = null;
            }

            iHasSourceControl.Update(false);
            iHasContainer.Update(false);
            iCanPause.Update(false);
            iCanSeek.Update(false);

            iHasSourceControl = null;
            iHasContainer = null;
            iCanPause = null;
            iCanSeek = null;
            iTransportState = null;

            iDisposed = true;
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                throw new NotImplementedException();
                //return iRadio.Snapshot;
            }
        }

        public void Play()
        {
            iSdp.Play();
        }

        public void Pause()
        {
            iSdp.Pause();
        }

        public void Stop()
        {
            iSdp.Stop();
        }

        public void Previous()
        {
            iSdp.Previous();
        }

        public void Next()
        {
           iSdp.Next();
        }

        public void Seek(uint aSeconds)
        {
            iSdp.SeekSecondAbsolute(aSeconds);
        }

        public void SetRepeat(bool aValue)
        {
            iSdp.SetRepeat(aValue);
        }

        public void SetShuffle(bool aValue)
        {
            iSdp.SetShuffle(aValue);
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

        public void ItemOpen(string aId, IInfoDetails aValue)
        {
            iCanPause.Update(aValue.Duration > 0);
            iCanSeek.Update(aValue.Duration > 0);
        }

        public void ItemUpdate(string aId, IInfoDetails aValue, IInfoDetails aPrevious)
        {
            iCanPause.Update(aValue.Duration > 0);
            iCanSeek.Update(aValue.Duration > 0);
        }

        public void ItemClose(string aId, IInfoDetails aValue)
        {
        }

        private bool iDisposed;

        private IProxySdp iSdp;
        private IProxyInfo iInfo;

        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iHasContainer;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<string> iTransportState;
    }
}
