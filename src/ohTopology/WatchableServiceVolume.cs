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
    public interface IServiceVolume
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
    }

    public interface IVolume : IServiceVolume
    {
        uint BalanceMax { get; }
        uint FadeMax { get; }
        uint VolumeMax { get; }
    }

    public abstract class ServiceVolume : Service, IVolume
    {
        protected ServiceVolume(INetwork aNetwork, string aId)
        {
            iBalance = new Watchable<int>(aNetwork, string.Format("Balance({0})", aId), 0);
            iFade = new Watchable<int>(aNetwork, string.Format("Fade({0})", aId), 0);
            iMute = new Watchable<bool>(aNetwork, string.Format("Mute({0})", aId), false);
            iValue = new Watchable<uint>(aNetwork, string.Format("Value({0})", aId), 0);
            iVolumeLimit = new Watchable<uint>(aNetwork, string.Format("VolumeLimit({0})", aId), 0);
            iVolumeMilliDbPerStep = new Watchable<uint>(aNetwork, string.Format("VolumeMilliDbPerStep({0})", aId), 0);
            iVolumeSteps = new Watchable<uint>(aNetwork, string.Format("VolumeSteps({0})", aId), 0);
            iVolumeUnity = new Watchable<uint>(aNetwork, string.Format("VolumeUnity({0})", aId), 0);
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

        public override IProxy OnCreate(IWatchableDevice aDevice)
        {
            return new ProxyVolume(this, aDevice);
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

    public class ServiceVolumeNetwork : ServiceVolume
    {
        public ServiceVolumeNetwork(INetwork aNetwork, string aId, CpDevice aDevice)
            : base(aNetwork, aId)
        {
            iNetwork = aNetwork;
            iSubscribe = new ManualResetEvent(false);
            iService = new CpProxyAvOpenhomeOrgVolume1(aDevice);

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
            iNetwork.Schedule(() =>
            {
                iVolumeUnity.Update(iService.PropertyVolumeUnity());
            });
        }

        private void HandleVolumeStepsChanged()
        {
            iNetwork.Schedule(() =>
            {
                iVolumeSteps.Update(iService.PropertyVolumeSteps());
            });
        }

        private void HandleVolumeMilliDbPerStepChanged()
        {
            iNetwork.Schedule(() =>
            {
                iVolumeMilliDbPerStep.Update(iService.PropertyVolumeMilliDbPerStep());
            });
        }

        private void HandleVolumeLimitChanged()
        {
            iNetwork.Schedule(() =>
            {
                iVolumeLimit.Update(iService.PropertyVolumeLimit());
            });
        }

        private void HandleVolumeChanged()
        {
            iNetwork.Schedule(() =>
            {
                iValue.Update(iService.PropertyVolume());
            });
        }

        private void HandleMuteChanged()
        {
            iNetwork.Schedule(() =>
            {
                iMute.Update(iService.PropertyMute());
            });
        }

        private void HandleFadeChanged()
        {
            iNetwork.Schedule(() =>
            {
                iFade.Update(iService.PropertyFade());
            });
        }

        private void HandleBalanceChanged()
        {
            iNetwork.Schedule(() =>
            {
                iBalance.Update(iService.PropertyBalance());
            });
        }

        private INetwork iNetwork;
        private ManualResetEvent iSubscribe;
        private CpProxyAvOpenhomeOrgVolume1 iService;
    }

    public class ServiceVolumeMock : ServiceVolume, IMockable
    {
        public ServiceVolumeMock(INetwork aNetwork, string aId, int aBalance, uint aBalanceMax, int aFade, uint aFadeMax, bool aMute, uint aValue, uint aVolumeLimit, uint aVolumeMax,
            uint aVolumeMilliDbPerStep, uint aVolumeSteps, uint aVolumeUnity)
            : base(aNetwork, aId)
        {
            iNetwork = aNetwork;

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

        protected override void OnSubscribe()
        {
            iNetwork.WatchableSubscribeThread.Execute(() =>
            {
            });
        }

        protected override void OnUnsubscribe()
        {
        }

        public override Task SetBalance(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
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
                iNetwork.Schedule(() =>
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
                iNetwork.Schedule(() =>
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
                    iNetwork.Schedule(() =>
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
                    iNetwork.Schedule(() =>
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
                    iNetwork.Schedule(() =>
                    {
                        iValue.Update(iCurrentVolume);
                    });
                }
            });
            return task;
        }

        public void Execute(IEnumerable<string> aValue)
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

        private INetwork iNetwork;

        private uint iCurrentVolume;
        private uint iCurrentVolumeLimit;
    }

    public class ProxyVolume : IVolume, IProxy
    {
        public ProxyVolume(ServiceVolume aService, IWatchableDevice aDevice)
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

        private ServiceVolume iService;
        private IWatchableDevice iDevice;
    }
}
