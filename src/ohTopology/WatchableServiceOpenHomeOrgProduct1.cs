using System;
using System.Threading;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Av
{
    public interface IManufacturer
    {
        string ManufacturerImageUri { get; }
        string ManufacturerInfo { get; }
        string ManufacturerName { get; }
        string ManufacturerUrl { get; }
    }

    public class Manufacturer : IManufacturer
    {
        public Manufacturer()
        {
            iManufacturerImageUri = string.Empty;
            iManufacturerInfo = string.Empty;
            iManufacturerName = string.Empty;
            iManufacturerUrl = string.Empty;
        }

        public Manufacturer(string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl)
        {
            iManufacturerImageUri = aManufacturerImageUri;
            iManufacturerInfo = aManufacturerInfo;
            iManufacturerName = aManufacturerName;
            iManufacturerUrl = aManufacturerUrl;
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

        private string iManufacturerImageUri;
        private string iManufacturerInfo;
        private string iManufacturerName;
        private string iManufacturerUrl;
    }

    public interface IModel
    {
        string ModelImageUri { get; }
        string ModelInfo { get; }
        string ModelName { get; }
        string ModelUrl { get; }
    }

    public class Model : IModel
    {
        public Model()
        {
            iModelImageUri = string.Empty;
            iModelInfo = string.Empty;
            iModelName = string.Empty;
            iModelUrl = string.Empty;
        }

        public Model(string aModelImageUri, string aModelInfo, string aModelName, string aModelUrl)
        {
            iModelImageUri = aModelImageUri;
            iModelInfo = aModelInfo;
            iModelName = aModelName;
            iModelUrl = aModelUrl;
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

        private string iModelImageUri;
        private string iModelInfo;
        private string iModelName;
        private string iModelUrl;
    }

    public interface IProduct
    {
        string ProductImageUri { get; }
        string ProductInfo { get; }
        string ProductName { get; }
        string ProductRoom { get; }
        string ProductUrl { get; }
    }

    public class Product : IProduct
    {
        public Product()
        {
            iProductImageUri = string.Empty;
            iProductInfo = string.Empty;
            iProductName = string.Empty;
            iProductRoom = string.Empty;
            iProductUrl = string.Empty;
        }

        public Product(string aProductImageUri, string aProductInfo, string aProductName, string aProductRoom, string aProductUrl)
        {
            iProductImageUri = aProductImageUri;
            iProductInfo = aProductInfo;
            iProductName = aProductName;
            iProductRoom = aProductRoom;
            iProductUrl = aProductUrl;
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

        public string ProductName
        {
            get
            {
                return iProductName;
            }
        }

        public string ProductRoom
        {
            get
            {
                return iProductRoom;
            }
        }

        public string ProductUrl
        {
            get
            {
                return iProductUrl;
            }
        }

        private string iProductImageUri;
        private string iProductInfo;
        private string iProductName;
        private string iProductRoom;
        private string iProductUrl;
    }

    public interface IWatchableServiceOpenHomeOrgProduct1
    {
        IWatchable<string> Attributes { get; }
        IWatchable<IManufacturer> Manufacturer { get; }
        IWatchable<IModel> Model { get; }
        IWatchable<IProduct> Product { get; }
        IWatchable<string> SourceIndex { get; }
        IWatchable<string> SourceXml { get; }
        IWatchable<string> Standby { get; }

        void SetSourceIndex(uint aValue);
        void SetSourceIndexByName(string aValue);
        void SetStandby(bool aValue);
    }

    public class WatchableServiceOpenHomeOrgProduct1 : IWatchableServiceOpenHomeOrgProduct1, IDisposable
    {
        public WatchableServiceOpenHomeOrgProduct1(WatchableThread aThread, WatchableDevice aDevice)
        {
            iLock = new object();
            iDisposed = false;

            iDevice = aDevice;
            iServiceProduct = new CpProxyAvOpenhomeOrgProduct1(aDevice.Device);

            iAttributes = new Watchable<string>(aThread, "Attributes", string.Empty);
            iServiceProduct.SetPropertyAttributesChanged(HandleAttributesChanged);

            iProductChanged = false;
            iProduct = new Watchable<IProduct>(aThread, "Product", new Product());
            iServiceProduct.SetPropertyProductImageUriChanged(HandleProductChanged);
            iServiceProduct.SetPropertyProductInfoChanged(HandleProductChanged);
            iServiceProduct.SetPropertyProductNameChanged(HandleProductChanged);
            iServiceProduct.SetPropertyProductRoomChanged(HandleProductChanged);
            iServiceProduct.SetPropertyProductUrlChanged(HandleProductChanged);

            iServiceProduct.SetPropertyChanged(HandlePropertyChanged);

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

        public IWatchable<IManufacturer> Manufacturer
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<IModel> Model
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<IProduct> Product
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceIndex
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceXml
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> Standby
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

        private void HandleProductChanged()
        {
            lock (iLock)
            {
                if (!iDisposed)
                {
                    iProductChanged = true;
                }
            }
        }

        private void HandlePropertyChanged()
        {
            lock (iLock)
            {
                if (!iDisposed)
                {
                    if (iProductChanged)
                    {
                        iProduct.Update(new Product(iServiceProduct.PropertyProductImageUri(), iServiceProduct.PropertyProductInfo(), iServiceProduct.PropertyProductName(),
                            iServiceProduct.PropertyProductRoom(), iServiceProduct.PropertyProductUrl()));
                        iProductChanged = false;
                    }
                }
            }
        }

        private object iLock;
        private bool iDisposed;

        private WatchableDevice iDevice;
        private CpProxyAvOpenhomeOrgProduct1 iServiceProduct;

        private Watchable<string> iAttributes;
        private bool iProductChanged;
        private Watchable<IProduct> iProduct;
    }

    public class MockServiceOpenHomeOrgProduct1 : IWatchableServiceOpenHomeOrgProduct1, IDisposable
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

        public IWatchable<IManufacturer> Manufacturer
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<IModel> Model
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<IProduct> Product
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceIndex
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> SourceXml
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<string> Standby
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

        private MockWatchableDevice iDevice;
    }
}
