using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
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

    public interface IServiceOpenHomeOrgInfo1
    {
        IWatchable<IInfoDetails> Details { get; }
        IWatchable<IInfoMetadata> Metadata { get; }
        IWatchable<IInfoMetatext> Metatext { get; }
    }

    public class InfoDetails : IInfoDetails
    {
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

    public abstract class Info : IServiceOpenHomeOrgInfo1, IWatchableService
    {
        public abstract void Dispose();

        public IService Create(IManagableWatchableDevice aDevice)
        {
            return new ServiceInfo(aDevice, this);
        }

        internal abstract IServiceOpenHomeOrgInfo1 Service { get; }

        public IWatchable<IInfoDetails> Details
        {
            get
            {
                return Service.Details;
            }
        }

        public IWatchable<IInfoMetadata> Metadata
        {
            get
            {
                return Service.Metadata;
            }
        }

        public IWatchable<IInfoMetatext> Metatext
        {
            get
            {
                return Service.Metatext;
            }
        }
    }

    public class ServiceOpenHomeOrgInfo1 : IServiceOpenHomeOrgInfo1
    {
        public ServiceOpenHomeOrgInfo1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgInfo1 aService)
        {
            iThread = aThread;

            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyBitDepthChanged(HandleDetailsChanged);
                iService.SetPropertyMetadataChanged(HandleMetadataChanged);
                iService.SetPropertyMetatextChanged(HandleMetatextChanged);

                IInfoDetails details = new InfoDetails(iService.PropertyBitDepth(), iService.PropertyBitRate(), iService.PropertyCodecName(),
                    iService.PropertyDuration(), iService.PropertyLossless(), iService.PropertySampleRate());
                IInfoMetadata metadata = new InfoMetadata(iService.PropertyMetadata(), iService.PropertyUri());
                IInfoMetatext metatext = new InfoMetatext(iService.PropertyMetatext());

                iDetails = new Watchable<IInfoDetails>(aThread, string.Format("Details({0})", aId), details);
                iMetadata = new Watchable<IInfoMetadata>(aThread, string.Format("Metadata({0})", aId), metadata);
                iMetatext = new Watchable<IInfoMetatext>(aThread, string.Format("Metatext({0})", aId), metatext);
            }
        }
        
        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgInfo1.Dispose");
                }

                iDetails.Dispose();
                iDetails = null;

                iMetadata.Dispose();
                iMetadata = null;

                iMetatext.Dispose();
                iMetatext = null;

                iService.Dispose();
                iService = null;

                iDisposed = true;
            }
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

        private void HandleDetailsChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

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
        }

        private void HandleMetadataChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iThread.Schedule(() =>
                {
                    iMetadata.Update(
                        new InfoMetadata(
                            iService.PropertyMetadata(),
                            iService.PropertyUri()
                        ));
                });
            }
        }

        private void HandleMetatextChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iThread.Schedule(() =>
                {
                    iMetatext.Update(new InfoMetatext(iService.PropertyMetatext()));
                });
            }
        }

        private object iLock;
        private bool iDisposed;

        private IWatchableThread iThread;
        private CpProxyAvOpenhomeOrgInfo1 iService;

        private Watchable<IInfoDetails> iDetails;
        private Watchable<IInfoMetadata> iMetadata;
        private Watchable<IInfoMetatext> iMetatext;
    }

    public class MockServiceOpenHomeOrgInfo1 : IServiceOpenHomeOrgInfo1, IMockable
    {
        public MockServiceOpenHomeOrgInfo1(IWatchableThread aThread, string aId, IInfoDetails aDetails, IInfoMetadata aMetadata, IInfoMetatext aMetatext)
        {
            iDetails = new Watchable<IInfoDetails>(aThread, string.Format("Details({0})", aId), aDetails);
            iMetadata = new Watchable<IInfoMetadata>(aThread, string.Format("Metadata({0})", aId), aMetadata);
            iMetatext = new Watchable<IInfoMetatext>(aThread, string.Format("Metatext({0})", aId), aMetatext);
        }

        public void Dispose()
        {
            iDetails.Dispose();
            iDetails = null;

            iMetadata.Dispose();
            iMetadata = null;

            iMetatext.Dispose();
            iMetatext = null;
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

        public void Execute(IEnumerable<string> aValue)
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

        private Watchable<IInfoDetails> iDetails;
        private Watchable<IInfoMetadata> iMetadata;
        private Watchable<IInfoMetatext> iMetatext;
    }

    public class WatchableInfoFactory : IWatchableServiceFactory
    {
        public WatchableInfoFactory(IWatchableThread aThread, IWatchableThread aSubscribeThread)
        {
            iLock = new object();
            iDisposed = false;
            iPendingSubscribes = new List<Action<IWatchableService>>();

            iThread = aThread;
            iSubscribeThread = aSubscribeThread;
        }

        public void Dispose()
        {
            iSubscribeThread.Execute(() =>
            {
                Unsubscribe();
                iDisposed = true;
            });
        }

        public void Subscribe(IWatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            iSubscribeThread.Schedule(() =>
            {
                lock (iLock)
                {
                    if (!iDisposed)
                    {
                        if (iPendingService == null)
                        {
                            WatchableDevice d = aDevice as WatchableDevice;
                            iPendingService = new CpProxyAvOpenhomeOrgInfo1(d.Device);
                            iPendingService.SetPropertyInitialEvent(delegate
                            {
                                lock (iLock)
                                {
                                    if (iPendingService != null)
                                    {
                                        iService = new WatchableInfo(iThread, string.Format("Info({0})", aDevice.Udn), iPendingService);
                                        iPendingService = null;
                                        aCallback(iService);
                                        foreach (Action<IWatchableService> c in iPendingSubscribes)
                                        {
                                            c(iService);
                                        }
                                        iPendingSubscribes.Clear();
                                    }
                                }
                            });
                            iPendingService.Subscribe();
                        }
                        else
                        {
                            iPendingSubscribes.Add(aCallback);
                        }
                    }
                }
            });
        }

        public void Unsubscribe()
        {
            iSubscribeThread.Schedule(() =>
            {
                lock (iLock)
                {
                    if (iPendingService != null)
                    {
                        iPendingService.Dispose();
                        iPendingService = null;
                    }
                }
            });
        }

        private object iLock;
        private bool iDisposed;
        private IWatchableThread iSubscribeThread;
        private CpProxyAvOpenhomeOrgInfo1 iPendingService;
        private WatchableInfo iService;
        private IWatchableThread iThread;
        private List<Action<IWatchableService>> iPendingSubscribes;
    }

    public class WatchableInfo : Info
    {
        public WatchableInfo(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgInfo1 aService)
        {
            iService = new ServiceOpenHomeOrgInfo1(aThread, aId, aService);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgInfo1 Service
        {
            get
            {
                return iService;
            }
        }

        private ServiceOpenHomeOrgInfo1 iService;
    }

    public class MockWatchableInfo : Info, IMockable
    {
        public MockWatchableInfo(IWatchableThread aThread, string aId, IInfoDetails aDetails, IInfoMetadata aMetadata, IInfoMetatext aMetatext)
        {
            iService = new MockServiceOpenHomeOrgInfo1(aThread, aId, aDetails, aMetadata, aMetatext);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgInfo1 Service
        {
            get
            {
                return iService;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            iService.Execute(aValue);
        }

        private MockServiceOpenHomeOrgInfo1 iService;
    }

    public class ServiceInfo : IServiceOpenHomeOrgInfo1, IService
    {
        public ServiceInfo(IManagableWatchableDevice aDevice, Info aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iDevice.Unsubscribe<ServiceInfo>();
            iDevice = null;
        }

        public IWatchableDevice Device
        {
            get { return iDevice; }
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

        private IManagableWatchableDevice iDevice;
        private Info iService;
    }
}
