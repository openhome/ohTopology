using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ITopology3Room
    {
        string Name { get; }
        IWatchableUnordered<ITopologymGroup> Groups { get; }

        void SetStandby(bool aValue);
    }

    class Topology3GroupWatcher : IWatcher<string>, IDisposable
    {
        private readonly Topology3 iTopology;
        private readonly ITopologymGroup iGroup;

        public Topology3GroupWatcher(Topology3 aTopology, ITopologymGroup aGroup)
        {
            iTopology = aTopology;
            iGroup = aGroup;
            iGroup.Room.AddWatcher(this);
        }

        // IWatcher<string>

        public void ItemOpen(string aId, string aValue)
        {
            iTopology.AddGroupToRoom(iGroup, aValue);
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iTopology.RemoveGroupFromRoom(iGroup, aPrevious);
            iTopology.AddGroupToRoom(iGroup, aValue);
        }

        public void ItemClose(string aId, string aValue)
        {
            iTopology.RemoveGroupFromRoom(iGroup, aValue);
        }

        // IDisposable

        public void Dispose()
        {
            iGroup.Room.RemoveWatcher(this);
        }
    }

    class Topology3Room : ITopology3Room
    {
        public Topology3Room(IWatchableThread aThread, string aName, ITopologymGroup aGroup)
        {
            iName = aName;
            iGroups = new List<ITopologymGroup>(); ;
            iWatchableGroups = new WatchableUnordered<ITopologymGroup>(aThread);

            Add(aGroup);
        }

        public void Dispose()
        {
            iWatchableGroups.Dispose();
            iWatchableGroups = null;

            iGroups.Clear();
            iGroups = null;
        }

        // ITopology3Room

        public string Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchableUnordered<ITopologymGroup> Groups
        {
            get
            {
                return iWatchableGroups;
            }
        }

        public void SetStandby(bool aValue)
        {
            foreach (ITopologymGroup g in iGroups)
            {
                g.SetStandby(aValue);
            }
        }

        // Topology3Room

        public void Add(ITopologymGroup aGroup)
        {
            iWatchableGroups.Add(aGroup);
            iGroups.Add(aGroup);
        }

        public bool Remove(ITopologymGroup aGroup)
        {
            iWatchableGroups.Remove(aGroup);
            iGroups.Remove(aGroup);

            return (iGroups.Count == 0);
        }

        private string iName;
        private List<ITopologymGroup> iGroups;
        private WatchableUnordered<ITopologymGroup> iWatchableGroups;
    }

    public interface ITopology3
    {
        IWatchableUnordered<ITopology3Room> Rooms { get; }
        INetwork Network { get; }
    }

    public class Topology3 : ITopology3, IUnorderedWatcher<ITopologymGroup>, IDisposable
    {
        public Topology3(ITopologym aTopologym)
        {
            iDisposed = false;
            iNetwork = aTopologym.Network;
            iTopologym = aTopologym;

            iRooms = new WatchableUnordered<ITopology3Room>(iNetwork);
            iGroupWatcherLookup = new Dictionary<ITopologymGroup, Topology3GroupWatcher>();
            iRoomLookup = new Dictionary<string, Topology3Room>();

            iNetwork.Schedule(() =>
            {
                iTopologym.Groups.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology3.Dispose");
            }

            iNetwork.Execute(() =>
            {
                iTopologym.Groups.RemoveWatcher(this);

                // removing these watchers should cause all rooms to be detached and disposed
                foreach (var group in iGroupWatcherLookup.Values)
                {
                    group.Dispose();
                }
            });

            iTopologym = null;

            iRoomLookup = null;
            iGroupWatcherLookup = null;

            iRooms.Dispose();
            iRooms = null;

            iDisposed = true;
        }

        public IWatchableUnordered<ITopology3Room> Rooms
        {
            get
            {
                return iRooms;
            }
        }

        public INetwork Network
        {
            get
            {
                return iNetwork;
            }
        }

        // IUnorderedWatcher<ITopologymGroup>

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedClose()
        {
        }

        public void UnorderedAdd(ITopologymGroup aItem)
        {
            iGroupWatcherLookup.Add(aItem, new Topology3GroupWatcher(this, aItem));
        }

        public void UnorderedRemove(ITopologymGroup aItem)
        {
            iGroupWatcherLookup[aItem].Dispose();
            iGroupWatcherLookup.Remove(aItem);
        }

        internal void AddGroupToRoom(ITopologymGroup aGroup, string aRoom)
        {
            Topology3Room room;

            if (iRoomLookup.TryGetValue(aRoom, out room))
            {
                // room already exists
                room.Add(aGroup);
            }
            else
            {
                // need to create a new room
                room = new Topology3Room(iNetwork, aRoom, aGroup);
                iRoomLookup.Add(aRoom, room);
                iRooms.Add(room);
            }
        }

        internal void RemoveGroupFromRoom(ITopologymGroup aGroup, string aRoom)
        {
            Topology3Room room;
            if (iRoomLookup.TryGetValue(aRoom, out room))
            {
                if (room.Remove(aGroup))
                {
                    // no more groups in room - remove it
                    iRooms.Remove(room);
                    iRoomLookup.Remove(aRoom);

                    room.Dispose();
                }
            }
        }

        private bool iDisposed;
        private INetwork iNetwork;
        private ITopologym iTopologym;

        private WatchableUnordered<ITopology3Room> iRooms;

        private Dictionary<ITopologymGroup, Topology3GroupWatcher> iGroupWatcherLookup;
        private Dictionary<string, Topology3Room> iRoomLookup;
    }
}
