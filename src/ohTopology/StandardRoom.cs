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

        public IMediaMetadata Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        private bool iEnabled;
        private IMediaMetadata iMetatext;
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

        IVolumeController CreateVolumeController();
    }

    class ZoneSender : IZoneSender, IDisposable
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
            iDisposeHandler = new DisposeHandler();
            iEnabled = aEnabled;
            iRoom = aRoom;
            iDevice = aDevice;

            iHasListeners = new Watchable<bool>(aRoom.Network, "HasListeners", false);
            iListeners = new List<StandardRoom>();
            iWatchableListeners = new WatchableOrdered<IStandardRoom>(aRoom.Network);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            foreach (StandardRoom r in iListeners)
            {
                r.RemovedFromZone(this);
            }
            iListeners.Clear();

            iHasListeners.Dispose();
            iWatchableListeners.Dispose();
        }

        public bool Enabled
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iEnabled;
                }
            }
        }

        public IStandardRoom Room
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iRoom;
                }
            }
        }

        public IDevice Sender
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iDevice;
                }
            }
        }

        public IWatchable<bool> HasListeners
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iHasListeners;
                }
            }
        }

        public IWatchableOrdered<IStandardRoom> Listeners
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iWatchableListeners;
                }
            }
        }

        public IVolumeController CreateVolumeController()
        {
            using (iDisposeHandler.Lock())
            {
                return new ZoneVolumeController(this);
            }
        }

        internal void AddToZone(StandardRoom aRoom)
        {
            iListeners.Add(aRoom);

            // calculate where to insert the sender
            int index = 0;
            foreach (IStandardRoom r in iWatchableListeners.Values)
            {
                if (r.Name.CompareTo(r.Name) < 0)
                {
                    break;
                }
                ++index;
            }

            // insert the room
            iWatchableListeners.Add(aRoom, (uint)index);
            iHasListeners.Update(iWatchableListeners.Values.Count() > 0);
            aRoom.AddedToZone(this);
        }

        internal void RemoveFromZone(StandardRoom aRoom)
        {
            iListeners.Remove(aRoom);

            iWatchableListeners.Remove(aRoom);
            iHasListeners.Update(iWatchableListeners.Values.Count() > 0);
            aRoom.RemovedFromZone(this);
        }

        private readonly DisposeHandler iDisposeHandler;
        private readonly bool iEnabled;
        private readonly StandardRoom iRoom;
        private readonly IDevice iDevice;
        private readonly Watchable<bool> iHasListeners;
        private readonly List<StandardRoom> iListeners;
        private readonly WatchableOrdered<IStandardRoom> iWatchableListeners;
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

        // multi-room interface
        IWatchableOrdered<IStandardRoom> Satellites { get; }
        IWatchable<IZoneSender> ZoneSender { get; }
        IWatchable<IZoneReceiver> ZoneReceiver { get; }

        IWatchable<ITopology4Source> Source { get; }
        IWatchable<IEnumerable<ITopology4Source>> Sources { get; }
        IEnumerable<ITopology4Group> Senders { get; }

        void SetStandby(bool aValue);
        void ListenTo(IStandardRoom aRoom);
        void Play(string aUri, IMediaMetadata aMetadata);

        IStandardRoomInfo CreateInfoController();
        IStandardRoomTime CreateTimeController();
        IStandardRoomController CreateController();
        IVolumeController CreateVolumeController();
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

            iZoneSender = new ZoneSender(this);
            iWatchableZoneSender = new Watchable<IZoneSender>(iNetwork, "ZoneSender", iZoneSender);
            iZoneReceiver = new ZoneReceiver(false);
            iWatchableZoneReceiver = new Watchable<IZoneReceiver>(iNetwork, "ZoneReceiver", iZoneReceiver);
            iSatellites = new WatchableOrdered<IStandardRoom>(iNetwork);

            iRoom.Roots.AddWatcher(this);
        }

        public void Dispose()
        {
            iNetwork.Execute(() =>
            {
                List<Action> linked = new List<Action>(iJoiners);
                foreach (Action a in linked)
                {
                    a();
                }
            });

            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                iRoom.Roots.RemoveWatcher(this);
                iDisposed = true;
            });

            iWatchableZoneSender.Dispose();
            iZoneSender = null;

            iWatchableZoneReceiver.Dispose();

            iRoots = null;
        }

        public string Name
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iRoom.Name;
                }
            }
        }

        public IWatchable<EStandby> Standby
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iRoom.Standby;
                }
            }
        }

        public IWatchable<ITopology4Source> Source
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iWatchableSource;
                }
            }
        }

        public IWatchable<IEnumerable<ITopology4Source>> Sources
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iWatchableSources;
                }
            }
        }

        public void Join(Action aAction)
        {
            using (iDisposeHandler.Lock())
            {
                iJoiners.Add(aAction);
            }
        }

        public void Unjoin(Action aAction)
        {
            using (iDisposeHandler.Lock())
            {
                iJoiners.Remove(aAction);
            }
        }

        public void SetStandby(bool aValue)
        {
            using (iDisposeHandler.Lock())
            {
                iRoom.SetStandby(aValue);
            }
        }

        public IWatchableOrdered<IStandardRoom> Satellites
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iSatellites;
                }
            }
        }

        public IWatchable<IZoneSender> ZoneSender
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iWatchableZoneSender;
                }
            }
        }

        public IWatchable<IZoneReceiver> ZoneReceiver
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iWatchableZoneReceiver;
                }
            }
        }

        public IEnumerable<ITopology4Group> Senders
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iSenders;
                }
            }
        }

        public void ListenTo(IStandardRoom aRoom)
        {
            using (iDisposeHandler.Lock())
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
                                    IMediaPreset preset = s.CreatePreset();
                                    preset.Play();
                                    preset.Dispose();
                                    receiver.Play(sender.Metadata.Value);
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
            using (iDisposeHandler.Lock())
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
                using (iDisposeHandler.Lock())
                {
                    return iNetwork;
                }
            }
        }

        public IStandardRoomInfo CreateInfoController()
        {
            using (iDisposeHandler.Lock())
            {
                return new StandardRoomInfo(this);
            }
        }

        public IStandardRoomTime CreateTimeController()
        {
            using (iDisposeHandler.Lock())
            {
                return new StandardRoomTime(this);
            }
        }

        public IStandardRoomController CreateController()
        {
            using (iDisposeHandler.Lock())
            {
                return new StandardRoomController(this);
            }
        }

        public IVolumeController CreateVolumeController()
        {
            using (iDisposeHandler.Lock())
            {
                return new StandardVolumeController(this);
            }
        }

        public void UnorderedOpen() { }

        public void UnorderedInitialised() { }

        public void UnorderedClose() { }

        public void ItemOpen(string aId, IEnumerable<ITopology4Root> aValue)
        {
            Do.Assert(iWatchableSources == null);

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

            EvaluateZoneReceiver();
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

            EvaluateZoneReceiver();
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Root> aValue)
        {
            iRoots = null;

            foreach (ITopology4Root r in aValue)
            {
                r.Source.RemoveWatcher(this);
                r.Senders.RemoveWatcher(this);
            }

            iWatchableSources.Dispose();
            iWatchableSources = null;

            // iSource = null;

            EvaluateZoneReceiver();
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
            // iSource = source;
        }

        private void SelectSource()
        {
            ITopology4Source source = iCurrentSources[0];

            iWatchableSource.Update(source);
            // iSource = source;
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Group> aValue)
        {
            iSenders.AddRange(aValue);
            EvaluateZoneSender(aValue);
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Group> aValue, IEnumerable<ITopology4Group> aPrevious)
        {
            foreach (ITopology4Group g in aPrevious)
            {
                iSenders.Remove(g);
            }
            iSenders.AddRange(aValue);
            EvaluateZoneSender(aValue);
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Group> aValue)
        {
            foreach (ITopology4Group g in aValue)
            {
                iSenders.Remove(g);
            }
        }

        private void EvaluateZoneSender(IEnumerable<ITopology4Group> aValue)
        {
            if (iRoots.Count() == 1 && aValue.Count() > 0)
            {
                ITopology4Root root = iRoots.First();
                foreach (ITopology4Group g in aValue)
                {
                    if (root == g)
                    {
                        if (!iWatchableZoneSender.Value.Enabled)
                        {
                            ZoneSender s = iZoneSender;
                            iZoneSender = new ZoneSender(this, root.Device);
                            iWatchableZoneSender.Update(iZoneSender);
                            s.Dispose();
                        }
                        break;
                    }
                    /*else if (aValue.Count() == 1)
                    {
                        if (!iWatchableZoneSender.Value.Enabled)
                        {
                            ZoneSender s = iZoneSender;
                            iZoneSender = new ZoneSender(this, g.Device);
                            iWatchableZoneSender.Update(iZoneSender);
                            s.Dispose();
                        }
                        break;
                    }*/
                }
            }
            else
            {
                if (iWatchableZoneSender.Value.Enabled)
                {
                    ZoneSender s = iZoneSender;
                    iZoneSender = new ZoneSender(this);
                    iWatchableZoneSender.Update(iZoneSender);
                    s.Dispose();
                }
            }
        }

        private void EvaluateZoneReceiver()
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

        internal void AddSatellite(IStandardRoom aRoom)
        {
            // calculate where to insert the room
            uint index = 0;
            foreach (IStandardRoom r in iSatellites.Values)
            {
                if (aRoom.Name.CompareTo(r.Name) < 0)
                {
                    break;
                }
                ++index;
            }

            iSatellites.Add(aRoom, index);
        }

        internal void RemoveSatellite(IStandardRoom aRoom)
        {
            iSatellites.Remove(aRoom);
        }

        internal bool AddToZone(IDevice aDevice, StandardRoom aRoom)
        {
            if (iZoneSender.Sender == aDevice)
            {
                iZoneSender.AddToZone(aRoom);
                return true;
            }

            return false;
        }

        internal bool RemoveFromZone(IDevice aDevice, StandardRoom aRoom)
        {
            if (iZoneSender.Sender == aDevice)
            {
                iZoneSender.RemoveFromZone(aRoom);
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

        // private ITopology4Source iSource;

        private readonly List<ITopology4Source> iCurrentSources;
        private List<ITopology4Source> iSources;
        private readonly List<ITopology4Group> iSenders;
        private Watchable<ITopology4Source> iWatchableSource;
        private Watchable<IEnumerable<ITopology4Source>> iWatchableSources;

        private readonly WatchableOrdered<IStandardRoom> iSatellites;
        private ZoneSender iZoneSender;
        private readonly Watchable<IZoneSender> iWatchableZoneSender;
        private ZoneReceiver iZoneReceiver;
        private readonly Watchable<IZoneReceiver> iWatchableZoneReceiver;
    }
}
