using System;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    /*
    public class WatchableContentDirectoryUnordered : WatchableUnordered<ContentDirectory>
    {
        public WatchableContentDirectoryUnordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<ContentDirectory>();
        }

        public new void Add(ContentDirectory aValue)
        {
            iList.Add(aValue);

            base.Add(aValue);
        }

        public new void Remove(ContentDirectory aValue)
        {
            iList.Remove(aValue);

            base.Remove(aValue);
        }

        private List<ContentDirectory> iList;
    }

    public interface ILibrary
    {
        IWatchableUnordered<ContentDirectory> ContentDirectories { get; }
    }

    public class Library : ILibrary, IUnorderedWatcher<IWatchableDevice>, IDisposable
    {
        public Library(IWatchableThread aThread, INetwork aNetwork)
        {
            iDisposed = false;

            iNetwork = aNetwork;

            iPendingSubscriptions = new List<IWatchableDevice>();
            iContentDirectoryLookup = new Dictionary<IWatchableDevice, ContentDirectory>();
            iContentDirectories = new WatchableContentDirectoryUnordered(aThread);

            iDevices = iNetwork.GetWatchableDeviceCollection<ContentDirectory>();
            iDevices.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Library.Dispose");
            }

            iDevices.RemoveWatcher(this);
            iDevices.Dispose();
            iDevices = null;

            // stop subscriptions for all content directories that are outstanding
            foreach (IWatchableDevice d in iPendingSubscriptions)
            {
                d.Unsubscribe<Product>();
            }
            iPendingSubscriptions = null;

            // dispose of all content directories, which will in turn unsubscribe
            foreach (ContentDirectory c in iContentDirectoryLookup.Values)
            {
                c.Dispose();
            }
            iContentDirectoryLookup = null;

            iDisposed = true;
        }

        public IWatchableUnordered<ContentDirectory> ContentDirectories
        {
            get
            {
                return iContentDirectories;
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

        public void UnorderedAdd(IWatchableDevice aItem)
        {
            aItem.Subscribe<ContentDirectory>(Subscribed);
            iPendingSubscriptions.Add(aItem);
        }

        public void UnorderedRemove(IWatchableDevice aItem)
        {
            ContentDirectory contentDirectory;
            if (iContentDirectoryLookup.TryGetValue(aItem, out contentDirectory))
            {
                iContentDirectories.Remove(contentDirectory);
                iContentDirectoryLookup.Remove(aItem);
            }
        }

        private void Subscribed(IWatchableDevice aDevice, ContentDirectory aContentDirectory)
        {
            iContentDirectories.Add(aContentDirectory);
            iContentDirectoryLookup.Add(aDevice, aContentDirectory);
            iPendingSubscriptions.Remove(aDevice);
        }

        private bool iDisposed;

        private INetwork iNetwork;

        private List<IWatchableDevice> iPendingSubscriptions;
        private Dictionary<IWatchableDevice, ContentDirectory> iContentDirectoryLookup;
        private WatchableContentDirectoryUnordered iContentDirectories;
        
        private WatchableDeviceUnordered iDevices;
    }
     */
}
