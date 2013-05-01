using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public interface INetwork
    {
        void Start();
        void Stop();
        void Refresh();
        WatchableDeviceUnordered GetWatchableDeviceCollection<T>() where T : IWatchableService;
    }

    public class ServiceWatchableDeviceCollection : WatchableDeviceUnordered
    {
        public ServiceWatchableDeviceCollection(IWatchableThread aThread, string aDomainName, string aServiceType, uint aVersion)
            : base(aThread)
        {
            iLock = new object();
            iDisposed = false;

            iCpDeviceList = new CpDeviceListUpnpServiceType(aDomainName, aServiceType, aVersion, Added, Removed);
            iCpDeviceLookup = new Dictionary<string, DisposableWatchableDevice>();
        }

        public new void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceWatchableDeviceCollection.Dispose");
                }

                base.Dispose();

                iCpDeviceList.Dispose();
                iCpDeviceList = null;

                foreach (DisposableWatchableDevice device in iCpDeviceLookup.Values)
                {
                    device.Dispose();
                }
                iCpDeviceLookup = null;

                iDisposed = true;
            }
        }

        public void Refresh()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceWatchableDeviceCollection.Refresh");
                }

                iCpDeviceList.Refresh();
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

                DisposableWatchableDevice device = new DisposableWatchableDevice(WatchableThread, aDevice);
                iCpDeviceLookup.Add(aDevice.Udn(), device);

                WatchableThread.Schedule(() =>
                {
                    Add(device);
                });
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

                    WatchableThread.Schedule(() =>
                    {
                        Remove(device);
                        device.Dispose();
                    });
                }
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpDeviceList iCpDeviceList;
        private Dictionary<string, DisposableWatchableDevice> iCpDeviceLookup;
    }

    public class Network : INetwork, IDisposable
    {
        public Network(IWatchableThread aThread)
        {
            iThread = aThread;
            iDeviceCollections = new Dictionary<Type, ServiceWatchableDeviceCollection>();
        }

        public void Dispose()
        {
            foreach (ServiceWatchableDeviceCollection c in iDeviceCollections.Values)
            {
                c.Dispose();
            }
            iDeviceCollections = null;
        }

        public void Start()
        {
            // add device lists for each type of watchable service
            iDeviceCollections.Add(typeof(Product), new ServiceWatchableDeviceCollection(iThread, "av.openhome.org", "Product", 1));
            iDeviceCollections.Add(typeof(ContentDirectory), new ServiceWatchableDeviceCollection(iThread, "upnp.org", "ContentDirectory", 1));
        }

        public void Stop()
        {
            foreach (ServiceWatchableDeviceCollection c in iDeviceCollections.Values)
            {
                c.Dispose();
            }
            iDeviceCollections.Clear();
        }

        public void Refresh()
        {
            foreach (ServiceWatchableDeviceCollection c in iDeviceCollections.Values)
            {
                c.Refresh();
            }
        }

        public WatchableDeviceUnordered GetWatchableDeviceCollection<T>() where T : IWatchableService
        {
            return iDeviceCollections[typeof(T)];
        }

        private IWatchableThread iThread;
        private Dictionary<Type, ServiceWatchableDeviceCollection> iDeviceCollections;
    }

    public class MockNetwork : INetwork, IMockable, IDisposable
    {
        public MockNetwork(IWatchableThread aThread, Mockable aMocker)
        {
            iLock = new object();

            iThread = aThread;
            iMocker = aMocker;

            iOnDevices = new Dictionary<string, MockWatchableDevice>();
            iOffDevices = new Dictionary<string, MockWatchableDevice>();
            iDeviceLists = new Dictionary<Type, List<WatchableDeviceUnordered>>();
        }

        public void Dispose()
        {
            foreach (MockWatchableDevice d in iOnDevices.Values)
            {
                d.Dispose();
            }
            iOnDevices.Clear();
            iOnDevices = null;

            foreach (MockWatchableDevice d in iOffDevices.Values)
            {
                d.Dispose();
            }
            iOffDevices.Clear();
            iOffDevices = null;

            iDeviceLists.Clear();
            iDeviceLists = null;
        }

        public virtual void Start()
        {
        }

        public virtual void Stop()
        {
            foreach (MockWatchableDevice d in iOnDevices.Values)
            {
                d.Dispose();
            }
            iOnDevices.Clear();

            foreach (MockWatchableDevice d in iOffDevices.Values)
            {
                d.Dispose();
            }
            iOffDevices.Clear();

            iDeviceLists.Clear();
        }

        public void Refresh()
        {
        }

        public void AddDevice(MockWatchableDevice aDevice)
        {
            lock (iLock)
            {
                iOffDevices.Remove(aDevice.Udn);
                iOnDevices.Add(aDevice.Udn, aDevice);
                iMocker.Add(aDevice.Udn, aDevice);

                foreach (KeyValuePair<Type, List<WatchableDeviceUnordered>> k in iDeviceLists)
                {
                    if (aDevice.HasService(k.Key))
                    {
                        foreach (WatchableDeviceUnordered c in k.Value)
                        {
                            iThread.Schedule(() =>
                            {
                                c.Add(aDevice);
                            });
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

                foreach (KeyValuePair<Type, List<WatchableDeviceUnordered>> k in iDeviceLists)
                {
                    if (aDevice.HasService(k.Key))
                    {
                        foreach (WatchableDeviceUnordered c in k.Value)
                        {
                            iThread.Schedule(() =>
                            {
                                c.Remove(aDevice);
                            });
                        }
                    }
                }
            }
        }

        public WatchableDeviceUnordered GetWatchableDeviceCollection<T>() where T : IWatchableService
        {
            WatchableDeviceUnordered list = new WatchableDeviceUnordered(iThread);
            Type key = typeof(T);

            if (iDeviceLists.ContainsKey(key))
            {
                iDeviceLists[key].Add(list);
            }
            else
            {
                iDeviceLists.Add(key, new List<WatchableDeviceUnordered>(new WatchableDeviceUnordered[] { list }));

                foreach (MockWatchableDevice d in iOnDevices.Values)
                {
                    if (d.HasService(key))
                    {
                        iThread.Schedule(() =>
                        {
                            list.Add(d);
                        });

                    }
                }
            }

            return list;
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "add")
            {
                IEnumerable<string> value = aValue.Skip(1);
                string type = value.First();
                if (type == "ds" || type == "dsm" || type == "mediaserver")
                {
                    value = value.Skip(1);
                    string udn = value.First();

                    MockWatchableDevice device;
                    if (iOffDevices.TryGetValue(udn, out device))
                    {
                        AddDevice(device);

                        return;
                    }

                    if (type == "ds")
                    {
                        AddDevice(new MockWatchableDs(iThread, udn));
                    }
                    else if(type == "dsm")
                    {
                        AddDevice(new MockWatchableDsm(iThread, udn));
                    }
                    else if (type == "mediaserver")
                    {
                        AddDevice(new MockWatchableMediaServer(iThread, udn));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            else if (command == "remove")
            {
                IEnumerable<string> value = aValue.Skip(1);
                string type = value.First();
                if (type == "ds" || type == "dsm" || type == "mediaserver")
                {
                    value = value.Skip(1);
                    RemoveDevice(value.First());
                }
                else
                {
                    throw new NotSupportedException();
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
                            if (iOffDevices.TryGetValue(udn, out device))
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
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private object iLock;

        protected IWatchableThread iThread;
        private Mockable iMocker;

        private Dictionary<string, MockWatchableDevice> iOnDevices;
        private Dictionary<string, MockWatchableDevice> iOffDevices;
        private Dictionary<Type, List<WatchableDeviceUnordered>> iDeviceLists;
    }

    public class FourDsMockNetwork : MockNetwork
    {
        public FourDsMockNetwork(IWatchableThread aThread, Mockable aMocker)
            : base(aThread, aMocker)
        {
        }

        public override void Start()
        {
            base.Start();

            AddDevice(new MockWatchableDs(iThread, "4c494e4e-0026-0f99-1111-ef000004013f", "Kitchen", "Sneaky Music DS", "Info Time Volume Sender"));
            AddDevice(new MockWatchableDsm(iThread, "4c494e4e-0026-0f99-1112-ef000004013f", "Sitting Room", "Klimax DSM", "Info Time Volume Sender"));
            AddDevice(new MockWatchableDsm(iThread, "4c494e4e-0026-0f99-1113-ef000004013f", "Bedroom", "Kiko DSM", "Info Time Volume Sender"));
            AddDevice(new MockWatchableDs(iThread, "4c494e4e-0026-0f99-1114-ef000004013f", "Dining Room", "Majik DS", "Info Time Volume Sender"));
        }
    }
}
