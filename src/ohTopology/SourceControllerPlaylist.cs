using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class SourceControllerPlaylist : IWatcher<string>, IWatcher<bool>, IWatcher<IInfoDetails>, IWatcher<IInfoMetadata>, ISourceController
    {
        public SourceControllerPlaylist(IWatchableThread aThread, ITopology4Source aSource, Watchable<bool> aHasSourceControl,
            Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<bool> aHasContainer, Watchable<string> aTransportState, Watchable<bool> aCanPause,
            Watchable<bool> aCanSkip, Watchable<bool> aCanSeek, Watchable<bool> aHasPlayMode, Watchable<bool> aShuffle, Watchable<bool> aRepeat)
        {
            iDisposed = false;

            iHasSourceControl = aHasSourceControl;
            iHasInfoNext = aHasInfoNext;
            iHasContainer = aHasContainer;
            iInfoNext = aInfoNext;
            iCanPause = aCanPause;
            iCanSeek = aCanSeek;
            iCanSkip = aCanSkip;
            iTransportState = aTransportState;
            iHasPlayMode = aHasPlayMode;
            iShuffle = aShuffle;
            iRepeat = aRepeat;

            aSource.Device.Create<IProxyPlaylist>((playlist) =>
            {
                if (!iDisposed)
                {
                    iPlaylist = playlist;

                    iWatchableSnapshot = new WatchableSourceSelectorWatchableSnapshot(aThread, aSource, playlist.Snapshot);

                    iHasContainer.Update(true);
                    iCanSkip.Update(true);
                    iCanSeek.Update(false);
                    iHasPlayMode.Update(true);

                    iPlaylist.TransportState.AddWatcher(this);
                    iPlaylist.Shuffle.AddWatcher(this);
                    iPlaylist.Repeat.AddWatcher(this);
                    iPlaylist.InfoNext.AddWatcher(this);

                    iHasSourceControl.Update(true);
                }
                else
                {
                    playlist.Dispose();
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
                throw new ObjectDisposedException("SourceControllerPlaylist.Dispose");
            }

            if (iPlaylist != null)
            {
                iPlaylist.InfoNext.RemoveWatcher(this);
                iPlaylist.Shuffle.RemoveWatcher(this);
                iPlaylist.Repeat.RemoveWatcher(this);
                iPlaylist.TransportState.RemoveWatcher(this);

                iWatchableSnapshot.Dispose();

                iPlaylist.Dispose();
                iPlaylist = null;
            }

            if (iInfo != null)
            {
                iInfo.Details.RemoveWatcher(this);

                iInfo.Dispose();
                iInfo = null;
            }

            iHasSourceControl.Update(false);
            iHasContainer.Update(false);
            iHasInfoNext.Update(false);
            iCanPause.Update(false);
            iCanSkip.Update(false);
            iCanSeek.Update(false);
            iHasPlayMode.Update(false);

            iHasSourceControl = null;
            iHasContainer = null;
            iInfoNext = null;
            iCanPause = null;
            iCanSeek = null;
            iCanSkip = null;
            iHasPlayMode = null;
            iTransportState = null;

            iDisposed = true;
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iWatchableSnapshot.Snapshot;
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

        public void Previous()
        {
            iPlaylist.Previous();
        }

        public void Next()
        {
            iPlaylist.Next();
        }

        public void Seek(uint aSeconds)
        {
            iPlaylist.SeekSecondAbsolute(aSeconds);
        }

        public void SetRepeat(bool aValue)
        {
            iPlaylist.SetRepeat(aValue);
        }

        public void SetShuffle(bool aValue)
        {
            iPlaylist.SetShuffle(aValue);
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

        public void ItemOpen(string aId, bool aValue)
        {
            if (aId == "Shuffle")
            {
                iShuffle.Update(aValue);
                iHasInfoNext.Update(HasInfoNext());
            }
            if (aId == "Repeat")
            {
                iRepeat.Update(aValue);
            }
        }

        public void ItemUpdate(string aId, bool aValue, bool aPrevious)
        {
            if (aId == "Shuffle")
            {
                iShuffle.Update(aValue);
                iHasInfoNext.Update(HasInfoNext());
            }
            if (aId == "Repeat")
            {
                iRepeat.Update(aValue);
            }
        }

        public void ItemClose(string aId, bool aValue)
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

        public void ItemOpen(string aId, IInfoMetadata aValue)
        {
            iHasInfoNext.Update(HasInfoNext());
            iInfoNext.Update(aValue);
        }

        public void ItemUpdate(string aId, IInfoMetadata aValue, IInfoMetadata aPrevious)
        {
            iHasInfoNext.Update(HasInfoNext());
            iInfoNext.Update(aValue);
        }

        public void ItemClose(string aId, IInfoMetadata aValue)
        {
            iHasInfoNext.Update(false);
            iInfoNext.Update(InfoMetadata.Empty);
        }

        private bool HasInfoNext()
        {
            return !(iPlaylist.Shuffle.Value || iPlaylist.InfoNext.Value == InfoMetadata.Empty);
        }

        private bool iDisposed;

        private IProxyPlaylist iPlaylist;
        private IProxyInfo iInfo;

        private WatchableSourceSelectorWatchableSnapshot iWatchableSnapshot;

        private Watchable<bool> iHasSourceControl;
        private Watchable<IInfoMetadata> iInfoNext;
        private Watchable<bool> iHasInfoNext;
        private Watchable<bool> iHasContainer;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<bool> iCanSkip;
        private Watchable<string> iTransportState;
        private Watchable<bool> iHasPlayMode;
        private Watchable<bool> iShuffle;
        private Watchable<bool> iRepeat;
    }
}
