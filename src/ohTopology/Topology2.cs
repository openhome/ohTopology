using System;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    internal interface ITopology2Source
    {
        IWatchable<string> Name { get; }
        IWatchable<string> Type { get; }
        IWatchable<bool> Visible { get; }
    }

    internal class WatchableSourceCollection : WatchableCollection<ITopology2Source>
    {
        public WatchableSourceCollection(WatchableThread aWatchableThread)
            : base(aWatchableThread)
        {
            iList = new List<ITopology2Source>();
        }

        public void Add(ITopology2Source aValue)
        {
            uint index = (uint)iList.Count;
            CollectionAdd(aValue, index);
        }

        public void Remove(ITopology2Source aValue)
        {
            uint index = (uint)iList.IndexOf(aValue);
            CollectionRemove(aValue, index);
        }

        private List<ITopology2Source> iList;
    }

    internal interface ITopology2Group
    {
        IWatchable<string> Room { get; }
        IWatchable<string> Name { get; }
        IWatchable<bool> Standby { get; }
        IWatchable<uint> SourceIndex { get; }
        IWatchableCollection<ITopology2Source> Sources { get; }

        void SetStandby(bool aValue);
        void SetSourceIndex(uint aValue);
    }

    internal interface ITopology2Product
    {
    }

    internal class Topology2Group : ITopology2Group
    {
        public Topology2Group(WatchableThread aWatchableThread, string aRoom, string aName, bool aStandby, uint aSourceIndex, IVolume aVolume)
        {
            iRoom = new Watchable<string>(aWatchableThread, "Room", aRoom);
            iName = new Watchable<string>(aWatchableThread, "Name", aName);
            iStandby = new Watchable<bool>(aWatchableThread, "Standby", aStandby);
            iSourceIndex = new Watchable<uint>(aWatchableThread, "SourceIndex", aSourceIndex);
            iVolume = new Watchable<IVolume>(aWatchableThread, "Volume", aVolume);
        }

        public IWatchable<string> Room
        {
            get
            {
                return iRoom;
            }
        }

        public IWatchable<string> Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchable<bool> Standby
        {
            get 
            {
                return iStandby;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get
            {
                return iSourceIndex;
            }
        }

        public IWatchableCollection<ITopology2Source> Sources
        {
            get
            {
                return iSources;
            }
        }

        public void SetStandby(bool aValue)
        {
            throw new NotImplementedException();
        }

        public void SetSourceIndex(uint aValue)
        {
            throw new NotImplementedException();
        }

        private Watchable<string> iRoom;
        private Watchable<string> iName;
        private Watchable<bool> iStandby;
        private Watchable<uint> iSourceIndex;
        private Watchable<IVolume> iVolume;
        private WatchableSourceCollection iSources;
    }

    public class Topology2 : ICollectionWatcher<IWatchableDevice>, IDisposable
    {
        public Topology2(ITopology1 aTopology1)
        {
            iDisposed = false;
            iLock = new object();

            iThread = aTopology1.WatchableThread;
            iTopology1 = aTopology1;

            iTopology1.Devices.AddWatcher(this);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2.Dispose");
                }

                iTopology1.Dispose();

                iDisposed = true;
            }
        }

        public void CollectionAdd(IWatchableDevice aItem, uint aIndex)
        {
            throw new NotImplementedException();
        }

        public void CollectionClose()
        {
            throw new NotImplementedException();
        }

        public void CollectionInitialised()
        {
            throw new NotImplementedException();
        }

        public void CollectionMove(IWatchableDevice aItem, uint aFrom, uint aTo)
        {
            throw new NotSupportedException();
        }

        public void CollectionOpen()
        {
            throw new NotImplementedException();
        }

        public void CollectionRemove(IWatchableDevice aItem, uint aIndex)
        {
            throw new NotImplementedException();
        }

        private object iLock;
        private bool iDisposed;

        private IWatchableThread iThread;
        private ITopology1 iTopology1;
    }
}
