using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

using OpenHome.Os.App;
using OpenHome.Os;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public interface INetwork : IWatchableThread, IDisposable
    {
        void Refresh();
        WatchableDeviceUnordered GetWatchableDeviceCollection<T>() where T : IService;
        IWatchableThread WatchableThread { get; }
    }

    public class ServiceWatchableDeviceCollection : WatchableDeviceUnordered, IEnumerable<IManagableWatchableDevice>
    {
        public ServiceWatchableDeviceCollection(IWatchableThread aThread, IWatchableThread aSubscribeThread, string aDomainName, string aServiceType, uint aVersion)
            : base(aThread)
        {
            iSubscribeThread = aSubscribeThread;
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

                DisposableWatchableDevice device = new DisposableWatchableDevice(WatchableThread, iSubscribeThread, aDevice);
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

                        WatchableThread.Schedule(() =>
                        {
                            device.Dispose();
                        });
                    });
                }
            }
        }

        public IEnumerator<IManagableWatchableDevice> GetEnumerator()
        {
            return iCpDeviceLookup.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return iCpDeviceLookup.Values.GetEnumerator();
        }

        private object iLock;
        private bool iDisposed;
        private IWatchableThread iSubscribeThread;

        private CpDeviceList iCpDeviceList;
        private Dictionary<string, DisposableWatchableDevice> iCpDeviceLookup;
    }

    public class Network : INetwork
    {
        public Network(IWatchableThread aThread, IWatchableThread aSubscribeThread)
        {
            iSubscribeThread = aSubscribeThread;
            iThread = aThread;

            iDeviceCollections = new Dictionary<Type, ServiceWatchableDeviceCollection>();

            // add device lists for each type of watchable service
            iDeviceCollections.Add(typeof(ServiceProduct), new ServiceWatchableDeviceCollection(iThread, iSubscribeThread, "av.openhome.org", "Product", 1));
            //iDeviceCollections.Add(typeof(ServiceContentDirectory), new ServiceWatchableDeviceCollection(iThread, "upnp.org", "ContentDirectory", 1));
        }

        public void Dispose()
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

        public WatchableDeviceUnordered GetWatchableDeviceCollection<T>() where T : IService
        {
            return iDeviceCollections[typeof(T)];
        }

        public IWatchableThread WatchableThread
        {
            get
            {
                return iThread;
            }
        }

        public void Assert()
        {
            iThread.Assert();
        }

        public void Schedule(Action aAction)
        {
            iThread.Schedule(aAction);
        }

        public void Execute(Action aAction)
        {
            iThread.Execute(aAction);
        }

        public void Wait()
        {
            iThread.Wait(() =>
            {
                iSubscribeThread.Wait();
            });
        }

        public void Wait(Action aAction)
        {
            iThread.Wait(() =>
            {
                iSubscribeThread.Wait(aAction);
            });
        }

        private IWatchableThread iThread;
        private IWatchableThread iSubscribeThread;

        private Dictionary<Type, ServiceWatchableDeviceCollection> iDeviceCollections;
    }

    public class MockNetwork : INetwork, IMockable
    {
        public MockNetwork(IWatchableThread aThread, IWatchableThread aSubscribeThread, Mockable aMocker)
        {
            iLock = new object();

            iSubscribeThread = aSubscribeThread;
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

                foreach (KeyValuePair<Type, List<WatchableDeviceUnordered>> k in iDeviceLists)
                {
                    if (aDevice.HasService(k.Key))
                    {
                        foreach (WatchableDeviceUnordered c in k.Value)
                        {
                            c.Remove(aDevice);
                        }
                    }
                }
            }
        }

        public WatchableDeviceUnordered GetWatchableDeviceCollection<T>() where T : IService
        {
            Type key = typeof(T);

            WatchableDeviceUnordered list = new WatchableDeviceUnordered(iThread);

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

        public IWatchableThread WatchableThread
        {
            get
            {
                return iThread;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();

            if (command == "add")
            {
                IEnumerable<string> value = aValue.Skip(1);

                string type = value.First();

                if (type == "ds" || type == "dsm" /*|| type == "mediaserver" */)
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
                        AddDevice(new MockWatchableDs(iThread, iSubscribeThread, udn));
                    }
                    else if(type == "dsm")
                    {
                        AddDevice(new MockWatchableDsm(iThread, iSubscribeThread, udn));
                    }
                    /*
                    else if (type == "mediaserver")
                    {
                        AddDevice(new MockWatchableMediaServer(iThread, iSubscribeThread, udn));
                    }
                    */
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

        public void Assert()
        {
            iThread.Assert();
        }

        public void Schedule(Action aAction)
        {
            iThread.Schedule(aAction);
        }

        public void Execute(Action aAction)
        {
            iThread.Execute(aAction);
        }

        public void Wait()
        {
            iThread.Wait(() =>
            {
                iSubscribeThread.Wait();
            });
        }

        public void Wait(Action aAction)
        {
            iThread.Wait(() =>
            {
                iSubscribeThread.Wait(aAction);
            });
        }

        private object iLock;

        protected IWatchableThread iThread;
        protected IWatchableThread iSubscribeThread;

        private Mockable iMocker;

        private Dictionary<string, MockWatchableDevice> iOnDevices;
        private Dictionary<string, MockWatchableDevice> iOffDevices;
        private Dictionary<Type, List<WatchableDeviceUnordered>> iDeviceLists;
    }

    public class FourDsMockNetwork : MockNetwork
    {
        public FourDsMockNetwork(IWatchableThread aThread, IWatchableThread aSubscribeThread, Mockable aMocker)
            : base(aThread, aSubscribeThread, aMocker)
        {
            iThread.Schedule(() =>
            {
                AddDevice(new MockWatchableDs(iThread, iSubscribeThread, "4c494e4e-0026-0f99-1111-ef000004013f", "Kitchen", "Sneaky Music DS", "Info Time Volume Sender"));
                AddDevice(new MockWatchableDsm(iThread, iSubscribeThread, "4c494e4e-0026-0f99-1112-ef000004013f", "Sitting Room", "Klimax DSM", "Info Time Volume Sender"));
                AddDevice(new MockWatchableDsm(iThread, iSubscribeThread, "4c494e4e-0026-0f99-1113-ef000004013f", "Bedroom", "Kiko DSM", "Info Time Volume Sender"));
                AddDevice(new MockWatchableDs(iThread, iSubscribeThread, "4c494e4e-0026-0f99-1114-ef000004013f", "Dining Room", "Majik DS", "Info Time Volume Sender"));
            });
        }
    }
}
