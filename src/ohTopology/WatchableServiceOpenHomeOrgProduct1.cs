using System;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Net.ControlPoint
{
    public interface IWatchableServiceOpenHomeOrgProduct1 : IDisposable
    {
        IWatchable<string> Attributes { get; }
        IWatchable<string> ManufacturerImageUri { get; }
        IWatchable<string> ManufacturerInfo { get; }
        IWatchable<string> ManufacturerName { get; }
        IWatchable<string> ManufacturerUrl { get; }
        IWatchable<string> ModelImageUri { get; }
        IWatchable<string> ModelInfo { get; }
        IWatchable<string> ModelName { get; }
        IWatchable<string> ModelUrl { get; }
        IWatchable<string> ProductImageUri { get; }
        IWatchable<string> ProductInfo { get; }
        IWatchable<string> ProductName { get; }
        IWatchable<string> ProductRoom { get; }
        IWatchable<string> ProductUrl { get; }
        IWatchable<string> SourceCount { get; }
        IWatchable<string> SourceIndex { get; }
        IWatchable<string> SourceName { get; }
        IWatchable<string> SourceType { get; }
        IWatchable<string> SourceVisible { get; }
        IWatchable<string> SourceXml { get; }
        IWatchable<string> SourceXmlChangeCount { get; }
        IWatchable<string> Standby { get; }

        void GetAttributes(out string aValue);
        void GetManufacturer(out string aName, out string aInfo, out string aUrl, out string aImageUri);
        void GetModel(out string aName, out string aInfo, out string aUrl, out string aImageUri);
        void GetProduct(out string aRoom, out string aName, out string aInfo, out string aUrl, out string aImageUri);
        void GetSource(uint aIndex, out string aSystemName, out string aType, out string aName, out bool aVisible);
        void GetSourceCount(out uint aValue);
        void GetSourceIndex(out uint aValue);
        void GetSourceXml(out string aValue);
        void GetSourceXmlChangeCount(out uint aValue);
        void GetStandby(out bool aValue);

        void SetSourceIndex(uint aValue);
        void SetSourceIndexByName(string aValue);
        void SetStandby(bool aValue);
    }

    public class WatchableServiceOpenHomeOrgProduct1 : IWatchableServiceOpenHomeOrgProduct1
    {
        public WatchableServiceOpenHomeOrgProduct1(WatchableThread aThread, WatchableDevice aDevice)
        {
            iLock = new object();
            iDisposed = false;

            iDevice = aDevice;
            iServiceProduct = new CpProxyAvOpenhomeOrgProduct1(aDevice.Device);

            iAttributes = new Watchable<string>(aThread, "Attributes", string.Empty);
            iServiceProduct.SetPropertyAttributesChanged(HandleAttributesChanged);

            iProductName = new Watchable<string>(aThread, "ProductName", string.Empty);
            iServiceProduct.SetPropertyProductNameChanged(HandleProductNameChanged);

            iProductRoom = new Watchable<string>(aThread, "ProductRoom", string.Empty);
            iServiceProduct.SetPropertyProductRoomChanged(HandleProductRoomChanged);

            iServiceProduct.Subscribe();
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("WatchableServiceOpenHomeOrgProduct1.Dispose");
                }

                iServiceProduct.Dispose();
                iServiceProduct = null;

                iDevice = null;

                iDisposed = true;
            }
        }

        public IWatchable<string> Attributes
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ManufacturerImageUri
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ManufacturerInfo
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ManufacturerName
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ManufacturerUrl
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ModelImageUri
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ModelInfo
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ModelName
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ModelUrl
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductImageUri
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductInfo
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductName
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductRoom
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductUrl
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceCount
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceIndex
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceName
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceType
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceVisible
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceXml
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceXmlChangeCount
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> Standby
        {
            get { throw new NotImplementedException(); }
        }

        public void GetAttributes(out string aValue)
        {
            throw new NotImplementedException();
        }

        public void GetManufacturer(out string aName, out string aInfo, out string aUrl, out string aImageUri)
        {
            throw new NotImplementedException();
        }

        public void GetModel(out string aName, out string aInfo, out string aUrl, out string aImageUri)
        {
            throw new NotImplementedException();
        }

        public void GetProduct(out string aRoom, out string aName, out string aInfo, out string aUrl, out string aImageUri)
        {
            throw new NotImplementedException();
        }

        public void GetSource(uint aIndex, out string aSystemName, out string aType, out string aName, out bool aVisible)
        {
            throw new NotImplementedException();
        }

        public void GetSourceCount(out uint aValue)
        {
            throw new NotImplementedException();
        }

        public void GetSourceIndex(out uint aValue)
        {
            throw new NotImplementedException();
        }

        public void GetSourceXml(out string aValue)
        {
            throw new NotImplementedException();
        }

        public void GetSourceXmlChangeCount(out uint aValue)
        {
            throw new NotImplementedException();
        }

        public void GetStandby(out bool aValue)
        {
            throw new NotImplementedException();
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

        private void HandleAttributesChanged()
        {
            lock (iLock)
            {
                if (!iDisposed)
                {
                    iAttributes.Update(iServiceProduct.PropertyAttributes());
                }
            }
        }

        private void HandleProductNameChanged()
        {
            lock (iLock)
            {
                if (!iDisposed)
                {
                    iProductName.Update(iServiceProduct.PropertyProductName());
                }
            }
        }

        private void HandleProductRoomChanged()
        {
            lock (iLock)
            {
                if (!iDisposed)
                {
                    iProductRoom.Update(iServiceProduct.PropertyProductRoom());
                }
            }
        }

        private object iLock;
        private bool iDisposed;

        private WatchableDevice iDevice;
        private CpProxyAvOpenhomeOrgProduct1 iServiceProduct;

        private Watchable<string> iAttributes;
        private Watchable<string> iProductName;
        private Watchable<string> iProductRoom;
    }

    public class MockServiceOpenHomeOrgProduct1 : IWatchableServiceOpenHomeOrgProduct1
    {
        public MockServiceOpenHomeOrgProduct1(MockWatchableDevice aDevice)
        {
            iDevice = aDevice;
        }

        public void Dispose()
        {
            iDevice = null;
        }

        public IWatchable<string> Attributes
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ManufacturerImageUri
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ManufacturerInfo
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ManufacturerName
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ManufacturerUrl
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ModelImageUri
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ModelInfo
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ModelName
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ModelUrl
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductImageUri
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductInfo
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductName
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductRoom
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> ProductUrl
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceCount
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceIndex
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceName
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceType
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceVisible
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceXml
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceXmlChangeCount
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> Standby
        {
            get { throw new NotImplementedException(); }
        }

        public void GetAttributes(out string aValue)
        {
            throw new NotImplementedException();
        }

        public void GetManufacturer(out string aName, out string aInfo, out string aUrl, out string aImageUri)
        {
            throw new NotImplementedException();
        }

        public void GetModel(out string aName, out string aInfo, out string aUrl, out string aImageUri)
        {
            throw new NotImplementedException();
        }

        public void GetProduct(out string aRoom, out string aName, out string aInfo, out string aUrl, out string aImageUri)
        {
            throw new NotImplementedException();
        }

        public void GetSource(uint aIndex, out string aSystemName, out string aType, out string aName, out bool aVisible)
        {
            throw new NotImplementedException();
        }

        public void GetSourceCount(out uint aValue)
        {
            throw new NotImplementedException();
        }

        public void GetSourceIndex(out uint aValue)
        {
            throw new NotImplementedException();
        }

        public void GetSourceXml(out string aValue)
        {
            throw new NotImplementedException();
        }

        public void GetSourceXmlChangeCount(out uint aValue)
        {
            throw new NotImplementedException();
        }

        public void GetStandby(out bool aValue)
        {
            throw new NotImplementedException();
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

        private MockWatchableDevice iDevice;
    }
}
