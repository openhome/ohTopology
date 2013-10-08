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
    public interface IProxyTime : IProxy
    {
        IWatchable<uint> Duration { get; }
        IWatchable<uint> Seconds { get; }
    }

    public abstract class ServiceTime : Service
    {
        protected ServiceTime(INetwork aNetwork, IInjectorDevice aDevice)
            : base(aNetwork, aDevice)
        {
            iDuration = new Watchable<uint>(Network, "Duration", 0);
            iSeconds = new Watchable<uint>(Network, "Seconds", 0);
        }

        public override void Dispose()
        {
            base.Dispose();

            iDuration.Dispose();
            iDuration = null;

            iSeconds.Dispose();
            iSeconds = null;
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyTime(this, aDevice);
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

        protected Watchable<uint> iDuration;
        protected Watchable<uint> iSeconds;
    }

    class ServiceTimeNetwork : ServiceTime
    {
        public ServiceTimeNetwork(INetwork aNetwork, IInjectorDevice aDevice, CpDevice aCpDevice)
            : base(aNetwork, aDevice)
        {
            iCpDevice = aCpDevice;
            iCpDevice.AddRef();

            iService = new CpProxyAvOpenhomeOrgTime1(aCpDevice);

            iService.SetPropertyDurationChanged(HandleDurationChanged);
            iService.SetPropertySecondsChanged(HandleSecondsChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            base.Dispose();

            iService.Dispose();
            iService = null;

            iCpDevice.RemoveRef();
        }

        protected override Task OnSubscribe()
        {
            Do.Assert(iSubscribedSource == null);

            iSubscribedSource = new TaskCompletionSource<bool>();

            iService.Subscribe();

            return iSubscribedSource.Task.ContinueWith((t) => { });
        }

        protected override void OnCancelSubscribe()
        {
            if (iSubscribedSource != null)
            {
                iSubscribedSource.TrySetCanceled();
            }
        }

        private void HandleInitialEvent()
        {
            iSubscribedSource.SetResult(true);
        }

        protected override void OnUnsubscribe()
        {
            if (iService != null)
            {
                iService.Unsubscribe();
            }

            iSubscribedSource = null;
        }

        private void HandleDurationChanged()
        {
            uint duration = iService.PropertyDuration();
            Network.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iDuration.Update(duration);
                });
            });
        }

        private void HandleSecondsChanged()
        {
            uint seconds = iService.PropertySeconds();
            Network.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iSeconds.Update(seconds);
                });
            });
        }

        private readonly CpDevice iCpDevice;
        private TaskCompletionSource<bool> iSubscribedSource;
        private CpProxyAvOpenhomeOrgTime1 iService;
    }

    class ServiceTimeMock : ServiceTime, IMockable
    {
        public ServiceTimeMock(INetwork aNetwork, IInjectorDevice aDevice, uint aSeconds, uint aDuration)
            : base(aNetwork, aDevice)
        {
            iDuration.Update(aDuration);
            iSeconds.Update(aSeconds);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "duration")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iDuration.Update(uint.Parse(value.First()));
            }
            else if (command == "seconds")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iSeconds.Update(uint.Parse(value.First()));
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public class ProxyTime : Proxy<ServiceTime>, IProxyTime
    {
        public ProxyTime(ServiceTime aService, IDevice aDevice)
            : base(aService, aDevice)
        {
        }

        public IWatchable<uint> Duration
        {
            get { return iService.Duration; }
        }

        public IWatchable<uint> Seconds
        {
            get { return iService.Seconds; }
        }
    }
}
