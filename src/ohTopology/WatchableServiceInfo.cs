using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Net.ControlPoint;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IInfoDetails
    {
        uint BitDepth { get; }
        uint BitRate { get; }
        string CodecName { get; }
        uint Duration { get; }
        bool Lossless { get; }
        uint SampleRate { get; }
    }

    public interface IInfoMetadata
    {
        string Metadata { get; }
        string Uri { get; }
    }

    public interface IInfoMetatext
    {
        string Metatext { get; }
    }

    public interface IProxyInfo : IProxy
    {
        IWatchable<IInfoDetails> Details { get; }
        IWatchable<IInfoMetadata> Metadata { get; }
        IWatchable<IInfoMetatext> Metatext { get; }
    }

    public class InfoDetails : IInfoDetails
    {
        internal InfoDetails()
        {
            iBitDepth = 0;
            iBitRate = 0;
            iCodecName = string.Empty;
            iDuration = 0;
            iLossless = false;
            iSampleRate = 0;
        }

        public InfoDetails(uint aBitDepth, uint aBitRate, string aCodecName, uint aDuration, bool aLossless, uint aSampleRate)
        {
            iBitDepth = aBitDepth;
            iBitRate = aBitRate;
            iCodecName = aCodecName;
            iDuration = aDuration;
            iLossless = aLossless;
            iSampleRate = aSampleRate;
        }

        public uint BitDepth
        {
            get
            {
                return iBitDepth;
            }
        }

        public uint BitRate
        {
            get
            {
                return iBitRate;
            }
        }

        public string CodecName
        {
            get
            {
                return iCodecName;
            }
        }

        public uint Duration
        {
            get
            {
                return iDuration;
            }
        }

        public bool Lossless
        {
            get
            {
                return iLossless;
            }
        }

        public uint SampleRate
        {
            get
            {
                return iSampleRate;
            }
        }

        private uint iBitDepth;
        private uint iBitRate;
        private string iCodecName;
        private uint iDuration;
        private bool iLossless;
        private uint iSampleRate;
    }

    public class InfoMetadata : IInfoMetadata
    {
        internal InfoMetadata()
        {
            iMetadata = string.Empty;
            iUri = string.Empty;
        }

        public InfoMetadata(string aMetadata, string aUri)
        {
            iMetadata = aMetadata;
            iUri = aUri;
        }

        public string Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public string Uri
        {
            get
            {
                return iUri;
            }
        }

        private string iMetadata;
        private string iUri;
    }

    public class InfoMetatext : IInfoMetatext
    {
        internal InfoMetatext()
        {
            iMetatext = string.Empty;
        }

        public InfoMetatext(string aMetatext)
        {
            iMetatext = aMetatext;
        }

        public string Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        private string iMetatext;
    }

    public abstract class ServiceInfo : Service
    {
        protected ServiceInfo(INetwork aNetwork)
            : base(aNetwork)
        {
            iDetails = new Watchable<IInfoDetails>(Network, "Details", new InfoDetails());
            iMetadata = new Watchable<IInfoMetadata>(Network, "Metadata", new InfoMetadata());
            iMetatext = new Watchable<IInfoMetatext>(Network, "Metatext", new InfoMetatext());
        }

        public override void Dispose()
        {
            base.Dispose();

            iDetails.Dispose();
            iDetails = null;

            iMetadata.Dispose();
            iMetadata = null;

            iMetatext.Dispose();
            iMetatext = null;
        }

        public override IProxy OnCreate(IWatchableDevice aDevice)
        {
            return new ProxyInfo(aDevice, this);
        }

        public IWatchable<IInfoDetails> Details
        {
            get
            {
                return iDetails;
            }
        }

        public IWatchable<IInfoMetadata> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<IInfoMetatext> Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        protected Watchable<IInfoDetails> iDetails;
        protected Watchable<IInfoMetadata> iMetadata;
        protected Watchable<IInfoMetatext> iMetatext;
    }

    public class ServiceInfoNetwork : ServiceInfo
    {
        public ServiceInfoNetwork(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork)
        {
            iThread = aNetwork;
            iSubscribe = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgInfo1(aDevice);

            iService.SetPropertyBitDepthChanged(HandleDetailsChanged);
            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyMetatextChanged(HandleMetatextChanged);

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
            iService.Subscribe();
            iSubscribe.WaitOne();
        }

        private void HandleInitialEvent()
        {
            iSubscribe.Set();
        }

        protected override void OnUnsubscribe()
        {
            iService.Unsubscribe();
            iSubscribe.Reset();
        }

        private void HandleDetailsChanged()
        {
            iThread.Schedule(() =>
            {
                iDetails.Update(
                    new InfoDetails(
                        iService.PropertyBitDepth(),
                        iService.PropertyBitRate(),
                        iService.PropertyCodecName(),
                        iService.PropertyDuration(),
                        iService.PropertyLossless(),
                        iService.PropertySampleRate()
                    ));
            });
        }

        private void HandleMetadataChanged()
        {
            iThread.Schedule(() =>
            {
                iMetadata.Update(
                    new InfoMetadata(
                        iService.PropertyMetadata(),
                        iService.PropertyUri()
                    ));
            });
        }

        private void HandleMetatextChanged()
        {
            iThread.Schedule(() =>
            {
                iMetatext.Update(new InfoMetatext(iService.PropertyMetatext()));
            });
        }

        private IWatchableThread iThread;
        private ManualResetEvent iSubscribe;
        private CpProxyAvOpenhomeOrgInfo1 iService;
    }

    public class ServiceInfoMock : ServiceInfo, IMockable
    {
        public ServiceInfoMock(INetwork aNetwork, IInfoDetails aDetails, IInfoMetadata aMetadata, IInfoMetatext aMetatext)
            : base(aNetwork)
        {
            iDetails.Update(aDetails);
            iMetadata.Update(aMetadata);
            iMetatext.Update(aMetatext);
        }

        protected override void OnSubscribe()
        {
            Network.SubscribeThread.Execute(() =>
            {
            });
        }

        protected override void OnUnsubscribe()
        {
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "details")
            {
                IEnumerable<string> value = aValue.Skip(1);
                if (value.Count() != 6)
                {
                    throw new NotSupportedException();
                }
                IInfoDetails details = new InfoDetails(
                    uint.Parse(value.ElementAt(0)),
                    uint.Parse(value.ElementAt(1)),
                    value.ElementAt(2),
                    uint.Parse(value.ElementAt(3)),
                    bool.Parse(value.ElementAt(4)),
                    uint.Parse(value.ElementAt(5)));
                iDetails.Update(details);
            }
            else if (command == "metadata")
            {
                IEnumerable<string> value = aValue.Skip(1);
                if (value.Count() != 2)
                {
                    throw new NotSupportedException();
                }
                IInfoMetadata metadata = new InfoMetadata(value.ElementAt(0), value.ElementAt(1));
                iMetadata.Update(metadata);
            }
            else if (command == "metatext")
            {
                IEnumerable<string> value = aValue.Skip(1);
                if (value.Count() != 1)
                {
                    throw new NotSupportedException();
                }
                IInfoMetatext metatext = new InfoMetatext(value.ElementAt(0));
                iMetatext.Update(metatext);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public class ProxyInfo : Proxy<ServiceInfo>, IProxyInfo
    {
        public ProxyInfo(IWatchableDevice aDevice, ServiceInfo aService)
            : base(aDevice, aService)
        {
        }

        public IWatchable<IInfoDetails> Details
        {
            get { return iService.Details; }
        }

        public IWatchable<IInfoMetadata> Metadata
        {
            get { return iService.Metadata; }
        }

        public IWatchable<IInfoMetatext> Metatext
        {
            get { return iService.Metatext; }
        }
    }
}
