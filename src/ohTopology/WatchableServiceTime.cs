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
    public interface IServiceTime
    {
        IWatchable<uint> Duration { get; }
        IWatchable<uint> Seconds { get; }
    }

    public abstract class Time : Service, IServiceTime
    {
        protected Time(INetwork aNetwork, string aId)
        {
            iDuration = new Watchable<uint>(aNetwork, string.Format("Duration({0})", aId), 0);
            iSeconds = new Watchable<uint>(aNetwork, string.Format("Seconds({0})", aId), 0);
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

    public class ServiceOpenHomeOrgTime1 : Time
    {
        public ServiceOpenHomeOrgTime1(INetwork aNetwork, string aId, CpDevice aDevice)
            : base(aNetwork, aId)
        {
            iNetwork = aNetwork;
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
            iSubscribe.Reset();
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
        }

        private void HandleDurationChanged()
        {
            iNetwork.Schedule(() =>
            {
                iDuration.Update(iService.PropertyDuration());
            });
        }

        private void HandleSecondsChanged()
        {
            iNetwork.Schedule(() =>
            {
                iSeconds.Update(iService.PropertySeconds());
            });
        }

        private INetwork iNetwork;
        private ManualResetEvent iSubscribe;
        private CpProxyAvOpenhomeOrgTime1 iService;
    }

    public class MockServiceOpenHomeOrgTime1 : Time, IMockable
    {
        public MockServiceOpenHomeOrgTime1(INetwork aNetwork, string aId, uint aSeconds, uint aDuration)
            : base(aNetwork, aId)
        {
            iNetwork = aNetwork;

            iDuration.Update(aDuration);
            iSeconds.Update(aSeconds);
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

        private INetwork iNetwork;
    }

    public class ProxyTime : IServiceTime, IProxy
    {
        public ProxyTime(Time aService, IWatchableDevice aDevice)
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

        public IWatchable<uint> Duration
        {
            get { return iService.Duration; }
        }

        public IWatchable<uint> Seconds
        {
            get { return iService.Seconds; }
        }

        private Time iService;
        private IWatchableDevice iDevice;
    }
}
