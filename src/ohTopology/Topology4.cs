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

            iSources = new List<ITopology2Source>();
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
            iSources.Add(aValue);
        }

        public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
        {
            iSources.Remove(aPrevious);
            iSources.Add(aValue);
        }

        public void ItemClose(string aId, ITopology2Source aValue)
        {
            iSources.Remove(aValue);
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
            foreach (ITopology2Source s in iSources)
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

            foreach (ITopology2Source s in iSources)
            {
                // only include if source is visible
                if (s.Visible)
                {
                    bool expanded = false;
                    foreach (ITopology4Group g in iChildren)
                    {
                        // if group is connected to source expand source to group sources
                        if (s.Name == g.Name)
                        {
                            sources.AddRange(g.Sources());
                            expanded = true;
                        }
                    }

                    if (!expanded)
                    {
                        sources.Add(new Topology4Source(this, s));
                    }
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

        private List<ITopology2Source> iSources;
    }

    public interface ITopology4Room
    {
        string Name { get; }
        IWatchable<EStandby> Standby { get; }
        IWatchable<IEnumerable<ITopology4Source>> Sources { get; }

        void SetStandby(bool aValue);
        void SetSourceIndex(uint aIndex);
    }

    public class Topology4Room : ITopology4Room, IUnorderedWatcher<ITopology2Group>, IWatcher<string>, IWatcher<bool>, IWatcher<ITopology2Source>,  IDisposable
    {
        public Topology4Room(IWatchableThread aThread, ITopology3Room aRoom)
        {
            iThread = aThread;

            iRoom = aRoom;
            iRoom.AddRef();

            iName = iRoom.Name;
            iStandbyCount = 0;
            iStandby = EStandby.eOn;
            iWatchableStandby = new Watchable<EStandby>(iThread, string.Format("Standby({0})", iName), EStandby.eOn);
            iWatchableSources = new Watchable<IEnumerable<ITopology4Source>>(iThread, "Topology4Sources", new List<ITopology4Source>());

            iCurrent = null;
            iSources = new List<ITopology4Source>();
            iGroups = new List<ITopology4Group>();
            iRoots = new List<ITopology4Group>();
            iGroup2Lookup = new Dictionary<string, ITopology2Group>();
            iSource2Lookup = new Dictionary<string, Topology4Group>();
            iGroup4Lookup = new Dictionary<ITopology2Group, Topology4Group>();

            iRoom.Groups.AddWatcher(this);
        }

        public void Dispose()
        {
            iRoots.Clear();
            iRoots = null;

            foreach (ITopology2Group g in iGroup2Lookup.Values)
            {
                g.Name.RemoveWatcher(this);

                foreach (IWatchable<ITopology2Source> s in g.Sources)
                {
                    s.RemoveWatcher(this);
                }
            }
            iGroup2Lookup.Clear();
            iGroup2Lookup = null;

            iSource2Lookup.Clear();
            iSource2Lookup = null;

            iGroup4Lookup.Clear();
            iGroup4Lookup = null;

            foreach (Topology4Group g in iGroups)
            {
                g.Dispose();
            }
            iGroups.Clear();
            iGroups = null;

            iWatchableSources.Dispose();
            iSources = null;

            iRoom.Groups.RemoveWatcher(this);
            iRoom.RemoveRef();
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
                return iWatchableStandby;
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
            if (iRoom != null)
            {
                iRoom.SetStandby(aValue);
            }
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
            iGroup2Lookup.Add(aItem.Name.Id, aItem);

            Topology4Group group = new Topology4Group(iThread, aItem);
            iGroup4Lookup.Add(aItem, group);

            foreach (IWatchable<ITopology2Source> s in aItem.Sources)
            {
                s.AddWatcher(this);
                iSource2Lookup.Add(s.Id, group);
            }

            AddGroup(group);

            // add watchers after group has been inserted into the tree
            aItem.Name.AddWatcher(this);
            aItem.Standby.AddWatcher(this);

            EvaluateSources();
        }

        public void UnorderedRemove(ITopology2Group aItem)
        {
            iGroup2Lookup.Remove(aItem.Name.Id);

            foreach (IWatchable<ITopology2Source> s in aItem.Sources)
            {
                s.RemoveWatcher(this);
                iSource2Lookup.Remove(s.Id);
            }

            Topology4Group group = iGroup4Lookup[aItem];
            iGroup4Lookup.Remove(aItem);

            RemoveGroup(group);

            aItem.Name.RemoveWatcher(this);
            aItem.Standby.RemoveWatcher(this);

            EvaluateSources();
        }

        public void ItemOpen(string aId, string aValue)
        {
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            ITopology2Group group2;
            if (iGroup2Lookup.TryGetValue(aId, out group2))
            {
                Topology4Group group4;
                if (iGroup4Lookup.TryGetValue(group2, out group4))
                {
                    iThread.Schedule(() =>
                    {
                        RemoveGroup(group4);
                        AddGroup(group4);

                        EvaluateSources();
                    });
                }
            }
        }

        public void ItemClose(string aId, string aValue)
        {
        }

        public void ItemOpen(string aId, bool aValue)
        {
            if (aValue)
            {
                ++iStandbyCount;
                EvaluateStandby();
            }
        }

        public void ItemUpdate(string aId, bool aValue, bool aPrevious)
        {
            if (aValue)
            {
                ++iStandbyCount;
            }
            else
            {
                --iStandbyCount;
            }

            EvaluateStandby();
        }

        public void ItemClose(string aId, bool aValue)
        {
            if(aValue)
            {
                --iStandbyCount;
                EvaluateStandby(iGroups.Count == 1);
            }
        }

        public void ItemOpen(string aId, ITopology2Source aValue)
        {
        }

        public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
        {
            if (aValue.Name != aPrevious.Name ||
                aValue.Visible != aPrevious.Visible || 
                aValue.Index != aPrevious.Index ||
                aValue.Type != aPrevious.Type)
            {
                Topology4Group group;
                if (iSource2Lookup.TryGetValue(aId, out group))
                {
                    // only need to check tree structure if we have more than one group
                    if (iGroups.Count > 1)
                    {
                        RemoveGroup(group);
                        AddGroup(group);
                    }

                    EvaluateSources();
                }
            }
        }

        public void ItemClose(string aId, ITopology2Source aValue)
        {
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
            if (iGroups.Count > 1)
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
            else
            {
                iRoom.Groups.RemoveWatcher(this);
            }
        }

        private void EvaluateStandby()
        {
        }

        private void EvaluateStandby(bool aLastGroup)
        {
            if (!aLastGroup)
            {
                EStandby standby = EStandby.eOff;

                if (iStandbyCount > 0)
                {
                    standby = EStandby.eMixed;

                    if (iStandbyCount == iGroups.Count)
                    {
                        standby = EStandby.eOn;
                    }
                }

                if (standby != iStandby)
                {
                    iStandby = standby;
                    iWatchableStandby.Update(standby);
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

            if (iSources.Count() == sources.Count)
            {
                int count = sources.Count;
                for (int i = 0; i < count; ++i)
                {
                    ITopology4Source oldSource = iSources.ElementAt(i);
                    ITopology4Source newSource = sources[i];
                    if (oldSource.Name != newSource.Name ||
                        oldSource.Visible != newSource.Visible ||
                        oldSource.Index != newSource.Index ||       // linn products cannot change a source's index
                        oldSource.Type != newSource.Type)           // linn products cannot change a source's type
                    {
                        iSources = sources;
                        iWatchableSources.Update(sources);

                        return;
                    }
                }

                // old and new source lists are identical - do nothing
                return;
            }

            iSources = sources;
            iWatchableSources.Update(sources);
        }

        private IWatchableThread iThread;
        private ITopology3Room iRoom;

        private string iName;
        private uint iStandbyCount;
        private EStandby iStandby;
        private Watchable<EStandby> iWatchableStandby;
        private IEnumerable<ITopology4Source> iSources;
        private Watchable<IEnumerable<ITopology4Source>> iWatchableSources;

        private ITopology4Group iCurrent;
        private List<ITopology4Group> iGroups;
        private List<ITopology4Group> iRoots;
        private Dictionary<string, ITopology2Group> iGroup2Lookup;
        private Dictionary<string, Topology4Group> iSource2Lookup;
        private Dictionary<ITopology2Group, Topology4Group> iGroup4Lookup;
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
            Topology4Room room = iRoomLookup[aItem];

            iRooms.Remove(room);
            iRoomLookup.Remove(aItem);

            iThread.Schedule(() =>
            {
                room.Dispose();
            });
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private ITopology3 iTopology3;

        private WatchableTopology4RoomUnordered iRooms;
        private Dictionary<ITopology3Room, Topology4Room> iRoomLookup;
    }
}
