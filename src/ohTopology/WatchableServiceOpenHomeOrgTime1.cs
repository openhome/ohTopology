using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgTime1
    {
        IWatchable<uint> Duration { get; }
        IWatchable<uint> Seconds { get; }
    }

    public abstract class Time : IServiceOpenHomeOrgTime1, IWatchableService
    {
        public abstract void Dispose();

        public IService Create(IManagableWatchableDevice aDevice)
        {
            return new ServiceTime(aDevice, this);
        }

        internal abstract IServiceOpenHomeOrgTime1 Service { get; }

        public IWatchable<uint> Duration
        {
            get
            {
                return Service.Duration;
            }
        }

        public IWatchable<uint> Seconds
        {
            get
            {
                return Service.Seconds;
            }
        }
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

                iDuration.Dispose();
                iDuration = null;

                iSeconds.Dispose();
                iSeconds = null;

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
            iDuration.Dispose();
            iDuration = null;

            iSeconds.Dispose();
            iSeconds = null;
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
        public WatchableTimeFactory(IWatchableThread aThread, IWatchableThread aSubscribeThread)
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
                    iPendingService = new CpProxyAvOpenhomeOrgTime1(d.Device);
                    iPendingService.SetPropertyInitialEvent(delegate
                    {
                        lock (iLock)
                        {
                            if (iPendingService != null)
                            {
                                iService = new WatchableTime(iThread, string.Format("Time({0})", aDevice.Udn), iPendingService);
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
                }
            });
        }

        private object iLock;
        private bool iDisposed;
        private IWatchableThread iSubscribeThread;
        private CpProxyAvOpenhomeOrgTime1 iPendingService;
        private WatchableTime iService;
        private IWatchableThread iThread;
    }

    public class WatchableTime : Time
    {
        public WatchableTime(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgTime1 aService)
        {
            iService = new ServiceOpenHomeOrgTime1(aThread, aId, aService);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgTime1 Service
        {
            get
            {
                return iService;
            }
        }

        private ServiceOpenHomeOrgTime1 iService;
    }

    public class MockWatchableTime : Time, IMockable
    {
        public MockWatchableTime(IWatchableThread aThread, string aId, uint aSeconds, uint aDuration)
        {
            iService = new MockServiceOpenHomeOrgTime1(aThread, aId, aSeconds, aDuration);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        public void Execute(IEnumerable<string> aValue)
        {
            iService.Execute(aValue);
        }

        internal override IServiceOpenHomeOrgTime1 Service
        {
            get
            {
                return iService;
            }
        }

        private MockServiceOpenHomeOrgTime1 iService;
    }

    public class ServiceTime : IServiceOpenHomeOrgTime1, IService
    {
        public ServiceTime(IManagableWatchableDevice aDevice, Time aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iDevice.Unsubscribe<ServiceTime>();
            iDevice = null;
        }

        public IWatchableDevice Device
        {
            get { return iDevice; }
        }

        public IWatchable<uint> Duration
        {
            get { return iService.Duration; }
        }

        public IWatchable<uint> Seconds
        {
            get { return iService.Seconds; }
        }

        private IManagableWatchableDevice iDevice;
        private Time iService;
    }
}
