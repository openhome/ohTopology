using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ITopology3Group
    {
        IWatchableUnordered<ITopology3Group> Children { get; }
    }

    public class Topology3Group : ITopology3Group, IDisposable
    {
        public Topology3Group(IWatchableThread aThread, ITopology2Group aGroup)
        {
            iGroup = aGroup;

            iChildren = new WatchableTopology3Unordered(aThread);
        }

        public void Dispose()
        {
        }

        public IWatchableUnordered<ITopology3Group> Children
        {
            get
            {
                return iChildren;
            }
        }

        private ITopology2Group iGroup;
        private WatchableTopology3Unordered iChildren;
    }

    public class WatchableTopology3Unordered : WatchableUnordered<ITopology3Group>
    {
        public WatchableTopology3Unordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<ITopology3Group>();
        }

        public new void Add(ITopology3Group aValue)
        {
            iList.Add(aValue);
            base.Add(aValue);
        }

        public new void Remove(ITopology3Group aValue)
        {
            iList.Remove(aValue);
            base.Remove(aValue);
        }

        private List<ITopology3Group> iList;
    }

    public interface ITopology3
    {
        IWatchableUnordered<ITopology3Group> Groups { get; }
    }

    public class Topology3 : ITopology3, IUnorderedWatcher<ITopology2Group>, IDisposable
    {
        public Topology3(IWatchableThread aThread, ITopology2 aTopology2)
        {
            iDisposed = false;
            iThread = aThread;
            iTopology2 = aTopology2;

            iGroups = new WatchableTopology3Unordered(aThread);

            iGroupLookup = new Dictionary<ITopology2Group, Topology3Group>();

            iTopology2.Groups.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology3.Dispose");
            }

            foreach (Topology3Group g in iGroupLookup.Values)
            {
                g.Dispose();
            }
            iGroupLookup = null;

            iGroups.Dispose();
            iGroups = null;

            iTopology2.Groups.RemoveWatcher(this);
            iTopology2 = null;
        }

        public IWatchableUnordered<ITopology3Group> Groups
        {
            get
            {
                return iGroups;
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

        public void UnorderedAdd(ITopology2Group aItem)
        {
            Topology3Group group = new Topology3Group(iThread, aItem);
            iGroups.Add(group);
        }

        public void UnorderedRemove(ITopology2Group aItem)
        {
            Topology3Group group;
            if (iGroupLookup.TryGetValue(aItem, out group))
            {
                iGroups.Remove(group);

                iGroupLookup.Remove(aItem);

                // schedule the disposale for the group for after all watchers of the group collection have been notified
                iThread.Schedule(() =>
                {
                    group.Dispose();
                });
            }
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private ITopology2 iTopology2;

        private WatchableTopology3Unordered iGroups;
        private Dictionary<ITopology2Group, Topology3Group> iGroupLookup;
    }
}
