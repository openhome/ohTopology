using System;
using System.Collections.Generic;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    internal class WatchableDeviceCollection : WatchableCollection<IWatchableDevice>
    {
        public WatchableDeviceCollection(IWatchableThread aThread)
            : base(aThread)
        {
            iCpDeviceLookup = new Dictionary<CpDevice, DisposableWatchableDevice>();
            iList = new List<IWatchableDevice>();
        }

        public new void Dispose()
        {
            base.Dispose();

            foreach (DisposableWatchableDevice device in iCpDeviceLookup.Values)
            {
                device.Dispose();
            }
            iCpDeviceLookup.Clear();
            iCpDeviceLookup = null;
        }

        internal void Add(CpDevice aValue)
        {
            uint index = (uint)iList.Count;

            DisposableWatchableDevice device = new DisposableWatchableDevice(aValue);
            iCpDeviceLookup.Add(aValue, device);
            iList.Add(device);

            CollectionAdd(device, index);
        }

        internal void Remove(CpDevice aValue)
        {
            DisposableWatchableDevice device;
            if (iCpDeviceLookup.TryGetValue(aValue, out device))
            {
                iCpDeviceLookup.Remove(aValue);

                uint index = (uint)iList.IndexOf(device);
                iList.Remove(device);
             
                CollectionRemove(device, index);

                device.Dispose();
            }
        }

        private Dictionary<CpDevice, DisposableWatchableDevice> iCpDeviceLookup;
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
            iDeviceList = new CpDeviceListUpnpServiceType("av.openhome.org", "Product", 1, Added, Removed);
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

        private void Added(CpDeviceList aList, CpDevice aDevice)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iTopologyDeviceList.Add(aDevice);
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

                iTopologyDeviceList.Remove(aDevice);
            }
        }

        private readonly object iLock;
        private bool iDisposed;

        private CpDeviceList iDeviceList;
        private WatchableDeviceCollection iTopologyDeviceList;
    }

    public class MockTopology1 : ITopology1
    {
        public MockTopology1(WatchableThread aThread)
        {
            iLock = new object();
            iDisposed = false;
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

        private object iLock;
        private bool iDisposed;

        private WatchableDeviceCollection iTopologyDeviceList;
    }
}
