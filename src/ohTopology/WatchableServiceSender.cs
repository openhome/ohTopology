using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Threading;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Net.ControlPoint;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface ISenderMetadata
    {
        string Name { get; }
        string Uri { get; }
        string ArtworkUri { get; }
    }

    public interface IServiceSender
    {
        IWatchable<bool> Audio { get; }
        IWatchable<ISenderMetadata> Metadata { get; }
        IWatchable<string> Status { get; }
    }

    public interface ISender : IServiceSender
    {
        string Attributes { get; }
        string PresentationUrl { get;}
    }

    public class SenderMetadata : ISenderMetadata
    {
        internal SenderMetadata()
        {
            iName = string.Empty;
            iUri = string.Empty;
            iArtworkUri = string.Empty;
        }

        public SenderMetadata(string aMetadata)
        {
            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
            nsManager.AddNamespace("didl", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
            nsManager.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
            nsManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            doc.LoadXml(aMetadata);

            XmlNode name = doc.FirstChild.SelectSingleNode("didl:item/dc:title", nsManager);
            iName = name.FirstChild.Value;
            XmlNode uri = doc.FirstChild.SelectSingleNode("didl:item/didl:res", nsManager);
            iUri = uri.FirstChild.Value;
            XmlNode artworkUri = doc.FirstChild.SelectSingleNode("didl:item/upnp:albumArtURI", nsManager);
            iArtworkUri = artworkUri.FirstChild.Value;
        }

        public string Name
        {
            get { return iName; }
        }

        public string Uri
        {
            get { return iUri; }
        }

        public string ArtworkUri
        {
            get { return iArtworkUri; }
        }

        public override string ToString()
        {
            return base.ToString();
        }

        private string iName;
        private string iUri;
        private string iArtworkUri;
    }

    public abstract class ServiceSender : Service, ISender
    {
        protected ServiceSender(INetwork aNetwork, string aId)
        {
            iAudio = new Watchable<bool>(aNetwork, string.Format("Audio({0})", aId), false);
            iMetadata = new Watchable<ISenderMetadata>(aNetwork, string.Format("Metadata({0})", aId), new SenderMetadata());
            iStatus = new Watchable<string>(aNetwork, string.Format("Status({0})", aId), string.Empty);
        }

        public override void Dispose()
        {
            base.Dispose();

            iAudio.Dispose();
            iAudio = null;

            iMetadata.Dispose();
            iMetadata = null;

            iStatus.Dispose();
            iStatus = null;
        }

        public override IProxy OnCreate(IWatchableDevice aDevice)
        {
            return new ProxySender(this, aDevice);
        }

        public IWatchable<bool> Audio
        {
            get
            {
                return iAudio;
            }
        }

        public IWatchable<ISenderMetadata> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<string> Status
        {
            get
            {
                return iStatus;
            }
        }

        public string Attributes
        {
            get
            {
                return iAttributes;
            }
        }

        public string PresentationUrl
        {
            get
            {
                return iPresentationUrl;
            }
        }

        protected string iAttributes;
        protected string iPresentationUrl;

        protected Watchable<bool> iAudio;
        protected Watchable<ISenderMetadata> iMetadata;
        protected Watchable<string> iStatus;
    }

    public class ServiceSenderNetwork : ServiceSender
    {
        public ServiceSenderNetwork(INetwork aNetwork, string aId, CpDevice aDevice)
            : base(aNetwork, aId)
        {
            iNetwork = aNetwork;
            iSubscribe = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgSender1(aDevice);

            iService.SetPropertyAudioChanged(HandleAudioChanged);
            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyStatusChanged(HandleStatusChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            iSubscribe.Dispose();
            iSubscribe = null;

            iService.Dispose();
            iService = null;

            base.Dispose();
        }

        protected override void OnSubscribe()
        {
            iSubscribe.Reset();
            iService.Subscribe();
            iSubscribe.WaitOne();
        }
        
        private void HandleInitialEvent()
        {
            iAttributes = iService.PropertyAttributes();
            iPresentationUrl = iService.PropertyPresentationUrl();

            iSubscribe.Set();
        }

        protected override void OnUnsubscribe()
        {
            iService.Unsubscribe();
        }

        private void HandleAudioChanged()
        {
            iNetwork.Schedule(() =>
            {
                iAudio.Update(iService.PropertyAudio());
            });
        }

        private void HandleMetadataChanged()
        {
            iNetwork.Schedule(() =>
            {
                iMetadata.Update(new SenderMetadata(iService.PropertyMetadata()));
            });
        }

        private void HandleStatusChanged()
        {
            iNetwork.Schedule(() =>
            {
                iStatus.Update(iService.PropertyStatus());
            });
        }

        private INetwork iNetwork;
        private ManualResetEvent iSubscribe;
        private CpProxyAvOpenhomeOrgSender1 iService;
    }

    public class ServiceSenderMock : ServiceSender, IMockable
    {
        public ServiceSenderMock(INetwork aNetwork, string aId, string aAttributes, string aPresentationUrl, bool aAudio, ISenderMetadata aMetadata, string aStatus)
            : base(aNetwork, aId)
        {
            iNetwork = aNetwork;

            iAttributes = aAttributes;
            iPresentationUrl = aPresentationUrl;

            iAudio.Update(aAudio);
            iMetadata.Update(aMetadata);
            iStatus.Update(aStatus);
        }

        protected override void OnSubscribe()
        {
            iNetwork.WatchableSubscribeThread.Execute(() =>
            {
            });
        }

        protected override void OnUnsubscribe()
        {
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "attributes")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iAttributes = value.First();
            }
            else if (command == "presentationurl")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iPresentationUrl = value.First();
            }
            else if (command == "audio")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iAudio.Update(bool.Parse(value.First()));
            }
            else if (command == "metadata")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iMetadata.Update(new SenderMetadata(value.First()));
            }
            else if (command == "status")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iStatus.Update(value.First());
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private INetwork iNetwork;
    }

    public class ProxySender : ISender, IProxy
    {
        public ProxySender(ServiceSender aService, IWatchableDevice aDevice)
        {
            iService = aService;
            iDevice = aDevice;
        }

        public void Dispose()
        {
            iService.Unsubscribe();
            iService = null;
        }

        public IWatchableDevice Device
        {
            get { return iDevice; }
        }

        public string Attributes
        {
            get { return iService.Attributes; }
        }

        public string PresentationUrl
        {
            get { return iService.PresentationUrl; }
        }

        public IWatchable<bool> Audio
        {
            get { return iService.Audio; }
        }

        public IWatchable<ISenderMetadata> Metadata
        {
            get { return iService.Metadata; }
        }

        public IWatchable<string> Status
        {
            get { return iService.Status; }
        }

        private ServiceSender iService;
        private IWatchableDevice iDevice;
    }
}
