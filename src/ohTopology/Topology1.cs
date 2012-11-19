using System;
using System.Collections.Generic;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class Topology1DeviceWatchableCollection : WatchableCollection<IWatchableDevice>, IDisposable
    {
        public Topology1DeviceWatchableCollection(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<IWatchableDevice>();
        }

        public void Add(IWatchableDevice aValue)
        {
            uint index = (uint)iList.Count;
            iList.Add(aValue);
            CollectionAdd(aValue, index);
        }

        public void Remove(IWatchableDevice aValue)
        {
            uint index = (uint)iList.IndexOf(aValue);
            iList.Add(aValue);
            CollectionRemove(aValue, index);
        }

        private List<IWatchableDevice> iList;
    }

    public interface ITopology1 : IDisposable
    {
        IWatchableCollection<IWatchableDevice> Devices { get; }
        void Refresh();
    }

    public class Topology1 : ITopology1
    {
        public Topology1(IWatchableThread aThread)
        {
            iLock = new object();
            iDisposed = false;
            iDeviceList = new CpDeviceListUpnpServiceType("openhome.org", "Product", 1, Added, Removed);
            iTopologyDeviceLookup = new Dictionary<CpDevice, WatchableDevice>();
            iTopologyDeviceList = new Topology1DeviceWatchableCollection(aThread);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology1");
                }

                iDeviceList.Dispose();
                iDeviceList = null;

                iTopologyDeviceList.Dispose();
                iTopologyDeviceList = null;

                iTopologyDeviceLookup = null;

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology1");
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
                        throw new ObjectDisposedException("Topology1");
                    }

                    return iTopologyDeviceList;
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

                WatchableDevice device = new WatchableDevice(aDevice);
                iTopologyDeviceLookup.Add(aDevice, device);
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

                WatchableDevice device;
                if (iTopologyDeviceLookup.TryGetValue(aDevice, out device))
                {
                    iTopologyDeviceList.Remove(device);
                }
            }
        }

        private readonly object iLock;
        private bool iDisposed;

        private CpDeviceList iDeviceList;
        private Dictionary<CpDevice, WatchableDevice> iTopologyDeviceLookup;
        private Topology1DeviceWatchableCollection iTopologyDeviceList;
    }

    public class MoqTopology1 : ITopology1
    {
        public MoqTopology1(WatchableThread aThread)
        {
            iTopologyDeviceList = new Topology1DeviceWatchableCollection(aThread);
        }

        public void Dispose()
        {
            iTopologyDeviceList.Dispose();
            iTopologyDeviceList = null;
        }

        public void Refresh()
        {
        }

        public IWatchableCollection<IWatchableDevice> Devices
        {
            get
            {
                return iTopologyDeviceList;
            }
        }

        private Topology1DeviceWatchableCollection iTopologyDeviceList;
    }
}
