using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

using OpenHome.Os.App;
using OpenHome.Os;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public abstract class DeviceInjector : IDisposable
    {
        protected DeviceInjector(Network aNetwork)
        {
            iNetwork = aNetwork;

            iCpDeviceLookup = new Dictionary<CpDevice, WatchableDevice>();
        }

        public void Dispose()
        {
            iDeviceList.Dispose();
            iDeviceList = null;

            iNetwork.Execute(() =>
            {
                foreach (WatchableDevice d in iCpDeviceLookup.Values)
                {
                    d.Dispose();
                }
            });
            iCpDeviceLookup.Clear();
            iCpDeviceLookup = null;
        }

        protected void Added(CpDeviceList aList, CpDevice aDevice)
        {
            iNetwork.Schedule(() =>
            {
                WatchableDevice device = WatchableDevice.Create(iNetwork, aDevice);
                iNetwork.Add(device);
            });
        }

        protected void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            iNetwork.Schedule(() =>
            {
                WatchableDevice device = iCpDeviceLookup[aDevice];
                iCpDeviceLookup.Remove(aDevice);
                iNetwork.Remove(device);

                iNetwork.Schedule(() =>
                {
                    device.Dispose();
                });
            });
        }

        protected CpDeviceListUpnpServiceType iDeviceList;

        private Network iNetwork;
        private Dictionary<CpDevice, WatchableDevice> iCpDeviceLookup;
    }

    public class ProductDeviceInjector : DeviceInjector
    {
        public ProductDeviceInjector(Network aNetwork)
            : base(aNetwork)
        {
            iDeviceList = new CpDeviceListUpnpServiceType("av.openhome.org", "Product", 1, Added, Removed);
        }
    }

    public class ContentDirectoryDeviceInjector : DeviceInjector
    {
        public ContentDirectoryDeviceInjector(Network aNetwork)
            : base(aNetwork)
        {
            iDeviceList = new CpDeviceListUpnpServiceType("upnp.org", "ContentDirectory", 1, Added, Removed);
        }
    }

    public interface INetwork : IWatchableThread, IDisposable
    {
        IWatchableThread WatchableThread { get; }
        IWatchableThread WatchableSubscribeThread { get; }
        //void Refresh();
        IWatchableUnordered<IWatchableDevice> Create<T>() where T : IProxy;
    }

    public class Network : INetwork, IMockable
    {
        public Network(IWatchableThread aThread, IWatchableThread aSubscribeThread)
        {
            iLock = new object();

            iSubscribeThread = aSubscribeThread;
            iThread = aThread;

            iDevices = new List<WatchableDevice>();
            iMockDevices = new Dictionary<string, WatchableDeviceMock>();
            iDeviceLists = new Dictionary<Type, WatchableUnordered<IWatchableDevice>>();
        }

        public void Dispose()
        {
            iDevices.Clear();
            iDevices = null;

            foreach (WatchableDeviceMock d in iMockDevices.Values)
            {
                d.Dispose();
            }
            iMockDevices.Clear();
            iMockDevices = null;

            foreach (WatchableUnordered<IWatchableDevice> l in iDeviceLists.Values)
            {
                l.Dispose();
            }
            iDeviceLists.Clear();
            iDeviceLists = null;
        }

        public void Add(WatchableDevice aDevice)
        {
            lock (iLock)
            {
                iDevices.Add(aDevice);

                foreach (KeyValuePair<Type, WatchableUnordered<IWatchableDevice>> l in iDeviceLists)
                {
                    if (aDevice.HasService(l.Key))
                    {
                        l.Value.Add(aDevice);
                    }
                }
            }
        }

        private void Add(WatchableDeviceMock aDevice)
        {
            iMockDevices.Add(aDevice.Udn, aDevice);
            Add(aDevice as WatchableDevice);
        }

        public void Remove(WatchableDevice aDevice)
        {
            lock (iLock)
            {
                iDevices.Remove(aDevice);

                foreach (KeyValuePair<Type, WatchableUnordered<IWatchableDevice>> l in iDeviceLists)
                {
                    if (aDevice.HasService(l.Key))
                    {
                        l.Value.Remove(aDevice);
                    }
                }
            }
        }

        private void Remove(string aUdn)
        {
            WatchableDeviceMock device;

            if (iMockDevices.TryGetValue(aUdn, out device))
            {
                Remove(device);
            }
        }

        public IWatchableUnordered<IWatchableDevice> Create<T>() where T : IProxy
        {
            lock (iLock)
            {
                Type key = typeof(T);

                WatchableUnordered<IWatchableDevice> list;
                if (iDeviceLists.TryGetValue(key, out list))
                {
                    return list;
                }
                else
                {
                    list = new WatchableUnordered<IWatchableDevice>(iThread);
                    iDeviceLists.Add(key, list);
                    foreach (WatchableDevice d in iDevices)
                    {
                        if (d.HasService(key))
                        {
                            list.Add(d);
                        }
                    }
                    return list;
                }
            }
        }

        public IWatchableThread WatchableThread
        {
            get
            {
                return iThread;
            }
        }

        public IWatchableThread WatchableSubscribeThread
        {
            get
            {
                return iSubscribeThread;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();

            if (command == "small")
            {
                Add(WatchableDeviceMock.CreateDsm(this, "4c494e4e-0026-0f99-1112-ef000004013f", "Sitting Room", "Klimax DSM", "Info Time Volume Sender"));
            }
            else if (command == "medium")
            {
                Add(WatchableDeviceMock.CreateDs(this, "4c494e4e-0026-0f99-1111-ef000004013f", "Kitchen", "Sneaky Music DS", "Info Time Volume Sender"));
                Add(WatchableDeviceMock.CreateDsm(this, "4c494e4e-0026-0f99-1112-ef000004013f", "Sitting Room", "Klimax DSM", "Info Time Volume Sender"));
                Add(WatchableDeviceMock.CreateDsm(this, "4c494e4e-0026-0f99-1113-ef000004013f", "Bedroom", "Kiko DSM", "Info Time Volume Sender"));
                Add(WatchableDeviceMock.CreateDs(this, "4c494e4e-0026-0f99-1114-ef000004013f", "Dining Room", "Majik DS", "Info Time Volume Sender"));
            }
            else if (command == "large")
            {
                throw new NotImplementedException();
            }
            else if (command == "add")
            {
                IEnumerable<string> value = aValue.Skip(1);

                string type = value.First();

                if (type == "ds" || type == "dsm" /*|| type == "mediaserver" */)
                {
                    value = value.Skip(1);

                    string udn = value.First();

                    WatchableDeviceMock device;

                    if (iMockDevices.TryGetValue(udn, out device))
                    {
                        Add(device as WatchableDevice);
                    }
                    else if (type == "ds")
                    {
                        Add(WatchableDeviceMock.CreateDs(this, udn));
                    }
                    else if (type == "dsm")
                    {
                        Add(WatchableDeviceMock.CreateDsm(this, udn));
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
                    Remove(value.First());
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

                        WatchableDeviceMock device;

                        if (iMockDevices.TryGetValue(udn, out device))
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

        private List<WatchableDevice> iDevices;
        private Dictionary<string, WatchableDeviceMock> iMockDevices;
        private Dictionary<Type, WatchableUnordered<IWatchableDevice>> iDeviceLists;
    }
}
