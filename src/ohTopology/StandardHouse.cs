using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class SatelliteWatcher : IWatcher<IEnumerable<ITopology4Source>>, IWatcher<IZoneSender>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly StandardHouse iHouse;
        private readonly StandardRoom iRoom;
        private readonly List<StandardRoom> iRooms;
        private string iMasterRoom;
        private bool iIsSatellite;

        public SatelliteWatcher(StandardHouse aHouse, StandardRoom aRoom, IEnumerable<StandardRoom> aRooms)
        {
            iDisposeHandler = new DisposeHandler();
            iHouse = aHouse;
            iRoom = aRoom;

            iMasterRoom = string.Empty;
            iIsSatellite = false;
            iRooms = new List<StandardRoom>();

            foreach (StandardRoom r in aRooms)
            {
                RoomAdded(r);
            }

            iRoom.Sources.AddWatcher(this);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iRoom.Sources.RemoveWatcher(this);

            List<StandardRoom> list = new List<StandardRoom>(iRooms);
            foreach (StandardRoom r in list)
            {
                RoomRemoved(r);
            }
            Do.Assert(iRooms.Count == 0);
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Receiver")
                {
                    iMasterRoom = s.Name;
                    iIsSatellite = iHouse.AddSatellite(s.Name, iRoom);
                    break;
                }
            }
            if (!iIsSatellite)
            {
                iHouse.AddRoom(iRoom);
            }
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            bool wasSatellite = false;
            string oldMaster = iMasterRoom;
            // set iMasterRoom to be empty here to cater for the case that 
            // we get a byebye from a DS before the byebye from its proxied device 
            iMasterRoom = string.Empty;
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Receiver")
                {
                    iMasterRoom = s.Name;
                    break;
                }
            }

            if (oldMaster != iMasterRoom)
            {
                bool remove = false;
                bool add = false;
                if (!string.IsNullOrEmpty(oldMaster))
                {
                    wasSatellite = iHouse.RemoveSatellite(oldMaster, iRoom);
                    if (!wasSatellite)
                    {
                        remove = true;
                    }
                }
                else
                {
                    remove = true;
                }

                iIsSatellite = false;
                
                if (!string.IsNullOrEmpty(iMasterRoom))
                {
                    iIsSatellite = iHouse.AddSatellite(iMasterRoom, iRoom);
                    if (!iIsSatellite)
                    {
                        add = true;
                    }
                }
                else
                {
                    add = true;
                }

                if (remove != add)
                {
                    if (remove)
                    {
                        iHouse.RemoveRoom(iRoom);
                    }
                    if (add)
                    {
                        iHouse.AddRoom(iRoom);
                    }
                }
            }
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
            bool wasSatellite = false;
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Receiver")
                {
                    //iMasterRoom = string.Empty;
                    wasSatellite = iHouse.RemoveSatellite(s.Name, iRoom);
                    break;
                }
            }
            if (!wasSatellite)
            {
                iHouse.RemoveRoom(iRoom);
            }
            iIsSatellite = false;
        }

        public void RoomAdded(StandardRoom aRoom)
        {
            iRooms.Add(aRoom);
            aRoom.ZoneSender.AddWatcher(this);
        }

        public void RoomRemoved(StandardRoom aRoom)
        {
            aRoom.ZoneSender.RemoveWatcher(this);
            iRooms.Remove(aRoom);
        }

        public void ItemOpen(string aId, IZoneSender aValue)
        {
            if (aValue.Enabled && aValue.Room.Name == iMasterRoom)
            {
                if (iHouse.AddSatellite(iMasterRoom, iRoom))
                {
                    iHouse.RemoveRoom(iRoom);
                    iIsSatellite = true;
                }
            }
        }

        public void ItemUpdate(string aId, IZoneSender aValue, IZoneSender aPrevious)
        {
            if (aPrevious.Enabled && aValue.Room.Name == iMasterRoom)
            {
                foreach (var r in iRooms)
                {
                    if (r.Name == iMasterRoom)
                    {
                        r.RemoveSatellite(iRoom);
                        iHouse.AddRoom(iRoom);
                        iIsSatellite = false;
                    }
                }
            }
            if (aValue.Enabled && aValue.Room.Name == iMasterRoom)
            {
                if (iHouse.AddSatellite(iMasterRoom, iRoom))
                {
                    iHouse.RemoveRoom(iRoom);
                    iIsSatellite = true;
                }
            }
        }

        public void ItemClose(string aId, IZoneSender aValue)
        {
            if (aValue.Enabled && aValue.Room.Name == iMasterRoom)
            {
                foreach (var r in iRooms)
                {
                    if (r.Name == iMasterRoom && iIsSatellite)
                    {
                        r.RemoveSatellite(iRoom);
                        iHouse.AddRoom(iRoom);
                        iIsSatellite = false;
                    }
                }
            }
        }
    }

    class RoomWatcher : IWatcher<ITopology4Source>, IWatcher<ITopologymSender>, IWatcher<IZoneSender>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly StandardHouse iHouse;
        private readonly StandardRoom iRoom;
        private readonly List<IStandardRoom> iRooms;
        private readonly IWatchable<ITopology4Source> iSource;
        private ITopologymSender iSender;

        public RoomWatcher(StandardHouse aHouse, StandardRoom aRoom, IEnumerable<StandardRoom> aRooms)
        {
            iDisposeHandler = new DisposeHandler();
            iHouse = aHouse;
            iRoom = aRoom;
            iSource = iRoom.Source;
            iRooms = new List<IStandardRoom>();

            foreach(IStandardRoom r in aRooms)
            {
                RoomAdded(r);
            }

            iSource.AddWatcher(this);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            foreach (IStandardRoom r in iRooms)
            {
                if (r != iRoom)
                {
                    r.ZoneSender.RemoveWatcher(this);
                }
            }

            iSource.RemoveWatcher(this);
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
            if (!(aPrevious.Enabled && aValue.Enabled && aPrevious.Device == aValue.Device))
            {
                if (aPrevious.Enabled)
                {
                    iHouse.RemoveFromZone(aPrevious.Device, iRoom);
                }
                if (aValue.Enabled)
                {
                    iHouse.AddToZone(aValue.Device, iRoom);
                }
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

        public void RoomAdded(IStandardRoom aRoom)
        {
            iRooms.Add(aRoom);
            if (aRoom != iRoom)
            {
                aRoom.ZoneSender.AddWatcher(this);
            }
        }

        public void RoomRemoved(IStandardRoom aRoom)
        {
            iRooms.Remove(aRoom);
            if (aRoom != iRoom)
            {
                aRoom.ZoneSender.RemoveWatcher(this);
            }
        }

        public void ItemOpen(string aId, IZoneSender aValue)
        {
            if (aValue.Enabled)
            {
                if (iSender != null && iSender.Enabled)
                {
                    if (aValue.Sender == iSender.Device)
                    {
                        iHouse.AddToZone(iSender.Device, iRoom);
                    }
                }
            }
        }

        public void ItemUpdate(string aId, IZoneSender aValue, IZoneSender aPrevious)
        {
            if (aValue.Enabled)
            {
                if (iSender != null && iSender.Enabled)
                {
                    if (aValue.Sender == iSender.Device)
                    {
                        iHouse.AddToZone(iSender.Device, iRoom);
                    }
                }
            }
        }

        public void ItemClose(string aId, IZoneSender aValue) { }
    }

    class SendersList : IUnorderedWatcher<IDevice>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly WatchableOrdered<IProxySender> iWatchableSenders;
        private IWatchableUnordered<IDevice> iSenders;
        private readonly Dictionary<IDevice, IProxySender> iSenderLookup;
        private bool iDisposed;

        public SendersList(INetwork aNetwork)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aNetwork;
            iDisposed = false;
            iWatchableSenders = new WatchableOrdered<IProxySender>(aNetwork);
            iSenderLookup = new Dictionary<IDevice, IProxySender>();

            iNetwork.Schedule(() =>
            {
                iSenders = aNetwork.Create<IProxySender>();
                iSenders.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                iSenders.RemoveWatcher(this);
            });

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
                using (iDisposeHandler.Lock())
                {
                    return iWatchableSenders;
                }
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

    public interface IStandardHouse
    {
        IWatchableOrdered<IStandardRoom> Rooms { get; }
        IWatchableOrdered<IProxySender> Senders { get; }
        IWatchable<IEnumerable<ITopology4Registration>> Registrations { get; }
        INetwork Network { get; }
    }

    public class StandardHouse : IUnorderedWatcher<ITopology4Room>, IWatcher<IEnumerable<ITopology4Registration>>, IStandardHouse, IMockable, IDisposable
    {
        public StandardHouse(INetwork aNetwork, ILog aLog)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aNetwork;

            iTopology1 = new Topology1(aNetwork, aLog);
            iTopology2 = new Topology2(iTopology1, aLog);
            iTopologym = new Topologym(iTopology2, aLog);
            iTopology3 = new Topology3(iTopologym, aLog);
            iTopology4 = new Topology4(iTopology3, aLog);
 
            iWatchableRooms = new WatchableOrdered<IStandardRoom>(iNetwork);
            iRoomLookup = new Dictionary<ITopology4Room, StandardRoom>();
            iRoomWatchers = new Dictionary<ITopology4Room, RoomWatcher>();
            iRegistrations = new Watchable<IEnumerable<ITopology4Registration>>(iNetwork, "Registrations", new List<ITopology4Registration>());
            iSatelliteWatchers = new Dictionary<ITopology4Room, SatelliteWatcher>();

            iSendersList = new SendersList(aNetwork);

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
                iTopology4.Rooms.RemoveWatcher(this);

                // remove listeners from zones before disposing of zones
                foreach (var kvp in iRoomWatchers)
                {
                    kvp.Key.Registrations.RemoveWatcher(this);
                    kvp.Value.Dispose();
                }

                foreach (var kvp in iSatelliteWatchers)
                {
                    kvp.Value.Dispose();
                }

                foreach (var kvp in iRoomLookup)
                {
                    kvp.Value.Dispose();
                }

                iSendersList.Dispose();
            });
            iWatchableRooms.Dispose();
            iRoomLookup.Clear();
            iRoomWatchers.Clear();
            iSatelliteWatchers.Clear();
            iRegistrations.Dispose();

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
                using (iDisposeHandler.Lock())
                {
                    return iWatchableRooms;
                }
            }
        }

        public IWatchableOrdered<IProxySender> Senders
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iSendersList.Senders;
                }
            }
        }

        public IWatchable<IEnumerable<ITopology4Registration>> Registrations
        {
            get
            {
                return iRegistrations;
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
            aRoom.Registrations.AddWatcher(this);

            StandardRoom room = new StandardRoom(iNetwork, aRoom);

            iRoomLookup.Add(aRoom, room);

            foreach (var kvp in iSatelliteWatchers)
            {
                kvp.Value.RoomAdded(room);
            }

            iSatelliteWatchers.Add(aRoom, new SatelliteWatcher(this, room, iRoomLookup.Values));

            foreach (var kvp in iRoomWatchers)
            {
                kvp.Value.RoomAdded(room);
            }

            // do this here so that room is added before other rooms are informed of this room listening to them
            iRoomWatchers.Add(aRoom, new RoomWatcher(this, room, iRoomLookup.Values));
        }

        public void UnorderedRemove(ITopology4Room aRoom)
        {
            aRoom.Registrations.RemoveWatcher(this);

            // remove the corresponding Room from the watchable collection
            StandardRoom room = iRoomLookup[aRoom];

            foreach (var kvp in iRoomWatchers)
            {
                kvp.Value.RoomRemoved(room);
            }

            iRoomWatchers[aRoom].Dispose();
            iRoomWatchers.Remove(aRoom);

            foreach (var kvp in iSatelliteWatchers)
            {
                kvp.Value.RoomRemoved(room);
            }

            iSatelliteWatchers[aRoom].Dispose();
            iSatelliteWatchers.Remove(aRoom);

            iRoomLookup.Remove(aRoom);

            room.Dispose();
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Registration> aValue)
        {
            List<ITopology4Registration> list = iRegistrations.Value.ToList();
            list.AddRange(aValue);
            iRegistrations.Update(list);
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Registration> aValue, IEnumerable<ITopology4Registration> aPrevious)
        {
            List<ITopology4Registration> list = iRegistrations.Value.ToList();
            foreach (var r in aPrevious)
            {
                list.Remove(r);
            }
            list.AddRange(aValue);
            iRegistrations.Update(list);
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Registration> aValue)
        {
            List<ITopology4Registration> list = iRegistrations.Value.ToList();
            foreach (var r in aValue)
            {
                list.Remove(r);
            }
            iRegistrations.Update(list);
        }

        public void Execute(IEnumerable<string> aValue)
        {
            using (iDisposeHandler.Lock())
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

                                    name = string.Join(" ", value);

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

        internal void AddRoom(StandardRoom aRoom)
        {
            // calculate where to insert the room
            uint index = 0;
            foreach (IStandardRoom r in iWatchableRooms.Values)
            {
                if (aRoom.Name.CompareTo(r.Name) < 0)
                {
                    break;
                }
                ++index;
            }

            // insert the room
            iWatchableRooms.Add(aRoom, index);
        }

        internal void RemoveRoom(StandardRoom aRoom)
        {
            iWatchableRooms.Remove(aRoom);
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
                if (r != aRoom && r.RemoveFromZone(aDevice, aRoom))
                {
                    break;
                }
            }
        }

        internal bool AddSatellite(string aName, IStandardRoom aRoom)
        {
            foreach (StandardRoom r in iRoomLookup.Values)
            {
                if (r.ZoneSender.Value.Enabled && r.Name == aName)
                {
                    r.AddSatellite(aRoom);
                    return true;
                }
            }

            return false;
        }

        internal bool RemoveSatellite(string aName, IStandardRoom aRoom)
        {
            foreach (StandardRoom r in iRoomLookup.Values)
            {
                if (r.ZoneSender.Value.Enabled && r.Name == aName)
                {
                    r.RemoveSatellite(aRoom);
                    return true;
                }
            }

            return false;
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

        private readonly SendersList iSendersList;
        private readonly Watchable<IEnumerable<ITopology4Registration>> iRegistrations;

        private readonly Dictionary<ITopology4Room, RoomWatcher> iRoomWatchers;
        private readonly Dictionary<ITopology4Room, SatelliteWatcher> iSatelliteWatchers;
    }
}
