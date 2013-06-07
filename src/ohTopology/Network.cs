using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.MediaServer;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public abstract class DeviceInjector : IDisposable
    {
        private Network iNetwork;
        private Dictionary<string, Device> iCpDeviceLookup;

        protected CpDeviceListUpnpServiceType iDeviceList;

        protected DeviceInjector(Network aNetwork)
        {
            iNetwork = aNetwork;

            iCpDeviceLookup = new Dictionary<string, Device>();
        }

        protected void Added(CpDeviceList aList, CpDevice aDevice)
        {
            iNetwork.Execute(() =>
            {
                Device device = Create(iNetwork, aDevice);
                iCpDeviceLookup.Add(device.Udn, device);
                iNetwork.Add(device);
            });
        }

        protected void Removed(CpDeviceList aList, CpDevice aDevice)
        {
            iNetwork.Execute(() =>
            {
                Device device = iCpDeviceLookup[aDevice.Udn()];
                iCpDeviceLookup.Remove(device.Udn);
                iNetwork.Remove(device);
                device.Dispose();
            });
        }

        protected virtual Device Create(INetwork aNetwork, CpDevice aDevice)
        {
            return (DeviceFactory.Create(aNetwork, aDevice));
        }

        // IDisposable

        public void Dispose()
        {
            iDeviceList.Dispose();
            iDeviceList = null;

            iNetwork.Execute(() =>
            {
                foreach (var device in iCpDeviceLookup.Values)
                {
                    device.Dispose();
                }
            });
            iCpDeviceLookup.Clear();
            iCpDeviceLookup = null;
        }
    }

    public class DeviceInjectorProduct : DeviceInjector
    {
        public DeviceInjectorProduct(Network aNetwork)
            : base(aNetwork)
        {
            iDeviceList = new CpDeviceListUpnpServiceType("av.openhome.org", "Product", 1, Added, Removed);
        }
    }

    public class DeviceInjectorContentDirectory : DeviceInjector
    {
        public DeviceInjectorContentDirectory(Network aNetwork)
            : base(aNetwork)
        {
            iDeviceList = new CpDeviceListUpnpServiceType("upnp.org", "ContentDirectory", 1, Added, Removed);
        }

        protected override Device Create(INetwork aNetwork, CpDevice aDevice)
        {
            return (new DeviceMediaServerUpnp(aNetwork, aDevice));
        }
    }

    public class DeviceInjectorMock : IMockable, IDisposable
    {
        private Network iNetwork;
        private Dictionary<string, Device> iMockDevices;

        public DeviceInjectorMock(Network aNetwork)
        {
            iNetwork = aNetwork;
            iMockDevices = new Dictionary<string, Device>();
        }

        public void Dispose()
        {
            iNetwork.Execute(() =>
            {
                foreach (Device d in iMockDevices.Values)
                {
                    iNetwork.Remove(d);
                    d.Dispose();
                }
                iMockDevices.Clear();
                iMockDevices = null;
            });
        }

        public void Execute(IEnumerable<string> aValue)
        {
            iNetwork.Execute(() =>
            {
                string command = aValue.First().ToLowerInvariant();

                if (command == "small")
                {
                    CreateAndAdd(DeviceFactory.CreateDsm(iNetwork, "4c494e4e-0026-0f99-1112-ef000004013f", "Sitting Room", "Klimax DSM", "Info Time Volume Sender"));
                    CreateAndAdd(DeviceFactory.CreateMediaServer(iNetwork, "4c494e4e-0026-0f99-0000-000000000000"));
                    return;
                }
                else if (command == "medium")
                {
                    CreateAndAdd(DeviceFactory.CreateDs(iNetwork, "4c494e4e-0026-0f99-1111-ef000004013f", "Kitchen", "Sneaky Music DS", "Info Time Volume Sender"));
                    CreateAndAdd(DeviceFactory.CreateDsm(iNetwork, "4c494e4e-0026-0f99-1112-ef000004013f", "Sitting Room", "Klimax DSM", "Info Time Volume Sender"));
                    CreateAndAdd(DeviceFactory.CreateDsm(iNetwork, "4c494e4e-0026-0f99-1113-ef000004013f", "Bedroom", "Kiko DSM", "Info Time Volume Sender"));
                    CreateAndAdd(DeviceFactory.CreateDs(iNetwork, "4c494e4e-0026-0f99-1114-ef000004013f", "Dining Room", "Majik DS", "Info Time Volume Sender"));
                    CreateAndAdd(DeviceFactory.CreateMediaServer(iNetwork, "4c494e4e-0026-0f99-0000-000000000000"));
                    return;
                }
                else if (command == "large")
                {
                    throw new NotImplementedException();
                }
                else if (command == "create")
                {
                    IEnumerable<string> value = aValue.Skip(1);

                    string type = value.First();

                    value = value.Skip(1);

                    string udn = value.First();

                    if (type == "ds")
                    {
                        Create(DeviceFactory.CreateDs(iNetwork, udn));
                        return;
                    }
                    else if (type == "dsm")
                    {
                        Create(DeviceFactory.CreateDsm(iNetwork, udn));
                        return;
                    }
                }
                else if (command == "add")
                {
                    IEnumerable<string> value = aValue.Skip(1);

                    string udn = value.First();

                    Device device;
                    if (iMockDevices.TryGetValue(udn, out device))
                    {
                        iNetwork.Add(device);
                        return;
                    }
                }
                else if (command == "remove")
                {
                    IEnumerable<string> value = aValue.Skip(1);

                    string udn = value.First();

                    Device device;
                    if (iMockDevices.TryGetValue(udn, out device))
                    {
                        iNetwork.Remove(device);
                        return;
                    }
                }
                else if (command == "destroy")
                {
                    IEnumerable<string> value = aValue.Skip(1);

                    string udn = value.First();

                    Device device;
                    if (iMockDevices.TryGetValue(udn, out device))
                    {
                        iNetwork.Remove(device);
                        iMockDevices.Remove(device.Udn);
                        device.Dispose();
                        return;
                    }
                }
                else if (command == "update")
                {
                    IEnumerable<string> value = aValue.Skip(1);

                    string udn = value.First();

                    Device device;

                    if (iMockDevices.TryGetValue(udn, out device))
                    {
                        device.Execute(value.Skip(1));
                        return;
                    }
                }

                throw new NotSupportedException();
            });
        }

        private void Create(Device aDevice)
        {
            iMockDevices.Add(aDevice.Udn, aDevice);
        }

        private void CreateAndAdd(Device aDevice)
        {
            Create(aDevice);
            iNetwork.Add(aDevice);
        }
    }

    public interface INetwork : IWatchableThread, IDisposable
    {
        ITagManager TagManager { get; }
        IWatchableUnordered<IDevice> Create<T>() where T : IProxy;
        //void Refresh();
    }

    public class Network : INetwork
    {
        private object iLock;

        protected readonly IWatchableThread iThread;

        private readonly ITagManager iTagManager;

        private List<Device> iDevices;
        private Dictionary<Type, WatchableUnordered<IDevice>> iDeviceLists;

        public Network(IWatchableThread aThread)
        {
            iLock = new object();

            iThread = aThread;

            iTagManager = new TagManager();
            iDevices = new List<Device>();
            iDeviceLists = new Dictionary<Type, WatchableUnordered<IDevice>>();
        }

        public void Dispose()
        {
            iDevices.Clear();
            iDevices = null;

            foreach (WatchableUnordered<IDevice> l in iDeviceLists.Values)
            {
                l.Dispose();
            }
            iDeviceLists.Clear();
            iDeviceLists = null;
        }

        public void Add(Device aDevice)
        {
            lock (iLock)
            {
                iDevices.Add(aDevice);

                foreach (KeyValuePair<Type, WatchableUnordered<IDevice>> kvp in iDeviceLists)
                {
                    if (aDevice.HasService(kvp.Key))
                    {
                        kvp.Value.Add(aDevice);
                    }
                }
            }
        }

        public void Remove(Device aDevice)
        {
            lock (iLock)
            {
                iDevices.Remove(aDevice);

                foreach (KeyValuePair<Type, WatchableUnordered<IDevice>> l in iDeviceLists)
                {
                    if (aDevice.HasService(l.Key))
                    {
                        l.Value.Remove(aDevice);
                    }
                }
            }
        }

        public IWatchableUnordered<IDevice> Create<T>() where T : IProxy
        {
            lock (iLock)
            {
                Type key = typeof(T);

                WatchableUnordered<IDevice> list;
                if (iDeviceLists.TryGetValue(key, out list))
                {
                    return list;
                }
                else
                {
                    list = new WatchableUnordered<IDevice>(iThread);
                    iDeviceLists.Add(key, list);
                    foreach (Device d in iDevices)
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

        public ITagManager TagManager
        {
            get
            {
                return (iTagManager);
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
            bool complete = false;

            while (!complete)
            {
                Device[] devices = null;
                lock (iLock)
                {
                    devices = iDevices.ToArray();
                }
                foreach (Device d in devices)
                {
                    complete |= d.Wait();
                }

                iThread.Wait();
            }
        }

        public void Wait(Action aAction)
        {
            bool complete = false;

            while (!complete)
            {
                Device[] devices = null;
                lock (iLock)
                {
                    devices = iDevices.ToArray();
                }
                foreach (Device d in devices)
                {
                    complete |= d.Wait();
                }

                iThread.Wait(aAction);
            }
        }
    }
}
