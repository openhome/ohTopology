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
    public interface IProxyVolume : IProxy
    {
        IWatchable<int> Balance { get; }
        IWatchable<int> Fade { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Value { get; }
        IWatchable<uint> VolumeLimit { get; }
        IWatchable<uint> VolumeMilliDbPerStep { get; }
        IWatchable<uint> VolumeSteps { get; }
        IWatchable<uint> VolumeUnity { get; }

        Task SetBalance(int aValue);
        Task SetFade(int aValue);
        Task SetMute(bool aValue);
        Task SetVolume(uint aValue);
        Task VolumeDec();
        Task VolumeInc();

        uint BalanceMax { get; }
        uint FadeMax { get; }
        uint VolumeMax { get; }
    }

    public abstract class ServiceVolume : Service
    {
        protected ServiceVolume(INetwork aNetwork, IDevice aDevice)
            : base(aNetwork, aDevice)
        {
            iBalance = new Watchable<int>(Network, "Balance", 0);
            iFade = new Watchable<int>(Network, "Fade", 0);
            iMute = new Watchable<bool>(Network, "Mute", false);
            iValue = new Watchable<uint>(Network, "Value", 0);
            iVolumeLimit = new Watchable<uint>(Network, "VolumeLimit", 0);
            iVolumeMilliDbPerStep = new Watchable<uint>(Network, "VolumeMilliDbPerStep", 0);
            iVolumeSteps = new Watchable<uint>(Network, "VolumeSteps", 0);
            iVolumeUnity = new Watchable<uint>(Network, "VolumeUnity", 0);
        }

        public override void Dispose()
        {
            base.Dispose();

            iBalance.Dispose();
            iBalance = null;

            iFade.Dispose();
            iFade = null;

            iMute.Dispose();
            iMute = null;

            iValue.Dispose();
            iValue = null;

            iVolumeLimit.Dispose();
            iVolumeLimit = null;

            iVolumeMilliDbPerStep.Dispose();
            iVolumeMilliDbPerStep = null;

            iVolumeSteps.Dispose();
            iVolumeSteps = null;

            iVolumeUnity.Dispose();
            iVolumeUnity = null;
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyVolume(this);
        }

        public IWatchable<int> Balance
        {
            get
            {
                return iBalance;
            }
        }

        public IWatchable<int> Fade
        {
            get
            {
                return iFade;
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                return iMute;
            }
        }

        public IWatchable<uint> Value
        {
            get
            {
                return iValue;
            }
        }

        public IWatchable<uint> VolumeLimit
        {
            get
            {
                return iVolumeLimit;
            }
        }

        public uint BalanceMax
        {
            get
            {
                return iBalanceMax;
            }
        }

        public uint FadeMax
        {
            get
            {
                return iFadeMax;
            }
        }

        public uint VolumeMax
        {
            get
            {
                return iVolumeMax;
            }
        }

        public IWatchable<uint> VolumeMilliDbPerStep
        {
            get
            {
                return iVolumeMilliDbPerStep;
            }
        }

        public IWatchable<uint> VolumeSteps
        {
            get
            {
                return iVolumeSteps;
            }
        }

        public IWatchable<uint> VolumeUnity
        {
            get
            {
                return iVolumeUnity;
            }
        }

        public abstract Task SetBalance(int aValue);
        public abstract Task SetFade(int aValue);
        public abstract Task SetMute(bool aValue);
        public abstract Task SetVolume(uint aValue);
        public abstract Task VolumeDec();
        public abstract Task VolumeInc();
   
        protected uint iBalanceMax;
        protected uint iFadeMax;
        protected uint iVolumeMax;

        protected Watchable<int> iBalance;
        protected Watchable<int> iFade;
        protected Watchable<bool> iMute;
        protected Watchable<uint> iValue;
        protected Watchable<uint> iVolumeLimit;
        protected Watchable<uint> iVolumeMilliDbPerStep;
        protected Watchable<uint> iVolumeSteps;
        protected Watchable<uint> iVolumeUnity;
    }

    class ServiceVolumeNetwork : ServiceVolume
    {
        public ServiceVolumeNetwork(INetwork aNetwork, IDevice aDevice, CpDevice aCpDevice)
            : base(aNetwork, aDevice)
        {
            iSubscribed = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgVolume1(aCpDevice);

            iService.SetPropertyBalanceChanged(HandleBalanceChanged);
            iService.SetPropertyFadeChanged(HandleFadeChanged);
            iService.SetPropertyMuteChanged(HandleMuteChanged);
            iService.SetPropertyVolumeChanged(HandleVolumeChanged);
            iService.SetPropertyVolumeLimitChanged(HandleVolumeLimitChanged);
            iService.SetPropertyVolumeMilliDbPerStepChanged(HandleVolumeMilliDbPerStepChanged);
            iService.SetPropertyVolumeStepsChanged(HandleVolumeStepsChanged);
            iService.SetPropertyVolumeUnityChanged(HandleVolumeUnityChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
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
            if (iService != null)
            {
                iService.Unsubscribe();
            }
            iSubscribed.Reset();
        }

        public override Task SetBalance(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetBalance(aValue);
            });
            return task;
        }

        public override Task SetFade(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetFade(aValue);
            });
            return task;
        }

        public override Task SetMute(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetMute(aValue);
            });
            return task;
        }

        public override Task SetVolume(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncSetVolume(aValue);
            });
            return task;
        }

        public override Task VolumeDec()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncVolumeDec();
            });
            return task;
        }

        public override Task VolumeInc()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iService.SyncVolumeInc();
            });
            return task;
        }

        private void HandleVolumeUnityChanged()
        {
            Network.Schedule(() =>
            {
                iVolumeUnity.Update(iService.PropertyVolumeUnity());
            });
        }

        private void HandleVolumeStepsChanged()
        {
            Network.Schedule(() =>
            {
                iVolumeSteps.Update(iService.PropertyVolumeSteps());
            });
        }

        private void HandleVolumeMilliDbPerStepChanged()
        {
            Network.Schedule(() =>
            {
                iVolumeMilliDbPerStep.Update(iService.PropertyVolumeMilliDbPerStep());
            });
        }

        private void HandleVolumeLimitChanged()
        {
            Network.Schedule(() =>
            {
                iVolumeLimit.Update(iService.PropertyVolumeLimit());
            });
        }

        private void HandleVolumeChanged()
        {
            Network.Schedule(() =>
            {
                Console.WriteLine("VolumeChanged: " + Device.Udn + " " + iService.PropertyVolume());
                iValue.Update(iService.PropertyVolume());
            });
        }

        private void HandleMuteChanged()
        {
            Network.Schedule(() =>
            {
                iMute.Update(iService.PropertyMute());
            });
        }

        private void HandleFadeChanged()
        {
            Network.Schedule(() =>
            {
                iFade.Update(iService.PropertyFade());
            });
        }

        private void HandleBalanceChanged()
        {
            Network.Schedule(() =>
            {
                iBalance.Update(iService.PropertyBalance());
            });
        }

        private ManualResetEvent iSubscribed;
        private CpProxyAvOpenhomeOrgVolume1 iService;
    }

    class ServiceVolumeMock : ServiceVolume, IMockable
    {
        public ServiceVolumeMock(INetwork aNetwork, IDevice aDevice, string aId, int aBalance, uint aBalanceMax, int aFade, uint aFadeMax, bool aMute, uint aValue, uint aVolumeLimit, uint aVolumeMax,
            uint aVolumeMilliDbPerStep, uint aVolumeSteps, uint aVolumeUnity)
            : base(aNetwork, aDevice)
        {
            uint volumeLimit = aVolumeLimit;
            if (volumeLimit > aVolumeMax)
            {
                volumeLimit = aVolumeMax;
            }
            iCurrentVolumeLimit = volumeLimit;

            uint value = aValue;
            if (value > aVolumeLimit)
            {
                value = aVolumeLimit;
            }
            iCurrentVolume = value;

            iBalance.Update(aBalance);
            iFade.Update(aFade);
            iMute.Update(aMute);
            iValue.Update(value);
            iVolumeLimit.Update(volumeLimit);
            iVolumeMilliDbPerStep.Update(aVolumeMilliDbPerStep);
            iVolumeSteps.Update(aVolumeSteps);
            iVolumeUnity.Update(aVolumeUnity);
        }

        public override Task SetBalance(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iBalance.Update(aValue);
                });
            });
            return task;
        }

        public override Task SetFade(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iFade.Update(aValue);
                });
            });
            return task;
        }

        public override Task SetMute(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iMute.Update(aValue);
                });
            });
            return task;
        }

        public override Task SetVolume(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                uint value = aValue;
                if (value > iCurrentVolumeLimit)
                {
                    value = iCurrentVolumeLimit;
                }

                if (value != iCurrentVolume)
                {
                    iCurrentVolume = value;
                    Network.Schedule(() =>
                    {
                        iValue.Update(aValue);
                    });
                }
            });
            return task;
        }

        public override Task VolumeDec()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                if (iCurrentVolume > 0)
                {
                    --iCurrentVolume;
                    Network.Schedule(() =>
                    {
                        iValue.Update(iCurrentVolume);
                    });
                }
            });
            return task;
        }

        public override Task VolumeInc()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                if (iCurrentVolume < iCurrentVolumeLimit)
                {
                    ++iCurrentVolume;
                    Network.Schedule(() =>
                    {
                        iValue.Update(iCurrentVolume);
                    });
                }
            });
            return task;
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
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
            else if (command == "value")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iValue.Update(uint.Parse(value.First()));
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
            {
                throw new NotSupportedException();
            }
        }

        private uint iCurrentVolume;
        private uint iCurrentVolumeLimit;
    }

    public class ProxyVolume : Proxy<ServiceVolume>, IProxyVolume
    {
        public ProxyVolume(ServiceVolume aService)
            : base(aService)
        {
        }

        public IWatchable<int> Balance
        {
            get { return iService.Balance; }
        }

        public IWatchable<int> Fade
        {
            get { return iService.Fade; }
        }

        public IWatchable<bool> Mute
        {
            get { return iService.Mute; }
        }

        public IWatchable<uint> Value
        {
            get { return iService.Value; }
        }

        public IWatchable<uint> VolumeLimit
        {
            get { return iService.VolumeLimit; }
        }

        public IWatchable<uint> VolumeMilliDbPerStep
        {
            get { return iService.VolumeMilliDbPerStep; }
        }

        public IWatchable<uint> VolumeSteps
        {
            get { return iService.VolumeSteps; }
        }

        public IWatchable<uint> VolumeUnity
        {
            get { return iService.VolumeUnity; }
        }

        public uint BalanceMax
        {
            get { return iService.BalanceMax; }
        }

        public uint FadeMax
        {
            get { return iService.FadeMax; }
        }

        public uint VolumeMax
        {
            get { return iService.VolumeMax; }
        }

        public Task SetBalance(int aValue)
        {
            return iService.SetBalance(aValue);
        }

        public Task SetFade(int aValue)
        {
            return iService.SetFade(aValue);
        }

        public Task SetMute(bool aValue)
        {
            return iService.SetMute(aValue);
        }

        public Task SetVolume(uint aValue)
        {
            return iService.SetVolume(aValue);
        }

        public Task VolumeDec()
        {
            return iService.VolumeDec();
        }

        public Task VolumeInc()
        {
            return iService.VolumeInc();
        }
    }
}
