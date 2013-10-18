using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class SourceSelectorMediaPreset : IMediaPreset
    {
        class MediaPresetPlaying : IWatcher<bool>, IDisposable
        {
            private readonly DisposeHandler iDisposeHandler;
            private readonly IMediaPreset iPreset;
            private readonly Action iAction;
            private bool iPlaying;

            public MediaPresetPlaying(IWatchableThread aThread, IMediaPreset aPreset, Action aAction)
            {
                iDisposeHandler = new DisposeHandler();
                iPreset = aPreset;
                iAction = aAction;

                aThread.Schedule(() =>
                {
                    iDisposeHandler.WhenNotDisposed(() =>
                    {
                        iPreset.Playing.AddWatcher(this);
                    });
                });
            }

            public void Dispose()
            {
                iDisposeHandler.Dispose();

                iPreset.Playing.RemoveWatcher(this);
            }

            public bool Playing
            {
                get
                {
                    return iPlaying;
                }
            }

            public void ItemOpen(string aId, bool aValue)
            {
                iPlaying = aValue;
                iAction();
            }

            public void ItemUpdate(string aId, bool aValue, bool aPrevious)
            {
                iPlaying = aValue;
                iAction();
            }

            public void ItemClose(string aId, bool aValue)
            {
            }
        }

        private readonly DisposeHandler iDisposeHandler;
        private readonly IWatchableThread iThread;
        private readonly IMediaPreset iSource;
        private readonly MediaPresetPlaying iSourcePlaying;
        private readonly IMediaPreset iPreset;
        private readonly MediaPresetPlaying iPresetPlaying;
        private readonly Watchable<bool> iPlaying;

        public SourceSelectorMediaPreset(IWatchableThread aThread, ITopology4Source aSource, IMediaPreset aPreset)
        {
            iDisposeHandler = new DisposeHandler();
            iThread = aThread;
            iSource = aSource.CreatePreset();
            iPreset = aPreset;

            iPlaying = new Watchable<bool>(iThread, "Playing", false);

            iSourcePlaying = new MediaPresetPlaying(iThread, iSource, UpdatePlaying);
            iPresetPlaying = new MediaPresetPlaying(iThread, iPreset, UpdatePlaying);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iThread.Execute(() =>
            {
                iSourcePlaying.Dispose();
                iPresetPlaying.Dispose();
            });

            iPlaying.Dispose();

            iSource.Dispose();
            iPreset.Dispose();
        }

        public uint Index
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iPreset.Index;
                }
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iPreset.Metadata;
                }
            }
        }

        public IWatchable<bool> Buffering
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iPreset.Buffering;
                }
            }
        }

        public IWatchable<bool> Playing
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iPlaying;
                }
            }
        }

        public void Play()
        {
            using (iDisposeHandler.Lock())
            {
                iSource.Play();
                iPreset.Play();
            }
        }

        private void UpdatePlaying()
        {
            bool playing = false;
            if (iSourcePlaying != null)
            {
                playing = iSourcePlaying.Playing;
            }
            if (iPresetPlaying != null)
            {
                playing &= iPresetPlaying.Playing;
            }
            iPlaying.Update(playing);
        }
    }

    class SourceSelectorWatchableSnapshot : IWatchableSnapshot<IMediaPreset>
    {
        private readonly IWatchableThread iThread;
        private readonly ITopology4Source iSource;
        private readonly IWatchableSnapshot<IMediaPreset> iSnapshot;

        public SourceSelectorWatchableSnapshot()
        {
        }

        public SourceSelectorWatchableSnapshot(IWatchableThread aThread, ITopology4Source aSource, IWatchableSnapshot<IMediaPreset> aSnapshot)
        {
            iThread = aThread;
            iSource = aSource;
            iSnapshot = aSnapshot;
        }

        public uint Total
        {
            get
            {
                if (iSnapshot == null)
                {
                    return 0;
                }

                return iSnapshot.Total;
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                if (iSnapshot == null)
                {
                    return null;
                }

                return iSnapshot.Alpha;
            }
        }

        public Task<IWatchableFragment<IMediaPreset>> Read(uint aIndex, uint aCount, CancellationToken aCancellationToken)
        {
            Do.Assert(iSnapshot != null);

            return iSnapshot.Read(aIndex, aCount, aCancellationToken).ContinueWith((t) =>
            {
                List<IMediaPreset> presets = new List<IMediaPreset>();

                foreach (IMediaPreset p in t.Result.Data)
                {
                    presets.Add(new SourceSelectorMediaPreset(iThread, iSource, p));
                }

                IWatchableFragment<IMediaPreset> fragment = new WatchableFragment<IMediaPreset>(aIndex, presets);

                return fragment;
            });
        }
    }

    class WatchableSourceSelectorWatchableSnapshot : IWatcher<IWatchableSnapshot<IMediaPreset>>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly IWatchableThread iThread;
        private readonly Watchable<IWatchableSnapshot<IMediaPreset>> iWatchableSourceSelectorSnapshot;
        private ITopology4Source iSource;
        private IWatchable<IWatchableSnapshot<IMediaPreset>> iWatchableSnapshot;

        public WatchableSourceSelectorWatchableSnapshot(IWatchableThread aThread, ITopology4Source aSource, IWatchable<IWatchableSnapshot<IMediaPreset>> aWatchableSnapshot)
        {
            iDisposeHandler = new DisposeHandler();
            iThread = aThread;
            iSource = aSource;
            iWatchableSnapshot = aWatchableSnapshot;

            iWatchableSourceSelectorSnapshot = new Watchable<IWatchableSnapshot<IMediaPreset>>(iThread, "Snapshot", new SourceSelectorWatchableSnapshot());

            iWatchableSnapshot.AddWatcher(this);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iThread.Execute(() =>
            {
                iWatchableSnapshot.RemoveWatcher(this);
            });

            iWatchableSourceSelectorSnapshot.Dispose();
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iWatchableSourceSelectorSnapshot;
                }
            }
        }

        public void ItemOpen(string aId, IWatchableSnapshot<IMediaPreset> aValue)
        {
            iWatchableSourceSelectorSnapshot.Update(new SourceSelectorWatchableSnapshot(iThread, iSource, aValue));
        }

        public void ItemUpdate(string aId, IWatchableSnapshot<IMediaPreset> aValue, IWatchableSnapshot<IMediaPreset> aPrevious)
        {
            iWatchableSourceSelectorSnapshot.Update(new SourceSelectorWatchableSnapshot(iThread, iSource, aValue));
        }

        public void ItemClose(string aId, IWatchableSnapshot<IMediaPreset> aValue)
        {
        }
    }
}
