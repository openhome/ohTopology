using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    interface IInvoker
    {
        void SetUri(string aUri, string aMetadata);
        void Play();
    }

    public class RoomDetails : IInfoDetails
    {
        internal RoomDetails()
        {
            iEnabled = false;
        }

        internal RoomDetails(IInfoDetails aDetails)
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

        public string Metadata
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
        private string iMetadata;
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
        public InfoWatcher(IWatchableThread aThread, IWatchableDevice aDevice, Watchable<RoomDetails> aDetails, Watchable<RoomMetadata> aMetadata, Watchable<RoomMetatext> aMetatext)
        {
            iLock = new object();
            iDisposed = false;

            iDevice = aDevice;
            iDetails = aDetails;
            iMetadata = aMetadata;
            iMetatext = aMetatext;

            iDevice.Create<ServiceInfo>((IWatchableDevice device, ServiceInfo info) =>
            {
                lock (iLock)
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
                }
            });
        }

        public void Dispose()
        {
            lock (iLock)
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

                    iInfo = null;
                }

                iDisposed = true;
            }
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

        private object iLock;
        private bool iDisposed;
        private ServiceInfo iInfo;

        private IWatchableDevice iDevice;
        private Watchable<RoomDetails> iDetails;
        private Watchable<RoomMetadata> iMetadata;
        private Watchable<RoomMetatext> iMetatext;
    }

    public interface IZone
    {
        IRoom Room { get; }
    }

    public interface IStandardRoom
    {
        string Name { get; }
        
        IWatchable<EStandby> Standby { get; }
        IWatchable<RoomDetails> Details { get; }
        IWatchable<RoomMetadata> Metadata { get; }
        IWatchable<RoomMetatext> Metatext { get; }

        IWatchable<bool> HasSender { get; }
        IWatchable<bool> HasReceiver {get;}
        IWatchableUnordered<IZone> Listeners { get; }

        IWatchable<ITopology4Source> Source { get; }

        StandardRoomController Create();

        void SetStandby(bool aValue);
        void Play(string aId, string aMetadata, string aUri);
    }

    public class StandardRoom : IStandardRoom, ITopologyObject, IWatcher<IEnumerable<ITopology4Root>>, IWatcher<ITopology4Source>, IDisposable
    {
        public StandardRoom(IWatchableThread aThread, StandardHouse aHouse, ITopology4Room aRoom)
        {
            iThread = aThread;
            iHouse = aHouse;
            iRoom = aRoom;

            iControllers = new List<StandardRoomController>();
            iRoots = new List<ITopology4Root>();
            iSources = new List<ITopology4Source>();

            iStandby = new WatchableProxy<EStandby>(iRoom.Standby);

            iDetails = new Watchable<RoomDetails>(iThread, string.Format("Details({0})", aRoom.Name), new RoomDetails());
            iMetadata = new Watchable<RoomMetadata>(iThread, string.Format("Metadata({0})", aRoom.Name), new RoomMetadata());
            iMetatext = new Watchable<RoomMetatext>(iThread, string.Format("Metatext({0})", aRoom.Name), new RoomMetatext());

            iRoom.Roots.AddWatcher(this);
        }

        public void Detach()
        {
            iRoom.Roots.RemoveWatcher(this);

            iStandby.Detach();

            List<StandardRoomController> controllers = new List<StandardRoomController>(iControllers);
            foreach (StandardRoomController c in controllers)
            {
                c.SetInactive();
            }
            if (iControllers.Count > 0)
            {
                throw new Exception("iControllers.Count > 0");
            }
            iControllers = null;
        }

        public void Dispose()
        {
            iStandby.Dispose();
            iStandby = null;

            iDetails.Dispose();
            iDetails = null;

            iMetadata.Dispose();
            iMetadata = null;

            iMetatext.Dispose();
            iMetatext = null;

            iRoots = null;
            iRoom = null;
            iHouse = null;
            iThread = null;
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
                return iStandby;
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

        public StandardRoomController Create()
        {
            return new StandardRoomController(iThread, this);
        }

        public void SetStandby(bool aValue)
        {
            iRoom.SetStandby(aValue);
        }

        public IWatchable<bool> HasSender
        {
            get
            {
                return iHasSender;
            }
        }

        public IWatchable<bool> HasReceiver
        {
            get
            {
                return iHasReceiver;
            }
        }

        public IWatchableUnordered<IZone> Listeners
        {
            get
            {
                return iListeners;
            }
        }

        public void Play(string aId, string aMetadata, string aUri)
        {
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
            iRoots = aValue;

            foreach (ITopology4Root r in aValue)
            {
                r.Source.AddWatcher(this);
            }

            SelectFirstSource();
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Root> aValue, IEnumerable<ITopology4Root> aPrevious)
        {
            foreach (ITopology4Root r in aPrevious)
            {
                r.Source.RemoveWatcher(this);
            }

            iRoots = aValue;

            foreach (ITopology4Root r in aValue)
            {
                r.Source.AddWatcher(this);
            }

            SelectSource();
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Root> aValue)
        {
            iRoots = null;

            foreach (ITopology4Root r in aValue)
            {
                r.Source.RemoveWatcher(this);
            }

            iInfoWatcher.Dispose();
            iInfoWatcher = null;

            iSource = null;
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            iSources.Add(aValue);
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            iSources.Remove(aPrevious);
            iSources.Add(aValue);

            SelectSource();
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            iSources.Remove(aValue);
        }

        private void SelectFirstSource()
        {
            ITopology4Source source = iSources[0];
            iWatchableSource = new Watchable<ITopology4Source>(iThread, string.Format("Source({0})", iRoom.Name), source);
            iInfoWatcher = new InfoWatcher(iThread, source.Device, iDetails, iMetadata, iMetatext);
            iSource = source;
        }

        private void SelectSource()
        {
            ITopology4Source source = iSources[0];

            foreach (ITopology4Source s in iSources)
            {
                // if we find the same source as was previously selected
                if (iSource.Index == s.Index && iSource.Device == s.Device)
                {
                    // if same source has different data update the source
                    if (!Topology4SourceComparer.Equals(iSource, source))
                    {
                        iWatchableSource.Update(s);
                        iSource = s;
                        return;
                    }

                    iSource = s;
                    return;
                }
            }

            if (iSource.Device != source.Device)
            {
                iInfoWatcher.Dispose();
                iInfoWatcher = null;

                iInfoWatcher = new InfoWatcher(iThread, source.Device, iDetails, iMetadata, iMetatext);
            }

            iWatchableSource.Update(source);
            iSource = source;
        }

        internal void AddController(StandardRoomController aController)
        {
            iControllers.Add(aController);
        }

        internal void RemoveController(StandardRoomController aController)
        {
            iControllers.Remove(aController);
        }

        private IWatchableThread iThread;
        private StandardHouse iHouse;
        private ITopology4Room iRoom;

        private IEnumerable<ITopology4Root> iRoots;
        private List<StandardRoomController> iControllers;
        private ITopology4Source iSource;
        private List<ITopology4Source> iSources;
        private Watchable<ITopology4Source> iWatchableSource;

        private Watchable<bool> iHasSender;
        private Watchable<bool> iHasReceiver;
        private WatchableUnordered<IZone> iListeners;

        private WatchableProxy<EStandby> iStandby;
        private InfoWatcher iInfoWatcher;
        private Watchable<RoomDetails> iDetails;
        private Watchable<RoomMetadata> iMetadata;
        private Watchable<RoomMetatext> iMetatext;
    }

    public class StandardHouse : IUnorderedWatcher<ITopology4Room>, IDisposable
    {
        public StandardHouse(IWatchableThread aThread, ITopology4 aTopology4)
        {
            iThread = aThread;
            iTopology4 = aTopology4;

            iWatchableRooms = new WatchableOrdered<IStandardRoom>(iThread);
            iRooms = new List<StandardRoom>();
            iRoomLookup = new Dictionary<ITopology4Room, StandardRoom>();

            iThread.Schedule(() =>
            {
                iTopology4.Rooms.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            if (iTopology4 == null)
            {
                throw new ObjectDisposedException("LinnHouse.Dispose");
            }

            iThread.Execute(() =>
            {
                iTopology4.Rooms.RemoveWatcher(this);

                foreach (StandardRoom room in iRooms)
                {
                    room.Detach();
                }
            });
            iWatchableRooms.Dispose();
            iWatchableRooms = null;

            foreach (StandardRoom room in iRooms)
            {
                room.Dispose();
            }
            iRoomLookup.Clear();
            iRoomLookup = null;

            iTopology4 = null;
        }

        public IWatchableOrdered<IStandardRoom> Rooms
        {
            get
            {
                return iWatchableRooms;
            }
        }

        // IUnorderedWatcher<ITopology4Room>

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedAdd(ITopology4Room aRoom)
        {
            StandardRoom room = new StandardRoom(iThread, this, aRoom);

            // calculate where to insert the room
            int index = 0;
            for (; index < iRooms.Count; ++index)
            {
                if (room.Name.CompareTo(iRooms[index].Name) < 0)
                {
                    break;
                }
            }

            // insert the room
            iRoomLookup.Add(aRoom, room);
            iRooms.Insert(index, room);
            iWatchableRooms.Add(room, (uint)index);
        }

        public void UnorderedRemove(ITopology4Room aRoom)
        {
            // remove the corresponding Room from the watchable collection
            StandardRoom room = iRoomLookup[aRoom];

            int index = iRooms.IndexOf(room);

            iRoomLookup.Remove(aRoom);
            iRooms.RemoveAt(index);
            iWatchableRooms.Remove(room, (uint)index);

            // detach the room from topology L5
            room.Detach();

            // schedule the Room object for disposal
            iThread.Schedule(() =>
            {
                room.Dispose();
            });
        }

        public void UnorderedClose()
        {
        }

        private IWatchableThread iThread;
        private ITopology4 iTopology4;

        private WatchableOrdered<IStandardRoom> iWatchableRooms;
        private List<StandardRoom> iRooms;
        private Dictionary<ITopology4Room, StandardRoom> iRoomLookup;
    }
}
