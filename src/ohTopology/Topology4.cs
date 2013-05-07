using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ITopology4Source
    {
        string Group { get; }

        uint Index { get; }
        string Name { get; }
        string Type { get; }
        bool Visible { get; }

        IEnumerable<ITopology4Group> Volumes { get; }
        IWatchableDevice Device { get; }
        bool HasInfo { get; }
        bool HasTime { get; }

        void Select();
    }

    public class Topology4SourceComparer
    {
        public static bool Equals(ITopology4Source aSource1, ITopology4Source aSource2)
        {
            bool volume = false;
            if (aSource1.Volumes.Count() == aSource2.Volumes.Count())
            {
                volume = true;
                int count = aSource1.Volumes.Count();
                for (int i = 0; i < count; ++i)
                {
                    if (aSource1.Volumes.ElementAt(i) != aSource2.Volumes.ElementAt(i))
                    {
                        volume = false;
                        break;
                    }
                }
            }

            return (aSource1.Index == aSource2.Index
                 && aSource1.Name == aSource2.Name
                 && aSource1.Group == aSource2.Group
                 && aSource1.Type == aSource2.Type
                 && aSource1.Visible == aSource2.Visible
                 && volume
                 && aSource1.Device == aSource2.Device
                 && aSource1.HasInfo == aSource2.HasInfo
                 && aSource1.HasTime == aSource2.HasTime);
        }
    }

    public class Topology4Source : ITopology4Source
    {
        public Topology4Source(Topology4Group aGroup, ITopology2Source aSource)
        {
            iGroup = aGroup;

            iIndex = aSource.Index;
            iName = aSource.Name;
            iType = aSource.Type;
            iVisible = aSource.Visible;
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
                return iGroup.Name;
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

        public IEnumerable<ITopology4Group> Volumes
        {
            get { return iVolumes; }
            set { iVolumes = value; }
        }

        public IWatchableDevice Device
        {
            get { return iDevice; }
            set { iDevice = value; }
        }

        public bool HasInfo
        {
            get { return iHasInfo; }
            set { iHasInfo = value; }
        }

        public bool HasTime
        {
            get { return iHasTime; }
            set { iHasTime = value; }
        }

        public void Select()
        {
            iGroup.SetSourceIndex(iIndex);
        }

        private Topology4Group iGroup;

        private uint iIndex;
        private string iName;
        private string iType;
        private bool iVisible;

        private IEnumerable<ITopology4Group> iVolumes;
        private IWatchableDevice iDevice;
        private bool iHasInfo;
        private bool iHasTime;
    }

    public interface ITopology4Group
    {
        string Name { get; }
        IWatchableDevice Device { get; }
    }
    
    public interface ITopology4Root : ITopology4Group
    {
        IWatchable<ITopology4Source> Source { get; }
        IEnumerable<ITopology4Source> Sources { get; }
        IWatchable<IEnumerable<ITopology4Group>> Senders { get; }
    }

    public class Topology4Root : ITopology4Root
    {
        public Topology4Root(Topology4Group aGroup)
        {
            iGroup = aGroup;
        }

        public string Name
        {
            get
            {
                return iGroup.Name;
            }
        }

        public IWatchableDevice Device
        {
            get
            {
                return iGroup.Device;
            }
        }

        public IWatchable<ITopology4Source> Source
        {
            get
            {
                return iGroup.Source;
            }
        }

        public IEnumerable<ITopology4Source> Sources
        {
            get
            {
                return iGroup.Sources;
            }
        }

        public IWatchable<IEnumerable<ITopology4Group>> Senders
        {
            get
            {
                return iGroup.Senders;
            }
        }

        private Topology4Group iGroup;
    }

    public class Topology4Group : ITopology4Root, IWatcher<uint>, IWatcher<string>, ITopologyObject, IDisposable
    {
        public Topology4Group(IWatchableThread aThread, string aName, ITopology3Group aGroup, IEnumerable<ITopology2Source> aSources)
        {
            iThread = aThread;
            iName = aName;
            iGroup = aGroup;

            iLock = new object();
            iDetached = false;

            iChildren = new List<Topology4Group>();

            iSources = new List<Topology4Source>();
            iVisibleSources = new List<ITopology4Source>();
            iSenders = new Watchable<IEnumerable<ITopology4Group>>(iThread, string.Format("Senders({0})", iName), new List<ITopology4Group>());

            foreach (ITopology2Source s in aSources)
            {
                Topology4Source source = new Topology4Source(this, s);
                iSources.Add(source);
            }

            iGroup.SourceIndex.AddWatcher(this);

            if (iGroup.Attributes.Contains("Sender"))
            {
                iGroup.Device.Create<ServiceSender>((IWatchableDevice device, ServiceSender sender) =>
                {
                    lock (iLock)
                    {
                        if (!iDetached)
                        {
                            iSender = sender;

                            iSender.Status.AddWatcher(this);
                        }
                        else
                        {
                            sender.Dispose();
                        }
                    }
                });
            }
        }

        public void Detach()
        {
            iGroup.SourceIndex.RemoveWatcher(this);

            lock (iLock)
            {
                if (iSender != null)
                {
                    iSender.Status.RemoveWatcher(this);
                }

                iDetached = true;
            }
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iSender != null)
                {
                    iSender.Dispose();
                    iSender = null;
                }

                iGroup = null;
            }
        }

        public string Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchableDevice Device
        {
            get
            {
                return iGroup.Device;
            }
        }

        public IWatchable<ITopology4Source> Source
        {
            get
            {
                return iWatchableSource;
            }
        }

        public void EvaluateSources()
        {
            bool hasInfo = iGroup.Attributes.Contains("Info");
            bool hasTime = iGroup.Attributes.Contains("Time") && hasInfo;

            // get list of all volume groups
            List<ITopology4Group> volumes = new List<ITopology4Group>();

            Topology4Group group = this;

            while (group != null)
            {
                if(group.iGroup.Attributes.Contains("Volume"))
                {
                    volumes.Insert(0, group);
                }

                group = group.Parent;
            }

            foreach (Topology4Source s in iSources)
            {
                s.Volumes = volumes;
                s.Device = iGroup.Device;
                s.HasInfo = hasInfo;
                s.HasTime = hasTime;
            }

            foreach (Topology4Group g in iChildren)
            {
                g.EvaluateSources();
            }

            for (int i = 0; i < iSources.Count; ++i)
            {
                Topology4Source s = iSources[i];
                
                bool expanded = false;
                foreach (Topology4Group g in iChildren)
                {
                    // if group is connected to source expand source to group sources
                    if (s.Name == g.Name)
                    {
                        iVisibleSources.AddRange(g.Sources);
                        expanded = true;
                    }
                }

                if (!expanded)
                {
                    // only include if source is visible
                    if (s.Visible)
                    {
                        iVisibleSources.Add(iSources[i]);
                    }
                }
            }

            ITopology4Source source = EvaluateSource();
            iWatchableSource.Update(source);
        }

        public IEnumerable<ITopology4Source> Sources
        {
            get
            {
                return iVisibleSources;
            }
        }

        public void EvaluateSenders()
        {
            foreach (Topology4Group g in iChildren)
            {
                g.EvaluateSenders();
            }

            List<ITopology4Group> senderDevices = new List<ITopology4Group>();

            foreach (Topology4Group g in iChildren)
            {
                senderDevices.AddRange(g.Senders.Value);
            }

            if (iHasSender)
            {
                senderDevices.Insert(0, this);
            }

            iSenders.Update(senderDevices);
        }

        public IWatchable<IEnumerable<ITopology4Group>> Senders
        {
            get
            {
                return iSenders;
            }
        }

        public Topology4Group Parent
        {
            get
            {
                return iParent;
            }
        }

        public bool AddIfIsChild(Topology4Group aGroup)
        {
            foreach (ITopology4Source s in iSources)
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

        private void SetParent(Topology4Group aGroup)
        {
            iParent = aGroup;
        }

        private void SetParent(Topology4Group aGroup, uint aIndex)
        {
            SetParent(aGroup);
            iParentSourceIndex = aIndex;
        }

        public void ItemOpen(string aId, uint aValue)
        {
            iSourceIndex = aValue;
            iWatchableSource = new Watchable<ITopology4Source>(iThread, string.Format("Source({0})", iName), EvaluateSource());
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            iSourceIndex = aValue;
            EvaluateSourceFromChild();
        }

        public void ItemClose(string aId, uint aValue)
        {
            iThread.Schedule(() =>
            {
                iWatchableSource.Dispose();
                iWatchableSource = null;
            });
        }

        private ITopology4Source EvaluateSource()
        {
            // set the source for this group
            Topology4Source source = iSources[(int)iSourceIndex];

            // check if the group's source is expanded by a child's group's sources
            foreach (Topology4Group g in iChildren)
            {
                if (g.Name == source.Name)
                {
                    return g.EvaluateSource();
                }
            }

            return source;
        }

        private void EvaluateSourceFromChild()
        {
            if (iParent != null)
            {
                iParent.EvaluateSourceFromChild();
            }


            ITopology4Source source = EvaluateSource();
            iWatchableSource.Update(source);
        }

        public void ItemOpen(string aId, string aValue)
        {
            iHasSender = (aValue == "Enabled");
            EvaluateSendersFromChild();
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iHasSender = (aValue == "Enabled");
            EvaluateSendersFromChild();
        }

        public void ItemClose(string aId, string aValue)
        {
        }

        private void EvaluateSendersFromChild()
        {
            if (iParent != null)
            {
                iParent.EvaluateSendersFromChild();
            }
            else
            {
                EvaluateSenders();
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

        private object iLock;
        private bool iDetached;
        private IWatchableThread iThread;
        private ITopology3Group iGroup;

        private string iName;

        private uint iParentSourceIndex;
        private Topology4Group iParent;
        private List<Topology4Group> iChildren;

        private ServiceSender iSender;
        private bool iHasSender;
        private Watchable<IEnumerable<ITopology4Group>> iSenders;

        private uint iSourceIndex;
        private Watchable<ITopology4Source> iWatchableSource;

        private List<Topology4Source> iSources;
        private List<ITopology4Source> iVisibleSources;
    }
    
    public interface ITopology4Room
    {
        string Name { get; }
        IWatchable<EStandby> Standby { get; }
        IWatchable<IEnumerable<ITopology4Root>> Roots { get; }
        IWatchable<IEnumerable<ITopology4Source>> Sources { get; }

        void SetStandby(bool aValue);
    }

    public class Topology4Room : ITopology4Room, ITopologyObject, IUnorderedWatcher<ITopology3Group>, IWatcher<bool>, IWatcher<string>, IWatcher<ITopology2Source>, IDisposable
    {
        public Topology4Room(IWatchableThread aThread, ITopology3Room aRoom)
        {
            iThread = aThread;
            iRoom = aRoom;

            iName = iRoom.Name;
            iStandbyCount = 0;
            iStandby = EStandby.eOn;

            iWatchableStandby = new Watchable<EStandby>(iThread, string.Format("Standby({0})", iName), EStandby.eOn);
            iWatchableRoots = new Watchable<IEnumerable<ITopology4Root>>(iThread, "Topology4Roots", new List<ITopology4Root>());
            iWatchableSources = new Watchable<IEnumerable<ITopology4Source>>(iThread, "Topology4Sources", new List<ITopology4Source>());

            iGroup3NameLookup = new Dictionary<ITopology3Group, string>();
            iGroup3SourcesLookup = new Dictionary<ITopology3Group, List<ITopology2Source>>();
            iGroups = new List<ITopology3Group>();
            iGroup4s = new List<Topology4Group>();
            iRoots = new List<Topology4Group>();

            iRoom.Groups.AddWatcher(this);
        }

        public void Detach()
        {
            foreach(ITopology3Group g in iGroups)
            {
                g.Name.RemoveWatcher(this);
                g.Standby.RemoveWatcher(this);

                foreach (IWatchable<ITopology2Source> s in g.Sources)
                {
                    s.RemoveWatcher(this);
                }
            }

            foreach (Topology4Group g in iGroup4s)
            {
                g.Detach();
            }

            iRoom.Groups.RemoveWatcher(this);
            iRoom = null;
        }

        public void Dispose()
        {
            iWatchableStandby.Dispose();
            iWatchableStandby = null;

            iWatchableRoots.Dispose();
            iWatchableRoots = null;
            
            iWatchableSources.Dispose();
            iWatchableSources = null;

            iGroup3NameLookup.Clear();
            iGroup3NameLookup = null;

            iGroup3SourcesLookup.Clear();
            iGroup3SourcesLookup = null;

            iGroups.Clear();
            iGroups = null;

            foreach (Topology4Group g in iGroup4s)
            {
                g.Dispose();
            }
            iGroup4s.Clear();
            iGroup4s = null;

            iRoots = null;
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

        public IWatchable<IEnumerable<ITopology4Root>> Roots
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
            iRoom.SetStandby(aValue);
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

        public void UnorderedAdd(ITopology3Group aItem)
        {
            iGroups.Add(aItem);

            aItem.Name.AddWatcher(this);
            aItem.Standby.AddWatcher(this);

            foreach (IWatchable<ITopology2Source> s in aItem.Sources)
            {
                s.AddWatcher(this);
            }

            CreateTree();
        }

        public void UnorderedRemove(ITopology3Group aItem)
        {
            iGroups.Remove(aItem);

            if (iGroups.Count > 0)
            {
                CreateTree();
            }

            aItem.Name.RemoveWatcher(this);
            aItem.Standby.RemoveWatcher(this);

            foreach (IWatchable<ITopology2Source> s in aItem.Sources)
            {
                s.RemoveWatcher(this);
            }
        }

        public void ItemOpen(string aId, string aValue)
        {
            foreach (ITopology3Group g in iGroups)
            {
                if (g.Name.Id == aId)
                {
                    iGroup3NameLookup.Add(g, aValue);
                    iGroup3SourcesLookup.Add(g, new List<ITopology2Source>());
                    return;
                }
            }
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            foreach (ITopology3Group g in iGroups)
            {
                if (g.Name.Id == aId)
                {
                    iGroup3NameLookup[g] = aValue;

                    CreateTree();

                    return;
                }
            }
        }

        public void ItemClose(string aId, string aValue)
        {
            foreach (ITopology3Group g in iGroups)
            {
                if (g.Name.Id == aId)
                {
                    iGroup3NameLookup.Remove(g);
                    iGroup3SourcesLookup.Remove(g);
                    return;
                }
            }
        }

        public void ItemOpen(string aId, ITopology2Source aValue)
        {
            foreach (ITopology3Group g in iGroups)
            {
                foreach (IWatchable<ITopology2Source> s in g.Sources)
                {
                    if (s.Id == aId)
                    {
                        iGroup3SourcesLookup[g].Add(aValue);
                        return;
                    }
                }
            }
        }

        public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
        {
            foreach (ITopology3Group g in iGroups)
            {
                for (int i = 0; i < g.Sources.Count(); ++i)
                {
                    if (g.Sources.ElementAt(i).Id == aId)
                    {
                        iGroup3SourcesLookup[g][i] = aValue;

                        CreateTree();

                        return;
                    }
                }
            }
        }

        public void ItemClose(string aId, ITopology2Source aValue)
        {
        }

        private void CreateTree()
        {
            List<Topology4Group> oldGroups = new List<Topology4Group>(iGroup4s);

            iGroup4s.Clear();
            iRoots.Clear();

            foreach (ITopology3Group g in iGroups)
            {
                InsertIntoTree(new Topology4Group(iThread, iGroup3NameLookup[g], g, iGroup3SourcesLookup[g]));
            }

            List<Topology4Root> roots = new List<Topology4Root>();
            List<ITopology4Source> sources = new List<ITopology4Source>();
            foreach (Topology4Group g in iRoots)
            {
                g.EvaluateSources();
                g.EvaluateSenders();
                sources.AddRange(g.Sources);
                roots.Add(new Topology4Root(g));
            }

            iWatchableRoots.Update(roots);
            iWatchableSources.Update(sources);

            foreach (Topology4Group g in oldGroups)
            {
                g.Detach();
            }

            iThread.Schedule(() =>
            {
                foreach (Topology4Group g in oldGroups)
                {
                    g.Dispose();
                }
            });
        }

        private void InsertIntoTree(Topology4Group aGroup)
        {
            // if group is the first group found
            if (iGroup4s.Count == 0)
            {
                iGroup4s.Add(aGroup);
                iRoots.Add(aGroup);
                return;
            }

            // check for an existing parent
            foreach (Topology4Group g in iGroup4s)
            {
                if (g.AddIfIsChild(aGroup))
                {
                    iGroup4s.Add(aGroup);
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

            iGroup4s.Add(aGroup);
            iRoots.Add(aGroup);

            return;
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
            if (aValue)
            {
                --iStandbyCount;
                EvaluateStandby(iGroups.Count == 0);
            }
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

        private IWatchableThread iThread;
        private ITopology3Room iRoom;

        private string iName;
        private uint iStandbyCount;
        private EStandby iStandby;

        private Watchable<EStandby> iWatchableStandby;
        private Watchable<IEnumerable<ITopology4Root>> iWatchableRoots;
        private Watchable<IEnumerable<ITopology4Source>> iWatchableSources;

        private Dictionary<ITopology3Group, string> iGroup3NameLookup;
        private Dictionary<ITopology3Group, List<ITopology2Source>> iGroup3SourcesLookup;
        private List<ITopology3Group> iGroups;
        private List<Topology4Group> iGroup4s;
        private List<Topology4Group> iRoots;
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

            iRooms = new WatchableUnordered<ITopology4Room>(aThread);

            iRoomLookup = new Dictionary<ITopology3Room, Topology4Room>();

            iThread.Schedule(() =>
            {
                iTopology3.Rooms.AddWatcher(this);
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
                iTopology3.Rooms.RemoveWatcher(this);

                foreach (Topology4Room r in iRoomLookup.Values)
                {
                    r.Detach();
                }
            });
            iTopology3 = null;

            foreach (Topology4Room r in iRoomLookup.Values)
            {
                r.Dispose();
            }
            iRoomLookup.Clear();
            iRoomLookup = null;

            iRooms.Dispose();
            iRooms = null;

            iDisposed = true;
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
            // schedule notification of L4 room removal
            Topology4Room room = iRoomLookup[aItem];
            iRooms.Remove(room);
            iRoomLookup.Remove(aItem);

            // detach L4 room from L3
            room.Detach();

            // schedule disposal of L4 room
            iThread.Schedule(() =>
            {
                room.Dispose();
            });
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private ITopology3 iTopology3;

        private WatchableUnordered<ITopology4Room> iRooms;
        private Dictionary<ITopology3Room, Topology4Room> iRoomLookup;
    }
}