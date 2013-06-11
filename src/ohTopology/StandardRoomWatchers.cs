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
                }
            }

            iEnabled.Dispose();
            iEnabled = null;

            iRoom = null;
        }

        public IStandardRoom Room
        {
            get
            {
                return iRoom;
            }
        }

        public IWatchable<bool> Active
        {
            get
            {
                return iActive;
            }
        }

        public IWatchable<bool> Enabled
        {
            get
            {
                return iEnabled;
            }
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
        {
            EvaluateEnabledOpen(aValue);
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            EvaluateEnabledUpdate(aValue, aPrevious);
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
            EvaluateEnabledClose(aValue);
            iEnabled.Update(false);
        }

        protected abstract void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue);
        protected abstract void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious);
        protected virtual void EvaluateEnabledClose(IEnumerable<ITopology4Source> aValue) { }
        
        protected void SetEnabled(bool aValue)
        {
            iEnabled.Update(aValue);
        }

        private void SetInactive()
        {
            lock (iActive)
            {
                iIsActive = false;

                iActive.Update(false);

                iRoom.Sources.RemoveWatcher(this);
                iRoom.UnJoin(SetInactive);
            }
        }

        protected INetwork iNetwork;

        private IStandardRoom iRoom;
        private bool iIsActive;
        private Watchable<bool> iActive;
        private Watchable<bool> iEnabled;
    }

    public class StandardRoomWatcherMusic : StandardRoomWatcher, IOrderedWatcher<IProxyMediaServer>
    {
        private IStandardHouse iHouse;
        private uint iServerCount;
        private bool iHasCompatibleSource;
        private IProxyPlaylist iPlaylist;

        public StandardRoomWatcherMusic(IStandardHouse aHouse, IStandardRoom aRoom)
            : base(aRoom)
        {
            iHouse = aHouse;
            iServerCount = 0;

            iHouse.Servers.AddWatcher(this);
        }

        public override void Dispose()
        {
            base.Dispose();

            iHouse.Servers.RemoveWatcher(this);

            if (iPlaylist != null)
            {
                iPlaylist.Dispose();
                iPlaylist = null;
            }
        }

        public IWatchableOrdered<IProxyMediaServer> Servers
        {
            get
            {
                return iHouse.Servers;
            }
        }

        public Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                return iPlaylist.Container;
            }
        }

        public IProxyPlaylist Playlist
        {
            get
            {
                return iPlaylist;
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
            }

            EvaluateEnabled(aValue);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            SetEnabled(false);

            iHasCompatibleSource = false;
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Playlist")
                {
                    s.Device.Create<IProxyPlaylist>((playlist) =>
                    {
                        iPlaylist = playlist;
                        iHasCompatibleSource = true;
                        SetEnabled(iServerCount > 0);
                    });
                    return;
                }
            }
        }

        public void OrderedOpen()
        {
        }

        public void OrderedInitialised()
        {
        }

        public void OrderedClose()
        {
        }

        public void OrderedAdd(IProxyMediaServer aItem, uint aIndex)
        {
            ++iServerCount;
            SetEnabled(iHasCompatibleSource && (iServerCount > 0));
        }

        public void OrderedMove(IProxyMediaServer aItem, uint aFrom, uint aTo)
        {
        }

        public void OrderedRemove(IProxyMediaServer aItem, uint aIndex)
        {
            --iServerCount;
            SetEnabled(iHasCompatibleSource && (iServerCount > 0));
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

        public Task<IWatchableContainer<IMediaPreset>> Container
        {
            get
            {
                return iRadio.Container;
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

        public IWatchableContainer<IMediaPreset> Configured
        {
            get
            {
                return iConfigured;
            }
        }

        public IWatchableContainer<IMediaPreset> Unconfigured
        {
            get
            {
                return iUnconfigured;
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

        protected override void EvaluateEnabledClose(IEnumerable<ITopology4Source> aValue)
        {
            base.EvaluateEnabledClose(aValue);

            iConfigured.Dispose();
            iConfigured = null;

            iUnconfigured.Dispose();
            iUnconfigured = null;
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
        private readonly IList<IMediaPreset> iSources;

        public ExternalSnapshot(IList<ITopology4Source> aSources)
        {
            iSources = new List<IMediaPreset>();

            foreach (ITopology4Source s in aSources)
            {
                iSources.Add(s.Preset);
            }
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
                return new WatchableFragment<IMediaPreset>(aIndex, iSources.Skip((int)aIndex).Take((int)aCount));
            });
            return task;
        }
    }
}
