using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgSender1
    {
        IWatchable<bool> Audio { get; }
        IWatchable<string> Metadata { get; }
        IWatchable<string> Status { get; }
    }

    public interface ISender : IServiceOpenHomeOrgSender1
    {
        string Attributes { get; }
        string PresentationUrl { get;}
    }

    public abstract class Sender : ISender, IWatchableService
    {
        public abstract void Dispose();

        public IService Create(IManagableWatchableDevice aDevice)
        {
            return new ServiceSender(aDevice, this);
        }

        internal abstract IServiceOpenHomeOrgSender1 Service { get; }

        public IWatchable<bool> Audio
        {
            get
            {
                return Service.Audio;
            }
        }

        public IWatchable<string> Metadata
        {
            get
            {
                return Service.Metadata;
            }
        }

        public IWatchable<string> Status
        {
            get
            {
                return Service.Status;
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
    }

    public class ServiceOpenHomeOrgSender1 : IServiceOpenHomeOrgSender1, IDisposable
    {
        public ServiceOpenHomeOrgSender1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgSender1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyAudioChanged(HandleAudioChanged);
                iService.SetPropertyMetadataChanged(HandleMetadataChanged);
                iService.SetPropertyStatusChanged(HandleStatusChanged);

                iAudio = new Watchable<bool>(aThread, string.Format("Audio({0})", aId), iService.PropertyAudio());
                iMetadata = new Watchable<string>(aThread, string.Format("Metadata({0})", aId), iService.PropertyMetadata());
                iStatus = new Watchable<string>(aThread, string.Format("Status({0})", aId), iService.PropertyStatus());
            }
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgSender1.Dispose");
                }

                iService.Dispose();
                iService = null;

                iAudio.Dispose();
                iAudio = null;

                iMetadata.Dispose();
                iMetadata = null;

                iStatus.Dispose();
                iStatus = null;

                iDisposed = true;
            }
        }

        public IWatchable<bool> Audio
        {
            get
            {
                return iAudio;
            }
        }

        public IWatchable<string> Metadata
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

        private void HandleAudioChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iAudio.Update(iService.PropertyAudio());
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

                iMetadata.Update(iService.PropertyMetadata());
            }
        }

        private void HandleStatusChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iStatus.Update(iService.PropertyStatus());
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgSender1 iService;

        private Watchable<bool> iAudio;
        private Watchable<string> iMetadata;
        private Watchable<string> iStatus;
    }

    public class MockServiceOpenHomeOrgSender1 : IServiceOpenHomeOrgSender1, IMockable, IDisposable
    {
        public MockServiceOpenHomeOrgSender1(IWatchableThread aThread, string aId, bool aAudio, string aMetadata, string aStatus)
        {
            iAudio = new Watchable<bool>(aThread, string.Format("Audio({0})", aId), aAudio);
            iMetadata = new Watchable<string>(aThread, string.Format("Metadata({0})", aId), aMetadata);
            iStatus = new Watchable<string>(aThread, string.Format("Status({0})", aId), aStatus);
        }

        public void Dispose()
        {
            iAudio.Dispose();
            iAudio = null;

            iMetadata.Dispose();
            iMetadata = null;

            iStatus.Dispose();
            iStatus = null;
        }

        public IWatchable<bool> Audio
        {
            get
            {
                return iAudio;
            }
        }

        public IWatchable<string> Metadata
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

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "audio")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iAudio.Update(bool.Parse(value.First()));
            }
            else if (command == "metadata")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iMetadata.Update(value.First());
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

        private Watchable<bool> iAudio;
        private Watchable<string> iMetadata;
        private Watchable<string> iStatus;
    }

    public class WatchableSenderFactory : IWatchableServiceFactory
    {
        public WatchableSenderFactory(IWatchableThread aThread, IWatchableThread aSubscribeThread)
        {
            iLock = new object();
            iDisposed = false;

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
                if (!iDisposed && iService == null && iPendingService == null)
                {
                    WatchableDevice d = aDevice as WatchableDevice;
                    iPendingService = new CpProxyAvOpenhomeOrgSender1(d.Device);
                    iPendingService.SetPropertyInitialEvent(delegate
                    {
                        lock (iLock)
                        {
                            if (iPendingService != null)
                            {
                                iService = new WatchableSender(iThread, string.Format("Sender({0})", aDevice.Udn), iPendingService);
                                iPendingService = null;
                                aCallback(iService);
                            }
                        }
                    });
                    iPendingService.Subscribe();
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

                    if (iService != null)
                    {
                        iService.Dispose();
                        iService = null;
                    }
                }
            });
        }

        private object iLock;
        private bool iDisposed;
        private IWatchableThread iSubscribeThread;
        private CpProxyAvOpenhomeOrgSender1 iPendingService;
        private WatchableSender iService;
        private IWatchableThread iThread;
    }

    public class WatchableSender : Sender
    {
        public WatchableSender(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgSender1 aService)
        {
            iAttributes = aService.PropertyAttributes();
            iPresentationUrl = aService.PropertyPresentationUrl();

            iService = new ServiceOpenHomeOrgSender1(aThread, aId, aService);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgSender1 Service
        {
            get
            {
                return iService;
            }
        }

        private ServiceOpenHomeOrgSender1 iService;
    }
        
    public class MockWatchableSender : Sender, IMockable
    {
        public MockWatchableSender(IWatchableThread aThread, string aId, string aAttributes, bool aAudio, string aMetadata, string aPresentationUrl, string aStatus)
        {
            iAttributes = aAttributes;
            iPresentationUrl = aPresentationUrl;

            iService = new MockServiceOpenHomeOrgSender1(aThread, aId, aAudio, aMetadata, aStatus);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgSender1 Service
        {
            get
            {
                return iService;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "audio" || command == "metadata" || command == "status")
            {
                iService.Execute(aValue);
            }
            else if (command == "attributes")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iAttributes = value.First();
            }
            else if (command == "presentationurl")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iPresentationUrl = value.First();
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private MockServiceOpenHomeOrgSender1 iService;
    }

    public class ServiceSender : ISender, IService
    {
        public ServiceSender(IManagableWatchableDevice aDevice, ISender aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iDevice.Unsubscribe<ServiceSender>();
            iDevice = null;
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

        public IWatchable<string> Metadata
        {
            get { return iService.Metadata; }
        }

        public IWatchable<string> Status
        {
            get { return iService.Status; }
        }

        private IManagableWatchableDevice iDevice;
        private ISender iService;
    }
}
