using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ITopology3Room : IRefCount
    {
        string Name { get; }
        IWatchableUnordered<ITopology2Group> Groups { get; }

        void SetStandby(bool aValue);
    }

    public class Topology3Room : RefCount, ITopology3Room
    {
        public Topology3Room(IWatchableThread aThread, string aName, ITopology2Group aGroup)
        {
            iName = aName;
            iGroups = new List<ITopology2Group>(); ;
            iWatchableGroups = new WatchableUnordered<ITopology2Group>(aThread);

            Add(aGroup);
        }

        protected override void CleanUp()
        {
            iWatchableGroups.Dispose();
            iWatchableGroups = null;

            iGroups.Clear();
            iGroups = null;
        }

        public string Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchableUnordered<ITopology2Group> Groups
        {
            get
            {
                return iWatchableGroups;
            }
        }

        public void SetStandby(bool aValue)
        {
            foreach (ITopology2Group g in iGroups)
            {
                g.SetStandby(aValue);
            }
        }

        public void Add(ITopology2Group aGroup)
        {
            iWatchableGroups.Add(aGroup);
            iGroups.Add(aGroup);
        }

        public bool Remove(ITopology2Group aGroup)
        {
            iWatchableGroups.Remove(aGroup);
            iGroups.Remove(aGroup);

            return (iGroups.Count == 0);
        }

        private string iName;
        private List<ITopology2Group> iGroups;
        private WatchableUnordered<ITopology2Group> iWatchableGroups;
    }

    public class WatchableTopology3RoomUnordered : WatchableUnordered<ITopology3Room>
    {
        public WatchableTopology3RoomUnordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<ITopology3Room>();
        }

        public new void Add(ITopology3Room aValue)
        {
            iList.Add(aValue);
            base.Add(aValue);
        }

        public new void Remove(ITopology3Room aValue)
        {
            iList.Remove(aValue);
            base.Remove(aValue);
        }

        private List<ITopology3Room> iList;
    }

    public interface ITopology3
    {
        IWatchableUnordered<ITopology3Room> Rooms { get; }
    }

    public class Topology3 : ITopology3, IUnorderedWatcher<ITopology2Group>, IWatcher<string>, IDisposable
    {
        public Topology3(IWatchableThread aThread, ITopology2 aTopology2)
        {
            iDisposed = false;
            iThread = aThread;
            iTopology2 = aTopology2;

            iRooms = new WatchableTopology3RoomUnordered(aThread);

            iGroupLookup = new Dictionary<string, ITopology2Group>();
            iRoomLookup = new Dictionary<string, Topology3Room>();

            iTopology2.Groups.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology3.Dispose");
            }

            iTopology2.Groups.RemoveWatcher(this);
            iTopology2 = null;

            foreach (ITopology2Group g in iGroupLookup.Values)
            {
                g.Name.RemoveWatcher(this);
            }
            iGroupLookup.Clear();
            iGroupLookup = null;

            foreach (Topology3Room r in iRoomLookup.Values)
            {
                r.RemoveRef();
            }
            iRoomLookup.Clear();
            iRoomLookup = null;

            iRooms.Dispose();
            iRooms = null;
        }

        public IWatchableUnordered<ITopology3Room> Rooms
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

        public void ItemOpen(string aId, string aValue)
        {
            ITopology2Group group;
            if (iGroupLookup.TryGetValue(aId, out group))
            {
                Topology3Room room = null;
                if (!iRoomLookup.TryGetValue(aValue, out room))
                {
                    room = new Topology3Room(iThread, aValue, group);
                    iRoomLookup.Add(aValue, room);
                    iRooms.Add(room);

                    return;
                }

                room.Add(group);
            }
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            ITopology2Group group;
            if (iGroupLookup.TryGetValue(aId, out group))
            {
                Topology3Room room;
                if (iRoomLookup.TryGetValue(aPrevious, out room))
                {
                    if (room.Remove(group))
                    {
                        iRooms.Remove(room);
                        iRoomLookup.Remove(room.Name);
                        room.RemoveRef();
                    }
                }

                Topology3Room newRoom;
                if (!iRoomLookup.TryGetValue(aValue, out newRoom))
                {
                    newRoom = new Topology3Room(iThread, aValue, group);
                    iRoomLookup.Add(aValue, newRoom);
                    iRooms.Add(newRoom);

                    return;
                }

                newRoom.Add(group);
            }
        }

        public void ItemClose(string aId, string aValue)
        {
            ITopology2Group group;
            if (iGroupLookup.TryGetValue(aId, out group))
            {
                Topology3Room room;
                if (iRoomLookup.TryGetValue(aValue, out room))
                {
                    if(room.Remove(group))
                    {
                        iRooms.Remove(room);
                        iRoomLookup.Remove(room.Name);
                        room.RemoveRef();
                    }
                }
            }
        }

        public void UnorderedAdd(ITopology2Group aItem)
        {
            iGroupLookup.Add(aItem.Room.Id, aItem);
            aItem.Room.AddWatcher(this);
        }

        public void UnorderedRemove(ITopology2Group aItem)
        {
            aItem.Room.RemoveWatcher(this);
            iGroupLookup.Remove(aItem.Room.Id);
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private ITopology2 iTopology2;

        private WatchableTopology3RoomUnordered iRooms;

        private Dictionary<string, ITopology2Group> iGroupLookup;
        private Dictionary<string, Topology3Room> iRoomLookup;
    }
}
