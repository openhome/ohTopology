using System;
using System.Threading;

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

    public interface ITopology1Product
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

    public abstract class Topology1Product : Watchable<IServiceOpenHomeOrgProduct1>, ITopology1Product
    {
        protected Topology1Product(WatchableThread aThread, string aId, IServiceOpenHomeOrgProduct1 aValue)
            : base(aThread, aId, aValue)
        {
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
    }

    public class ServiceOpenHomeOrgProduct1 : IServiceOpenHomeOrgProduct1, IDisposable
    {
        public ServiceOpenHomeOrgProduct1(WatchableThread aThread, CpProxyAvOpenhomeOrgProduct1 aService)
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

    public class WatchableServiceOpenHomeOrgProduct1 : Topology1Product
    {
        public WatchableServiceOpenHomeOrgProduct1(WatchableThread aThread, string aId, CpProxyAvOpenhomeOrgProduct1 aService)
            : base(aThread, aId, new ServiceOpenHomeOrgProduct1(aThread, aService))
        {
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
    }

    public class MockServiceOpenHomeOrgProduct1 : IServiceOpenHomeOrgProduct1, IDisposable
    {
        public MockServiceOpenHomeOrgProduct1()
        {
        }

        public void Dispose()
        {
        }

        public IWatchable<string> Room
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> Name
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<uint> SourceIndex
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceXml
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<bool> Standby
        {
            get { throw new NotImplementedException(); }
        }

        public void SetSourceIndex(uint aValue)
        {
            throw new NotImplementedException();
        }

        public void SetSourceIndexByName(string aValue)
        {
            throw new NotImplementedException();
        }

        public void SetStandby(bool aValue)
        {
            throw new NotImplementedException();
        }
    }

    public class MockWatchableServiceOpenHomeOrgProduct1 : Topology1Product
    {
        public MockWatchableServiceOpenHomeOrgProduct1(WatchableThread aThread, string aId)
            : base(aThread, aId, new MockServiceOpenHomeOrgProduct1())
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
    }
}
