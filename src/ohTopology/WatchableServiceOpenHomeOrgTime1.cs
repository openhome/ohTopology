using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgTime1 : IWatchableService
    {
        IWatchable<uint> Duration { get; }
        IWatchable<uint> Seconds { get; }
    }

    public abstract class Time : IServiceOpenHomeOrgTime1
    {
        protected Time(string aId, IWatchableDevice aDevice, IServiceOpenHomeOrgTime1 aService)
        {
            iId = aId;
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        public string Id
        {
            get
            {
                return iId;
            }
        }

        public IWatchableDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public IWatchable<uint> Duration
        {
            get
            {
                return iService.Duration;
            }
        }

        public IWatchable<uint> Seconds
        {
            get
            {
                return iService.Seconds;
            }
        }

        private string iId;
        private IWatchableDevice iDevice;

        protected IServiceOpenHomeOrgTime1 iService;
    }

    public class ServiceOpenHomeOrgTime1 : IServiceOpenHomeOrgTime1, IDisposable
    {
        public ServiceOpenHomeOrgTime1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgTime1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyDurationChanged(HandleDurationChanged);
                iService.SetPropertySecondsChanged(HandleSecondsChanged);

                iDuration = new Watchable<uint>(aThread, string.Format("Duration({0})", aId), iService.PropertyDuration());
                iSeconds = new Watchable<uint>(aThread, string.Format("Seconds({0})", aId), iService.PropertySeconds());
            }
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgTime1.Dispose");
                }

                iService.Dispose();
                iService = null;

                iDisposed = true;
            }
        }

        public IWatchable<uint> Duration
        {
            get
            {
                return iDuration;
            }
        }

        public IWatchable<uint> Seconds
        {
            get
            {
                return iSeconds;
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

        private void HandleSecondsChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iSeconds.Update(iService.PropertySeconds());
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpProxyAvOpenhomeOrgTime1 iService;

        private Watchable<uint> iDuration;
        private Watchable<uint> iSeconds;
    }

    public class MockServiceOpenHomeOrgTime1 : IServiceOpenHomeOrgTime1, IMockable, IDisposable
    {
        public MockServiceOpenHomeOrgTime1(IWatchableThread aThread, string aId, uint aSeconds, uint aDuration)
        {
            iDuration = new Watchable<uint>(aThread, string.Format("Duration({0})", aId), aDuration);
            iSeconds = new Watchable<uint>(aThread, string.Format("Seconds({0})", aId), aSeconds);
        }

        public void Dispose()
        {
        }

        public IWatchable<uint> Duration
        {
            get
            {
                return iDuration;
            }
        }

        public IWatchable<uint> Seconds
        {
            get
            {
                return iSeconds;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            throw new NotImplementedException();
        }

        private Watchable<uint> iDuration;
        private Watchable<uint> iSeconds;
    }

    public class WatchableTimeFactory : IWatchableServiceFactory
    {
        public WatchableTimeFactory(IWatchableThread aThread)
        {
            iThread = aThread;

            iService = null;
        }

        public Type Type
        {
            get
            {
                return typeof(Time);
            }
        }

        public void Subscribe(IWatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            if (iService == null && iPendingService == null)
            {
                WatchableDevice d = aDevice as WatchableDevice;
                iPendingService = new CpProxyAvOpenhomeOrgTime1(d.Device);
                iPendingService.SetPropertyInitialEvent(delegate
                {
                    iThread.Schedule(() =>
                    {
                        iService = new WatchableTime(iThread, string.Format("Time({0})", aDevice.Udn), aDevice, iPendingService);
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

        private CpProxyAvOpenhomeOrgTime1 iPendingService;
        private WatchableTime iService;
        private IWatchableThread iThread;
    }

    public class WatchableTime : Time
    {
        public WatchableTime(IWatchableThread aThread, string aId, IWatchableDevice aDevice, CpProxyAvOpenhomeOrgTime1 aService)
            : base(aId, aDevice, new ServiceOpenHomeOrgTime1(aThread, aId, aService))
        {
        }
    }

    public class MockWatchableTimeFactory : IWatchableServiceFactory
    {
        public MockWatchableTimeFactory(IWatchableThread aThread, uint aSeconds, uint aDuration)
        {
            iThread = aThread;

            iPendingService = false;
            iService = null;

            iSeconds = aSeconds;
            iDuration = aDuration;
        }

        public Type Type
        {
            get
            {
                return typeof(Time);
            }
        }

        public void Subscribe(IWatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            if (iService == null && iPendingService == false)
            {
                iPendingService = true;
                iThread.Schedule(() =>
                {
                    if (iPendingService)
                    {
                        iService = new MockWatchableTime(iThread, string.Format("Time({0})", aDevice.Udn), aDevice, iSeconds, iDuration);
                        iPendingService = false;
                        aCallback(iService);
                    }
                });
            }
        }

        public void Unsubscribe()
        {
            iPendingService = false;

            if (iService != null)
            {
                iService.Dispose();
                iService = null;
            }
        }

        private bool iPendingService;
        private MockWatchableTime iService;
        private IWatchableThread iThread;

        private uint iSeconds;
        private uint iDuration;
    }

    public class MockWatchableTime : Time, IMockable
    {
        public MockWatchableTime(IWatchableThread aThread, string aId, IWatchableDevice aDevice, uint aSeconds, uint aDuration)
            : base(aId, aDevice, new MockServiceOpenHomeOrgTime1(aThread, aId, aSeconds, aDuration))
        {
        }

        public void Execute(IEnumerable<string> aValue)
        {
            MockServiceOpenHomeOrgTime1 t = iService as MockServiceOpenHomeOrgTime1;
            t.Execute(aValue);
        }
    }
}
