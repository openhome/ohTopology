using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomWatcher : IDisposable
    {
        IWatchable<bool> Enabled { get; }
    }

    public abstract class StandardRoomWatcher : IStandardRoomWatcher, IWatcher<IEnumerable<ITopology4Source>>
    {
        protected StandardRoomWatcher(IStandardRoom aRoom)
        {
            iRoom = aRoom;
            iActive = true;

            iRoom.Sources.AddWatcher(this);

            iRoom.Join(SetInactive);
        }

        public virtual void Dispose()
        {
            if (iActive)
            {
                iRoom.UnJoin(SetInactive);
            }

            iEnabled.Dispose();
            iEnabled = null;

            iRoom = null;
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
            iEnabled.Update(EvaluateEnabled(aValue));
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            iEnabled.Update(EvaluateEnabled(aValue));
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
            iEnabled.Update(false);
        }

        protected abstract bool EvaluateEnabled(IEnumerable<ITopology4Source> aValue);
        
        protected void SetEnabled(bool aValue)
        {
            iEnabled.Update(aValue);
        }

        private void SetInactive()
        {
            iRoom.Sources.RemoveWatcher(this);
            iRoom.UnJoin(SetInactive);
            iActive = false;
        }

        private IStandardRoom iRoom;
        private bool iActive;
        private Watchable<bool> iEnabled;
    }

    public class StandardRoomWatcherPlaylist : StandardRoomWatcher, IOrderedWatcher<IProxyMediaServer>
    {
        private IStandardHouse iHouse;
        private uint iServerCount;
        private bool iHasCompatibleSource;

        public StandardRoomWatcherPlaylist(IStandardHouse aHouse, IStandardRoom aRoom)
            : base(aRoom)
        {
            iHouse = aHouse;
            iServerCount = 0;
            iHasCompatibleSource = false;

            iHouse.Servers.AddWatcher(this);
        }

        public override void Dispose()
        {
            base.Dispose();

            iHouse.Servers.RemoveWatcher(this);
        }

        protected override bool EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            iHasCompatibleSource = false;
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Playlist" || s.Type == "Radio")
                {
                    iHasCompatibleSource = true;
                    return (iServerCount > 0);
                }
            }

            return false;
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
        public StandardRoomWatcherRadio(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        protected override bool EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Radio")
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class StandardRoomWatcherReceiver : StandardRoomWatcher
    {
        public StandardRoomWatcherReceiver(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        protected override bool EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Receiver")
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class StandardRoomWatcherExternal : StandardRoomWatcher
    {
        public StandardRoomWatcherExternal(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        protected override bool EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Digital" || s.Type == "Analog" || s.Type == "Hdmi")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
