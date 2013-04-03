using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ITopology5Group
    {
        string Name { get; }
        IWatchable<ITopology4Source> Source { get; }
    }

    public class Topology5Group : ITopology5Group, ITopologyObject
    {
        public Topology5Group(ITopology4Group aGroup)
        {
            iName = aGroup.Name;
            iSource = new WatchableProxy<ITopology4Source>(aGroup.Source);
        }

        public void Detach()
        {
            iSource.Detach();
        }

        public void Dispose()
        {
            iSource.Dispose();
            iSource = null;
        }

        public string Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchable<ITopology4Source> Source
        {
            get
            {
                return iSource;
            }
        }

        private string iName;
        private WatchableProxy<ITopology4Source> iSource;
    }

    public interface ITopology5Room
    {
        string Name { get; }
        IWatchable<EStandby> Standby { get; }
        IWatchableUnordered<ITopology5Group> Roots { get; }
        IWatchableUnordered<ITopology4Source> Sources { get; }

        void SetStandby(bool aValue);
    }

    public class Topology5Room : ITopology5Room, IWatcher<IEnumerable<ITopology4Group>>, IWatcher<IEnumerable<ITopology4Source>>, ITopologyObject
    {
        public Topology5Room(IWatchableThread aThread, ITopology4Room aRoom)
        {
            iThread = aThread;
            iRoom = aRoom;
            iName = iRoom.Name;

            iStandby = new WatchableProxy<EStandby>(iRoom.Standby);
            iRoots = new WatchableUnordered<ITopology5Group>(iThread);
            iSources = new WatchableUnordered<ITopology4Source>(iThread);
            iRootLookup = new Dictionary<ITopology4Group, Topology5Group>();

            iRoom.Roots.AddWatcher(this);
            iRoom.Sources.AddWatcher(this);
        }

        public void Detach()
        {
            iStandby.Detach();

            // removing the roots watcher will cause all L5 roots to be detached and scheduled for disposal
            iRoom.Roots.RemoveWatcher(this);
            iRoom.Sources.RemoveWatcher(this);
            iRoom = null;
        }

        public void Dispose()
        {
            iStandby.Dispose();
            iRoots.Dispose();
            iSources.Dispose();

            iStandby = null;
            iRoots = null;
            iSources = null;
        }

        // ITopology5Room

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
                return iStandby;
            }
        }

        public IWatchableUnordered<ITopology5Group> Roots
        {
            get
            {
                return iRoots;
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

        // IWatcher<IEnumerable<ITopology4Group>>

        public void ItemOpen(string aId, IEnumerable<ITopology4Group> aValue)
        {
            foreach (ITopology4Group g in aValue)
            {
                AddRoot(g);
            }
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Group> aValue, IEnumerable<ITopology4Group> aPrevious)
        {
            List<ITopology4Group> removed = new List<ITopology4Group>();
            foreach (ITopology4Group g in aPrevious)
            {
                if (!aValue.Contains(g))
                {
                    removed.Add(g);
                }
            }

            List<ITopology4Group> added = new List<ITopology4Group>();
            foreach (ITopology4Group g in aValue)
            {
                if (!aPrevious.Contains(g))
                {
                    added.Add(g);
                }
            }

            foreach (ITopology4Group g in removed)
            {
                RemoveRoot(g);
            }

            foreach (ITopology4Group g in added)
            {
                AddRoot(g);
            }
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Group> aValue)
        {
            foreach (ITopology4Group g in aValue)
            {
                RemoveRoot(g);
            }
        }

        private void AddRoot(ITopology4Group aRoot)
        {
            Topology5Group root = new Topology5Group(aRoot);
            iRoots.Add(root);
            iRootLookup.Add(aRoot, root);
        }

        private void RemoveRoot(ITopology4Group aRoot)
        {
            // schedule removal the root
            Topology5Group root = iRootLookup[aRoot];
            iRootLookup.Remove(aRoot);
            iRoots.Remove(root);

            // detach root from L4
            root.Detach();

            // schedule disposal of root
            iThread.Schedule(() =>
            {
                root.Dispose();
            });
        }

        // IWatcher<IEnumerable<ITopology4Source>>

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
            foreach (ITopology4Source s in aPrevious)
            {
                if (!aValue.Contains(s))
                {
                    removed.Add(s);
                }
            }

            List<ITopology4Source> added = new List<ITopology4Source>();
            foreach (ITopology4Source s in aValue)
            {
                if (!aPrevious.Contains(s))
                {
                    added.Add(s);
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
            foreach (ITopology4Source s in aValue)
            {
                iSources.Remove(s);
            }
        }

        private IWatchableThread iThread;
        private ITopology4Room iRoom;
        private string iName;
        private WatchableProxy<EStandby> iStandby;
        private WatchableUnordered<ITopology5Group> iRoots;
        private WatchableUnordered<ITopology4Source> iSources;
        private Dictionary<ITopology4Group, Topology5Group> iRootLookup;
    }

    public interface ITopology5
    {
        IWatchableUnordered<ITopology5Room> Rooms { get; }
    }

    public class Topology5 : ITopology5, IUnorderedWatcher<ITopology4Room>, IDisposable
    {
        public Topology5(IWatchableThread aThread, ITopology4 aTopology4)
        {
            iThread = aThread;
            iTopology4 = aTopology4;

            iRooms = new WatchableUnordered<ITopology5Room>(iThread);
            iRoomLookup = new Dictionary<ITopology4Room, Topology5Room>();

            iTopology4.Rooms.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iTopology4 == null)
            {
                throw new ObjectDisposedException("Topology5.Dispose");
            }

            iTopology4.Rooms.RemoveWatcher(this);
            iTopology4 = null;

            foreach (Topology5Room r in iRoomLookup.Values)
            {
                r.Detach();
                r.Dispose();
            }

            iRoomLookup.Clear();
            iRoomLookup = null;

            iRooms.Dispose();
            iRooms = null;
        }

        // ITopology5 

        public IWatchableUnordered<ITopology5Room> Rooms
        {
            get
            {
                return iRooms;
            }
        }

        // IUnorderedWatcher<ITopology4Room>

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
            // schedule the removal of the L5 room
            Topology5Room room = iRoomLookup[aItem];
            iRooms.Remove(room);
            iRoomLookup.Remove(aItem);

            // detach the L5 from L4
            room.Detach();

            // schedule the disposal of L5 room
            iThread.Schedule(() =>
            {
                room.Dispose();
            });
        }

        private IWatchableThread iThread;
        private ITopology4 iTopology4;
        private WatchableUnordered<ITopology5Room> iRooms;
        private Dictionary<ITopology4Room, Topology5Room> iRoomLookup;
    }
}
