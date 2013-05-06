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

    public interface IStandardRoomController : ISourceController
    {
        IWatchable<bool> Active { get; }

        IWatchable<EStandby> Standby { get; }
        void SetStandby(bool aValue);
        
        IWatchable<bool> HasVolume { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Volume { get; }
        void SetMute(bool aMute);
        void SetVolume(uint aVolume);
        void VolumeInc();
        void VolumeDec();
    }

    public class StandardRoomController : IWatcher<ITopology4Source>, IDisposable
    {
        internal StandardRoomController(IWatchableThread aThread, StandardRoom aRoom)
        {
            iThread = aThread;
            iRoom = aRoom;

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(aThread, string.Format("Active({0})", aRoom.Name), true);

            iStandby = new WatchableProxy<EStandby>(aRoom.Standby);

            iHasSourceControl = new Watchable<bool>(aThread, string.Format("HasSourceControl({0})", aRoom.Name), false);
            iHasInfoNext = new Watchable<bool>(aThread, string.Format("HasInfoNext({0})", aRoom.Name), false);
            iInfoNext = new Watchable<IInfoMetadata>(iThread, string.Format("InfoNext({0})", iRoom.Name), new RoomMetadata());
            
            iHasVolume = new Watchable<bool>(aThread, string.Format("HasVolume({0})", aRoom.Name), false);
            iMute = new Watchable<bool>(aThread, string.Format("Mute({0})", aRoom.Name), false);
            iValue = new Watchable<uint>(aThread, string.Format("Volume({0})", aRoom.Name), 0);

            iCanPause = new Watchable<bool>(aThread, string.Format("CanPause({0})", aRoom.Name), false);
            iCanSkip = new Watchable<bool>(aThread, string.Format("CanSkip({0})", aRoom.Name), false);
            iCanSeek = new Watchable<bool>(aThread, string.Format("CanSeek({0})", aRoom.Name), false);
            iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0})", aRoom.Name), string.Empty);

            iRoom.Source.AddWatcher(this);

            iRoom.AddController(this);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iIsActive)
                {
                    iStandby.Detach();

                    iRoom.Source.RemoveWatcher(this);
                    iRoom.RemoveController(this);
                }
            }

            iRoom = null;

            iActive.Dispose();
            iActive = null;

            iStandby.Dispose();
            iStandby = null;

            iHasSourceControl.Dispose();
            iHasSourceControl = null;

            iHasInfoNext.Dispose();
            iHasInfoNext = null;

            iInfoNext.Dispose();
            iInfoNext = null;

            iHasVolume.Dispose();
            iHasVolume = null;

            iMute.Dispose();
            iMute = null;

            iValue.Dispose();
            iValue = null;

            iTransportState.Dispose();
            iTransportState = null;

            iCanPause.Dispose();
            iCanPause = null;

            iCanSkip.Dispose();
            iCanSkip = null;

            iCanSeek.Dispose();
            iCanSeek = null;
        }

        internal void SetInactive()
        {
            lock (iLock)
            {
                iIsActive = false;

                iActive.Update(false);

                iStandby.Detach();

                iRoom.Source.RemoveWatcher(this);
                iRoom.RemoveController(this);
            }
        }

        public IWatchable<bool> Active
        {
            get
            {
                return iActive;
            }
        }

        public IWatchable<EStandby> Standby
        {
            get
            {
                return iStandby;
            }
        }

        public void SetStandby(bool aValue)
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iRoom.SetStandby(aValue);
                }
            });
        }

        public IWatchable<bool> HasVolume
        {
            get
            {
                return iHasVolume;
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                return iMute;
            }
        }
        public IWatchable<uint> Volume
        {
            get
            {
                return iValue;
            }
        }

        public void SetMute(bool aMute)
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iVolumeController.SetMute(aMute);
                }
            });
        }

        public void SetVolume(uint aVolume)
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasVolume.Value)
                    {
                        iVolumeController.SetVolume(aVolume);
                    }
                }
            });
        }

        public void VolumeInc()
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasVolume.Value)
                    {
                        iVolumeController.VolumeInc();
                    }
                }
            });
        }

        public void VolumeDec()
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    if (iHasVolume.Value)
                    {
                        iVolumeController.VolumeDec();
                    }
                }
            });
        }

        public string Name
        {
            get
            {
                return iRoom.Name;
            }
        }

        public IWatchable<bool> HasSourceControl
        {
            get
            {
                return iHasSourceControl;
            }
        }

        public IWatchable<bool> HasInfoNext
        {
            get
            {
                return iHasInfoNext;
            }
        }

        public IWatchable<IInfoMetadata> InfoNext
        {
            get
            {
                return iInfoNext;
            }
        }

        public IWatchable<string> TransportState
        {
            get
            {
                return iTransportState;
            }
        }

        public IWatchable<bool> CanPause
        {
            get
            {
                return iCanPause;
            }
        }

        public void Play()
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iSourceController.Play();
                }
            });
        }

        public void Pause()
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iSourceController.Pause();
                }
            });
        }

        public void Stop()
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iSourceController.Stop();
                }
            });
        }

        public IWatchable<bool> CanSkip
        {
            get
            {
                return iCanSkip;
            }
        }

        public void Previous()
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iSourceController.Previous();
                }
            });
        }

        public void Next()
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iSourceController.Next();
                }
            });
        }

        public IWatchable<bool> CanSeek
        {
            get
            {
                return iCanSeek;
            }
        }

        public void Seek(uint aSeconds)
        {
            iThread.Schedule(() =>
            {
                if (iActive.Value)
                {
                    iSourceController.Seek(aSeconds);
                }
            });
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            iSourceController = SourceController.Create(iThread, aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iTransportState, iCanPause, iCanSkip, iCanSeek);

            if (aValue.VolumeDevices.Count() > 0)
            {
                IWatchableDevice device = aValue.VolumeDevices.ElementAt(0);
                iVolumeController = new VolumeController(iThread, device, iHasVolume, iMute, iValue);
            }
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if (iSourceController != null)
            {
                iSourceController.Dispose();
                iSourceController = null;
            }

            iSourceController = SourceController.Create(iThread, aValue, iHasSourceControl, iHasInfoNext, iInfoNext, iTransportState, iCanPause, iCanSkip, iCanSeek);

            if (aValue.VolumeDevices.Count() > 0)
            {
                IWatchableDevice device = aValue.VolumeDevices.ElementAt(0);
                if (iVolumeController != null)
                {
                    if (device != iVolumeController.Device)
                    {
                        iVolumeController.Dispose();
                        iVolumeController = new VolumeController(iThread, device, iHasVolume, iMute, iValue);
                    }
                }
                else
                {
                    iVolumeController = new VolumeController(iThread, device, iHasVolume, iMute, iValue);
                }
            }
            else
            {
                if (iVolumeController != null)
                {
                    iVolumeController.Dispose();
                    iVolumeController = null;
                }
            }
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            if (iSourceController != null)
            {
                iSourceController.Dispose();
                iSourceController = null;
            }

            if (iVolumeController != null)
            {
                iVolumeController.Dispose();
                iVolumeController = null;
            }
        }

        private IWatchableThread iThread;
        private StandardRoom iRoom;

        private object iLock;
        private bool iIsActive;
        private Watchable<bool> iActive;
        private WatchableProxy<EStandby> iStandby;

        private VolumeController iVolumeController;
        private Watchable<bool> iHasVolume;
        private Watchable<bool> iMute;
        private Watchable<uint> iValue;

        private ISourceController iSourceController;
        private Watchable<bool> iHasSourceControl;
        private Watchable<bool> iHasInfoNext;
        private Watchable<IInfoMetadata> iInfoNext;
        private Watchable<string> iTransportState;
        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSkip;
        private Watchable<bool> iCanSeek;
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

    public interface IStandardRoom
    {
        string Name { get; }
        
        IWatchable<EStandby> Standby { get; }
        IWatchable<RoomDetails> Details { get; }
        IWatchable<RoomMetadata> Metadata { get; }
        IWatchable<RoomMetatext> Metatext { get; }

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
            
            iName = aRoom.Name;

            iControllers = new List<StandardRoomController>();
            iRoots = new List<ITopology4Root>();
            iSources = new List<ITopology4Source>();

            iStandby = new WatchableProxy<EStandby>(iRoom.Standby);

            iDetails = new Watchable<RoomDetails>(iThread, string.Format("Details({0})", iName), new RoomDetails());
            iMetadata = new Watchable<RoomMetadata>(iThread, string.Format("Metadata({0})", iName), new RoomMetadata());
            iMetatext = new Watchable<RoomMetatext>(iThread, string.Format("Metatext({0})", iName), new RoomMetatext());

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
                return iName;
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
            iWatchableSource = new Watchable<ITopology4Source>(iThread, string.Format("Source({0})", iName), source);
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
        private string iName;

        private IEnumerable<ITopology4Root> iRoots;
        private List<StandardRoomController> iControllers;
        private ITopology4Source iSource;
        private List<ITopology4Source> iSources;
        private Watchable<ITopology4Source> iWatchableSource;

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
