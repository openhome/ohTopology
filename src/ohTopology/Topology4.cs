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
        string Group { get; }
        string Type { get; }
        bool Visible { get; }

        IEnumerable<IWatchableDevice> VolumeDevices { get; }
        IWatchableDevice Device { get; }
        bool HasInfo { get; }
        bool HasTime { get; }

        void Select();
    }

    public class Topology4SourceComparer
    {
        public static bool Equals(ITopology4Source aSource1, ITopology4Source aSource2)
        {
            return (aSource1.Index == aSource2.Index
                 && aSource1.Name == aSource2.Name
                 && aSource1.Group == aSource2.Group
                 && aSource1.Type == aSource2.Type
                 && aSource1.Visible == aSource2.Visible
                 && aSource1.VolumeDevices == aSource2.VolumeDevices
                 && aSource1.Device == aSource2.Device
                 && aSource1.HasInfo == aSource2.HasInfo
                 && aSource1.HasTime == aSource2.HasTime);
        }
    }

    public class Topology4Source : ITopology4Source
    {
        public Topology4Source(Topology4Group aGroup, ITopology2Source aSource, IEnumerable<IWatchableDevice> aVolumeDevices, IWatchableDevice aDevice, bool aHasInfo, bool aHasTime)
        {
            iGroup = aGroup;

            iGroupName = iGroup.Name;
            iIndex = aSource.Index;
            iName = aSource.Name;
            iType = aSource.Type;
            iVisible = aSource.Visible;

            iVolumeDevices = aVolumeDevices;
            iDevice = aDevice;
            iHasInfo = aHasInfo;
            iHasTime = aHasTime;
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

        public string Group
        {
            get
            {
                return iGroupName;
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

        public IEnumerable<IWatchableDevice> VolumeDevices
        {
            get { return iVolumeDevices; }
        }

        public IWatchableDevice Device
        {
            get { return iDevice; }
        }

        public bool HasInfo
        {
            get { return iHasInfo; }
        }

        public bool HasTime
        {
            get { return iHasTime; }
        }

        public void Select()
        {
            iGroup.SetSourceIndex(iIndex);
        }

        private Topology4Group iGroup;

        private string iGroupName;
        private uint iIndex;
        private string iName;
        private string iType;
        private bool iVisible;

        private IEnumerable<IWatchableDevice> iVolumeDevices;
        private IWatchableDevice iDevice;
        private bool iHasInfo;
        private bool iHasTime;
    }

    public interface ITopology4Group
    {
        string Name { get; }

        IWatchable<ITopology4Source> Source { get; }
        IEnumerable<ITopology4Source> Sources { get; }
    }

    internal interface IGroup4WatcherHandler
    {
        void SourceChanged(Topology4Group aGroup, ITopology4Source aSource);
    }

    public class Topology4Group : ITopology4Group, IWatcher<ITopology2Source>, IWatcher<uint>, IGroup4WatcherHandler, IDisposable
    {
        private class Group4Watcher : IWatcher<ITopology4Source>, IDisposable
        {
            public Group4Watcher(Topology4Group aGroup, IGroup4WatcherHandler aHandler)
            {
                iGroup = aGroup;
                iHandler = aHandler;

                iGroup.Source.AddWatcher(this);
            }

            public void Dispose()
            {
                iGroup.Source.RemoveWatcher(this);
                iGroup = null;
                iHandler = null;
            }

            public ITopology4Source Source
            {
                get
                {
                    return iSource;
                }
            }
            
            public void ItemOpen(string aId, ITopology4Source aValue)
            {
                iSource = aValue;
                iHandler.SourceChanged(iGroup, aValue);
            }

            public void ItemUpdate (string aId, ITopology4Source aValue, ITopology4Source aPrevious)
            {
                if (iHandler == null) {
                    Console.WriteLine(iHandler);
                }
                iSource = aValue;
                iHandler.SourceChanged(iGroup, aValue);
            }

            public void ItemClose(string aId, ITopology4Source aValue)
            {
            }

            private Topology4Group iGroup;
            private IGroup4WatcherHandler iHandler;

            private ITopology4Source iSource;
        }

        public Topology4Group(IWatchableThread aThread, string aName, ITopology2Group aGroup)
        {
            iThread = aThread;
            iName = aName;
            iGroup = aGroup;

            iSource2s = new List<ITopology2Source>();
            iSource4s = new List<ITopology4Source>();
            iChildren = new List<Topology4Group>();
            iGroup4WatcherLookup = new Dictionary<Topology4Group, Group4Watcher>();

            foreach (IWatchable<ITopology2Source> s in iGroup.Sources)
            {
                s.AddWatcher(this);
            }

            iGroup.SourceIndex.AddWatcher(this);
        }

        public void Dispose()
        {
            // ensure group is not hooked into the tree
            RemoveFromTree();

            foreach (IWatchable<ITopology2Source> s in iGroup.Sources)
            {
                s.RemoveWatcher(this);
            }

            iSource2s.Clear();
            iSource2s = null;
            iSource4s.Clear();
            iSource4s = null;

            foreach (Group4Watcher w in iGroup4WatcherLookup.Values)
            {
                w.Dispose();
            }
            iGroup4WatcherLookup.Clear();
            iGroup4WatcherLookup = null;

            iGroup.SourceIndex.RemoveWatcher(this);

            iGroup = null;
        }

        // ITopology4Group

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
                return iWatchableSource;
            }
        }

        public IEnumerable<ITopology4Source> Sources
        {
            get
            {
                List<ITopology4Source> sources = new List<ITopology4Source>();

                foreach (ITopology4Source s in iSource4s)
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
                                sources.AddRange(g.Sources);
                                expanded = true;
                            }
                        }

                        if (!expanded)
                        {
                            sources.Add(s);
                        }
                    }
                }

                return sources;
            }
        }

        // IWatcher<ITopology2Source>

        public void ItemOpen(string aId, ITopology2Source aValue)
        {
            ITopology4Source source = CreateSource(aValue);
            iSource2s.Add(aValue);
            iSource4s.Add(source);
        }

        public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
        {
            int index = iSource2s.IndexOf(aPrevious);

            ITopology4Source oldSource = iSource4s[index];
            ITopology4Source newSource = CreateSource(aValue);

            iSource2s[index] = aValue;
            iSource4s[index] = newSource;

            if (iGroupSource == oldSource)
            {
                iGroupSource = newSource;
            }
        }

        public void ItemClose(string aId, ITopology2Source aValue)
        {
            int index = iSource2s.IndexOf(aValue);
            iSource2s.RemoveAt(index);
            iSource4s.RemoveAt(index);
        }

        private ITopology4Source CreateSource(ITopology2Source aSource2)
        {
            IWatchableDevice device = iGroup.Device;
            bool hasInfo = iGroup.Attributes.Contains("Info");
            bool hasTime = iGroup.Attributes.Contains("Time") && hasInfo;

            // get list of all volume devices
            List<IWatchableDevice> volDevices = new List<IWatchableDevice>();

            Topology4Group group = this;

            while (group != null)
            {
                IWatchableDevice volDevice = group.iGroup.Attributes.Contains("Volume") ? group.iGroup.Device : null;

                if (volDevice != null)
                {
                    volDevices.Insert(0, volDevice);
                }

                group = group.Parent;
            }

            return new Topology4Source(this, aSource2, volDevices, device, hasInfo, hasTime);
        }

        // IWatcher<uint>

        public void ItemOpen(string aId, uint aValue)
        {
            // set the source for this group
            iGroupSource = iSource4s[(int)aValue];

            // check if the group's source is expanded by a child's group's sources
            iExpandedSource = iGroupSource;

            foreach (Topology4Group g in iChildren)
            {
                if (g.Name == iGroupSource.Name)
                {
                    Group4Watcher watcher = iGroup4WatcherLookup[g];
                    iExpandedSource = watcher.Source;
                    break;
                }
            }

            iWatchableSource = new Watchable<ITopology4Source>(iThread, string.Format("Source({0})", iName), iExpandedSource);
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            // set the source for this group
            iGroupSource = iSource4s[(int)aValue];

            // check if the group's source is expanded by a child's group's sources
            iExpandedSource = iGroupSource;

            foreach (Topology4Group g in iChildren)
            {
                if (g.Name == iGroupSource.Name)
                {
                    Group4Watcher watcher = iGroup4WatcherLookup[g];
                    iExpandedSource = watcher.Source;
                    break;
                }
            }

            iWatchableSource.Update(iExpandedSource);
        }

        public void ItemClose(string aId, uint aValue)
        {
            iGroupSource = null;
            iExpandedSource = null;

            iWatchableSource.Dispose();
            iWatchableSource = null;
        }

        // IGroup4WatcherHandler

        public void SourceChanged(Topology4Group aGroup, ITopology4Source aSource)
        {
            // the source of a child group has changed - check if this group is attached to
            // the current source for this group
            if (aGroup.Name == iGroupSource.Name)
            {
                if (aSource != iExpandedSource)
                {
                    iExpandedSource = aSource;
                    iWatchableSource.Update(aSource);
                }
            }
        }

        // Hierarchy methods

        public Topology4Group Parent
        {
            get
            {
                return iParent;
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

        public void RemoveFromTree()
        {
            if (iParent != null)
            {
                iParent.RemoveChild(this);
            }

            ITopology4Group[] children = iChildren.ToArray();
            foreach (Topology4Group g in children)
            {
                RemoveChild(g);
            }
        }

        public bool AddIfIsChild(Topology4Group aGroup)
        {
            foreach (ITopology4Source s in iSource4s)
            {
                if (aGroup.Name == s.Name)
                {
                    aGroup.SetParent(this, s.Index);
                    iChildren.Add(aGroup);

                    Group4Watcher watcher = new Group4Watcher(aGroup, this);
                    iGroup4WatcherLookup.Add(aGroup, watcher);

                    return true;
                }
            }

            return false;
        }

        private void RemoveChild(Topology4Group aGroup)
        {
            iChildren.Remove(aGroup);
            aGroup.SetParent(null);

            Group4Watcher watcher = iGroup4WatcherLookup[aGroup];
            iGroup4WatcherLookup.Remove(aGroup);

            iThread.Schedule(() =>
            {
                watcher.Dispose();
            });
        }

        private void SetParent(Topology4Group aGroup)
        {
            iParent = aGroup;

            // parent has changed - volume controls for sources potentially change as well
            for (int i = 0; i < iSource2s.Count; i++)
            {
                iSource4s[i] = CreateSource(iSource2s[i]);
            }
        }

        private void SetParent(Topology4Group aGroup, uint aIndex)
        {
            SetParent(aGroup);
            iParentSourceIndex = aIndex;
        }

        private IWatchableThread iThread;
        private ITopology2Group iGroup;
        private string iName;

        private List<ITopology2Source> iSource2s;
        private List<ITopology4Source> iSource4s;
        private ITopology4Source iGroupSource;
        private ITopology4Source iExpandedSource;

        private Watchable<ITopology4Source> iWatchableSource;

        private uint iParentSourceIndex;
        private Topology4Group iParent;
        private List<Topology4Group> iChildren;

        private Dictionary<Topology4Group, Group4Watcher> iGroup4WatcherLookup;
    }

    public interface ITopology4Room
    {
        string Name { get; }
        IWatchable<EStandby> Standby { get; }
        IWatchable<IEnumerable<ITopology4Group>> Roots { get; }
        IWatchable<IEnumerable<ITopology4Source>> Sources { get; }

        void SetStandby(bool aValue);
    }

    internal interface IGroup2WatcherHandler
    {
        void GroupNameOpened(Topology4Group aValue);
        void GroupNameChanged(Topology4Group aValue, Topology4Group aPrevious);
        void GroupNameClosed(Topology4Group aValue);
        void GroupSourcesChanged(Topology4Group aValue);
    }

    public class Topology4Room : ITopology4Room, IUnorderedWatcher<ITopology2Group>, IWatcher<bool>, IGroup2WatcherHandler, IDisposable
    {
        private class Group2Watcher : IWatcher<string>, IWatcher<ITopology2Source>, IDisposable
        {
            public Group2Watcher(IWatchableThread aThread, ITopology2Group aGroup, IGroup2WatcherHandler aHandler)
            {
                iThread = aThread;
                iGroup2 = aGroup;
                iHandler = aHandler;

                iGroup2.Name.AddWatcher(this);
                foreach (IWatchable<ITopology2Source> s in iGroup2.Sources)
                {
                    s.AddWatcher(this);
                }
            }

            public void Dispose()
            {
                iGroup2.Name.RemoveWatcher(this);
                foreach (IWatchable<ITopology2Source> s in iGroup2.Sources)
                {
                    s.RemoveWatcher(this);
                }
            }

            public void ItemOpen(string aId, string aValue)
            {
                iGroup4 = new Topology4Group(iThread, aValue, iGroup2);

                iHandler.GroupNameOpened(iGroup4);
            }

            public void ItemUpdate(string aId, string aValue, string aPrevious)
            {
                Topology4Group previous = iGroup4;
                Topology4Group value = new Topology4Group(iThread, aValue, iGroup2);

                iHandler.GroupNameChanged(value, previous);

                iGroup4 = value;

                iThread.Schedule(() =>
                {
                    previous.Dispose();
                    previous = null;
                });
            }

            public void ItemClose(string aId, string aValue)
            {
                iHandler.GroupNameClosed(iGroup4);

                iThread.Schedule(() =>
                {
                    iGroup4.Dispose();
                    iGroup4 = null;
                });
            }

            public void ItemOpen(string aId, ITopology2Source aValue)
            {
            }

            public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
            {
                iHandler.GroupSourcesChanged(iGroup4);
            }

            public void ItemClose(string aId, ITopology2Source aValue)
            {
            }

            private IWatchableThread iThread;
            private ITopology2Group iGroup2;
            private Topology4Group iGroup4;
            private IGroup2WatcherHandler iHandler;
        }

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
            iWatchableRoots = new Watchable<IEnumerable<ITopology4Group>>(iThread, "Topology4Roots", new List<ITopology4Group>());

            iSources = new List<ITopology4Source>();
            iGroups = new List<ITopology4Group>();
            iRoots = new List<ITopology4Group>();
            iGroup2WatcherLookup = new Dictionary<ITopology2Group, Group2Watcher>();

            iRoom.Groups.AddWatcher(this);
        }

        public void Dispose()
        {
            foreach (Group2Watcher w in iGroup2WatcherLookup.Values)
            {
                w.Dispose();
            }
            iGroup2WatcherLookup.Clear();
            iGroup2WatcherLookup = null;

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

            iRoots.Clear();
            iRoots = null;

            iWatchableRoots.Dispose();
            iWatchableStandby.Dispose();
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

        public IWatchable<IEnumerable<ITopology4Group>> Roots
        {
            get
            {
                return iWatchableRoots;
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
            // add watchers after group has been inserted into the tree
            Group2Watcher watcher = new Group2Watcher(iThread, aItem, this);
            iGroup2WatcherLookup.Add(aItem, watcher);

            aItem.Standby.AddWatcher(this);
        }

        public void UnorderedRemove(ITopology2Group aItem)
        {
            Group2Watcher watcher = iGroup2WatcherLookup[aItem];
            iGroup2WatcherLookup.Remove(aItem);
            watcher.Dispose();

            aItem.Standby.RemoveWatcher(this);
        }

        public void GroupNameOpened(Topology4Group aValue)
        {
            bool changed = AddGroup(aValue);

            EvaluateSources();

            if (changed)
            {
                iWatchableRoots.Update(new List<ITopology4Group>(iRoots));
            }
        }

        public void GroupNameChanged(Topology4Group aValue, Topology4Group aPrevious)
        {
            bool changed = false;

            changed = RemoveGroup(aPrevious);
            changed |= AddGroup(aValue);

            EvaluateSources();

            if (changed)
            {
                iWatchableRoots.Update(new List<ITopology4Group>(iRoots));
            }
        }

        public void GroupNameClosed(Topology4Group aValue)
        {
            bool changed = RemoveGroup(aValue);

            EvaluateSources();

            if (changed)
            {
                iWatchableRoots.Update(new List<ITopology4Group>(iRoots));
            }
        }

        public void GroupSourcesChanged(Topology4Group aValue)
        {
            bool changed = false;

            changed = RemoveGroup(aValue);
            changed |= AddGroup(aValue);

            EvaluateSources();

            if (changed)
            {
                iWatchableRoots.Update(new List<ITopology4Group>(iRoots));
            }
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
                EvaluateStandby(iGroups.Count == 0);
            }
        }

        private bool AddGroup(Topology4Group aGroup)
        {
            // if group is the first group found
            if (iGroups.Count == 0)
            {
                iGroups.Add(aGroup);
                iRoots.Add(aGroup);

                return true;
            }

            // check for an existing parent
            foreach (Topology4Group g in iGroups)
            {
                if (g.AddIfIsChild(aGroup))
                {
                    iGroups.Add(aGroup);

                    return false;
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

            return true;
        }

        private bool RemoveGroup(Topology4Group aGroup)
        {
            bool changed = false;

            iGroups.Remove(aGroup);

            if (iRoots.Contains(aGroup))
            {
                iRoots.Remove(aGroup);
                changed = true;
            }

            aGroup.RemoveFromTree();

            // check for orphaned groups
            foreach (Topology4Group g in iGroups)
            {
                // if group has no parent and it is not a root group - promote it to a root
                if (g.Parent == null && !iRoots.Contains(g))
                {
                    iRoots.Add(g);
                    changed = true;
                }
            }

            return changed;
        }

        private void EvaluateStandby()
        {
            EvaluateStandby(false);
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
                sources.AddRange(g.Sources);
            }

            if (iSources.Count() == sources.Count)
            {
                int count = sources.Count;
                for (int i = 0; i < count; ++i)
                {
                    ITopology4Source oldSource = iSources.ElementAt(i);
                    ITopology4Source newSource = sources[i];

                    if (!Topology4SourceComparer.Equals(oldSource, newSource))
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
        private Watchable<IEnumerable<ITopology4Group>> iWatchableRoots;
        private Watchable<IEnumerable<ITopology4Source>> iWatchableSources;

        private List<ITopology4Group> iGroups;
        private List<ITopology4Group> iRoots;
        private Dictionary<ITopology2Group, Group2Watcher> iGroup2WatcherLookup;
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

            iTopology3.Rooms.RemoveWatcher(this);
            iTopology3 = null;

            foreach (Topology4Room r in iRoomLookup.Values)
            {
                r.Dispose();
            }
            iRoomLookup.Clear();
            iRoomLookup = null;

            iRooms.Dispose();
            iRooms = null;
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
