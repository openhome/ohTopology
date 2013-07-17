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
        protected ServiceTime(INetwork aNetwork, IDevice aDevice)
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
            return new ProxyTime(this);
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
        public ServiceTimeNetwork(INetwork aNetwork, IDevice aDevice, CpDevice aCpDevice)
            : base(aNetwork, aDevice)
        {
            iSubscribed = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgTime1(aCpDevice);

            iService.SetPropertyDurationChanged(HandleDurationChanged);
            iService.SetPropertySecondsChanged(HandleSecondsChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            // cause in flight or blocked subscription to complete
            iSubscribed.Set();

            base.Dispose();

            iSubscribed.Dispose();
            iSubscribed = null;

            iService.Dispose();
            iService = null;
        }

        protected override Task OnSubscribe()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.Subscribe();
                iSubscribed.WaitOne();
            });
            return task;
        }

        protected override void OnCancelSubscribe()
        {
            iSubscribed.Set();
        }

        private void HandleInitialEvent()
        {
            iSubscribed.Set();
        }

        protected override void OnUnsubscribe()
        {
            iService.Unsubscribe();
            iSubscribed.Reset();
        }

        private void HandleDurationChanged()
        {
            Network.Schedule(() =>
            {
                iDuration.Update(iService.PropertyDuration());
            });
        }

        private void HandleSecondsChanged()
        {
            Network.Schedule(() =>
            {
                iSeconds.Update(iService.PropertySeconds());
            });
        }

        private ManualResetEvent iSubscribed;
        private CpProxyAvOpenhomeOrgTime1 iService;
    }

    class ServiceTimeMock : ServiceTime, IMockable
    {
        public ServiceTimeMock(INetwork aNetwork, IDevice aDevice, uint aSeconds, uint aDuration)
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
        public ProxyTime(ServiceTime aService)
            : base(aService)
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
