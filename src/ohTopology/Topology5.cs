using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class WatchableTopology4SourceUnordered : WatchableUnordered<ITopology4Source>
    {
        public WatchableTopology4SourceUnordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<ITopology4Source>();
        }

        public new void Add(ITopology4Source aValue)
        {
            iList.Add(aValue);
            base.Add(aValue);
        }

        public new void Remove(ITopology4Source aValue)
        {
            iList.Remove(aValue);
            base.Remove(aValue);
        }

        private List<ITopology4Source> iList;
    }

    public interface ITopology5Room
    {
        string Name { get; }
        IWatchable<EStandby> Standby { get; }
        IWatchableUnordered<ITopology4Source> Sources { get; }

        void SetStandby(bool aValue);
    }

    public class Topology5Room : ITopology5Room, IWatcher<IEnumerable<ITopology4Source>>, IDisposable
    {
        public Topology5Room(IWatchableThread aThread, ITopology4Room aRoom)
        {
            iThread = aThread;
            iRoom = aRoom;

            iName = iRoom.Name;
            iStandby = iRoom.Standby;

            iSources = new WatchableTopology4SourceUnordered(iThread);

            iRoom.Sources.AddWatcher(this);
        }

        public void Dispose()
        {
            iSources.Dispose();
            iSources = null;

            iRoom.Sources.RemoveWatcher(this);
            iRoom = null;
        }

        public string Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchable<EStandby> Standby
        {
            get
            {
                return iRoom.Standby;
            }
        }

        public IWatchableUnordered<ITopology4Source> Sources
        {
            get
            {
                return iSources;
            }
        }

        public void SetStandby(bool aValue)
        {
            if (iRoom != null)
            {
                iRoom.SetStandby(aValue);
            }
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                iSources.Add(s);
            }
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            List<ITopology4Source> removed = new List<ITopology4Source>();
            foreach (ITopology4Source s1 in aPrevious)
            {
                bool found = false;
                foreach (ITopology4Source s2 in aValue)
                {
                    if (s1 == s2)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    removed.Add(s1);
                }
            }

            List<ITopology4Source> added = new List<ITopology4Source>();
            foreach (ITopology4Source s1 in aValue)
            {
                bool found = false;
                foreach (ITopology4Source s2 in aPrevious)
                {
                    if (s1 == s2)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    added.Add(s1);
                }
            }

            foreach(ITopology4Source s in removed)
            {
                iSources.Remove(s);
            }

            foreach (ITopology4Source s in added)
            {
                iSources.Add(s);
            }
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
        }

        private IWatchableThread iThread;
        private ITopology4Room iRoom;

        private string iName;
        private IWatchable<EStandby> iStandby;
        private WatchableTopology4SourceUnordered iSources;
    }

    public class WatchableTopology5RoomUnordered : WatchableUnordered<ITopology5Room>
    {
        public WatchableTopology5RoomUnordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<ITopology5Room>();
        }

        public new void Add(ITopology5Room aValue)
        {
            iList.Add(aValue);
            base.Add(aValue);
        }

        public new void Remove(ITopology5Room aValue)
        {
            iList.Remove(aValue);
            base.Remove(aValue);
        }

        private List<ITopology5Room> iList;
    }

    public interface ITopology5
    {
        IWatchableUnordered<ITopology5Room> Rooms { get; }
    }

    public class Topology5 : ITopology5, IUnorderedWatcher<ITopology4Room>, IDisposable
    {
        public Topology5(IWatchableThread aThread, ITopology4 aTopology4)
        {
            iDisposed = false;
            iThread = aThread;
            iTopology4 = aTopology4;

            iRooms = new WatchableTopology5RoomUnordered(iThread);
            iRoomLookup = new Dictionary<ITopology4Room, Topology5Room>();

            iTopology4.Rooms.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology5.Dispose");
            }

            iTopology4.Rooms.RemoveWatcher(this);
            iTopology4 = null;

            foreach (Topology5Room r in iRoomLookup.Values)
            {
                r.Dispose();
            }
            iRoomLookup.Clear();
            iRoomLookup = null;

            iRooms.Dispose();
            iRooms = null;

            iDisposed = true;
        }

        public IWatchableUnordered<ITopology5Room> Rooms
        {
            get
            {
                return iRooms;
            }
        }

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedClose()
        {
        }

        public void UnorderedAdd(ITopology4Room aItem)
        {
            Topology5Room room = new Topology5Room(iThread, aItem);
            iRooms.Add(room);
            iRoomLookup.Add(aItem, room);
        }

        public void UnorderedRemove(ITopology4Room aItem)
        {
            Topology5Room room = iRoomLookup[aItem];

            iRooms.Remove(room);
            iRoomLookup.Remove(aItem);

            iThread.Schedule(() =>
            {
                room.Dispose();
            });
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private ITopology4 iTopology4;

        private WatchableTopology5RoomUnordered iRooms;
        private Dictionary<ITopology4Room, Topology5Room> iRoomLookup;
    }
}
