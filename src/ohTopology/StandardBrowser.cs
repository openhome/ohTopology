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
        public InfoWatcher(INetwork aNetwork, IDevice aDevice, Watchable<RoomDetails> aDetails, Watchable<RoomMetadata> aMetadata, Watchable<RoomMetatext> aMetatext)
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

    public interface IZoneSender
    {
        bool Enabled { get; }
        IStandardRoom Room { get; }
        IDevice Sender { get; }
        IWatchable<bool> HasListeners { get; }
        IWatchableOrdered<IStandardRoom> Listeners { get; }
    }

    class ZoneSender : IZoneSender
    {
        public ZoneSender(StandardRoom aRoom)
            : this(false, aRoom, null)
        {
        }

        public ZoneSender(StandardRoom aRoom, IDevice aDevice)
            : this(true, aRoom, aDevice)
        {
        }

        private ZoneSender(bool aEnabled, StandardRoom aRoom, IDevice aDevice)
        {
            iEnabled = aEnabled;
            iRoom = aRoom;
            iDevice = aDevice;

            iHasListeners = new Watchable<bool>(aRoom.Network, "HasListeners", false);
            iListeners = new WatchableOrdered<IStandardRoom>(aRoom.Network);
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
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

        public IWatchable<bool> HasListeners
        {
            get
            {
                return iHasListeners;
            }
        }

        public IWatchableOrdered<IStandardRoom> Listeners
        {
            get
            {
                return iListeners;
            }
        }

        internal void AddToZone(IStandardRoom aRoom)
        {
            // calculate where to insert the sender
            int index = 0;
            foreach (IStandardRoom r in iListeners.Values)
            {
                if (r.Name.CompareTo(r.Name) < 0)
                {
                    break;
                }
                ++index;
            }

            // insert the room
            iListeners.Add(aRoom, (uint)index);
            iHasListeners.Update(iListeners.Values.Count() > 0);
        }

        internal void RemoveFromZone(IStandardRoom aRoom)
        {
            iListeners.Remove(aRoom);
            iHasListeners.Update(iListeners.Values.Count() > 0);
        }

        private bool iEnabled;
        private StandardRoom iRoom;
        private IDevice iDevice;
        private Watchable<bool> iHasListeners;
        private WatchableOrdered<IStandardRoom> iListeners;
    }

    public interface IZoneReceiver
    {
        bool Enabled { get; }
        IZoneSender ZoneSender { get; }
    }

    class ZoneReceiver : IZoneReceiver
    {
        private bool iEnabled;
        private IZoneSender iZoneSender;

        public ZoneReceiver(bool aEnabled)
        {
            iEnabled = aEnabled;
            iZoneSender = null;
        }

        public ZoneReceiver(IZoneSender aZoneSender)
        {
            iEnabled = true;
            iZoneSender = aZoneSender;
        }

        public bool Enabled
        {
            get { return iEnabled; }
        }

        public IZoneSender ZoneSender
        {
            get { return iZoneSender; }
        }
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
        IWatchable<IZoneSender> ZoneSender { get; }
        IWatchable<IZoneReceiver> ZoneReceiver { get; }

        IWatchable<ITopology4Source> Source { get; }
        IWatchable<IEnumerable<ITopology4Source>> Sources { get; }
        IEnumerable<ITopology4Group> Senders { get; }

        void SetStandby(bool aValue);
        void ListenTo(IStandardRoom aRoom);
        void Play(string aUri, IMediaMetadata aMetadata);
    }

    class StandardRoom : IStandardRoom, IWatcher<IEnumerable<ITopology4Root>>, IWatcher<IEnumerable<ITopology4Group>>, IWatcher<ITopology4Source>, IMockable, IDisposable
    {
        public StandardRoom(INetwork aNetwork, ITopology4Room aRoom)
        {
            iDisposeHandler = new DisposeHandler();
            iDisposed = false;

            iNetwork = aNetwork;
            iRoom = aRoom;

            iJoiners = new List<Action>();
            iRoots = new List<ITopology4Root>();
            iCurrentSources = new List<ITopology4Source>();
            iSources = new List<ITopology4Source>();
            iSenders = new List<ITopology4Group>();

            iDetails = new Watchable<RoomDetails>(iNetwork, "Details", new RoomDetails());
            iMetadata = new Watchable<RoomMetadata>(iNetwork, "Metadata", new RoomMetadata());
            iMetatext = new Watchable<RoomMetatext>(iNetwork, "Metatext", new RoomMetatext());

            iZoneSender = new ZoneSender(this);
            iWatchableZoneSender = new Watchable<IZoneSender>(iNetwork, "ZoneSender", iZoneSender);
            iZoneReceiver = new ZoneReceiver(false);
            iWatchableZoneReceiver = new Watchable<IZoneReceiver>(iNetwork, "ZoneReceiver", iZoneReceiver);

            iRoom.Roots.AddWatcher(this);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                List<Action> linked = new List<Action>(iJoiners);
                foreach (Action a in linked)
                {
                    a();
                }
                if (iJoiners.Count > 0)
                {
                    throw new Exception("StandardRoom joiners > 0");
                }
                
                iRoom.Roots.RemoveWatcher(this);
                iDisposed = true;
            });

            iDetails.Dispose();

            iMetadata.Dispose();

            iMetatext.Dispose();

            iWatchableZoneSender.Dispose();
            iZoneSender = null;

            iWatchableZoneReceiver.Dispose();

            iRoots = null;
        }

        public string Name
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iRoom.Name;
                }
            }
        }

        public IWatchable<EStandby> Standby
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iRoom.Standby;
                }
            }
        }

        public IWatchable<RoomDetails> Details
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iDetails;
                }
            }
        }

        public IWatchable<RoomMetadata> Metadata
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iMetadata;
                }
            }
        }

        public IWatchable<RoomMetatext> Metatext
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iMetatext;
                }
            }
        }

        public IWatchable<ITopology4Source> Source
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableSource;
                }
            }
        }

        public IWatchable<IEnumerable<ITopology4Source>> Sources
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableSources;
                }
            }
        }

        public void Join(Action aAction)
        {
            using (iDisposeHandler.Lock)
            {
                iJoiners.Add(aAction);
            }
        }

        public void UnJoin(Action aAction)
        {
            iJoiners.Remove(aAction);
        }

        public void SetStandby(bool aValue)
        {
            using (iDisposeHandler.Lock)
            {
                iRoom.SetStandby(aValue);
            }
        }

        public IWatchable<IZoneSender> ZoneSender
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableZoneSender;
                }
            }
        }

        public IWatchable<IZoneReceiver> ZoneReceiver
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableZoneReceiver;
                }
            }
        }

        public IEnumerable<ITopology4Group> Senders
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iSenders;
                }
            }
        }

        public void ListenTo(IStandardRoom aRoom)
        {
            using (iDisposeHandler.Lock)
            {
                foreach (ITopology4Source s in iSources)
                {
                    if (s.Type == "Receiver")
                    {
                        s.Device.Create<IProxyReceiver>((receiver) =>
                        {
                            aRoom.ZoneSender.Value.Sender.Create<IProxySender>((sender) =>
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
        }

        public void Play(string aUri, IMediaMetadata aMetadata)
        {
            using (iDisposeHandler.Lock)
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
        }

        public INetwork Network
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iNetwork;
                }
            }
        }

        public void UnorderedOpen() { }

        public void UnorderedInitialised() { }

        public void UnorderedClose() { }

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
                        if (!iWatchableZoneSender.Value.Enabled)
                        {
                            iZoneSender = new ZoneSender(this, root.Device);
                            iWatchableZoneSender.Update(iZoneSender);
                        }
                        break;
                    }
                }
            }
            else
            {
                if (iWatchableZoneSender.Value.Enabled)
                {
                    iZoneSender = new ZoneSender(this);
                    iWatchableZoneSender.Update(iZoneSender);
                }
            }
        }

        private void EvaluateZoneable()
        {
            foreach (ITopology4Source s in iSources)
            {
                if (s.Type == "Receiver")
                {
                    iWatchableZoneReceiver.Update(new ZoneReceiver(true));
                    return;
                }
            }

            iWatchableZoneReceiver.Update(new ZoneReceiver(false));
        }

        public void Execute(IEnumerable<string> aValue)
        {
        }

        internal bool AddToZone(IDevice aDevice, StandardRoom aRoom)
        {
            if (iZoneSender.Sender == aDevice)
            {
                iZoneSender.AddToZone(aRoom);
                aRoom.AddedToZone(iZoneSender);
                return true;
            }

            return false;
        }

        internal bool RemoveFromZone(IDevice aDevice, StandardRoom aRoom)
        {
            if (iZoneSender.Sender == aDevice)
            {
                iZoneSender.RemoveFromZone(aRoom);
                aRoom.RemovedFromZone(iZoneSender);
                return true;
            }

            return false;
        }

        internal void AddedToZone(IZoneSender aZone)
        {
            iWatchableZoneReceiver.Update(new ZoneReceiver(aZone));
        }

        internal void RemovedFromZone(IZoneSender aZone)
        {
            iWatchableZoneReceiver.Update(new ZoneReceiver(iWatchableZoneReceiver.Value.Enabled));
        }

        private readonly DisposeHandler iDisposeHandler;
        private bool iDisposed;
        private readonly INetwork iNetwork;
        private readonly ITopology4Room iRoom;

        private IEnumerable<ITopology4Root> iRoots;
        private readonly List<Action> iJoiners;
        private ITopology4Source iSource;
        private readonly List<ITopology4Source> iCurrentSources;
        private List<ITopology4Source> iSources;
        private readonly List<ITopology4Group> iSenders;
        private Watchable<ITopology4Source> iWatchableSource;
        private Watchable<IEnumerable<ITopology4Source>> iWatchableSources;

        private ZoneSender iZoneSender;
        private readonly Watchable<IZoneSender> iWatchableZoneSender;
        private ZoneReceiver iZoneReceiver;
        private readonly Watchable<IZoneReceiver> iWatchableZoneReceiver;

        private InfoWatcher iInfoWatcher;
        private readonly Watchable<RoomDetails> iDetails;
        private readonly Watchable<RoomMetadata> iMetadata;
        private readonly Watchable<RoomMetatext> iMetatext;
    }

    class RoomWatcher : IWatcher<ITopology4Source>, IWatcher<ITopologymSender>, IOrderedWatcher<IStandardRoom>, IDisposable
    {
        private readonly StandardHouse iHouse;
        private readonly IWatchableOrdered<IStandardRoom> iRooms;
        private readonly StandardRoom iRoom;
        private bool iRoomsInitialised;
        private ITopologymSender iSender;

        public RoomWatcher(StandardHouse aHouse, IWatchableOrdered<IStandardRoom> aRooms, StandardRoom aRoom)
        {
            iHouse = aHouse;
            iRooms = aRooms;
            iRoom = aRoom;

            iRoom.Source.AddWatcher(this);
            iRooms.AddWatcher(this);
        }

        public void Dispose()
        {
            iRooms.RemoveWatcher(this);
            iRoom.Source.RemoveWatcher(this);
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            if (aValue.Type == "Receiver")
            {
                aValue.Group.Sender.AddWatcher(this);
            }
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
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            if (aValue.Type == "Receiver")
            {
                aValue.Group.Sender.RemoveWatcher(this);
            }
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
                    if (aItem.ZoneSender.Value.Sender == iSender.Device)
                    {
                        iHouse.AddToZone(iSender.Device, iRoom);
                    }
                }
            }
        }

        public void OrderedMove(IStandardRoom aItem, uint aFrom, uint aTo) { }

        public void OrderedRemove(IStandardRoom aItem, uint aIndex) { }
    }

    class SendersWatcher : IUnorderedWatcher<IDevice>, IDisposable
    {
        private readonly WatchableOrdered<IProxySender> iWatchableSenders;
        private readonly IWatchableUnordered<IDevice> iSenders;
        private readonly Dictionary<IDevice, IProxySender> iSenderLookup;
        private bool iDisposed;

        public SendersWatcher(INetwork aNetwork)
        {
            iDisposed = false;
            iWatchableSenders = new WatchableOrdered<IProxySender>(aNetwork);
            iSenders = aNetwork.Create<IProxySender>();
            iSenderLookup = new Dictionary<IDevice, IProxySender>();

            aNetwork.Schedule(() =>
            {
                iSenders.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            iSenders.RemoveWatcher(this);

            foreach (var kvp in iSenderLookup)
            {
                kvp.Value.Dispose();
            }

            iWatchableSenders.Dispose();

            iDisposed = true;
        }

        public IWatchableOrdered<IProxySender> Senders
        {
            get
            {
                return iWatchableSenders;
            }
        }

        public void UnorderedOpen() { }

        public void UnorderedInitialised() { }

        public void UnorderedClose() { }

        public void UnorderedAdd(IDevice aItem)
        {
            aItem.Create<IProxySender>((sender) =>
            {
                if (!iDisposed)
                {
                    // calculate where to insert the sender
                    int index = 0;
                    foreach (IProxySender s in iWatchableSenders.Values)
                    {
                        if (sender.Metadata.Value.Name.CompareTo(s.Metadata.Value.Name) < 0)
                        {
                            break;
                        }
                        ++index;
                    }

                    // insert the sender
                    iSenderLookup.Add(aItem, sender);
                    iWatchableSenders.Add(sender, (uint)index);
                }
                else
                {
                    sender.Dispose();
                }
            });
        }

        public void UnorderedRemove(IDevice aItem)
        {
            IProxySender sender;
            if (iSenderLookup.TryGetValue(aItem, out sender))
            {
                // remove the corresponding sender from the watchable collection
                iSenderLookup.Remove(aItem);
                iWatchableSenders.Remove(sender);

                sender.Dispose();
            }
        }
    }

    class MediaServersWatcher : IUnorderedWatcher<IDevice>, IDisposable
    {
        private readonly WatchableOrdered<IProxyMediaServer> iWatchableMediaServers;
        private readonly IWatchableUnordered<IDevice> iMediaServers;
        private readonly Dictionary<IDevice, IProxyMediaServer> iMediaServerLookup;
        private bool iDisposed;

        public MediaServersWatcher(INetwork aNetwork)
        {
            iDisposed = false;
            iWatchableMediaServers = new WatchableOrdered<IProxyMediaServer>(aNetwork);
            iMediaServers = aNetwork.Create<IProxyMediaServer>();
            iMediaServerLookup = new Dictionary<IDevice, IProxyMediaServer>();

            aNetwork.Schedule(() =>
            {
                iMediaServers.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            iMediaServers.RemoveWatcher(this);

            foreach (var kvp in iMediaServerLookup)
            {
                kvp.Value.Dispose();
            }

            iWatchableMediaServers.Dispose();

            iDisposed = true;
        }

        public IWatchableOrdered<IProxyMediaServer> MediaServers
        {
            get
            {
                return iWatchableMediaServers;
            }
        }

        public void UnorderedOpen() { }

        public void UnorderedInitialised() { }

        public void UnorderedClose() { }

        public void UnorderedAdd(IDevice aItem)
        {
            aItem.Create<IProxyMediaServer>((server) =>
            {
                if (!iDisposed)
                {
                    // calculate where to insert the sender
                    int index = 0;
                    foreach (IProxyMediaServer ms in iWatchableMediaServers.Values)
                    {
                        if (server.ProductName.CompareTo(ms.ProductName) < 0)
                        {
                            break;
                        }
                        ++index;
                    }

                    // insert the sender
                    iMediaServerLookup.Add(aItem, server);
                    iWatchableMediaServers.Add(server, (uint)index);
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
            if (iMediaServerLookup.TryGetValue(aItem, out server))
            {
                // remove the corresponding sender from the watchable collection
                iMediaServerLookup.Remove(aItem);
                iWatchableMediaServers.Remove(server);

                server.Dispose();
            }
        }
    }

    public interface IStandardHouse
    {
        IWatchableOrdered<IStandardRoom> Rooms { get; }
        IWatchableOrdered<IProxyMediaServer> Servers { get; }
        INetwork Network { get; }
    }

    public class StandardHouse : IUnorderedWatcher<ITopology4Room>, IStandardHouse, IMockable, IDisposable
    {
        public StandardHouse(INetwork aNetwork)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aNetwork;

            iTopology1 = new Topology1(aNetwork);
            iTopology2 = new Topology2(iTopology1);
            iTopologym = new Topologym(iTopology2);
            iTopology3 = new Topology3(iTopologym);
            iTopology4 = new Topology4(iTopology3);
 
            iWatchableRooms = new WatchableOrdered<IStandardRoom>(iNetwork);
            iRoomLookup = new Dictionary<ITopology4Room, StandardRoom>();
            iRoomWatchers = new Dictionary<ITopology4Room, RoomWatcher>();

            iMediaServersWatcher = new MediaServersWatcher(aNetwork);
            iSendersWatcher = new SendersWatcher(aNetwork);

            iNetwork.Schedule(() =>
            {
                iTopology4.Rooms.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                iMediaServersWatcher.Dispose();
                iSendersWatcher.Dispose();

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
            iRoomLookup.Clear();
            iRoomWatchers.Clear();

            iTopology4.Dispose();
            iTopology3.Dispose();
            iTopologym.Dispose();
            iTopology2.Dispose();
            iTopology1.Dispose();
        }

        public IWatchableOrdered<IStandardRoom> Rooms
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableRooms;
                }
            }
        }

        public IWatchableOrdered<IProxyMediaServer> Servers
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iMediaServersWatcher.MediaServers;
                }
            }
        }

        public IWatchableOrdered<IProxySender> Senders
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iSendersWatcher.Senders;
                }
            }
        }

        public INetwork Network
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iNetwork;
                }
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
            StandardRoom room = new StandardRoom(iNetwork, aRoom);

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
            iRoomWatchers.Add(aRoom, new RoomWatcher(this, iWatchableRooms, room));
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

        public void Execute(IEnumerable<string> aValue)
        {
            using (iDisposeHandler.Lock)
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
        }

        internal void AddToZone(IDevice aDevice, StandardRoom aRoom)
        {
            foreach (StandardRoom r in iRoomLookup.Values)
            {
                if (r.AddToZone(aDevice, aRoom))
                {
                    break;
                }
            }
        }

        internal void RemoveFromZone(IDevice aDevice, StandardRoom aRoom)
        {
            foreach (StandardRoom r in iRoomLookup.Values)
            {
                if (r.RemoveFromZone(aDevice, aRoom))
                {
                    break;
                }
            }
        }

        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly Topology1 iTopology1;
        private readonly Topology2 iTopology2;
        private readonly Topologym iTopologym;
        private readonly Topology3 iTopology3;
        private readonly Topology4 iTopology4;

        private readonly WatchableOrdered<IStandardRoom> iWatchableRooms;
        private readonly Dictionary<ITopology4Room, StandardRoom> iRoomLookup;

        private readonly MediaServersWatcher iMediaServersWatcher;
        private readonly SendersWatcher iSendersWatcher;

        private readonly Dictionary<ITopology4Room, RoomWatcher> iRoomWatchers;
    }
}
