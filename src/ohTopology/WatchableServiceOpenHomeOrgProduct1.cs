using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgProduct1
    {
        IWatchable<string> Room { get; }
        IWatchable<string> Name { get; }
        IWatchable<uint> SourceIndex { get; }
        IWatchable<string> SourceXml { get; }
        IWatchable<bool> Standby { get; }

        void SetSourceIndex(uint aValue);
        void SetSourceIndexByName(string aValue);
        void SetStandby(bool aValue);
    }

    public interface IProduct : IServiceOpenHomeOrgProduct1
    {
        string Attributes { get; }
        string ManufacturerImageUri { get; }
        string ManufacturerInfo { get; }
        string ManufacturerName { get; }
        string ManufacturerUrl { get; }
        string ModelImageUri { get; }
        string ModelInfo { get; }
        string ModelName { get; }
        string ModelUrl { get; }
        string ProductImageUri { get; }
        string ProductInfo { get; }
        string ProductUrl { get; }
    }

    public class WatchableProductCollection : WatchableCollection<Product>
    {
        public WatchableProductCollection(IWatchableThread aThread)
            : base(aThread)
        {
            iLock = new object();
            iDisposed = false;

            iThread = aThread;

            iList = new List<Product>();
            iProductLookup = new Dictionary<IWatchableDevice, Product>();
            iPendingServices = new Dictionary<IWatchableDevice, CpProxyAvOpenhomeOrgProduct1>();
        }

        public new void Dispose()
        {
            lock (iLock)
            {
                if(iDisposed)
                {
                    throw new ObjectDisposedException("WatchableProductCollection.Dispose");
                }

                foreach (CpProxyAvOpenhomeOrgProduct1 s in iPendingServices.Values)
                {
                    s.Dispose();
                }
                iPendingServices = null;

                foreach (Product p in iList)
                {
                    p.Dispose();
                }
                iList = null;
                iProductLookup = null;

                iDisposed = true;
            }
        }

        public void Add(IWatchableDevice aDevice)
        {
            if (aDevice is WatchableDevice)
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("WatchableProductCollection.Add");
                    }

                    WatchableDevice device = aDevice as WatchableDevice;

                    CpProxyAvOpenhomeOrgProduct1 service = new CpProxyAvOpenhomeOrgProduct1(device.Device);
                    service.SetPropertyInitialEvent(delegate
                    {
                        lock (iLock)
                        {
                            if (iDisposed)
                            {
                                return;
                            }

                            if (iPendingServices.ContainsKey(aDevice))
                            {
                                iPendingServices.Remove(aDevice);

                                Product product = new WatchableProduct(iThread, service);

                                uint index = (uint)iList.Count;
                                iList.Add(product);
                                iProductLookup.Add(aDevice, product);

                                CollectionAdd(product, index);
                            }
                        }
                    });

                    iPendingServices.Add(aDevice, service);
                    service.Subscribe();
                }
            }
            else if (aDevice is MockWatchableDevice)
            {
                Product product = new MockWatchableProduct(iThread);

                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("WatchableProductCollection.Dispose");
                    }

                    uint index = (uint)iList.Count;
                    iList.Add(product);
                    iProductLookup.Add(aDevice, product);

                    CollectionAdd(product, index);
                }
            }
        }

        public void Remove(IWatchableDevice aDevice)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("WatchableProductCollection.Remove");
                }

                // handle outstanding subscription
                CpProxyAvOpenhomeOrgProduct1 service;
                if (iPendingServices.TryGetValue(aDevice, out service))
                {
                    iPendingServices.Remove(aDevice);

                    service.Dispose();
                    
                    return;
                }

                Product product;
                if (iProductLookup.TryGetValue(aDevice, out product))
                {
                    uint index = (uint)iList.IndexOf(product);
                    iList.Remove(product);
                    iProductLookup.Remove(aDevice);

                    CollectionRemove(product, index);

                    product.Dispose();
                }
            }
        }

        private object iLock;
        private bool iDisposed;

        private IWatchableThread iThread;
        private Dictionary<IWatchableDevice, Product> iProductLookup;
        private List<Product> iList;
        private Dictionary<IWatchableDevice, CpProxyAvOpenhomeOrgProduct1> iPendingServices;
    }

    public abstract class Product : IProduct, IDisposable
    {
        protected Product(IServiceOpenHomeOrgProduct1 aService)
        {
            iService = aService;
        }

        public abstract void Dispose();

        public IWatchable<string> Room
        {
            get
            {
                return iService.Room;
            }
        }

        public IWatchable<string> Name
        {
            get 
            {
                return iService.Name;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get 
            {
                return iService.SourceIndex;
            }
        }

        public IWatchable<string> SourceXml
        {
            get
            {
                return iService.SourceXml;
            }
        }

        public IWatchable<bool> Standby
        {
            get
            {
                return iService.Standby;
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            iService.SetSourceIndex(aValue);
        }

        public void SetSourceIndexByName(string aValue)
        {
            iService.SetSourceIndexByName(aValue);
        }

        public void SetStandby(bool aValue)
        {
            iService.SetStandby(aValue);
        }

        public string Attributes
        {
            get
            {
                return iAttributes;
            }
        }

        public string ManufacturerImageUri
        {
            get
            {
                return iManufacturerImageUri;
            }
        }

        public string ManufacturerInfo
        {
            get
            {
                return iManufacturerInfo;
            }
        }

        public string ManufacturerName
        {
            get
            {
                return iManufacturerName;
            }
        }

        public string ManufacturerUrl
        {
            get
            {
                return iManufacturerUrl;
            }
        }

        public string ModelImageUri
        {
            get
            {
                return iModelImageUri;
            }
        }

        public string ModelInfo
        {
            get
            {
                return iModelInfo;
            }
        }

        public string ModelName
        {
            get
            {
                return iModelName;
            }
        }

        public string ModelUrl
        {
            get
            {
                return iModelUrl;
            }
        }

        public string ProductImageUri
        {
            get
            {
                return iProductImageUri;
            }
        }

        public string ProductInfo
        {
            get
            {
                return iProductInfo;
            }
        }

        public string ProductUrl
        {
            get
            {
                return iProductUrl;
            }
        }

        protected string iAttributes;
        protected string iManufacturerImageUri;
        protected string iManufacturerInfo;
        protected string iManufacturerName;
        protected string iManufacturerUrl;
        protected string iModelImageUri;
        protected string iModelInfo;
        protected string iModelName;
        protected string iModelUrl;
        protected string iProductImageUri;
        protected string iProductInfo;
        protected string iProductUrl;

        private IServiceOpenHomeOrgProduct1 iService;
    }

    public class ServiceOpenHomeOrgProduct1 : IServiceOpenHomeOrgProduct1, IDisposable
    {
        public ServiceOpenHomeOrgProduct1(IWatchableThread aThread, CpProxyAvOpenhomeOrgProduct1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyProductNameChanged(HandleRoomChanged);
                iService.SetPropertyProductNameChanged(HandleNameChanged);
                iService.SetPropertySourceIndexChanged(HandleSourceIndexChanged);
                iService.SetPropertySourceXmlChanged(HandleSourceXmlChanged);
                iService.SetPropertyStandbyChanged(HandleStandbyChanged);

                iRoom = new Watchable<string>(aThread, "Room", iService.PropertyProductRoom());
                iName = new Watchable<string>(aThread, "Name", iService.PropertyProductName());
                iSourceIndex = new Watchable<uint>(aThread, "SourceIndex", iService.PropertySourceIndex());
                iSourceXml = new Watchable<string>(aThread, "SourceXml", iService.PropertySourceXml());
                iStandby = new Watchable<bool>(aThread, "Standby", iService.PropertyStandby());
            }
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.Dispose");
                }

                iService = null;

                iDisposed = true;
            }
        }

        public IWatchable<string> Room
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.Room");
                    }

                    return iRoom;
                }
            }
        }

        public IWatchable<string> Name
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.Name");
                    }

                    return iName;
                }
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get
            {
                return iSourceIndex;
            }
        }

        public IWatchable<string> SourceXml
        {
            get
            {
                return iSourceXml;
            }
        }

        public IWatchable<bool> Standby
        {
            get
            {
                return iStandby;
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.SetSourceIndex");
                }

                iService.BeginSetSourceIndex(aValue, null);
            }
        }

        public void SetSourceIndexByName(string aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.SetSourceIndexByName");
                }

                iService.BeginSetSourceIndexByName(aValue, null);
            }
        }

        public void SetStandby(bool aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgProduct1.SetStandby");
                }

                iService.BeginSetStandby(aValue, null);
            }
        }

        private void HandleRoomChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }
                
                iRoom.Update(iService.PropertyProductRoom());
            }
        }

        private void HandleNameChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }
                
                iName.Update(iService.PropertyProductName());
            }
        }

        private void HandleSourceIndexChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iSourceIndex.Update(iService.PropertySourceIndex());
            }
        }

        private void HandleSourceXmlChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iSourceXml.Update(iService.PropertySourceXml());
            }
        }

        private void HandleStandbyChanged()
        {
            lock (iLock)
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        return;
                    }

                    iStandby.Update(iService.PropertyStandby());
                }
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgProduct1 iService;

        private Watchable<string> iRoom;
        private Watchable<string> iName;
        private Watchable<uint> iSourceIndex;
        private Watchable<string> iSourceXml;
        private Watchable<bool> iStandby;
    }

    public class WatchableProduct : Product
    {
        public WatchableProduct(IWatchableThread aThread, CpProxyAvOpenhomeOrgProduct1 aService)
            : base(new ServiceOpenHomeOrgProduct1(aThread, aService))
        {
            iCpService = aService;

            iAttributes = aService.PropertyAttributes();
            iManufacturerImageUri = aService.PropertyManufacturerImageUri();
            iManufacturerInfo = aService.PropertyManufacturerInfo();
            iManufacturerName = aService.PropertyManufacturerName();
            iManufacturerUrl = aService.PropertyManufacturerUrl();
            iModelImageUri = aService.PropertyModelImageUri();
            iModelInfo = aService.PropertyModelInfo();
            iModelName = aService.PropertyModelName();
            iModelUrl = aService.PropertyModelUrl();
            iProductImageUri = aService.PropertyProductImageUri();
            iProductInfo = aService.PropertyProductInfo();
            iProductUrl = aService.PropertyProductUrl();
        }

        public override void Dispose()
        {
            if (iCpService != null)
            {
                iCpService.Dispose();
            }
        }

        private CpProxyAvOpenhomeOrgProduct1 iCpService;
    }

    public class MockServiceOpenHomeOrgProduct1 : IServiceOpenHomeOrgProduct1, IMockable, IDisposable
    {
        public MockServiceOpenHomeOrgProduct1(IWatchableThread aThread)
        {
            iRoom = new Watchable<string>(aThread, "Room", string.Empty);
            iName = new Watchable<string>(aThread, "Name", string.Empty);
            iSourceIndex = new Watchable<uint>(aThread, "SourceIndex", 0);
            iSourceXml = new Watchable<string>(aThread, "SourceXml", string.Empty);
            iStandby = new Watchable<bool>(aThread, "Standby", false);
        }

        public void Dispose()
        {
        }

        public void Execute(IEnumerable<string> aCommand)
        {
        }

        public IWatchable<string> Room
        {
            get
            {
                return iRoom;
            }
        }

        public IWatchable<string> Name
        {
            get
            {
                return iName;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get
            {
                return iSourceIndex;
            }
        }

        public IWatchable<string> SourceXml
        {
            get
            {
                return iSourceXml;
            }
        }

        public IWatchable<bool> Standby
        {
            get
            {
                return iStandby;
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            iSourceIndex.Update(aValue);
        }

        public void SetSourceIndexByName(string aValue)
        {
            throw new NotSupportedException();
        }

        public void SetStandby(bool aValue)
        {
            iStandby.Update(aValue);
        }

        private Watchable<string> iRoom;
        private Watchable<string> iName;
        private Watchable<uint> iSourceIndex;
        private Watchable<string> iSourceXml;
        private Watchable<bool> iStandby;
    }

    public class MockWatchableProduct : Product, IMockable
    {
        public MockWatchableProduct(IWatchableThread aThread)
            : base(new MockServiceOpenHomeOrgProduct1(aThread))
        {
            iAttributes = string.Empty;
            iManufacturerImageUri = string.Empty;
            iManufacturerInfo = string.Empty;
            iManufacturerName = string.Empty;
            iManufacturerUrl = string.Empty;
            iModelImageUri = string.Empty;
            iModelInfo = string.Empty;
            iModelName = string.Empty;
            iModelUrl = string.Empty;
            iProductImageUri = string.Empty;
            iProductInfo = string.Empty;
            iProductUrl = string.Empty;
        }

        public override void Dispose()
        {
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First();
            if (command == "attributes")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iAttributes = value.First();
            }
            if (command == "manufacturerimageuri")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerImageUri = value.First();
            }
            if (command == "manufacturerinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerInfo = value.First();
            }
            if (command == "manufacturername")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerName = value.First();
            }
            if (command == "manufacturerurl")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerUrl = value.First();
            }
        }
    }
}
