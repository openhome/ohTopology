using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ITopology6Room
    {
        string Name { get; }
        IWatchable<EStandby> Standby { get; }
        IWatchableUnordered<ITopology4Source> Sources { get; }

        void SetStandby(bool aValue);
        void SetMute(bool aValue);
        void SetVolume(uint Value);
        void VolumeInc();
        void VolumeDec();
    }

    public class Topology6Room : ITopology6Room, IDisposable
    {
        public Topology6Room(IWatchableThread aThread, ITopology5Room aRoom)
        {
            iThread = aThread;
            iRoom = aRoom;

            iName = iRoom.Name;
            iStandby = iRoom.Standby;
            iSources = iRoom.Sources;
        }

        public void Dispose()
        {
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

        public IWatchableUnordered<ITopology4Source> Sources
        {
            get
            {
                return iSources;
            }
        }

        public void SetStandby(bool aValue)
        {
            if (iRoom != null)
            {
                iRoom.SetStandby(aValue);
            }
        }

        public void SetMute(bool aValue)
        {
        }

        public void SetVolume(uint Value)
        {
        }

        public void VolumeInc()
        {
        }

        public void VolumeDec()
        {
        }

        private IWatchableThread iThread;
        private ITopology5Room iRoom;

        private string iName;
        private IWatchable<EStandby> iStandby;
        private IWatchableUnordered<ITopology4Source> iSources;
    }

    public class WatchableTopology6RoomUnordered : WatchableUnordered<ITopology6Room>
    {
        public WatchableTopology6RoomUnordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<ITopology6Room>();
        }

        public new void Add(ITopology6Room aValue)
        {
            iList.Add(aValue);
            base.Add(aValue);
        }

        public new void Remove(ITopology6Room aValue)
        {
            iList.Remove(aValue);
            base.Remove(aValue);
        }

        private List<ITopology6Room> iList;
    }

    public interface ITopology6
    {
        IWatchableUnordered<ITopology6Room> Rooms { get; }
    }

    public class Topology6 : ITopology6, IUnorderedWatcher<ITopology5Room>, IDisposable
    {

        public Topology6(IWatchableThread aThread, ITopology5 aTopology5)
        {
            iThread = aThread;
            iTopology5 = aTopology5;
            iDisposed = false;

            iRooms = new WatchableTopology6RoomUnordered(iThread);
            iRoomLookup = new Dictionary<ITopology5Room, Topology6Room>();

            iTopology5.Rooms.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology6.Dispose");
            }

            iTopology5.Rooms.RemoveWatcher(this);
            iTopology5 = null;

            foreach (Topology6Room r in iRoomLookup.Values)
            {
                r.Dispose();
            }
            iRoomLookup.Clear();
            iRoomLookup = null;

            iRooms.Dispose();
            iRooms = null;

            iDisposed = true;
        }

        public IWatchableUnordered<ITopology6Room> Rooms
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

        public void UnorderedAdd(ITopology5Room aItem)
        {
            Topology6Room room = new Topology6Room(iThread, aItem);

            iRooms.Add(room);
            iRoomLookup.Add(aItem, room);
        }

        public void UnorderedRemove(ITopology5Room aItem)
        {
            Topology6Room room = iRoomLookup[aItem];

            iRooms.Remove(room);
            iRoomLookup.Remove(aItem);

            iThread.Schedule(() =>
            {
                room.Dispose();
            });
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private ITopology5 iTopology5;

        private WatchableTopology6RoomUnordered iRooms;
        private Dictionary<ITopology5Room, Topology6Room> iRoomLookup;
    }
}
