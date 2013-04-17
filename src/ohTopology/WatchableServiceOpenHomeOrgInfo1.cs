using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgInfo1
    {
        IWatchable<uint> BitDepth { get; }
        IWatchable<uint> BitRate { get; }
        IWatchable<string> CodecName { get; }
        IWatchable<uint> Duration { get; }
        IWatchable<bool> Lossless { get; }
        IWatchable<string> Metadata { get; }
        IWatchable<string> Metatext { get; }
        IWatchable<uint> SampleRate { get; }
        IWatchable<string> Uri { get; }
    }

    public abstract class Info : IWatchableService, IServiceOpenHomeOrgInfo1, IDisposable
    {
        protected Info(string aId, IServiceOpenHomeOrgInfo1 aService)
        {
            iId = aId;
            iService = aService;
        }

        public abstract void Dispose();

        public string Id
        {
            get
            {
                return iId;
            }
        }

        public string Type
        {
            get
            {
                return "AvOpenHomeInfo1";
            }
        }

        public IWatchable<uint> BitDepth
        {
            get
            {
                return iService.BitDepth;
            }
        }

        public IWatchable<uint> BitRate
        {
            get
            {
                return iService.BitRate;
            }
        }

        public IWatchable<string> CodecName
        {
            get
            {
                return iService.CodecName;
            }
        }

        public IWatchable<uint> Duration
        {
            get
            {
                return iService.Duration;
            }
        }

        public IWatchable<bool> Lossless
        {
            get
            {
                return iService.Lossless;
            }
        }

        public IWatchable<string> Metadata
        {
            get
            {
                return iService.Metadata;
            }
        }

        public IWatchable<string> Metatext
        {
            get
            {
                return iService.Metatext;
            }
        }

        public IWatchable<uint> SampleRate
        {
            get
            {
                return iService.SampleRate;
            }
        }

        public IWatchable<string> Uri
        {
            get
            {
                return iService.Uri;
            }
        }

        private string iId;
        protected IServiceOpenHomeOrgInfo1 iService;
    }

    public class ServiceOpenHomeOrgInfo1 : IServiceOpenHomeOrgInfo1, IDisposable
    {
        public ServiceOpenHomeOrgInfo1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgInfo1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyBitDepthChanged(HandleBitDepthChanged);
                iService.SetPropertyBitRateChanged(HandleBitRateChanged);
                iService.SetPropertyCodecNameChanged(HandleBitCodecNameChanged);
                iService.SetPropertyDurationChanged(HandleDurationChanged);
                iService.SetPropertyLosslessChanged(HandleLosslessChanged);
                iService.SetPropertyMetadataChanged(HandleMetadataChanged);
                iService.SetPropertyMetatextChanged(HandleMetatextChanged);
                iService.SetPropertySampleRateChanged(HandleSampleRateChanged);
                iService.SetPropertyUriChanged(HandleUriChanged);

                iBitDepth = new Watchable<uint>(aThread, string.Format("BitDepth({0})", aId), iService.PropertyBitDepth());
                iBitRate = new Watchable<uint>(aThread, string.Format("BitRate({0})", aId), iService.PropertyBitRate());
                iCodecName = new Watchable<string>(aThread, string.Format("CodecName({0})", aId), iService.PropertyCodecName());
                iDuration = new Watchable<uint>(aThread, string.Format("Duration({0})", aId), iService.PropertyDuration());
                iLossless = new Watchable<bool>(aThread, string.Format("Lossless({0})", aId), iService.PropertyLossless());
                iMetadata = new Watchable<string>(aThread, string.Format("Metadata({0})", aId), iService.PropertyMetadata());
                iMetatext = new Watchable<string>(aThread, string.Format("Metatext({0})", aId), iService.PropertyMetatext());
                iSampleRate = new Watchable<uint>(aThread, string.Format("SampleRate({0})", aId), iService.PropertySampleRate());
                iUri = new Watchable<string>(aThread, string.Format("Uri({0})", aId), iService.PropertyUri());
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

                iService = null;

                iDisposed = true;
            }
        }

        public IWatchable<uint> BitDepth
        {
            get
            {
                return iBitDepth;
            }
        }

        public IWatchable<uint> BitRate
        {
            get 
            {
                return iBitRate;
            }
        }

        public IWatchable<string> CodecName
        {
            get
            {
                return iCodecName;
            }
        }

        public IWatchable<uint> Duration
        {
            get
            {
                return iDuration;
            }
        }

        public IWatchable<bool> Lossless
        {
            get
            {
                return iLossless;
            }
        }

        public IWatchable<string> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<string> Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        public IWatchable<uint> SampleRate
        {
            get
            {
                return iSampleRate;
            }
        }

        public IWatchable<string> Uri
        {
            get
            {
                return iUri;
            }
        }

        private void HandleBitDepthChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iBitDepth.Update(iService.PropertyBitDepth());
            }
        }

        private void HandleBitRateChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iBitRate.Update(iService.PropertyBitRate());
            }
        }

        private void HandleBitCodecNameChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iCodecName.Update(iService.PropertyCodecName());
            }
        }

        private void HandleDurationChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iDuration.Update(iService.PropertyDuration());
            }
        }

        private void HandleLosslessChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iLossless.Update(iService.PropertyLossless());
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

        private void HandleMetatextChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iMetatext.Update(iService.PropertyMetatext());
            }
        }

        private void HandleSampleRateChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iSampleRate.Update(iService.PropertySampleRate());
            }
        }

        private void HandleUriChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iUri.Update(iService.PropertyUri());
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgInfo1 iService;

        private Watchable<uint> iBitDepth;
        private Watchable<uint> iBitRate;
        private Watchable<string> iCodecName;
        private Watchable<uint> iDuration;
        private Watchable<bool> iLossless;
        private Watchable<string> iMetadata;
        private Watchable<string> iMetatext;
        private Watchable<uint> iSampleRate;
        private Watchable<string> iUri;
    }

    public class MockServiceOpenHomeOrgInfo1 : IServiceOpenHomeOrgInfo1, IMockable, IDisposable
    {
        public MockServiceOpenHomeOrgInfo1(IWatchableThread aThread, string aId, uint aBitDepth, uint aBitRate, string aCodecName, uint aDuration, bool aLossless, string aMetadata, string aMetatext,
            uint aSampleRate, string aUri)
        {
            iBitDepth = new Watchable<uint>(aThread, string.Format("BitDepth({0})", aId), aBitDepth);
            iBitRate = new Watchable<uint>(aThread, string.Format("BitRate({0})", aId), aBitRate);
            iCodecName = new Watchable<string>(aThread, string.Format("CodecName({0})", aId), aCodecName);
            iDuration = new Watchable<uint>(aThread, string.Format("Duration({0})", aId), aDuration);
            iLossless = new Watchable<bool>(aThread, string.Format("Lossless({0})", aId), aLossless);
            iMetadata = new Watchable<string>(aThread, string.Format("Metadata({0})", aId), aMetadata);
            iMetatext = new Watchable<string>(aThread, string.Format("Metatext({0})", aId), aMetatext);
            iSampleRate = new Watchable<uint>(aThread, string.Format("SampleRate({0})", aId), aSampleRate);
            iUri = new Watchable<string>(aThread, string.Format("Uri({0})", aId), aUri);
        }

        public void Dispose()
        {
        }

        public IWatchable<uint> BitDepth
        {
            get
            {
                return iBitDepth;
            }
        }

        public IWatchable<uint> BitRate
        {
            get
            {
                return iBitRate;
            }
        }

        public IWatchable<string> CodecName
        {
            get
            {
                return iCodecName;
            }
        }

        public IWatchable<uint> Duration
        {
            get
            {
                return iDuration;
            }
        }

        public IWatchable<bool> Lossless
        {
            get
            {
                return iLossless;
            }
        }

        public IWatchable<string> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<string> Metatext
        {
            get
            {
                return iMetatext;
            }
        }

        public IWatchable<uint> SampleRate
        {
            get
            {
                return iSampleRate;
            }
        }

        public IWatchable<string> Uri
        {
            get
            {
                return iUri;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            /*string command = aValue.First().ToLowerInvariant();
            if (command == "balance")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iBalance.Update(int.Parse(value.First()));
            }
            else if (command == "fade")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iFade.Update(int.Parse(value.First()));
            }
            else if (command == "mute")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iMute.Update(bool.Parse(value.First()));
            }
            else if (command == "volume")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iVolume.Update(uint.Parse(value.First()));
            }
            else if (command == "volumeinc")
            {
                VolumeInc();
            }
            else if (command == "volumedec")
            {
                VolumeDec();
            }
            else
            {*/
                throw new NotSupportedException();
            //}
        }

        private Watchable<uint> iBitDepth;
        private Watchable<uint> iBitRate;
        private Watchable<string> iCodecName;
        private Watchable<uint> iDuration;
        private Watchable<bool> iLossless;
        private Watchable<string> iMetadata;
        private Watchable<string> iMetatext;
        private Watchable<uint> iSampleRate;
        private Watchable<string> iUri;
    }

    public class WatchableInfoFactory : IWatchableServiceFactory
    {
        public WatchableInfoFactory(IWatchableThread aThread)
        {
            iThread = aThread;

            iService = null;
        }

        public void Subscribe(WatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            if (iService == null && iPendingService == null)
            {
                iPendingService = new CpProxyAvOpenhomeOrgInfo1(aDevice.Device);
                iPendingService.SetPropertyInitialEvent(delegate
                {
                    iThread.Schedule(() =>
                    {
                        iService = new WatchableInfo(iThread, string.Format("Info({0})", aDevice.Udn), iPendingService);
                        iPendingService = null;
                        aCallback(iService);
                    });
                });
                iPendingService.Subscribe();
            }
        }

        public void Unsubscribe()
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

        private CpProxyAvOpenhomeOrgInfo1 iPendingService;
        private WatchableInfo iService;
        private IWatchableThread iThread;
    }

    public class WatchableInfo : Info
    {
        public WatchableInfo(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgInfo1 aService)
            : base(aId, new ServiceOpenHomeOrgInfo1(aThread, aId, aService))
        {
            iCpService = aService;
        }

        public override void Dispose()
        {
            if (iCpService != null)
            {
                iCpService.Dispose();
            }
        }

        private CpProxyAvOpenhomeOrgInfo1 iCpService;
    }

    public class MockWatchableInfo : Info, IMockable
    {
        public MockWatchableInfo(IWatchableThread aThread, string aId, uint aBitDepth, uint aBitRate, string aCodecName, uint aDuration, bool aLossless, string aMetadata, string aMetatext,
            uint aSampleRate, string aUri)
            : base(aId, new MockServiceOpenHomeOrgInfo1(aThread, aId, aBitDepth, aBitRate, aCodecName, aDuration, aLossless, aMetadata, aMetatext, aSampleRate, aUri))
        {
        }

        public override void Dispose()
        {
        }

        public void Execute(IEnumerable<string> aValue)
        {
            MockServiceOpenHomeOrgInfo1 i = iService as MockServiceOpenHomeOrgInfo1;
            i.Execute(aValue);
        }
    }
}
