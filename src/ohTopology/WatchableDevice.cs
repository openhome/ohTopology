using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IWatchableServiceFactory
    {
        void Subscribe(WatchableDevice aDevice, Action<IWatchableService> aCallback);
        void Unsubscribe();
    }

    public interface IWatchableDevice
    {
        string Udn { get; }
        bool GetAttribute(string aKey, out string aValue);
        void Subscribe<T>(Action<IWatchableDevice, T> aCallback) where T : IWatchableService;
        void Unsubscribe<T>() where T : IWatchableService;
    }

    public class WatchableDeviceCollection : WatchableCollection<IWatchableDevice>
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

    public class WatchableDevice : IWatchableDevice
    {
        public WatchableDevice(IWatchableThread aThread, CpDevice aDevice)
        {
            iLock = new object();
            iDisposed = false;

            iFactories = new Dictionary<Type, IWatchableServiceFactory>();

            // add a factory for each type of watchable service
            IWatchableServiceFactory factory = new WatchableProductFactory(aThread);
            iFactories.Add(typeof(Product), factory);

            iServices = new Dictionary<Type, IWatchableService>();
            iServiceRefCount = new Dictionary<Type, uint>();

            iDevice = aDevice;
            iDevice.AddRef();
        }

        public string Udn
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("WatchableDevice.Udn");
                    }

                    return iDevice.Udn();
                }
            }
        }

        public bool GetAttribute(string aKey, out string aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("WatchableDevice.GetAttribute");
                }

                return iDevice.GetAttribute(aKey, out aValue);
            }
        }

        public void Subscribe<T>(Action<IWatchableDevice, T> aCallback) where T : IWatchableService
        {
            lock (iLock)
            {
                IWatchableService service;
                if (iServices.TryGetValue(typeof(T), out service))
                {
                    ++iServiceRefCount[typeof(T)];

                    Task task = new Task(new Action(delegate
                    {
                        aCallback(this, (T)service);
                    }));
                    task.Start();
                }
                else
                {
                    IWatchableServiceFactory factory = iFactories[typeof(T)];
                    factory.Subscribe(this, delegate(IWatchableService aService)
                    {
                        lock (iLock)
                        {
                            iServices.Add(typeof(T), aService);
                            iServiceRefCount.Add(typeof(T), 1);
                        }

                        aCallback(this, (T)aService);
                    });
                }
            }
        }

        public void Unsubscribe<T>() where T : IWatchableService
        {
            lock (iLock)
            {
                IWatchableService service;
                if (iServices.TryGetValue(typeof(T), out service))
                {
                    --iServiceRefCount[typeof(T)];

                    if (iServiceRefCount[typeof(T)] == 0)
                    {
                        service.Dispose();

                        iServices.Remove(typeof(T));
                        iServiceRefCount.Remove(typeof(T));
                    }
                }
                else
                {
                    // service could be pending subscribe
                    IWatchableServiceFactory factory = iFactories[typeof(T)];
                    factory.Unsubscribe();
                }
            }
        }

        public CpDevice Device
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("WatchableDevice.Device");
                    }

                    return iDevice;
                }
            }
        }

        protected object iLock;
        protected bool iDisposed;

        protected CpDevice iDevice;

        private Dictionary<Type, IWatchableServiceFactory> iFactories;

        private Dictionary<Type, IWatchableService> iServices;
        private Dictionary<Type, uint> iServiceRefCount;
    }

    public class DisposableWatchableDevice : WatchableDevice, IDisposable
    {
        public DisposableWatchableDevice(IWatchableThread aThread, CpDevice aDevice)
            : base(aThread, aDevice)
        {
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("DisposableWatchableDevice.Dispose");
                }

                iDevice.RemoveRef();
                iDevice = null;

                iDisposed = true;
            }
        }
    }

    public class MockWatchableDevice : IWatchableDevice, IMockable
    {
        public MockWatchableDevice(string aUdn)
        {
            iUdn = aUdn;
            iServices = new Dictionary<Type, IWatchableService>();
        }

        public string Udn
        {
            get
            {
                return iUdn;
            }
        }

        public bool GetAttribute(string aKey, out string aValue)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T>(Action<T> aCallback) where T : IWatchableService
        {
        }

        public void Subscribe<T>(Action<IWatchableDevice, T> aCallback) where T : IWatchableService
        {
            IWatchableService service;
            if (iServices.TryGetValue(typeof(T), out service))
            {
                Task task = new Task(new Action(delegate { 
                    aCallback(this, (T)service);
                }));
                task.Start();

                return;
            }

            throw new NotSupportedException();
        }

        public void Unsubscribe<T>() where T : IWatchableService
        {
        }

        public void Add<T>(IWatchableService aService) where T : IWatchableService
        {
            iServices.Add(typeof(T), aService);
        }

        internal bool HasService(Type aType)
        {
            foreach (Type s in iServices.Keys)
            {
                if (aType == s)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void Execute(IEnumerable<string> aValue)
        {
        }

        protected Dictionary<Type, IWatchableService> iServices;

        private string iUdn;
    }

    public class MockWatchableDs : MockWatchableDevice
    {
        public MockWatchableDs(IWatchableThread aThread, string aUdn)
            : base(aUdn)
        {
            // add a mock product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            SourceXml xml = new SourceXml(sources.ToArray());
            MockWatchableProduct product = new MockWatchableProduct(aThread, aUdn, "Main Room", "Mock DS", 0, xml, true, "",
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", "");
            Add<Product>(product);
        }

        public MockWatchableDs(IWatchableThread aThread, string aUdn, string aRoom, string aName)
            : base(aUdn)
        {
            // add a mock product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            SourceXml xml = new SourceXml(sources.ToArray());
            MockWatchableProduct product = new MockWatchableProduct(aThread, aUdn, aRoom, aName, 0, xml, true, "",
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", "");
            Add<Product>(product);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            base.Execute(aValue);

            Type key = typeof(Product);
            string command = aValue.First();
            if (command == "product")
            {
                foreach (KeyValuePair<Type, IWatchableService> s in iServices)
                {
                    if (s.Key == key)
                    {
                        MockWatchableProduct p = s.Value as MockWatchableProduct;
                        p.Execute(aValue.Skip(1));
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public class MockWatchableDsm : MockWatchableDevice
    {
        public MockWatchableDsm(IWatchableThread aThread, string aUdn)
            : base(aUdn)
        {
            // add a mock product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            sources.Add(new SourceXml.Source("Analog1", "Analog", true));
            sources.Add(new SourceXml.Source("Analog2", "Analog", true));
            sources.Add(new SourceXml.Source("Phono", "Analog", true));
            sources.Add(new SourceXml.Source("SPDIF1", "Digital", true));
            sources.Add(new SourceXml.Source("SPDIF2", "Digital", true));
            sources.Add(new SourceXml.Source("TOSLINK1", "Digital", true));
            SourceXml xml = new SourceXml(sources.ToArray());
            MockWatchableProduct product = new MockWatchableProduct(aThread, aUdn, "Main Room", "Mock DS", 0, xml, true, "",
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", "");
            Add<Product>(product);
        }

        public MockWatchableDsm(IWatchableThread aThread, string aUdn, string aRoom, string aName)
            : base(aUdn)
        {
            // add a mock product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            sources.Add(new SourceXml.Source("Analog1", "Analog", true));
            sources.Add(new SourceXml.Source("Analog2", "Analog", true));
            sources.Add(new SourceXml.Source("Phono", "Analog", true));
            sources.Add(new SourceXml.Source("SPDIF1", "Digital", true));
            sources.Add(new SourceXml.Source("SPDIF2", "Digital", true));
            sources.Add(new SourceXml.Source("TOSLINK1", "Digital", true));
            sources.Add(new SourceXml.Source("TOSLINK2", "Digital", true));
            SourceXml xml = new SourceXml(sources.ToArray());
            MockWatchableProduct product = new MockWatchableProduct(aThread, aUdn, aRoom, aName, 0, xml, true, "",
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", "");
            Add<Product>(product);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            base.Execute(aValue);

            Type key = typeof(Product);
            string command = aValue.First();
            if (command == "product")
            {
                foreach (KeyValuePair<Type, IWatchableService> s in iServices)
                {
                    if (s.Key == key)
                    {
                        MockWatchableProduct p = s.Value as MockWatchableProduct;
                        p.Execute(aValue.Skip(1));
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
