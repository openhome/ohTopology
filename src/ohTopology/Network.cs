using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface INetwork
    {
        void Refresh();
        WatchableDeviceCollection GetWatchableDeviceCollection<T>() where T : IWatchableService;
    }

    public class Network : INetwork, IDisposable
    {
         //iDeviceList = new CpDeviceListUpnpServiceType("av.openhome.org", "Product", 1, Added, Removed);
            //iCpDeviceLookup = new Dictionary<string, DisposableWatchableDevice>();

        public void Refresh()
        {
            /*lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology1.Refresh");
                }

                iDeviceList.Refresh();
            }*/
        }

        public void Dispose()
        {
            /*lock (iLock)
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
            }*/
        }

        public WatchableDeviceCollection GetWatchableDeviceCollection<T>() where T : IWatchableService
        {
            throw new NotImplementedException();
        }

        /*private void Added(CpDeviceList aList, CpDevice aDevice)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                DisposableWatchableDevice device = new DisposableWatchableDevice(aDevice);
                iCpDeviceLookup.Add(aDevice.Udn(), device);

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
                if (iCpDeviceLookup.TryGetValue(aDevice.Udn(), out device))
                {
                    iCpDeviceLookup.Remove(aDevice.Udn());

                    iTopologyDeviceList.Remove(device);

                    device.Dispose();
                }
            }
        }*/

        //private CpDeviceList iDeviceList;
        //private Dictionary<string, DisposableWatchableDevice> iCpDeviceLookup;
        //private WatchableDeviceCollection iTopologyDeviceList;
    }

    public class MockNetwork : INetwork, IMockable
    {
        public MockNetwork(IWatchableThread aThread, Mockable aMocker)
        {
            iLock = new object();

            iThread = aThread;
            iMocker = aMocker;

            iOnDevices = new Dictionary<string, MockWatchableDevice>();
            iOffDevices = new Dictionary<string, MockWatchableDevice>();
            iDeviceLists = new Dictionary<Type, List<WatchableDeviceCollection>>();
        }

        public void Refresh()
        {
        }

        public void AddDevice(MockWatchableDevice aDevice)
        {
            lock (iLock)
            {
                iOnDevices.Add(aDevice.Udn, aDevice);
                iMocker.Add(aDevice.Udn, aDevice);

                foreach (KeyValuePair<Type, List<WatchableDeviceCollection>> k in iDeviceLists)
                {
                    if (aDevice.HasService(k.Key))
                    {
                        foreach (WatchableDeviceCollection c in k.Value)
                        {
                            c.Add(aDevice);
                        }
                    }
                }
            }
        }

        internal void RemoveDevice(string aUdn)
        {
            lock (iLock)
            {
                MockWatchableDevice device;
                if(iOnDevices.TryGetValue(aUdn, out device))
                {
                    RemoveDevice(device);
                }
            }
        }

        public void RemoveDevice(MockWatchableDevice aDevice)
        {
            lock (iLock)
            {
                iOnDevices.Remove(aDevice.Udn);
                iOffDevices.Add(aDevice.Udn, aDevice);
                iMocker.Remove(aDevice.Udn);

                foreach (KeyValuePair<Type, List<WatchableDeviceCollection>> k in iDeviceLists)
                {
                    if (aDevice.HasService(k.Key))
                    {
                        foreach (WatchableDeviceCollection c in k.Value)
                        {
                            c.Remove(aDevice);
                        }
                    }
                }
            }
        }

        public WatchableDeviceCollection GetWatchableDeviceCollection<T>() where T : IWatchableService
        {
            WatchableDeviceCollection list = new WatchableDeviceCollection(iThread);
            Type key = new ServiceType<T>().GetType();

            if (iDeviceLists.ContainsKey(key))
            {
                iDeviceLists[key].Add(list);
            }
            else
            {
                iDeviceLists.Add(key, new List<WatchableDeviceCollection>(new WatchableDeviceCollection[] { list }));

                foreach (MockWatchableDevice d in iOnDevices.Values)
                {
                    if (d.HasService(key))
                    {
                        list.Add(d);
                    }
                }
            }

            return list;
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First();
            if (command == "add")
            {
                IEnumerable<string> value = aValue.Skip(1);
                string type = value.First();
                if (type == "ds")
                {
                    value = value.Skip(1);
                    string udn = value.First();

                    MockWatchableDevice device;
                    if (iOffDevices.TryGetValue(udn, out device))
                    {
                        AddDevice(device);

                        return;
                    }

                    AddDevice(new MockWatchableDs(iThread, udn));
                }
            }
            else if (command == "remove")
            {
                IEnumerable<string> value = aValue.Skip(1);
                string type = value.First();
                if (type == "ds")
                {
                    value = value.Skip(1);
                    RemoveDevice(value.First());
                }
            }
            else if (command == "update")
            {
                IEnumerable<string> value = aValue.Skip(1);
                string type = value.First();
                if (type == "ds")
                {
                    value = value.Skip(1);
                    lock (iLock)
                    {
                        string udn = value.First();
                        MockWatchableDevice device;
                        if (iOnDevices.TryGetValue(udn, out device))
                        {
                            device.Execute(value.Skip(1));
                        }
                        else
                        {
                            throw new KeyNotFoundException();
                        }
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private object iLock;

        private IWatchableThread iThread;
        private Mockable iMocker;

        private Dictionary<string, MockWatchableDevice> iOnDevices;
        private Dictionary<string, MockWatchableDevice> iOffDevices;
        private Dictionary<Type, List<WatchableDeviceCollection>> iDeviceLists;
    }

    /*public static class NetworkExtensions
    {
        public static IWatchableCollection<IWatchableDevice> GetWatchableDeviceCollection<T>(this INetwork aNetwork)
        {
            return aNetwork.GetDevices<IServiceOpenHomeOrgProduct1>();
        }
    }*/
}
