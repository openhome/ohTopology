using System;
using System.Collections.Generic;
using System.Linq;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    internal class WatchableDeviceCollection : WatchableCollection<IWatchableDevice>
    {
        public WatchableDeviceCollection(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<IWatchableDevice>();
        }

        internal void Add(IWatchableDevice aValue)
        {
            uint index = (uint)iList.Count;
            iList.Add(aValue);

            CollectionAdd(aValue, index);
        }

        internal void Remove(IWatchableDevice aValue)
        {
            uint index = (uint)iList.IndexOf(aValue);
            iList.Remove(aValue);
             
            CollectionRemove(aValue, index);
        }

        private List<IWatchableDevice> iList;
    }

    public interface ITopology1
    {
        IWatchableThread WatchableThread { get; }
        IWatchableCollection<IWatchableDevice> Devices { get; }
        void Refresh();
    }

    public class Topology1 : ITopology1, IDisposable
    {
        public Topology1(IWatchableThread aThread)
        {
            iLock = new object();
            iDisposed = false;
            iThread = aThread;

            iDeviceList = new CpDeviceListUpnpServiceType("av.openhome.org", "Product", 1, Added, Removed);
            iCpDeviceLookup = new Dictionary<CpDevice, DisposableWatchableDevice>();
            iTopologyDeviceList = new WatchableDeviceCollection(aThread);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology1.Dispose");
                }

                iDeviceList.Dispose();
                iDeviceList = null;

                iTopologyDeviceList.Dispose();
                iTopologyDeviceList = null;

                foreach (DisposableWatchableDevice device in iCpDeviceLookup.Values)
                {
                    device.Dispose();
                }
                iCpDeviceLookup.Clear();
                iCpDeviceLookup = null;

                iThread = null;

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology1.Refresh");
                }

                iDeviceList.Refresh();
            }
        }

        public IWatchableCollection<IWatchableDevice> Devices
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("Topology1.Devices");
                    }

                    return iTopologyDeviceList;
                }
            }
        }

        public IWatchableThread WatchableThread
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("Topology1.WatchableThread");
                    }

                    return iThread;
                }
            }
        }

        private void Added(CpDeviceList aList, CpDevice aDevice)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                DisposableWatchableDevice device = new DisposableWatchableDevice(aDevice);
                iCpDeviceLookup.Add(aDevice, device);

                iTopologyDeviceList.Add(device);
            }
        }

        private void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                DisposableWatchableDevice device;
                if (iCpDeviceLookup.TryGetValue(aDevice, out device))
                {
                    iCpDeviceLookup.Remove(aDevice);

                    iTopologyDeviceList.Remove(device);

                    device.Dispose();
                }
            }
        }

        private readonly object iLock;
        private bool iDisposed;

        private IWatchableThread iThread;
        private CpDeviceList iDeviceList;

        private Dictionary<CpDevice, DisposableWatchableDevice> iCpDeviceLookup;
        private WatchableDeviceCollection iTopologyDeviceList;
    }

    public class MockTopology1 : ITopology1, IMockable, IDisposable
    {
        public MockTopology1(WatchableThread aThread)
        {
            iLock = new object();
            iDisposed = false;

            iThread = aThread;
            iUdnLookup = new Dictionary<string, MockWatchableDevice>();
            iTopologyDeviceList = new WatchableDeviceCollection(aThread);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("MoqTopology1.Dispose");
                }

                iTopologyDeviceList.Dispose();
                iTopologyDeviceList = null;

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("MoqTopology1.Refresh");
                }
            }
        }

        public IWatchableCollection<IWatchableDevice> Devices
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("MoqTopology1.Devices");
                    }
                    
                    return iTopologyDeviceList;
                }
            }
        }

        public IWatchableThread WatchableThread
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("MoqTopology1.WatchableThread");
                    }
                 
                    return iThread;
                }
            }
        }

        private void Add(string aUdn)
        {
            MockWatchableDevice device = new MockWatchableDevice(aUdn);
            iUdnLookup.Add(aUdn, device);
            iTopologyDeviceList.Add(device);
        }

        private void Remove(string aUdn)
        {
            MockWatchableDevice device;
            if (iUdnLookup.TryGetValue(aUdn, out device))
            {
                iUdnLookup.Remove(aUdn);

                iTopologyDeviceList.Remove(device);
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First();
            if (command == "add")
            {
                IEnumerable<string> value = aValue.Skip(1);
                Add(value.First());
            }
            else if (command == "remove")
            {
                IEnumerable<string> value = aValue.Skip(1);
                Remove(value.First());
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private object iLock;
        private bool iDisposed;

        private IWatchableThread iThread;

        private Dictionary<string, MockWatchableDevice> iUdnLookup;
        private WatchableDeviceCollection iTopologyDeviceList;
    }
}
