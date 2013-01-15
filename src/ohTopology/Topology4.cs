using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ITopology4Source
    {
        uint Index { get; }
        string Name { get; }
        string Type { get; }
        bool Visible { get; }

        void Select();
    }

    public class Topology4Source : ITopology4Source
    {
        public Topology4Source(ITopology4Group aGroup, ITopology2Source aSource)
        {
            iGroup = aGroup;
            iSource = aSource;

            iIndex = iSource.Index;
            iName = iSource.Name;
            iType = iSource.Type;
            iVisible = iSource.Visible;
        }

        public uint Index
        {
            get
            {
                return iIndex;
            }
        }

        public string Name
        {
            get
            {
                return iName;
            }
        }

        public string Type
        {
            get
            {
                return iType;
            }
        }

        public bool Visible
        {
            get
            {
                return iVisible;
            }
        }

        public void Select()
        {
            iGroup.SetSourceIndex(iIndex);
        }

        private ITopology4Group iGroup;
        private ITopology2Source iSource;

        private uint iIndex;
        private string iName;
        private string iType;
        private bool iVisible;
    }

    public interface ITopology4Group
    {
        string Name { get; }
        IWatchable<bool> Standby { get; }
        IWatchable<uint> SourceIndex { get; }

        ITopology4Group Parent { get; }

        bool AddIfIsChild(ITopology4Group aGroup);
        void RemoveChild(ITopology4Group aGroup);
        void SetParent(ITopology4Group aGroup);
        void SetParent(ITopology4Group aGroup, uint aIndex);
        IEnumerable<ITopology4Source> Sources();

        void SetStandby(bool aValue);
        void SetSourceIndex(uint aValue);
    }

    public class Topology4Group : ITopology4Group, IWatcher<string>, IWatcher<ITopology2Source>, IDisposable
    {
        public Topology4Group(IWatchableThread aThread, ITopology2Group aGroup)
        {
            iGroup = aGroup;

            iStandby = iGroup.Standby;
            iSourceIndex = iGroup.SourceIndex;

            iChildren = new List<ITopology4Group>();

            iGroup.Name.AddWatcher(this);

            iSources = new Dictionary<string, ITopology2Source>();
            foreach (IWatchable<ITopology2Source> s in iGroup.Sources)
            {
                s.AddWatcher(this);
            }
        }

        public void Dispose()
        {
            foreach (IWatchable<ITopology2Source> s in iGroup.Sources)
            {
                s.RemoveWatcher(this);
            }
            iSources.Clear();
            iSources = null;

            iGroup.Name.RemoveWatcher(this);
            iGroup = null;
        }

        public string Name
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

        public void ItemOpen(string aId, ITopology2Source aValue)
        {
            iSources.Add(aId, aValue);
        }

        public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
        {
            iSources[aId] = aValue;
        }

        public void ItemClose(string aId, ITopology2Source aValue)
        {
            iSources.Remove(aId);
        }

        public void ItemOpen(string aId, string aValue)
        {
            iName = aValue;
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iName = aValue;
        }

        public void ItemClose(string aId, string aValue)
        {
            iName = null;
        }

        public ITopology4Group Parent
        {
            get
            {
                return iParent;
            }
        }

        public bool AddIfIsChild(ITopology4Group aGroup)
        {
            foreach (ITopology2Source s in iSources.Values)
            {
                if (aGroup.Name == s.Name)
                {
                    aGroup.SetParent(this, s.Index);
                    iChildren.Add(aGroup);
                    return true;
                }
            }

            return false;
        }

        public void RemoveChild(ITopology4Group aGroup)
        {
            iChildren.Remove(aGroup);
            aGroup.SetParent(null);
        }

        public void SetParent(ITopology4Group aGroup)
        {
            iParent = aGroup;
        }

        public void SetParent(ITopology4Group aGroup, uint aIndex)
        {
            SetParent(aGroup);
            iParentSourceIndex = aIndex;
        }

        public IEnumerable<ITopology4Source> Sources()
        {
            List<ITopology4Source> sources = new List<ITopology4Source>();

            foreach (ITopology2Source s in iSources.Values)
            {
                // only include if source is visible
                if (s.Visible)
                {
                    foreach (ITopology4Group g in iChildren)
                    {
                        // if group is connected to source expand source to group sources
                        if (s.Name == g.Name)
                        {
                            sources.AddRange(g.Sources());
                            continue;
                        }
                    }

                    sources.Add(new Topology4Source(this, s));
                }
            }

            return sources;
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
                if (iParent != null)
                {
                    iParent.SetSourceIndex(iParentSourceIndex);
                }

                iGroup.SetSourceIndex(aValue);
            }
        }

        private string iName;

        private IWatchable<bool> iStandby;
        private IWatchable<uint> iSourceIndex;

        private ITopology2Group iGroup;

        private uint iParentSourceIndex;
        private ITopology4Group iParent;
        private List<ITopology4Group> iChildren;

        private Dictionary<string, ITopology2Source> iSources;
    }

    public interface ITopology4Room
    {
        string Name { get; }
        IWatchable<IEnumerable<ITopology4Source>> Sources { get; }

        void SetStandby(bool aValue);
        void SetSourceIndex(uint aIndex);
    }

    public class Topology4Room : ITopology4Room, IUnorderedWatcher<ITopology2Group>, IDisposable
    {
        public Topology4Room(IWatchableThread aThread, ITopology3Room aRoom)
        {
            iThread = aThread;
            iRoom = aRoom;

            iName = iRoom.Name;

            iWatchableSources = new Watchable<IEnumerable<ITopology4Source>>(iThread, "Topology4Sources", new List<ITopology4Source>());

            iCurrent = null;
            iGroups = new List<ITopology4Group>();
            iRoots = new List<ITopology4Group>();
            iGroupLookup = new Dictionary<ITopology2Group, Topology4Group>();

            iRoom.Groups.AddWatcher(this);
        }

        public void Dispose()
        {
            iRoots.Clear();
            iRoots = null;

            iGroupLookup.Clear();
            iGroupLookup = null;

            foreach (Topology4Group g in iGroups)
            {
                g.Dispose();
            }
            iGroups.Clear();
            iGroups = null;

            iRoom.Groups.RemoveWatcher(this);
            iRoom = null;
        }

        public string Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchable<IEnumerable<ITopology4Source>> Sources
        {
            get
            {
                return iWatchableSources;
            }
        }

        public void SetStandby(bool aValue)
        {
        }

        public void SetSourceIndex(uint aIndex)
        {
            iSources.ElementAt((int)aIndex).Select();
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

        public void UnorderedAdd(ITopology2Group aItem)
        {
            Topology4Group group = new Topology4Group(iThread, aItem);
            iGroupLookup.Add(aItem, group);

            AddGroup(group);

            EvaluateSources();
        }

        public void UnorderedRemove(ITopology2Group aItem)
        {
            Topology4Group group = iGroupLookup[aItem];

            iGroupLookup.Remove(aItem);

            RemoveGroup(group);

            EvaluateSources();
        }

        private void AddGroup(ITopology4Group aGroup)
        {
            // if group is the first group found
            if (iCurrent == null)
            {
                iCurrent = aGroup;
                iGroups.Add(aGroup);
                iRoots.Add(aGroup);

                return;
            }

            // check for an existing parent
            foreach (Topology4Group g in iGroups)
            {
                if (g.AddIfIsChild(aGroup))
                {
                    iGroups.Add(aGroup);

                    return;
                }
            }

            // check for parent of an existing root
            foreach (Topology4Group g in iRoots)
            {
                if (aGroup.AddIfIsChild(g))
                {
                    iRoots.Remove(g);
                    break;
                }
            }

            iGroups.Add(aGroup);
            iRoots.Add(aGroup);
        }

        private void RemoveGroup(ITopology4Group aGroup)
        {
            iGroups.Remove(aGroup);

            if (iRoots.Contains(aGroup))
            {
                iRoots.Remove(aGroup);
            }
            else
            {
                // unhook group from group tree
                aGroup.Parent.RemoveChild(aGroup);
            }

            // check for orphaned groups
            foreach (Topology4Group g in iGroups)
            {
                // if group has no parent and it is not a root group - promote it to a root
                if (g.Parent != null && !iRoots.Contains(g))
                {
                    iRoots.Add(g);
                }
            }
        }

        private void EvaluateSources()
        {
            List<ITopology4Source> sources = new List<ITopology4Source>();

            foreach (ITopology4Group g in iRoots)
            {
                sources.AddRange(g.Sources());
            }

            iSources = sources;
            iWatchableSources.Update(sources);
        }

        private IWatchableThread iThread;
        private ITopology3Room iRoom;

        private string iName;

        private IEnumerable<ITopology4Source> iSources;
        private Watchable<IEnumerable<ITopology4Source>> iWatchableSources;

        private ITopology4Group iCurrent;
        private List<ITopology4Group> iGroups;
        private List<ITopology4Group> iRoots;
        private Dictionary<ITopology2Group, Topology4Group> iGroupLookup;
    }

    public class WatchableTopology4RoomUnordered : WatchableUnordered<ITopology4Room>
    {
        public WatchableTopology4RoomUnordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<ITopology4Room>();
        }

        public new void Add(ITopology4Room aValue)
        {
            iList.Add(aValue);
            base.Add(aValue);
        }

        public new void Remove(ITopology4Room aValue)
        {
            iList.Remove(aValue);
            base.Remove(aValue);
        }

        private List<ITopology4Room> iList;
    }

    public interface ITopology4
    {
        IWatchableUnordered<ITopology4Room> Rooms { get; }
    }

    public class Topology4 : ITopology4, IUnorderedWatcher<ITopology3Room>, IDisposable
    {
        public Topology4(IWatchableThread aThread, ITopology3 aTopology3)
        {
            iDisposed = false;
            iThread = aThread;
            iTopology3 = aTopology3;

            iRooms = new WatchableTopology4RoomUnordered(aThread);

            iRoomLookup = new Dictionary<ITopology3Room, Topology4Room>();

            iTopology3.Rooms.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology3.Dispose");
            }

            foreach (Topology4Room r in iRoomLookup.Values)
            {
                r.Dispose();
            }
            iRoomLookup.Clear();
            iRoomLookup = null;

            iRooms.Dispose();
            iRooms = null;

            iTopology3.Rooms.RemoveWatcher(this);
            iTopology3 = null;
        }

        public IWatchableUnordered<ITopology4Room> Rooms
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

        public void UnorderedAdd(ITopology3Room aItem)
        {
            Topology4Room room = new Topology4Room(iThread, aItem);
            iRooms.Add(room);
            iRoomLookup.Add(aItem, room);
        }

        public void UnorderedRemove(ITopology3Room aItem)
        {
            iRooms.Remove(iRoomLookup[aItem]);
            iRoomLookup[aItem].Dispose();
            iRoomLookup.Remove(aItem);
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private ITopology3 iTopology3;

        private WatchableTopology4RoomUnordered iRooms;
        private Dictionary<ITopology3Room, Topology4Room> iRoomLookup;
    }
}
