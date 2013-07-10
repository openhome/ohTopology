using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class RoomWatcher : IWatcher<ITopology4Source>, IWatcher<ITopologymSender>, IOrderedWatcher<IStandardRoom>, IWatcher<IZoneSender>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly StandardHouse iHouse;
        private readonly IWatchableOrdered<IStandardRoom> iRooms;
        private readonly StandardRoom iRoom;
        private bool iRoomsInitialised;
        private ITopologymSender iSender;

        public RoomWatcher(StandardHouse aHouse, IWatchableOrdered<IStandardRoom> aRooms, StandardRoom aRoom)
        {
            iDisposeHandler = new DisposeHandler();
            iHouse = aHouse;
            iRooms = aRooms;
            iRoom = aRoom;

            iRoom.Source.AddWatcher(this);
            iRooms.AddWatcher(this);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            foreach (IStandardRoom r in iRooms.Values)
            {
                if (r != iRoom)
                {
                    r.ZoneSender.RemoveWatcher(this);
                }
            }

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

        public void OrderedOpen() { }

        public void OrderedInitialised() { iRoomsInitialised = true; }

        public void OrderedClose() { }

        public void OrderedAdd(IStandardRoom aItem, uint aIndex)
        {
            if (aItem != iRoom)
            {
                aItem.ZoneSender.AddWatcher(this);
            }

            /*if (iRoomsInitialised)
            {
                if (iSender != null && iSender.Enabled)
                {
                    if (aItem.ZoneSender.Value.Sender == iSender.Device)
                    {
                        iHouse.AddToZone(iSender.Device, iRoom);
                    }
                }
            }*/
        }

        public void OrderedMove(IStandardRoom aItem, uint aFrom, uint aTo) { }

        public void OrderedRemove(IStandardRoom aItem, uint aIndex)
        {
            if (aItem != iRoom)
            {
                aItem.ZoneSender.RemoveWatcher(this);
            }
        }

        public void ItemOpen(string aId, IZoneSender aValue)
        {
            if (iRoomsInitialised)
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
        }

        public void ItemUpdate(string aId, IZoneSender aValue, IZoneSender aPrevious)
        {
            if (iRoomsInitialised)
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
        }

        public void ItemClose(string aId, IZoneSender aValue)
        {
        }
    }

    class SendersWatcher : IUnorderedWatcher<IDevice>, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly WatchableOrdered<IProxySender> iWatchableSenders;
        private IWatchableUnordered<IDevice> iSenders;
        private readonly Dictionary<IDevice, IProxySender> iSenderLookup;
        private bool iDisposed;

        public SendersWatcher(INetwork aNetwork)
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
                using (iDisposeHandler.Lock)
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

        private readonly SendersWatcher iSendersWatcher;

        private readonly Dictionary<ITopology4Room, RoomWatcher> iRoomWatchers;
    }
}
