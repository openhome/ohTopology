using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomWatcher : IDisposable
    {
        IStandardRoom Room { get; }
        IWatchable<bool> Active { get; }
        IWatchable<bool> Enabled { get; }
    }

    public abstract class StandardRoomWatcher : IStandardRoomWatcher, IWatcher<IEnumerable<ITopology4Source>>
    {
        protected StandardRoomWatcher(IStandardRoom aRoom)
        {
            iDisposeHandler = new DisposeHandler();
            iRoom = aRoom;
            iNetwork = aRoom.Network;
            iIsActive = true;
            iActive = new Watchable<bool>(iNetwork, "Active", true);
            iEnabled = new Watchable<bool>(iNetwork, "Enabled", false);

            iRoom.Sources.AddWatcher(this);

            iRoom.Join(SetInactive);
        }

        public virtual void Dispose()
        {
            lock (iActive)
            {
                if (iIsActive)
                {
                    iNetwork.Execute(() =>
                    {
                        iRoom.Sources.RemoveWatcher(this);
                    });
                    iRoom.UnJoin(SetInactive);
                    iIsActive = false;
                }
            }

            iEnabled.Dispose();

            iDisposeHandler.Dispose();
        }

        public IStandardRoom Room
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iRoom;
                }
            }
        }

        public IWatchable<bool> Active
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iActive;
                }
            }
        }

        public IWatchable<bool> Enabled
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iEnabled;
                }
            }
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
        {
            using (iDisposeHandler.Lock)
            {
                EvaluateEnabledOpen(aValue);
            }
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            using (iDisposeHandler.Lock)
            {
                EvaluateEnabledUpdate(aValue, aPrevious);
            }
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
            using (iDisposeHandler.Lock)
            {
                EvaluateEnabledClose(aValue);
                iEnabled.Update(false);
            }
        }

        protected abstract void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue);
        protected abstract void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious);
        protected virtual void EvaluateEnabledClose(IEnumerable<ITopology4Source> aValue) { }
        
        protected void SetEnabled(bool aValue)
        {
            using (iDisposeHandler.Lock)
            {
                iEnabled.Update(aValue);
            }
        }

        protected virtual void OnSetInactive() { }

        private void SetInactive()
        {
            using (iDisposeHandler.Lock)
            {
                lock (iActive)
                {
                    if (iIsActive)
                    {
                        iIsActive = false;

                        iActive.Update(false);

                        iRoom.Sources.RemoveWatcher(this);

                        OnSetInactive();

                        iRoom.UnJoin(SetInactive);
                    }
                }
            }
        }

        protected DisposeHandler iDisposeHandler;
        protected readonly INetwork iNetwork;

        private readonly IStandardRoom iRoom;
        private bool iIsActive;
        private readonly Watchable<bool> iActive;
        private readonly Watchable<bool> iEnabled;
    }

    public class StandardRoomWatcherMusic : StandardRoomWatcher
    {
        private IProxyPlaylist iPlaylist;
        private IPlaylistWriter iPlaylistWriter;

        public StandardRoomWatcherMusic(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            if (iPlaylist != null)
            {
                iPlaylist.Dispose();
                iPlaylist = null;
                iPlaylistWriter = null;
            }
        }

        protected override void OnSetInactive()
        {
            using (iDisposeHandler.Lock)
            {
                if (iPlaylist != null)
                {
                    iPlaylist.Dispose();
                    iPlaylist = null;
                    iPlaylistWriter = null;
                }
            }
        }

        public Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPlaylist.Container;
                }
            }
        }

        public IPlaylistWriter PlaylistWriter
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPlaylistWriter;
                }
            }
        }

        protected override void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue)
        {
            EvaluateEnabled(aValue);
        }

        protected override void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            SetEnabled(false);

            if (iPlaylist != null)
            {
                iPlaylist.Dispose();
                iPlaylist = null;
                iPlaylistWriter = null;
            }

            EvaluateEnabled(aValue);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            SetEnabled(false);

            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Playlist")
                {
                    s.Device.Create<IProxyPlaylist>((playlist) =>
                    {
                        iPlaylist = playlist;
                        iPlaylistWriter = new PlaylistWriter(playlist);
                        SetEnabled(true);
                    });
                    return;
                }
            }
        }
    }

    public class StandardRoomWatcherRadio : StandardRoomWatcher
    {
        private IProxyRadio iRadio;

        public StandardRoomWatcherRadio(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            if (iRadio != null)
            {
                iRadio.Dispose();
                iRadio = null;
            }
        }

        protected override void OnSetInactive()
        {
            using (iDisposeHandler.Lock)
            {
                if (iRadio != null)
                {
                    iRadio.Dispose();
                    iRadio = null;
                }
            }
        }

        public Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iRadio.Container;
                }
            }
        }

        protected override void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue)
        {
            EvaluateEnabled(aValue);
        }

        protected override void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            SetEnabled(false);

            if (iRadio != null)
            {
                iRadio.Dispose();
                iRadio = null;
            }

            EvaluateEnabled(aValue);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Radio")
                {
                    s.Device.Create<IProxyRadio>((radio) =>
                    {
                        iRadio = radio;
                        SetEnabled(true);
                    });
                    return;
                }
            }
        }
    }

    public class StandardRoomWatcherReceiver : StandardRoomWatcher
    {
        public StandardRoomWatcherReceiver(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        protected override void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue)
        {
            EvaluateEnabled(aValue);
        }

        protected override void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            EvaluateEnabled(aValue);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Receiver")
                {
                    SetEnabled(true);
                    return;
                }
            }

            SetEnabled(false);
        }
    }

    public class StandardRoomWatcherExternal : StandardRoomWatcher
    {
        private ExternalContainer iConfigured;
        private ExternalContainer iUnconfigured;

        public StandardRoomWatcherExternal(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            iConfigured.Dispose();

            iUnconfigured.Dispose();
        }

        public IWatchableContainer<IMediaPreset> Configured
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iConfigured;
                }
            }
        }

        public IWatchableContainer<IMediaPreset> Unconfigured
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iUnconfigured;
                }
            }
        }

        protected override void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue)
        {
            iConfigured = new ExternalContainer(iNetwork);
            iUnconfigured = new ExternalContainer(iNetwork);
            EvaluateEnabled(aValue);
        }

        protected override void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            EvaluateEnabled(aValue);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            SetEnabled(BuildLists(aValue));
        }

        private bool BuildLists(IEnumerable<ITopology4Source> aValue)
        {
            uint cIndex = 0;
            uint uIndex = 0;
            bool hasExternal = false;
            List<ITopology4Source> configured = new List<ITopology4Source>();
            List<ITopology4Source> unconfigured = new List<ITopology4Source>();
            foreach (ITopology4Source s in aValue)
            {
                if (IsExternal(s))
                {
                    hasExternal = true;

                    if (IsConfigured(s))
                    {
                        configured.Add(s);
                        cIndex++;
                    }
                    else
                    {
                        unconfigured.Add(s);
                        uIndex++;
                    }
                }
            }

            iUnconfigured.UpdateSnapshot(unconfigured);
            iConfigured.UpdateSnapshot(configured);

            return hasExternal;
        }

        private bool IsExternal(ITopology4Source aSource)
        {
            return (aSource.Type == "Analog" || aSource.Type == "Digital" || aSource.Type == "Hdmi");
        }

        private bool IsConfigured(ITopology4Source aSource)
        {
            if (aSource.Name.StartsWith("Hdmi"))
            {
                try
                {
                    if (aSource.Name.Length == 4)
                    {
                        return false;
                    }
                    uint.Parse(aSource.Name.Substring(4));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name.StartsWith("Analog"))
            {
                try
                {
                    if (aSource.Name.Length == 6)
                    {
                        return false;
                    }
                    uint.Parse(aSource.Name.Substring(6));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name.StartsWith("SPDIF") || aSource.Name.StartsWith("Spdif"))
            {
                try
                {
                    if (aSource.Name.Length == 5)
                    {
                        return false;
                    }
                    uint.Parse(aSource.Name.Substring(5));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name.StartsWith("TOSLINK") || aSource.Name.StartsWith("Toslink"))
            {
                try
                {
                    if (aSource.Name.Length == 7)
                    {
                        return false;
                    }
                    uint.Parse(aSource.Name.Substring(7));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name == "Phono")
            {
                return false;
            }
            if (aSource.Name == "Front Aux")
            {
                return false;
            }

            return true;
        }
    }

    class ExternalContainer : IWatchableContainer<IMediaPreset>, IDisposable
    {
        private Watchable<IWatchableSnapshot<IMediaPreset>> iSnapshot;

        public ExternalContainer(INetwork aNetwork)
        {
            iSnapshot = new Watchable<IWatchableSnapshot<IMediaPreset>>(aNetwork, "Snapshot", new ExternalSnapshot(new List<ITopology4Source>()));
        }

        public void Dispose()
        {
            iSnapshot.Dispose();
            iSnapshot = null;
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iSnapshot;
            }
        }

        public void UpdateSnapshot(IList<ITopology4Source> aSources)
        {
            iSnapshot.Update(new ExternalSnapshot(aSources));
        }
    }

    class ExternalSnapshot : IWatchableSnapshot<IMediaPreset>
    {
        private readonly IList<ITopology4Source> iSources;

        public ExternalSnapshot(IList<ITopology4Source> aSources)
        {
            iSources = aSources;
        }

        public uint Total
        {
            get
            {
                return ((uint)iSources.Count);
            }
        }

        public IEnumerable<uint> AlphaMap
        {
            get
            {
                return null;
            }
        }

        public Task<IWatchableFragment<IMediaPreset>> Read(uint aIndex, uint aCount)
        {
            Task<IWatchableFragment<IMediaPreset>> task = Task<IWatchableFragment<IMediaPreset>>.Factory.StartNew(() =>
            {
                List<IMediaPreset> presets = new List<IMediaPreset>();
                iSources.Skip((int)aIndex).Take((int)aCount).ToList().ForEach(v => presets.Add(v.Preset));
                return new WatchableFragment<IMediaPreset>(aIndex, presets);
            });
            return task;
        }
    }
}
