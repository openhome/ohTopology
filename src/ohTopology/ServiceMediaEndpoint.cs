using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public interface IMediaEndpointSession : IDisposable
    {
        IWatchableSnapshot<IMediaDatum> Snapshot { get; }
        void Browse(IMediaDatum aDatum, Action aAction); // null = home
        void List(ITag aTag, Action aAction);
        void Link(ITag aTag, string aValue, Action aAction);
        void Match(ITag aTag, string aValue, Action aAction);
        void Search(string aValue, Action aAction);
    }

    public interface IProxyMediaEndpoint : IProxy
    {
        string Id { get; }
        string Type { get; }
        string Name { get; }
        string Info { get; }
        string Url { get; }
        string Artwork { get; }
        string ManufacturerName { get; }
        string ManufacturerInfo { get; }
        string ManufacturerUrl { get; }
        string ManufacturerArtwork { get; }
        string ModelName { get; }
        string ModelInfo { get; }
        string ModelUrl { get; }
        string ModelArtwork { get; }
        DateTime Started { get; }
        IEnumerable<string> Attributes { get; }
        Task<IMediaEndpointSession> CreateSession();
    }

    internal class ProxyMediaEndpoint : Proxy<ServiceMediaEndpoint>, IProxyMediaEndpoint
    {
        public ProxyMediaEndpoint(ServiceMediaEndpoint aService)
            : base(aService)
        {
        }

        // IProxyMediaEndpoint

        public string Id
        {
            get { return (iService.Id); }
        }

        public string Type
        {
            get { return (iService.Type); }
        }

        public string Name
        {
            get { return (iService.Name); }
        }

        public string Info
        {
            get { return (iService.Info); }
        }

        public string Url
        {
            get { return (iService.Url); }
        }

        public string Artwork
        {
            get { return (iService.Artwork); }
        }

        public string ManufacturerName
        {
            get { return (iService.ManufacturerName); }
        }

        public string ManufacturerInfo
        {
            get { return (iService.ManufacturerInfo); }
        }

        public string ManufacturerUrl
        {
            get { return (iService.ManufacturerUrl); }
        }

        public string ManufacturerArtwork
        {
            get { return (iService.ManufacturerArtwork); }
        }

        public string ModelName
        {
            get { return (iService.ModelName); }
        }

        public string ModelInfo
        {
            get { return (iService.ModelInfo); }
        }

        public string ModelUrl
        {
            get { return (iService.ModelUrl); }
        }

        public string ModelArtwork
        {
            get { return (iService.ModelArtwork); }
        }

        public DateTime Started
        {
            get { return (iService.Started); }
        }

        public IEnumerable<string> Attributes
        {
            get { return (iService.Attributes); }
        }

        public Task<IMediaEndpointSession> CreateSession()
        {
            return (iService.CreateSession());
        }
    }

    public abstract class ServiceMediaEndpoint : Service
    {
        protected readonly string iId;
        protected readonly string iType;
        protected readonly string iName;
        protected readonly string iInfo;
        protected readonly string iUrl;
        protected readonly string iArtwork;
        protected readonly string iManufacturerName;
        protected readonly string iManufacturerInfo;
        protected readonly string iManufacturerUrl;
        protected readonly string iManufacturerArtwork;
        protected readonly string iModelName;
        protected readonly string iModelInfo;
        protected readonly string iModelUrl;
        protected readonly string iModelArtwork;
        protected readonly DateTime iStarted;
        protected readonly IEnumerable<string> iAttributes;

        protected ServiceMediaEndpoint(INetwork aNetwork, IDevice aDevice, string aId, string aType, string aName, string aInfo,
            string aUrl, string aArtwork, string aManufacturerName, string aManufacturerInfo, string aManufacturerUrl,
            string aManufacturerArtwork, string aModelName, string aModelInfo, string aModelUrl, string aModelArtwork,
            DateTime aStarted, IEnumerable<string> aAttributes)
            : base (aNetwork, aDevice)
        {
            iId = aId;
            iType = aType;
            iName = aName;
            iInfo = aInfo;
            iUrl = aUrl;
            iArtwork = aArtwork;
            iManufacturerName = aManufacturerName;
            iManufacturerInfo = aManufacturerInfo;
            iManufacturerUrl = aManufacturerUrl;
            iManufacturerArtwork = aManufacturerArtwork;
            iModelName = aModelName;
            iModelInfo = aModelInfo;
            iModelUrl = aModelUrl;
            iModelArtwork = aModelArtwork;
            iStarted = aStarted;
            iAttributes = aAttributes;
        }

        // IProxyMediaEndpoint

        public string Id
        {
            get { return (iId); }
        }

        public string Type
        {
            get { return (iType); }
        }

        public string Name
        {
            get { return (iName); }
        }

        public string Info
        {
            get { return (iInfo); }
        }

        public string Url
        {
            get { return (iUrl); }
        }

        public string Artwork
        {
            get { return (iArtwork); }
        }

        public string ManufacturerName
        {
            get { return (iManufacturerName); }
        }

        public string ManufacturerInfo
        {
            get { return (iManufacturerInfo); }
        }

        public string ManufacturerUrl
        {
            get { return (iManufacturerUrl); }
        }

        public string ManufacturerArtwork
        {
            get { return (iManufacturerArtwork); }
        }

        public string ModelName
        {
            get { return (iModelName); }
        }

        public string ModelInfo
        {
            get { return (iModelInfo); }
        }

        public string ModelUrl
        {
            get { return (iModelUrl); }
        }

        public string ModelArtwork
        {
            get { return (iModelArtwork); }
        }

        public DateTime Started
        {
            get { return (iStarted); }
        }

        public IEnumerable<string> Attributes
        {
            get { return (iAttributes); }
        }

        public abstract Task<IMediaEndpointSession> CreateSession();
    }

    public static class ServiceMediaEndpointExtensions
    {
        public static bool SupportsBrowse(this IProxyMediaEndpoint aProxy)
        {
            return (aProxy.Attributes.Contains("Browse"));
        }

        public static bool SupportsList(this IProxyMediaEndpoint aProxy)
        {
            return (aProxy.Attributes.Contains("List"));
        }

        public static bool SupportsLink(this IProxyMediaEndpoint aProxy)
        {
            return (aProxy.Attributes.Contains("Link"));
        }

        public static bool SupportsLink(this IProxyMediaEndpoint aProxy, ITag aTag)
        {
            if (aProxy.Attributes.Contains("Link"))
            {
                switch (aTag.FullName)
                {
                    case "audio.artist":
                    case "audio.album":
                    case "audio.genre":
                    case "audio.year":
                        return (true);
                    default:
                        break;
                }
            }

            return (false);
        }

        public static bool SupportsSearch(this IProxyMediaEndpoint aProxy)
        {
            return (aProxy.Attributes.Contains("Search"));
        }
    }
}
