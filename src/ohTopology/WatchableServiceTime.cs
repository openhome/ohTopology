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
        protected ServiceTime(INetwork aNetwork)
            : base(aNetwork)
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

        public override IProxy OnCreate(IWatchableDevice aDevice)
        {
            return new ProxyTime(aDevice, this);
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

    public class ServiceTimeNetwork : ServiceTime
    {
        public ServiceTimeNetwork(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork)
        {
            iSubscribe = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgTime1(aDevice);

            iService.SetPropertyDurationChanged(HandleDurationChanged);
            iService.SetPropertySecondsChanged(HandleSecondsChanged);

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

        private ManualResetEvent iSubscribe;
        private CpProxyAvOpenhomeOrgTime1 iService;
    }

    public class ServiceTimeMock : ServiceTime, IMockable
    {
        public ServiceTimeMock(INetwork aNetwork, uint aSeconds, uint aDuration)
            : base(aNetwork)
        {
            iDuration.Update(aDuration);
            iSeconds.Update(aSeconds);
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
        public ProxyTime(IWatchableDevice aDevice, ServiceTime aService)
            : base(aDevice, aService)
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
