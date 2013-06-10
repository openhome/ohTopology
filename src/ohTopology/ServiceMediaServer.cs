using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using OpenHome.Os.App;
using OpenHome.MediaServer;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public interface IMediaServerSession : IDisposable
    {
        Task<IWatchableContainer<IMediaDatum>> Browse(IMediaDatum aDatum); // null = home
        Task<IWatchableContainer<IMediaDatum>> Link(ITag aTag, string aValue);
        Task<IWatchableContainer<IMediaDatum>> Search(string aValue);
        Task<IWatchableContainer<IMediaDatum>> Query(string aValue);
    }

    public interface IProxyMediaServer : IProxy
    {
        IEnumerable<string> Attributes { get; }
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
        string ProductName { get; }
        string ProductUrl { get; }
        Task<IMediaServerSession> CreateSession();
    }

    internal class ProxyMediaServer : Proxy<ServiceMediaServer>, IProxyMediaServer
    {
        public ProxyMediaServer(IDevice aDevice, ServiceMediaServer aService)
            : base(aDevice, aService)
        {
        }

        // IProxyMediaServer

        public IEnumerable<string> Attributes
        {
            get { return (iService.Attributes); }
        }

        public string ManufacturerImageUri
        {
            get { return (iService.ManufacturerImageUri); }
        }

        public string ManufacturerInfo
        {
            get { return (iService.ManufacturerInfo); }
        }

        public string ManufacturerName
        {
            get { return (iService.ManufacturerName); }
        }

        public string ManufacturerUrl
        {
            get { return (iService.ManufacturerUrl); }
        }

        public string ModelImageUri
        {
            get { return (iService.ModelImageUri); }
        }

        public string ModelInfo
        {
            get { return (iService.ModelInfo); }
        }

        public string ModelName
        {
            get { return (iService.ModelName); }
        }

        public string ModelUrl
        {
            get { return (iService.ModelUrl); }
        }

        public string ProductImageUri
        {
            get { return (iService.ProductImageUri); }
        }

        public string ProductInfo
        {
            get { return (iService.ProductInfo); }
        }

        public string ProductName
        {
            get { return (iService.ProductName); }
        }

        public string ProductUrl
        {
            get { return (iService.ProductUrl); }
        }

        public Task<IMediaServerSession> CreateSession()
        {
            return (iService.CreateSession());
        }
    }

    public abstract class ServiceMediaServer : Service
    {
        private readonly IEnumerable<string> iAttributes;
        private readonly string iManufacturerImageUri;
        private readonly string iManufacturerInfo;
        private readonly string iManufacturerName;
        private readonly string iManufacturerUrl;
        private readonly string iModelImageUri;
        private readonly string iModelInfo;
        private readonly string iModelName;
        private readonly string iModelUrl;
        private readonly string iProductImageUri;
        private readonly string iProductInfo;
        private readonly string iProductName;
        private readonly string iProductUrl;

        protected ServiceMediaServer(INetwork aNetwork, IEnumerable<string> aAttributes, 
            string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl,
            string aModelImageUri, string aModelInfo, string aModelName, string aModelUrl,
            string aProductImageUri, string aProductInfo, string aProductName, string aProductUrl)
            : base (aNetwork)
        {
            iAttributes = aAttributes;
            iManufacturerImageUri = aManufacturerImageUri;
            iManufacturerInfo = aManufacturerInfo;
            iManufacturerName = aManufacturerName;
            iManufacturerUrl = aManufacturerUrl;
            iModelImageUri = aModelImageUri;
            iModelInfo = aModelInfo;
            iModelName = aModelName;
            iModelUrl = aModelUrl;
            iProductImageUri = aProductImageUri;
            iProductInfo = aProductInfo;
            iProductName = aProductName;
            iProductUrl = aProductUrl;
        }

        // IProxyMediaServer

        public IEnumerable<string> Attributes
        {
            get { return (iAttributes); }
        }

        public string ManufacturerImageUri
        {
            get { return (iManufacturerImageUri); }
        }

        public string ManufacturerInfo
        {
            get { return (iManufacturerInfo); }
        }

        public string ManufacturerName
        {
            get { return (iManufacturerName); }
        }

        public string ManufacturerUrl
        {
            get { return (iManufacturerUrl); }
        }

        public string ModelImageUri
        {
            get { return (iModelImageUri); }
        }

        public string ModelInfo
        {
            get { return (iModelInfo); }
        }

        public string ModelName
        {
            get { return (iModelName); }
        }

        public string ModelUrl
        {
            get { return (iModelUrl); }
        }

        public string ProductImageUri
        {
            get { return (iProductImageUri); }
        }

        public string ProductInfo
        {
            get { return (iProductInfo); }
        }

        public string ProductName
        {
            get { return (iProductName); }
        }

        public string ProductUrl
        {
            get { return (iProductUrl); }
        }

        public abstract Task<IMediaServerSession> CreateSession();
    }

    public static class ServiceMediaServerExtensions
    {
        public static bool SupportsBrowse(this IProxyMediaServer aProxy)
        {
            return (aProxy.Attributes.Contains("Browse"));
        }

        public static bool SupportsLink(this IProxyMediaServer aProxy)
        {
            return (aProxy.Attributes.Contains("Link"));
        }

        public static bool SupportsLink(this IProxyMediaServer aProxy, ITag aTag)
        {
            var value = string.Format("Link:{0}", aTag.FullName);
            return (aProxy.Attributes.Contains(value));
        }

        public static bool SupportsSearch(this IProxyMediaServer aProxy)
        {
            return (aProxy.Attributes.Contains("Search"));
        }

        public static bool SupportsQuery(this IProxyMediaServer aProxy)
        {
            return (aProxy.Attributes.Contains("Query"));
        }
    }
}
