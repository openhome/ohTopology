using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class RoomDetails : IInfoDetails
    {
        public RoomDetails()
        {
            iEnabled = false;
        }

        public RoomDetails(IInfoDetails aDetails)
        {
            iEnabled = true;
            iBitDepth = aDetails.BitDepth;
            iBitRate = aDetails.BitRate;
            iCodecName = aDetails.CodecName;
            iDuration = aDetails.Duration;
            iLossless = aDetails.Lossless;
            iSampleRate = aDetails.SampleRate;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
        }

        public uint BitDepth
        {
            get
            {
                return iBitDepth;
            }
        }

        public uint BitRate
        {
            get
            {
                return iBitRate;
            }
        }

        public string CodecName
        {
            get
            {
                return iCodecName;
            }
        }

        public uint Duration
        {
            get
            {
                return iDuration;
            }
        }

        public bool Lossless
        {
            get
            {
                return iLossless;
            }
        }

        public uint SampleRate
        {
            get
            {
                return iSampleRate;
            }
        }

        private bool iEnabled;
        private uint iBitDepth;
        private uint iBitRate;
        private string iCodecName;
        private uint iDuration;
        private bool iLossless;
        private uint iSampleRate;
    }

    public class RoomMetadata : IInfoMetadata
    {
        public RoomMetadata()
        {
            iEnabled = false;
        }

        public RoomMetadata(IInfoMetadata aMetadata)
        {
            iEnabled = true;
            iMetadata = aMetadata.Metadata;
            iUri = aMetadata.Uri;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public string Uri
        {
            get
            {
                return iUri;
            }
        }

        private bool iEnabled;
        private IMediaMetadata iMetadata;
        private string iUri;
    }

    public class RoomMetatext : IInfoMetatext
    {
        public RoomMetatext()
        {
            iEnabled = false;
        }

        public RoomMetatext(IInfoMetatext aMetatext)
        {
            iEnabled = true;
            iMetatext = aMetatext.Metatext;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
        }

        public string Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        private bool iEnabled;
        private string iMetatext;
    }

    class InfoWatcher : IWatcher<IInfoDetails>, IWatcher<IInfoMetadata>, IWatcher<IInfoMetatext>, IDisposable
    {
        public InfoWatcher(IWatchableThread aThread, IDevice aDevice, Watchable<RoomDetails> aDetails, Watchable<RoomMetadata> aMetadata, Watchable<RoomMetatext> aMetatext)
        {
            iDisposed = false;

            iDevice = aDevice;
            iDetails = aDetails;
            iMetadata = aMetadata;
            iMetatext = aMetatext;

            iDevice.Create<IProxyInfo>((info) =>
            {
                if (!iDisposed)
                {
                    iInfo = info;

                    iInfo.Details.AddWatcher(this);
                    iInfo.Metadata.AddWatcher(this);
                    iInfo.Metatext.AddWatcher(this);
                }
                else
                {
                    info.Dispose();
                }
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("InfoWatcher.Dispose");
            }

            if (iInfo != null)
            {
                iInfo.Details.RemoveWatcher(this);
                iInfo.Metadata.RemoveWatcher(this);
                iInfo.Metatext.RemoveWatcher(this);

                iInfo.Dispose();
                iInfo = null;
            }

            iDisposed = true;
        }

        public void ItemOpen(string aId, IInfoDetails aValue)
        {
            iDetails.Update(new RoomDetails(aValue));
        }

        public void ItemUpdate(string aId, IInfoDetails aValue, IInfoDetails aPrevious)
        {
            iDetails.Update(new RoomDetails(aValue));
        }

        public void ItemClose(string aId, IInfoDetails aValue)
        {
            iDetails.Update(new RoomDetails());
        }

        public void ItemOpen(string aId, IInfoMetadata aValue)
        {
            iMetadata.Update(new RoomMetadata(aValue));
        }

        public void ItemUpdate(string aId, IInfoMetadata aValue, IInfoMetadata aPrevious)
        {
            iMetadata.Update(new RoomMetadata(aValue));
        }

        public void ItemClose(string aId, IInfoMetadata aValue)
        {
            iMetadata.Update(new RoomMetadata());
        }

        public void ItemOpen(string aId, IInfoMetatext aValue)
        {
            iMetatext.Update(new RoomMetatext(aValue));
        }

        public void ItemUpdate(string aId, IInfoMetatext aValue, IInfoMetatext aPrevious)
        {
            iMetatext.Update(new RoomMetatext(aValue));
        }

        public void ItemClose(string aId, IInfoMetatext aValue)
        {
            iMetatext.Update(new RoomMetatext());
        }

        private bool iDisposed;
        private IProxyInfo iInfo;

        private IDevice iDevice;
        private Watchable<RoomDetails> iDetails;
        private Watchable<RoomMetadata> iMetadata;
        private Watchable<RoomMetatext> iMetatext;
    }

    public class RoomSenderMetadata : ISenderMetadata
    {
        public RoomSenderMetadata()
        {
            iEnabled = false;
        }

        public RoomSenderMetadata(ISenderMetadata aMetadata)
        {
            iEnabled = true;
            iName = aMetadata.Name;
            iUri = aMetadata.Uri;
            iArtworkUri = aMetadata.ArtworkUri;
        }

        public bool Enabled
        {
            get { return iEnabled; }
        }

        public string Name
        {
            get { return iName; }
        }

        public string Uri
        {
            get { return iUri; }
        }

        public string ArtworkUri
        {
            get { return iArtworkUri; }
        }

        private bool iEnabled;
        private string iName;
        private string iUri;
        private string iArtworkUri;
    }

    public interface IZone
    {
        bool Active { get; }
        IStandardRoom Room { get; }
        IDevice Sender { get; }
        IWatchableUnordered<IStandardRoom> Listeners { get; }
    }

    public class Zone : IZone
    {
        public Zone(bool aActive, StandardRoom aRoom, IDevice aDevice)
        {
            iActive = aActive;
            iRoom = aRoom;
            iDevice = aDevice;

            iListeners = new WatchableUnordered<IStandardRoom>(aRoom.Network);
        }

        public bool Active
        {
            get
            {
                return iActive;
            }
        }

        public IStandardRoom Room
        {
            get
            {
                return iRoom;
            }
        }

        public IDevice Sender
        {
            get
            {
                return iDevice;
            }
        }

        public IWatchableUnordered<IStandardRoom> Listeners
        {
            get
            {
                return iListeners;
            }
        }

        public void AddToZone(IStandardRoom aRoom)
        {
            iListeners.Add(aRoom);
        }

        public void RemoveFromZone(IStandardRoom aRoom)
        {
            iListeners.Remove(aRoom);
        }

        private bool iActive;
        private StandardRoom iRoom;
        private IDevice iDevice;
        private WatchableUnordered<IStandardRoom> iListeners;
    }

    public interface IStandardRoom : IJoinable
    {
        INetwork Network { get; }
        string Name { get; }

        IWatchable<EStandby> Standby { get; }
        IWatchable<RoomDetails> Details { get; }
        IWatchable<RoomMetadata> Metadata { get; }
        IWatchable<RoomMetatext> Metatext { get; }

        // multi-room interface
        IWatchable<IZone> Zone { get; }
        IWatchable<bool> Zoneable { get; }

        IWatchable<ITopology4Source> Source { get; }
        IWatchable<IEnumerable<ITopology4Source>> Sources { get; }
        IEnumerable<ITopology4Group> Senders { get; }

        void SetStandby(bool aValue);
        void ListenTo(IStandardRoom aRoom);
        void Play(string aUri, IMediaMetadata aMetadata);
    }

    public class StandardRoom : IStandardRoom, IWatcher<IEnumerable<ITopology4Root>>, IWatcher<IEnumerable<ITopology4Group>>, IWatcher<ITopology4Source>, IMockable, IDisposable
    {
        public StandardRoom(StandardHouse aHouse, ITopology4Room aRoom)
        {
            iDisposed = false;

            iNetwork = aHouse.Network;
            iHouse = aHouse;
            iRoom = aRoom;

            iJoiners = new List<Action>();
            iRoots = new List<ITopology4Root>();
            iCurrentSources = new List<ITopology4Source>();
            iSources = new List<ITopology4Source>();
            iSenders = new List<ITopology4Group>();

            iDetails = new Watchable<RoomDetails>(iNetwork, "Details", new RoomDetails());
            iMetadata = new Watchable<RoomMetadata>(iNetwork, "Metadata", new RoomMetadata());
            iMetatext = new Watchable<RoomMetatext>(iNetwork, "Metatext", new RoomMetatext());

            iZoneable = new Watchable<bool>(iNetwork, "HasReceiver", false);
            iZone = new Zone(false, this, null);
            iWatchableZone = new Watchable<IZone>(iNetwork, "Zone", iZone);

            iRoom.Roots.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("StandardRoom.Dispose");
            }

            List<Action> linked = new List<Action>(iJoiners);
            foreach (Action a in linked)
            {
                a();
            }
            if (iJoiners.Count > 0)
            {
                throw new Exception("StandardRoom joiners > 0");
            }
            iJoiners = null;

            iNetwork.Execute(() =>
            {
                iRoom.Roots.RemoveWatcher(this);
            });

            iDetails.Dispose();
            iDetails = null;

            iMetadata.Dispose();
            iMetadata = null;

            iMetatext.Dispose();
            iMetatext = null;

            iZoneable.Dispose();
            iZoneable = null;

            iWatchableZone.Dispose();
            iWatchableZone = null;
            iZone = null;

            iRoots = null;
            iRoom = null;
            iHouse = null;
            iNetwork = null;

            iDisposed = true;
        }

        public string Name
        {
            get
            {
                return iRoom.Name;
            }
        }

        public IWatchable<EStandby> Standby
        {
            get
            {
                return iRoom.Standby;
            }
        }

        public IWatchable<RoomDetails> Details
        {
            get
            {
                return iDetails;
            }
        }

        public IWatchable<RoomMetadata> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<RoomMetatext> Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        public IWatchable<ITopology4Source> Source
        {
            get
            {
                return iWatchableSource;
            }
        }

        public IWatchable<IEnumerable<ITopology4Source>> Sources
        {
            get
            {
                return iWatchableSources;
            }
        }

        public void Join(Action aAction)
        {
            iJoiners.Add(aAction);
        }

        public void UnJoin(Action aAction)
        {
            iJoiners.Remove(aAction);
        }

        public void SetStandby(bool aValue)
        {
            iRoom.SetStandby(aValue);
        }

        public IWatchable<IZone> Zone
        {
            get
            {
                return iWatchableZone;
            }
        }

        public IWatchable<bool> Zoneable
        {
            get
            {
                return iZoneable;
            }
        }

        public IEnumerable<ITopology4Group> Senders
        {
            get
            {
                return iSenders;
            }
        }

        public void ListenTo(IStandardRoom aRoom)
        {
            foreach (ITopology4Source s in iSources)
            {
                if (s.Type == "Receiver")
                {
                    s.Device.Create<IProxyReceiver>((receiver) =>
                    {
                        aRoom.Zone.Value.Sender.Create<IProxySender>((sender) =>
                        {
                            if (!iDisposed)
                            {
                                Task action = receiver.SetSender(sender.Metadata.Value);
                                action.ContinueWith((t) => { receiver.Play(); });
                                receiver.Dispose();
                                sender.Dispose();
                            }
                            else
                            {
                                receiver.Dispose();
                                sender.Dispose();
                            }
                        });
                    });
                    return;
                }
            }
        }

        public void Play(string aUri, IMediaMetadata aMetadata)
        {
            foreach (ITopology4Source s in iSources)
            {
                if (s.Type == "Radio")
                {
                    s.Device.Create<IProxyRadio>((radio) =>
                    {
                        radio.SetChannel(aUri, aMetadata);
                        radio.Dispose();
                    });
                    return;
                }
            }
        }

        public INetwork Network
        {
            get
            {
                return iNetwork;
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

        public void ItemOpen(string aId, IEnumerable<ITopology4Root> aValue)
        {
            iWatchableSources = new Watchable<IEnumerable<ITopology4Source>>(iNetwork, "Sources", new List<ITopology4Source>());
            
            iRoots = aValue;

            foreach (ITopology4Root r in aValue)
            {
                iSources.AddRange(r.Sources);
                r.Source.AddWatcher(this);
                r.Senders.AddWatcher(this);
            }

            iWatchableSources.Update(iSources);
            SelectFirstSource();

            EvaluateZoneable();
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Root> aValue, IEnumerable<ITopology4Root> aPrevious)
        {
            iSources = new List<ITopology4Source>();

            foreach (ITopology4Root r in aPrevious)
            {
                r.Source.RemoveWatcher(this);
                r.Senders.RemoveWatcher(this);
            }

            iRoots = aValue;

            foreach (ITopology4Root r in aValue)
            {
                iSources.AddRange(r.Sources);
                r.Source.AddWatcher(this);
                r.Senders.AddWatcher(this);
            }

            iWatchableSources.Update(iSources);
            SelectSource();

            EvaluateZoneable();
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Root> aValue)
        {
            iRoots = null;

            foreach (ITopology4Root r in aValue)
            {
                r.Source.RemoveWatcher(this);
                r.Senders.RemoveWatcher(this);
            }

            iInfoWatcher.Dispose();
            iInfoWatcher = null;

            iWatchableSources.Dispose();
            iWatchableSources = null;

            iSource = null;

            EvaluateZoneable();
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            iCurrentSources.Add(aValue);
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            iCurrentSources.Remove(aPrevious);
            iCurrentSources.Add(aValue);

            SelectSource();
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            iCurrentSources.Remove(aValue);
        }

        private void SelectFirstSource()
        {
            ITopology4Source source = iCurrentSources[0];
            iWatchableSource = new Watchable<ITopology4Source>(iNetwork, "Source", source);
            if (source.HasInfo)
            {
                iInfoWatcher = new InfoWatcher(iNetwork, source.Device, iDetails, iMetadata, iMetatext);
            }
            iSource = source;
        }

        private void SelectSource()
        {
            ITopology4Source source = iCurrentSources[0];

            if (iSource.Device != source.Device)
            {
                if (iInfoWatcher != null)
                {
                    iInfoWatcher.Dispose();
                    iInfoWatcher = null;
                }

                if (source.HasInfo)
                {
                    iInfoWatcher = new InfoWatcher(iNetwork, source.Device, iDetails, iMetadata, iMetatext);
                }
            }
            else if(!iSource.HasInfo && source.HasInfo)
            {
                iInfoWatcher = new InfoWatcher(iNetwork, source.Device, iDetails, iMetadata, iMetatext);
            }

            iWatchableSource.Update(source);
            iSource = source;
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Group> aValue)
        {
            iSenders.AddRange(aValue);
            EvaluateZone(aValue);
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Group> aValue, IEnumerable<ITopology4Group> aPrevious)
        {
            foreach (ITopology4Group g in aPrevious)
            {
                iSenders.Remove(g);
            }
            iSenders.AddRange(aValue);
            EvaluateZone(aValue);
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Group> aValue)
        {
            foreach (ITopology4Group g in aValue)
            {
                iSenders.Remove(g);
            }
        }

        private void EvaluateZone(IEnumerable<ITopology4Group> aValue)
        {
            if (iRoots.Count() == 1)
            {
                ITopology4Root root = iRoots.First();
                foreach (ITopology4Group g in aValue)
                {
                    if (root == g)
                    {
                        if (!iWatchableZone.Value.Active)
                        {
                            iZone = new Zone(true, this, root.Device);
                            iWatchableZone.Update(iZone);
                        }
                        break;
                    }
                }
            }
            else
            {
                if (iWatchableZone.Value.Active)
                {
                    iZone = new Zone(false, this, null);
                    iWatchableZone.Update(iZone);
                }
            }
        }

        private void EvaluateZoneable()
        {
            foreach (ITopology4Source s in iSources)
            {
                if (s.Type == "Receiver")
                {
                    iZoneable.Update(true);
                    return;
                }
            }

            iZoneable.Update(false);
        }

        public void Execute(IEnumerable<string> aValue)
        {
        }

        internal bool AddToZone(IDevice aDevice, IStandardRoom aRoom)
        {
            if (iRoots.First().Device == aDevice)
            {
                iZone.AddToZone(aRoom);
                return true;
            }

            return false;
        }

        internal bool RemoveFromZone(IDevice aDevice, IStandardRoom aRoom)
        {
            if (iRoots.First().Device == aDevice)
            {
                iZone.RemoveFromZone(aRoom);
                return true;
            }

            return false;
        }

        private bool iDisposed;
        private INetwork iNetwork;
        private StandardHouse iHouse;
        private ITopology4Room iRoom;

        private IEnumerable<ITopology4Root> iRoots;
        private List<Action> iJoiners;
        private ITopology4Source iSource;
        private List<ITopology4Source> iCurrentSources;
        private List<ITopology4Source> iSources;
        private List<ITopology4Group> iSenders;
        private Watchable<ITopology4Source> iWatchableSource;
        private Watchable<IEnumerable<ITopology4Source>> iWatchableSources;

        private Watchable<bool> iZoneable;
        private Zone iZone;
        private Watchable<IZone> iWatchableZone;

        private InfoWatcher iInfoWatcher;
        private Watchable<RoomDetails> iDetails;
        private Watchable<RoomMetadata> iMetadata;
        private Watchable<RoomMetatext> iMetatext;
    }

    class RoomWatcher : IWatcher<ITopology4Source>, IWatcher<ITopologymSender>, IOrderedWatcher<IStandardRoom>, IDisposable
    {
        private StandardHouse iHouse;
        private IStandardRoom iRoom;
        private bool iRoomsInitialised;
        private ITopology4Source iSource;
        private ITopologymSender iSender;

        public RoomWatcher(StandardHouse aHouse, IStandardRoom aRoom)
        {
            iHouse = aHouse;
            iRoom = aRoom;

            iRoom.Source.AddWatcher(this);
            iHouse.Rooms.AddWatcher(this);
        }

        public void Dispose()
        {
            iHouse.Rooms.RemoveWatcher(this);
            iRoom.Source.RemoveWatcher(this);

            iRoom = null;
            iHouse = null;
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            if (aValue.Type == "Receiver")
            {
                aValue.Group.Sender.AddWatcher(this);
            }
            iSource = aValue;
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if (aPrevious.Type == "Receiver")
            {
                aPrevious.Group.Sender.RemoveWatcher(this);
            }
            if (aValue.Type == "Receiver")
            {
                aValue.Group.Sender.AddWatcher(this);
            }
            iSource = aValue;
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            if (aValue.Type == "Receiver")
            {
                aValue.Group.Sender.RemoveWatcher(this);
            }
            iSource = null;
        }

        public void ItemOpen(string aId, ITopologymSender aValue)
        {
            if (aValue.Enabled)
            {
                iHouse.AddToZone(aValue.Device, iRoom);
            }
            iSender = aValue;
        }

        public void ItemUpdate(string aId, ITopologymSender aValue, ITopologymSender aPrevious)
        {
            if (aPrevious.Enabled)
            {
                iHouse.RemoveFromZone(aPrevious.Device, iRoom);
            }
            if (aValue.Enabled)
            {
                iHouse.AddToZone(aValue.Device, iRoom);
            }
            iSender = aValue;
        }

        public void ItemClose(string aId, ITopologymSender aValue)
        {
            if (aValue.Enabled)
            {
                iHouse.RemoveFromZone(aValue.Device, iRoom);
            }
            iSender = null;
        }

        public void OrderedOpen() { }

        public void OrderedInitialised() { iRoomsInitialised = true; }

        public void OrderedClose() { }

        public void OrderedAdd(IStandardRoom aItem, uint aIndex)
        {
            if (iRoomsInitialised)
            {
                if (iSender != null && iSender.Enabled)
                {
                    if (aItem.Zone.Value.Sender == iSender.Device)
                    {
                        iHouse.AddToZone(iSender.Device, iRoom);
                    }
                }
            }
        }

        public void OrderedMove(IStandardRoom aItem, uint aFrom, uint aTo) { }

        public void OrderedRemove(IStandardRoom aItem, uint aIndex) { }
    }

    public interface IStandardHouse
    {
        IWatchableOrdered<IStandardRoom> Rooms { get; }
        IWatchableOrdered<IProxyMediaServer> Servers { get; }
        INetwork Network { get; }
    }

    public class StandardHouse : IUnorderedWatcher<ITopology4Room>, IUnorderedWatcher<IDevice>, IStandardHouse, IMockable, IDisposable
    {
        public StandardHouse(INetwork aNetwork)
        {
            iDisposed = false;
            iNetwork = aNetwork;

            iTopology1 = new Topology1(aNetwork);
            iTopology2 = new Topology2(iTopology1);
            iTopologym = new Topologym(iTopology2);
            iTopology3 = new Topology3(iTopologym);
            iTopology4 = new Topology4(iTopology3);

            iWatchableServers = new WatchableOrdered<IProxyMediaServer>(iNetwork);
            iMediaServers = iNetwork.Create<IProxyMediaServer>();
            iServerLookup = new Dictionary<IDevice, IProxyMediaServer>();
 
            iWatchableRooms = new WatchableOrdered<IStandardRoom>(iNetwork);
            iRoomLookup = new Dictionary<ITopology4Room, StandardRoom>();

            iRoomWatchers = new Dictionary<ITopology4Room, RoomWatcher>();

            iNetwork.Schedule(() =>
            {
                iMediaServers.AddWatcher(this);
                iTopology4.Rooms.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("StandardHouse.Dispose");
            }

            iNetwork.Execute(() =>
            {
                iMediaServers.RemoveWatcher(this);

                foreach (var kvp in iServerLookup)
                {
                    kvp.Value.Dispose();
                }

                iTopology4.Rooms.RemoveWatcher(this);

                // remove listeners from zones before disposing of zones
                foreach (var kvp in iRoomWatchers)
                {
                    kvp.Value.Dispose();
                }

                foreach (var kvp in iRoomLookup)
                {
                    kvp.Value.Dispose();
                }
            });
            iWatchableRooms.Dispose();
            iWatchableRooms = null;

            iRoomLookup.Clear();
            iRoomLookup = null;

            iRoomWatchers.Clear();
            iRoomWatchers = null;

            iWatchableServers.Dispose();
            iWatchableServers = null;

            iServerLookup = null;

            iTopology4.Dispose();
            iTopology3.Dispose();
            iTopologym.Dispose();
            iTopology2.Dispose();
            iTopology1.Dispose();

            iDisposed = true;
        }

        public IWatchableOrdered<IStandardRoom> Rooms
        {
            get
            {
                return iWatchableRooms;
            }
        }

        public IWatchableOrdered<IProxyMediaServer> Servers
        {
            get
            {
                return iWatchableServers;
            }
        }

        public INetwork Network
        {
            get
            {
                return iNetwork;
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

        public void UnorderedAdd(ITopology4Room aRoom)
        {
            StandardRoom room = new StandardRoom(this, aRoom);

            // calculate where to insert the room
            int index = 0;
            foreach (IStandardRoom r in iWatchableRooms.Values)
            {
                if (room.Name.CompareTo(r.Name) < 0)
                {
                    break;
                }
                ++index;
            }

            // insert the room
            iRoomLookup.Add(aRoom, room);
            iWatchableRooms.Add(room, (uint)index);

            // do this here so that room is added before other rooms are informed of this room listening to them
            iRoomWatchers.Add(aRoom, new RoomWatcher(this, room));
        }

        public void UnorderedRemove(ITopology4Room aRoom)
        {
            // remove the corresponding Room from the watchable collection
            StandardRoom room = iRoomLookup[aRoom];

            iRoomWatchers[aRoom].Dispose();
            iRoomWatchers.Remove(aRoom);

            iRoomLookup.Remove(aRoom);
            iWatchableRooms.Remove(room);

            room.Dispose();
        }

        public void UnorderedAdd(IDevice aItem)
        {
            aItem.Create<IProxyMediaServer>((server) =>
            {
                if (!iDisposed)
                {
                    // calculate where to insert the server
                    int index = 0;
                    foreach (IProxyMediaServer ms in iWatchableServers.Values)
                    {
                        if (server.ProductName.CompareTo(ms.ProductName) < 0)
                        {
                            break;
                        }
                        ++index;
                    }

                    // insert the server
                    iServerLookup.Add(aItem, server);
                    iWatchableServers.Add(server, (uint)index);
                }
                else
                {
                    server.Dispose();
                }
            });
        }

        public void UnorderedRemove(IDevice aItem)
        {
            IProxyMediaServer server;
            if (iServerLookup.TryGetValue(aItem, out server))
            {
                // remove the corresponding server from the watchable collection
                iServerLookup.Remove(aItem);
                iWatchableServers.Remove(server);

                server.Dispose();
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            iNetwork.Execute(() =>
            {
                string command = aValue.First().ToLowerInvariant();

                if (command == "zone")
                {
                    IEnumerable<string> value = aValue.Skip(1);

                    string name = value.First();

                    foreach (StandardRoom r1 in iRoomLookup.Values)
                    {
                        if (r1.Name == name)
                        {
                            value = value.Skip(1);

                            command = value.First().ToLowerInvariant();

                            if (command == "add")
                            {
                                value = value.Skip(1);

                                name = value.First();

                                foreach (StandardRoom r2 in iRoomLookup.Values)
                                {
                                    if (r2.Name == name)
                                    {
                                        r2.ListenTo(r1);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        internal void AddToZone(IDevice aDevice, IStandardRoom aRoom)
        {
            foreach (StandardRoom r in iRoomLookup.Values)
            {
                if (r.AddToZone(aDevice, aRoom))
                {
                    break;
                }
            }
        }

        internal void RemoveFromZone(IDevice aDevice, IStandardRoom aRoom)
        {
            foreach (StandardRoom r in iRoomLookup.Values)
            {
                if (r.RemoveFromZone(aDevice, aRoom))
                {
                    break;
                }
            }
        }

        private bool iDisposed;
        private INetwork iNetwork;
        private Topology1 iTopology1;
        private Topology2 iTopology2;
        private Topologym iTopologym;
        private Topology3 iTopology3;
        private Topology4 iTopology4;

        private WatchableOrdered<IProxyMediaServer> iWatchableServers;
        private IWatchableUnordered<IDevice> iMediaServers;
        private Dictionary<IDevice, IProxyMediaServer> iServerLookup;

        private WatchableOrdered<IStandardRoom> iWatchableRooms;
        private Dictionary<ITopology4Room, StandardRoom> iRoomLookup;

        private Dictionary<ITopology4Room, RoomWatcher> iRoomWatchers;
    }
}
