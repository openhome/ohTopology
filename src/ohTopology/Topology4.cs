using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    class Topology4Source : ITopology4Source
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

    class Topology4Group : ITopology4Root, IWatcher<uint>, IWatcher<string>, IDisposable
    {
        public Topology4Group(IWatchableThread aThread, string aName, ITopology2Group aGroup, IEnumerable<ITopology2Source> aSources)
        {
            iDisposed = false;

            iThread = aThread;
            iName = aName;
            iGroup = aGroup;

            iChildren = new List<Topology4Group>();

            iSources = new List<Topology4Source>();
            iVisibleSources = new List<ITopology4Source>();
            iSenders = new Watchable<IEnumerable<ITopology4Group>>(iThread, "senders", new List<ITopology4Group>());

            foreach (ITopology2Source s in aSources)
            {
                Topology4Source source = new Topology4Source(this, s);
                iSources.Add(source);
            }

            iGroup.SourceIndex.AddWatcher(this);

            if (iGroup.Attributes.Contains("Sender"))
            {
                iGroup.Device.Create<IProxySender>((sender) =>
                {
                    iThread.Schedule(() =>
                    {
                        if (!iDisposed)
                        {
                            iSender = sender;

                            iSender.Status.AddWatcher(this);
                        }
                        else
                        {
                            sender.Dispose();
                        }
                    });
                });
            }
        }

        public void Dispose()
        {
            iGroup.SourceIndex.RemoveWatcher(this);
            iGroup = null;
   
            if (iSender != null)
            {
                iSender.Status.RemoveWatcher(this);
                iSender.Dispose();
                iSender = null;
            }

            iDisposed = true;
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

        private bool iDisposed;

        private IWatchableThread iThread;
        private ITopology2Group iGroup;

        private string iName;

        private uint iParentSourceIndex;
        private Topology4Group iParent;
        private List<Topology4Group> iChildren;

        private IProxySender iSender;
        private bool iHasSender;
        private Watchable<IEnumerable<ITopology4Group>> iSenders;

        private uint iSourceIndex;
        private Watchable<ITopology4Source> iWatchableSource;

        private List<Topology4Source> iSources;
        private List<ITopology4Source> iVisibleSources;
    }

    class Topology4GroupWatcher : IWatcher<string>, IWatcher<ITopology2Source>, IDisposable
    {
        private Topology4Room iRoom;
        private ITopology2Group iGroup;
        private string iName;
        private List<ITopology2Source> iSources;

        public Topology4GroupWatcher(Topology4Room aRoom, ITopology2Group aGroup)
        {
            iRoom = aRoom;
            iGroup = aGroup;

            iSources = new List<ITopology2Source>();

            iGroup.Name.AddWatcher(this);
            foreach (IWatchable<ITopology2Source> s in iGroup.Sources)
            {
                s.AddWatcher(this);
            }
        }

        public void Dispose()
        {
            iGroup.Name.RemoveWatcher(this);
            foreach (IWatchable<ITopology2Source> s in iGroup.Sources)
            {
                s.RemoveWatcher(this);
            }
            iGroup = null;
            iRoom = null;
        }

        public string Name
        {
            get
            {
                return iName;
            }
        }

        public IEnumerable<ITopology2Source> Sources
        {
            get
            {
                return iSources;
            }
        }

        public void ItemOpen(string aId, string aValue)
        {
            iName = aValue;
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iName = aValue;
            iRoom.CreateTree();
        }

        public void ItemClose(string aId, string aValue)
        {
        }

        public void ItemOpen(string aId, ITopology2Source aValue)
        {
            iSources.Add(aValue);
        }

        public void ItemUpdate(string aId, ITopology2Source aValue, ITopology2Source aPrevious)
        {
            for (int i = 0; i < iSources.Count(); ++i)
            {
                if (iSources.ElementAt(i) == aPrevious)
                {
                    iSources[i] = aValue;
                    break;
                }
            }

            iRoom.CreateTree();
        }

        public void ItemClose(string aId, ITopology2Source aValue)
        {
        }
    }
    
    public interface ITopology4Room
    {
        string Name { get; }
        IWatchable<EStandby> Standby { get; }
        IWatchable<IEnumerable<ITopology4Root>> Roots { get; }
        IWatchable<IEnumerable<ITopology4Source>> Sources { get; }

        void SetStandby(bool aValue);
    }

    class Topology4Room : ITopology4Room, IUnorderedWatcher<ITopology2Group>, IWatcher<bool>, IDisposable
    {
        public Topology4Room(IWatchableThread aThread, ITopology3Room aRoom)
        {
            iThread = aThread;
            iRoom = aRoom;

            iName = iRoom.Name;
            iStandbyCount = 0;
            iStandby = EStandby.eOn;

            iWatchableStandby = new Watchable<EStandby>(iThread, "standby", EStandby.eOn);
            iWatchableRoots = new Watchable<IEnumerable<ITopology4Root>>(iThread, "roots", new List<ITopology4Root>());
            iWatchableSources = new Watchable<IEnumerable<ITopology4Source>>(iThread, "sources", new List<ITopology4Source>());

            iGroupLookup = new Dictionary<ITopology2Group, Topology4GroupWatcher>();
            iGroup4s = new List<Topology4Group>();
            iRoots = new List<Topology4Group>();

            iRoom.Groups.AddWatcher(this);
        }

        public void Dispose()
        {
            iRoom.Groups.RemoveWatcher(this);
            iRoom = null;
   
            foreach (var kvp in iGroupLookup)
            {
                kvp.Key.Standby.RemoveWatcher(this);
                kvp.Value.Dispose();
            }
            iGroupLookup = null;

            foreach (Topology4Group g in iGroup4s)
            {
                g.Dispose();
            }
            iGroup4s.Clear();
            iGroup4s = null;

            iWatchableStandby.Dispose();
            iWatchableStandby = null;

            iWatchableRoots.Dispose();
            iWatchableRoots = null;
            
            iWatchableSources.Dispose();
            iWatchableSources = null;

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

        public void UnorderedAdd(ITopology2Group aItem)
        {
            iGroupLookup.Add(aItem, new Topology4GroupWatcher(this, aItem));
            aItem.Standby.AddWatcher(this);
            CreateTree();
        }

        public void UnorderedRemove(ITopology2Group aItem)
        {
            iGroupLookup[aItem].Dispose();
            iGroupLookup.Remove(aItem);
            aItem.Standby.RemoveWatcher(this);

            if (iGroupLookup.Count > 0)
            {
                CreateTree();
            }
        }

        internal void CreateTree()
        {
            List<Topology4Group> oldGroups = new List<Topology4Group>(iGroup4s);

            iGroup4s.Clear();
            iRoots.Clear();

            foreach (var kvp in iGroupLookup)
            {
                InsertIntoTree(new Topology4Group(iThread, kvp.Value.Name, kvp.Key, kvp.Value.Sources));
            }

            List<ITopology4Root> roots = new List<ITopology4Root>();
            List<ITopology4Source> sources = new List<ITopology4Source>();
            foreach (Topology4Group g in iRoots)
            {
                g.EvaluateSources();
                g.EvaluateSenders();
                sources.AddRange(g.Sources);
                roots.Add(g);
            }

            iWatchableRoots.Update(roots);
            iWatchableSources.Update(sources);

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
                EvaluateStandby(iGroupLookup.Count == 0);
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

                    if (iStandbyCount == iGroupLookup.Count)
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

        private Dictionary<ITopology2Group, Topology4GroupWatcher> iGroupLookup;
        private List<Topology4Group> iGroup4s;
        private List<Topology4Group> iRoots;
    }
    
    public interface ITopology4
    {
        IWatchableUnordered<ITopology4Room> Rooms { get; }
        IWatchableThread WatchableThread { get; }
    }

    public class Topology4 : ITopology4, IUnorderedWatcher<ITopology3Room>, IDisposable
    {
        public Topology4(ITopology3 aTopology3)
        {
            iDisposed = false;
            iThread = aTopology3.WatchableThread;
            iTopology3 = aTopology3;

            iRooms = new WatchableUnordered<ITopology4Room>(iThread);

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
                    r.Dispose();
                }
            });
            iTopology3 = null;
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

        public IWatchableThread WatchableThread
        {
            get
            {
                return iThread;
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