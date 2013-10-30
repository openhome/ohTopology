using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class MediaPresetExternal : IMediaPreset, IWatcher<ITopology4Source>
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly IWatchableThread iThread;
        private readonly uint iIndex;
        private readonly IMediaMetadata iMetadata;
        private readonly Topology4Source iSource;
        private readonly Topology4Group iGroup;
        private readonly Watchable<bool> iBuffering;
        private readonly Watchable<bool> iPlaying;

        public MediaPresetExternal(IWatchableThread aThread, Topology4Group aGroup, uint aIndex, IMediaMetadata aMetadata, Topology4Source aSource)
        {
            iDisposeHandler = new DisposeHandler();

            iThread = aThread;
            iIndex = aIndex;
            iMetadata = aMetadata;
            iSource = aSource;
            iGroup = aGroup;

            iBuffering = new Watchable<bool>(aThread, "Buffering", false);
            iPlaying = new Watchable<bool>(aThread, "Playing", false);
            aThread.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iGroup.Source.AddWatcher(this);
                });
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();
            iThread.Execute(() =>
            {
                iGroup.Source.RemoveWatcher(this);
            });
            iBuffering.Dispose();
            iPlaying.Dispose();
        }

        public uint Index
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iIndex;
                }
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iMetadata;
                }
            }
        }

        public IWatchable<bool> Buffering
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iBuffering;
                }
            }
        }

        public IWatchable<bool> Playing
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iPlaying;
                }
            }
        }

        public void Play()
        {
            using (iDisposeHandler.Lock())
            {
                iSource.Select();
            }
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            iPlaying.Update(aValue == iSource);
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            iPlaying.Update(aValue == iSource);
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            iPlaying.Update(false);
        }
    }

    public interface ITopology4Source
    {
        ITopology4Group Group { get; }

        uint Index { get; }
        string Name { get; }
        string Type { get; }
        bool Visible { get; }

        IMediaPreset CreatePreset();

        IEnumerable<ITopology4Group> Volumes { get; }
        IDevice Device { get; }
        bool HasInfo { get; }
        bool HasTime { get; }
    }

    class Topology4SourceNull : ITopology4Source
    {
        public ITopology4Group Group
        {
            get { throw new NotImplementedException(); }
        }

        public uint Index
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string Type
        {
            get { throw new NotImplementedException(); }
        }

        public bool Visible
        {
            get { throw new NotImplementedException(); }
        }

        public IMediaPreset CreatePreset()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITopology4Group> Volumes
        {
            get { throw new NotImplementedException(); }
        }

        public IDevice Device
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasInfo
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasTime
        {
            get { throw new NotImplementedException(); }
        }
    }

    class Topology4Source : ITopology4Source
    {
        public Topology4Source(INetwork aNetwork, Topology4Group aGroup, ITopology2Source aSource)
        {
            iNetwork = aNetwork;
            iGroup = aGroup;
            iSource = aSource;
        }

        public uint Index
        {
            get
            {
                return iSource.Index;
            }
        }

        public string Name
        {
            get
            {
                return iSource.Name;
            }
        }

        public ITopology4Group Group
        {
            get
            {
                return iGroup;
            }
        }

        public string Type
        {
            get
            {
                return iSource.Type;
            }
        }

        public bool Visible
        {
            get
            {
                return iSource.Visible;
            }
        }

        public IMediaPreset CreatePreset()
        {
            MediaMetadata metadata = new MediaMetadata();
            metadata.Add(iNetwork.TagManager.Audio.Title, iSource.Name);
            metadata.Add(iNetwork.TagManager.Audio.Artwork, "external://" + iSource.Name);

            // get the root group of this group
            Topology4Group group = iGroup;
            while (group.Parent != null)
            {
                group = group.Parent;
            }

            return new MediaPresetExternal(iNetwork, group, iSource.Index, metadata, this);
        }

        public IEnumerable<ITopology4Group> Volumes
        {
            get { return iVolumes; }
            set { iVolumes = value; }
        }

        public IDevice Device
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

        internal void Select()
        {
            iGroup.SetSourceIndex(iSource.Index);
        }

        private INetwork iNetwork;
        private Topology4Group iGroup;
        private ITopology2Source iSource;

        private IEnumerable<ITopology4Group> iVolumes;
        private IDevice iDevice;
        private bool iHasInfo;
        private bool iHasTime;
    }

    public interface ITopology4Registration
    {
        string Room { get; }
        string ModelName { get; }
        string ManufacturerName { get; }
        string ProductId { get; }
        IWatchable<string> Registration { get; }
        void SetRegistration(string aValue);
    }

    public interface ITopology4Group
    {
        string Name { get; }
        IDevice Device { get; }
        IWatchable<ITopologymSender> Sender { get; }
    }
    
    public interface ITopology4Root : ITopology4Group
    {
        IWatchable<ITopology4Source> Source { get; }
        IEnumerable<ITopology4Source> Sources { get; }
        IWatchable<IEnumerable<ITopology4Group>> Senders { get; }
    }

    class Topology4Group : ITopology4Root, ITopology4Registration, IWatcher<uint>, IWatcher<string>, IDisposable
    {
        public Topology4Group(INetwork aNetwork, string aRoom, string aName, ITopologymGroup aGroup, IEnumerable<ITopology2Source> aSources, ILog aLog)
        {
            iDisposed = false;

            iLog = aLog;

            iNetwork = aNetwork;
            iName = aName;
            iRoom = aRoom;
            iGroup = aGroup;

            iChildren = new List<Topology4Group>();

            iSources = new List<Topology4Source>();
            iVisibleSources = new List<ITopology4Source>();
            iWatchableSource = new Watchable<ITopology4Source>(iNetwork, "source", new Topology4SourceNull());
            iSenders = new Watchable<IEnumerable<ITopology4Group>>(iNetwork, "senders", new List<ITopology4Group>());

            foreach (ITopology2Source s in aSources)
            {
                Topology4Source source = new Topology4Source(aNetwork, this, s);
                iSources.Add(source);
            }

            iGroup.SourceIndex.AddWatcher(this);

            if (iGroup.Attributes.Contains("Sender"))
            {
                iGroup.Device.Create<IProxySender>((sender) =>
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
            }
        }

        public void Dispose()
        {
            iGroup.SourceIndex.RemoveWatcher(this);
            iGroup = null;

            iWatchableSource.Dispose();
            iWatchableSource = null;
   
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

        public IDevice Device
        {
            get
            {
                return iGroup.Device;
            }
        }

        public string Room
        {
            get
            {
                return iRoom;
            }
        }

        public string ModelName
        {
            get
            {
                return iGroup.ModelName;
            }
        }

        public string ManufacturerName
        {
            get
            {
                return iGroup.ManufacturerName;
            }
        }

        public string ProductId
        {
            get
            {
                return iGroup.ProductId;
            }
        }

        public IWatchable<string> Registration
        {
            get
            {
                return iGroup.Registration;
            }
        }

        public IWatchable<ITopologymSender> Sender
        {
            get
            {
                return iGroup.Sender;
            }
        }

        public IWatchable<ITopology4Source> Source
        {
            get
            {
                return iWatchableSource;
            }
        }

        public void SetRegistration(string aValue)
        {
            if (iGroup != null)
            {
                iGroup.SetRegistration(aValue);
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
            foreach (Topology4Source s in iSources)
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
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            iSourceIndex = aValue;
            EvaluateSourceFromChild();
        }

        public void ItemClose(string aId, uint aValue)
        {
        }

        private ITopology4Source EvaluateSource()
        {
            if(iSources.Count <= iSourceIndex)
            {
                iLog.Write("EvaluateSource of {0}, iSources.Count={1}, iSourceIndex={2}", iName, iSources.Count, iSourceIndex);
            }

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

        private readonly ILog iLog;

        private INetwork iNetwork;
        private ITopologymGroup iGroup;

        private string iRoom;
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
        private ITopologymGroup iGroup;
        private string iRoomName;
        private string iName;
        private List<ITopology2Source> iSources;

        public Topology4GroupWatcher(Topology4Room aRoom, ITopologymGroup aGroup)
        {
            iRoom = aRoom;
            iGroup = aGroup;
            iRoomName = iRoom.Name;

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

        public string Room
        {
            get
            {
                return iRoomName;
            }
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
        IWatchable<IEnumerable<ITopology4Registration>> Registrations { get; }

        void SetStandby(bool aValue);
    }

    class Topology4Room : ITopology4Room, IUnorderedWatcher<ITopologymGroup>, IWatcher<bool>, IDisposable
    {
        public Topology4Room(INetwork aNetwork, ITopology3Room aRoom, ILog aLog)
        {
            iNetwork = aNetwork;
            iRoom = aRoom;

            iLog = aLog;

            iName = iRoom.Name;
            iStandbyCount = 0;
            iStandby = EStandby.eOn;

            iWatchableStandby = new Watchable<EStandby>(iNetwork, "standby", EStandby.eOn);
            iWatchableRoots = new Watchable<IEnumerable<ITopology4Root>>(iNetwork, "roots", new List<ITopology4Root>());
            iWatchableSources = new Watchable<IEnumerable<ITopology4Source>>(iNetwork, "sources", new List<ITopology4Source>());
            iWatchableRegistrations = new Watchable<IEnumerable<ITopology4Registration>>(iNetwork, "registration", new List<ITopology4Registration>());

            iGroupLookup = new Dictionary<ITopologymGroup, Topology4GroupWatcher>();
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

            iWatchableRegistrations.Dispose();
            iWatchableRegistrations = null;

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

        public IWatchable<IEnumerable<ITopology4Registration>> Registrations
        {
            get
            {
                return iWatchableRegistrations;
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

        public void UnorderedAdd(ITopologymGroup aItem)
        {
            iGroupLookup.Add(aItem, new Topology4GroupWatcher(this, aItem));
            aItem.Standby.AddWatcher(this);
            CreateTree();
        }

        public void UnorderedRemove(ITopologymGroup aItem)
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
            List<ITopology4Registration> registrations = new List<ITopology4Registration>();
            List<Topology4Group> oldGroups = new List<Topology4Group>(iGroup4s);

            iGroup4s.Clear();
            iRoots.Clear();

            foreach (var kvp in iGroupLookup)
            {
                Topology4Group group = new Topology4Group(iNetwork, kvp.Value.Room, kvp.Value.Name, kvp.Key, kvp.Value.Sources, iLog);
                InsertIntoTree(group);
                if (!string.IsNullOrEmpty(group.ProductId))
                {
                    registrations.Add(group);
                }
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
            iWatchableRegistrations.Update(registrations);

            foreach (Topology4Group g in oldGroups)
            {
                g.Dispose();
            }
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
            }

            EvaluateStandby();
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
            }

            EvaluateStandby(iGroupLookup.Count == 0);
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

        private INetwork iNetwork;
        private ITopology3Room iRoom;

        private readonly ILog iLog;

        private string iName;
        private uint iStandbyCount;
        private EStandby iStandby;

        private Watchable<EStandby> iWatchableStandby;
        private Watchable<IEnumerable<ITopology4Root>> iWatchableRoots;
        private Watchable<IEnumerable<ITopology4Source>> iWatchableSources;
        private Watchable<IEnumerable<ITopology4Registration>> iWatchableRegistrations;

        private Dictionary<ITopologymGroup, Topology4GroupWatcher> iGroupLookup;
        private List<Topology4Group> iGroup4s;
        private List<Topology4Group> iRoots;
    }
    
    public interface ITopology4
    {
        IWatchableUnordered<ITopology4Room> Rooms { get; }
        INetwork Network { get; }
    }

    public class Topology4 : ITopology4, IUnorderedWatcher<ITopology3Room>, IDisposable
    {
        public Topology4(ITopology3 aTopology3, ILog aLog)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aTopology3.Network;
            iTopology3 = aTopology3;
            iLog = aLog;

            iRooms = new WatchableUnordered<ITopology4Room>(iNetwork);

            iRoomLookup = new Dictionary<ITopology3Room, Topology4Room>();

            iNetwork.Schedule(() =>
            {
                iTopology3.Rooms.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                iTopology3.Rooms.RemoveWatcher(this);

                foreach (Topology4Room r in iRoomLookup.Values)
                {
                    r.Dispose();
                }
            });
            iRoomLookup.Clear();

            iRooms.Dispose();
        }

        public IWatchableUnordered<ITopology4Room> Rooms
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iRooms;
                }
            }
        }

        public INetwork Network
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iNetwork;
                }
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
            iDisposeHandler.WhenNotDisposed(() =>
            {
                Topology4Room room = new Topology4Room(iNetwork, aItem, iLog);
                iRooms.Add(room);
                iRoomLookup.Add(aItem, room);
            });
        }

        public void UnorderedRemove(ITopology3Room aItem)
        {
            iDisposeHandler.WhenNotDisposed(() =>
            {
                // schedule notification of L4 room removal
                Topology4Room room = iRoomLookup[aItem];
                iRooms.Remove(room);
                iRoomLookup.Remove(aItem);

                room.Dispose();
            });
        }

        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly ITopology3 iTopology3;
        private readonly ILog iLog;

        private readonly WatchableUnordered<ITopology4Room> iRooms;
        private readonly Dictionary<ITopology3Room, Topology4Room> iRoomLookup;
    }
}