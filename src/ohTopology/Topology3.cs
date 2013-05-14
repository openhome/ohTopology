using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    // L3 group is a simple wrapper around an L2 group that proxies the watchables
    public interface ITopology3Group : ITopology2Group
    {
    }

    class Topology3Group : ITopology3Group, ITopologyObject
    {
        public Topology3Group(ITopology2Group aGroup)
        {
            iDisposed = false;
            iGroup = aGroup;
            iId = aGroup.Id;
            iAttributes = aGroup.Attributes;
            iDevice = aGroup.Device;

            // create watchable proxies for the L2 watchables
            iRoom = new WatchableProxy<string>(aGroup.Room);
            iName = new WatchableProxy<string>(aGroup.Name);
            iStandby = new WatchableProxy<bool>(aGroup.Standby);
            iSourceIndex = new WatchableProxy<uint>(aGroup.SourceIndex);

            iSources = new List<WatchableProxy<ITopology2Source>>();
            foreach (IWatchable<ITopology2Source> s in aGroup.Sources)
            {
                iSources.Add(new WatchableProxy<ITopology2Source>(s));
            }
        }

        // ITopologyObject

        public void Detach()
        {
            iRoom.Detach();
            iName.Detach();
            iStandby.Detach();
            iSourceIndex.Detach();

            foreach (WatchableProxy<ITopology2Source> s in iSources)
            {
                s.Detach();
            }

            iGroup = null;
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology3Group.Dispose");
            }

            iRoom.Dispose();
            iName.Dispose();
            iStandby.Dispose();
            iSourceIndex.Dispose();

            foreach (WatchableProxy<ITopology2Source> s in iSources)
            {
                s.Dispose();
            }

            iDisposed = true;
        }

        // ITopology3Group

        public string Id
        {
            get
            {
                return iId;
            }
        }
        
        public string Attributes
        {
            get
            {
                return iAttributes;
            }
        }

        public IWatchableDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public IWatchable<string> Room
        {
            get
            {
                return iRoom;
            }
        }

        public IWatchable<string> Name
        {
            get
            {
                return iName;
            }
        }
        
        public IWatchable<bool> Standby
        {
            get
            {
                return iStandby;
            }
        }
        
        public IWatchable<uint> SourceIndex
        {
            get
            {
                return iSourceIndex;
            }
        }

        public IEnumerable<IWatchable<ITopology2Source>> Sources
        {
            get
            {
                return iSources;
            }
        }

        public void SetStandby(bool aValue)
        {
            if (iGroup != null)
            {
                iGroup.SetStandby(aValue);
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            if (iGroup != null)
            {
                iGroup.SetSourceIndex(aValue);
            }
        }

        private bool iDisposed;
        private ITopology2Group iGroup;
        private string iId;
        private string iAttributes;
        private IWatchableDevice iDevice;
        private WatchableProxy<string> iRoom;
        private WatchableProxy<string> iName;
        private WatchableProxy<bool> iStandby;
        private WatchableProxy<uint> iSourceIndex;
        private List<WatchableProxy<ITopology2Source>> iSources;
    }

    public interface ITopology3Room
    {
        string Name { get; }
        IWatchableUnordered<ITopology3Group> Groups { get; }

        void SetStandby(bool aValue);
    }

    class Topology3Room : ITopology3Room, ITopologyObject
    {
        public Topology3Room(IWatchableThread aThread, string aName, ITopology3Group aGroup)
        {
            iName = aName;
            iGroups = new List<ITopology3Group>(); ;
            iWatchableGroups = new WatchableUnordered<ITopology3Group>(aThread);

            Add(aGroup);
        }

        // ITopologyObject

        public void Detach()
        {
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

        public IWatchableUnordered<ITopology3Group> Groups
        {
            get
            {
                return iWatchableGroups;
            }
        }

        public void SetStandby(bool aValue)
        {
            foreach (ITopology3Group g in iGroups)
            {
                g.SetStandby(aValue);
            }
        }

        // Topology3Room

        public void Add(ITopology3Group aGroup)
        {
            iWatchableGroups.Add(aGroup);
            iGroups.Add(aGroup);
        }

        public bool Remove(ITopology3Group aGroup)
        {
            iWatchableGroups.Remove(aGroup);
            iGroups.Remove(aGroup);

            return (iGroups.Count == 0);
        }

        private string iName;
        private List<ITopology3Group> iGroups;
        private WatchableUnordered<ITopology3Group> iWatchableGroups;
    }

    public interface ITopology3
    {
        IWatchableUnordered<ITopology3Room> Rooms { get; }
        IWatchableThread WatchableThread { get; }
    }

    public class Topology3 : ITopology3, IUnorderedWatcher<ITopology2Group>, IWatcher<string>, IDisposable
    {
        public Topology3(ITopology2 aTopology2)
        {
            iDisposed = false;
            iThread = aTopology2.WatchableThread;
            iTopology2 = aTopology2;

            iRooms = new WatchableUnordered<ITopology3Room>(iThread);

            iRoomIdToGroup3Lookup = new Dictionary<string,Topology3Group>();
            iGroup2ToGroup3Lookup = new Dictionary<ITopology2Group,Topology3Group>();
            iRoomNameToRoom3Lookup = new Dictionary<string,Topology3Room>();

            iThread.Schedule(() =>
            {
                iTopology2.Groups.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology3.Dispose");
            }

            iThread.Execute(() =>
            {
                iTopology2.Groups.RemoveWatcher(this);

                // removing these watchers should cause all rooms to be detached and disposed
                foreach (Topology3Group group in iGroup2ToGroup3Lookup.Values)
                {
                    group.Room.RemoveWatcher(this);
                }

                // any created L3 groups must be disposed
                foreach (Topology3Group group in iGroup2ToGroup3Lookup.Values)
                {
                    group.Detach();
                }
            });
            iTopology2 = null;

            foreach (Topology3Group group in iGroup2ToGroup3Lookup.Values)
            {
                group.Dispose();
            }

            iGroup2ToGroup3Lookup.Clear();
            iRoomIdToGroup3Lookup.Clear();
            iRoomNameToRoom3Lookup.Clear();

            iGroup2ToGroup3Lookup = null;
            iRoomIdToGroup3Lookup = null;
            iRoomNameToRoom3Lookup = null;

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

        public IWatchableThread WatchableThread
        {
            get
            {
                return iThread;
            }
        }

        // IUnorderedWatcher<ITopology2Group>

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedClose()
        {
        }

        public void UnorderedAdd(ITopology2Group aItem)
        {
            Topology3Group group = new Topology3Group(aItem);

            iRoomIdToGroup3Lookup.Add(group.Room.Id, group);
            iGroup2ToGroup3Lookup.Add(aItem, group);

            group.Room.AddWatcher(this);
        }

        public void UnorderedRemove(ITopology2Group aItem)
        {
            Topology3Group group;
            if (iGroup2ToGroup3Lookup.TryGetValue(aItem, out group))
            {
                // schedule notification of L3 group removal
                group.Room.RemoveWatcher(this);
                iRoomIdToGroup3Lookup.Remove(group.Room.Id);
                iGroup2ToGroup3Lookup.Remove(aItem);

                // detach group from L2
                group.Detach();

                // schedule disposal of L3 group
                iThread.Schedule(() =>
                {
                    group.Dispose();
                });
            }
        }

        // IWatcher<string>

        public void ItemOpen(string aId, string aValue)
        {
            // get the L3 group for this id
            Topology3Group group;
            if (iRoomIdToGroup3Lookup.TryGetValue(aId, out group))
            {
                AddGroupToRoom(group, aValue);
            }
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            // get the L3 group for this id
            Topology3Group group;
            if (iRoomIdToGroup3Lookup.TryGetValue(aId, out group))
            {
                RemoveGroupFromRoom(group, aPrevious);
                AddGroupToRoom(group, aValue);
            }
        }

        public void ItemClose(string aId, string aValue)
        {
            // get the L3 group for this id
            Topology3Group group;
            if (iRoomIdToGroup3Lookup.TryGetValue(aId, out group))
            {
                RemoveGroupFromRoom(group, aValue);
            }
        }

        private void AddGroupToRoom(Topology3Group aGroup, string aRoom)
        {
            Topology3Room room;
            if (iRoomNameToRoom3Lookup.TryGetValue(aRoom, out room))
            {
                // room already exists
                room.Add(aGroup);
            }
            else
            {
                // need to create a new room
                room = new Topology3Room(iThread, aRoom, aGroup);
                iRoomNameToRoom3Lookup.Add(aRoom, room);
                iRooms.Add(room);
            }
        }

        private void RemoveGroupFromRoom(Topology3Group aGroup, string aRoom)
        {
            Topology3Room room;
            if (iRoomNameToRoom3Lookup.TryGetValue(aRoom, out room))
            {
                if (room.Remove(aGroup))
                {
                    // no more groups in room - remove it
                    iRooms.Remove(room);
                    iRoomNameToRoom3Lookup.Remove(aRoom);

                    // detach the room from L2
                    room.Detach();

                    // schedule disposal of the room
                    iThread.Schedule(() =>
                    {
                        room.Dispose();
                    });
                }
            }
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private ITopology2 iTopology2;

        private WatchableUnordered<ITopology3Room> iRooms;

        private Dictionary<string, Topology3Group> iRoomIdToGroup3Lookup;
        private Dictionary<ITopology2Group, Topology3Group> iGroup2ToGroup3Lookup;
        private Dictionary<string, Topology3Room> iRoomNameToRoom3Lookup;
    }
}
