using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ITopology3Room
    {
        string Name { get; }
        IWatchableUnordered<ITopology2Group> Groups { get; }
    }

    public class Topology3Room : ITopology3Room, IDisposable
    {
        public Topology3Room(string aName, IWatchableThread aThread)
        {
            iName = aName;
            iGroupCount = 0;
            iGroups = new WatchableUnordered<ITopology2Group>(aThread);
        }

        public void Dispose()
        {
            iGroups.Dispose();
            iGroups = null;

            iGroupCount = 0;
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
                return iGroups;
            }
        }

        public void Add(ITopology2Group aGroup)
        {
            iGroups.Add(aGroup);
            ++iGroupCount;
        }

        public bool Remove(ITopology2Group aGroup)
        {
            iGroups.Remove(aGroup);
            --iGroupCount;

            return (iGroupCount == 0);
        }

        private string iName;
        private uint iGroupCount;
        private WatchableUnordered<ITopology2Group> iGroups;
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

            foreach (ITopology2Group g in iGroupLookup.Values)
            {
                g.Name.RemoveWatcher(this);
            }
            iGroupLookup.Clear();
            iGroupLookup = null;

            foreach (Topology3Room r in iRoomLookup.Values)
            {
                r.Dispose();
            }
            iRoomLookup.Clear();
            iRoomLookup = null;

            iRooms.Dispose();
            iRooms = null;

            iTopology2.Groups.RemoveWatcher(this);
            iTopology2 = null;
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
            Topology3Room room = null;
            if (!iRoomLookup.TryGetValue(aValue, out room))
            {
                room = new Topology3Room(aValue, iThread);
                iRoomLookup.Add(aValue, room);
                iRooms.Add(room);
            }

            ITopology2Group group;
            if (iGroupLookup.TryGetValue(aId, out group))
            {
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

                        // schedule the disposale for the room for after all watchers of the room collection have been notified
                        iThread.Schedule(() =>
                        {
                            room.Dispose();
                        });
                    }
                }

                Topology3Room newRoom;
                if (!iRoomLookup.TryGetValue(aValue, out newRoom))
                {
                    newRoom = new Topology3Room(aValue, iThread);
                    iRoomLookup.Add(aValue, newRoom);
                    iRooms.Add(newRoom);
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

                        // schedule the disposale for the room for after all watchers of the room collection have been notified
                        iThread.Schedule(() =>
                        {
                            room.Dispose();
                        });
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
