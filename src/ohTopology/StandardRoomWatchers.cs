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

        public void Dispose()
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

    public class StandardRoomWatcherPlaylist : StandardRoomWatcher
    {
        public StandardRoomWatcherPlaylist(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        protected override bool EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Playlist")
                {
                    return true;
                }
            }

            return false;
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
