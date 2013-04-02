using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgVolume1
    {
        IWatchable<int> Balance { get; }
        IWatchable<uint> BalanceMax { get; }
        IWatchable<int> Fade { get; }
        IWatchable<uint> FadeMax { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Volume { get; }
        IWatchable<uint> VolumeLimit { get; }
        IWatchable<uint> VolumeMax { get; }
        IWatchable<uint> VolumeMilliDbPerStep { get; }
        IWatchable<uint> VolumeSteps { get; }
        IWatchable<uint> VolumeUnity { get; }

        void SetBalance(int aValue);
        void SetFade(int aValue);
        void SetMute(bool aValue);
        void SetVolume(uint aValue);
        void VolumeDec();
        void VolumeInc();
    }

    public abstract class Volume : IWatchableService, IServiceOpenHomeOrgVolume1
    {

        protected Volume(string aId, IServiceOpenHomeOrgVolume1 aService)
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

        public IWatchable<int> Balance
        {
            get
            {
                return iService.Balance;
            }
        }

        public IWatchable<uint> BalanceMax
        {
            get
            {
                return iService.BalanceMax;
            }
        }

        public IWatchable<int> Fade
        {
            get
            {
                return iService.Fade;
            }
        }

        public IWatchable<uint> FadeMax
        {
            get
            {
                return iService.FadeMax;
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                return iService.Mute;
            }
        }

        IWatchable<uint> IServiceOpenHomeOrgVolume1.Volume
        {
            get
            {
                return iService.Volume;
            }
        }

        public IWatchable<uint> VolumeLimit
        {
            get
            {
                return iService.VolumeLimit;
            }
        }

        public IWatchable<uint> VolumeMax
        {
            get
            {
                return iService.VolumeMax;
            }
        }

        public IWatchable<uint> VolumeMilliDbPerStep
        {
            get
            {
                return iService.VolumeMilliDbPerStep;
            }
        }

        public IWatchable<uint> VolumeSteps
        {
            get
            {
                return iService.VolumeSteps;
            }
        }

        public IWatchable<uint> VolumeUnity
        {
            get
            {
                return iService.VolumeUnity;
            }
        }

        public void SetBalance(int aValue)
        {
            iService.SetBalance(aValue);
        }

        public void SetFade(int aValue)
        {
            iService.SetFade(aValue);
        }

        public void SetMute(bool aValue)
        {
            iService.SetMute(aValue);
        }

        public void SetVolume(uint aValue)
        {
            iService.SetVolume(aValue);
        }

        public void VolumeDec()
        {
            iService.VolumeDec();
        }

        public void VolumeInc()
        {
            iService.VolumeInc();
        }

        private string iId;
        protected IServiceOpenHomeOrgVolume1 iService;
    }

    public class ServiceOpenHomeOrgVolume1 : IServiceOpenHomeOrgVolume1, IDisposable
    {
        public ServiceOpenHomeOrgVolume1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgVolume1 aService)
        {
            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyBalanceChanged(HandleBalanceChanged);
                iService.SetPropertyBalanceMaxChanged(HandleBalanceMaxChanged);
                iService.SetPropertyFadeChanged(HandleFadeChanged);
                iService.SetPropertyFadeMaxChanged(HandleFadeMaxChanged);
                iService.SetPropertyMuteChanged(HandleMuteChanged);
                iService.SetPropertyVolumeChanged(HandleVolumeChanged);
                iService.SetPropertyVolumeLimitChanged(HandleVolumeLimiteChanged);
                iService.SetPropertyVolumeMaxChanged(HandleVolumeMaxChanged);
                iService.SetPropertyVolumeMilliDbPerStepChanged(HandleVolumeMilliDbPerStepChanged);
                iService.SetPropertyVolumeStepsChanged(HandleVolumeStepsChanged);
                iService.SetPropertyVolumeUnityChanged(HandleVolumeUnityChanged);

                iBalance = new Watchable<int>(aThread, string.Format("Balance({0})", aId), iService.PropertyBalance());
                iBalanceMax = new Watchable<uint>(aThread, string.Format("BalanceMax({0})", aId), iService.PropertyBalanceMax());
                iFade = new Watchable<int>(aThread, string.Format("Fade({0})", aId), iService.PropertyFade());
                iFadeMax = new Watchable<uint>(aThread, string.Format("FadeMax({0})", aId), iService.PropertyFadeMax());
                iMute = new Watchable<bool>(aThread, string.Format("Mute({0})", aId), iService.PropertyMute());
                iVolume = new Watchable<uint>(aThread, string.Format("Volume({0})", aId), iService.PropertyVolume());
                iVolumeLimit = new Watchable<uint>(aThread, string.Format("VolumeLimit({0})", aId), iService.PropertyVolumeLimit());
                iVolumeMax = new Watchable<uint>(aThread, string.Format("VolumeMax({0})", aId), iService.PropertyVolumeMax());
                iVolumeMilliDbPerStep = new Watchable<uint>(aThread, string.Format("VolumeMilliDbPerStep({0})", aId), iService.PropertyVolumeMilliDbPerStep());
                iVolumeSteps = new Watchable<uint>(aThread, string.Format("VolumeSteps({0})", aId), iService.PropertyVolumeSteps());
                iVolumeUnity = new Watchable<uint>(aThread, string.Format("VolumeUnity({0})", aId), iService.PropertyVolumeUnity());
            }
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.Dispose");
                }

                iService = null;

                iDisposed = true;
            }
        }

        public IWatchable<int> Balance
        {
            get
            {
                return iBalance;
            }
        }

        public IWatchable<uint> BalanceMax
        {
            get
            {
                return iBalanceMax;
            }
        }

        public IWatchable<int> Fade
        {
            get
            {
                return iFade;
            }
        }

        public IWatchable<uint> FadeMax
        {
            get
            {
                return iFadeMax;
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                return iMute;
            }
        }

        public IWatchable<uint> Volume
        {
            get{
                return iVolume;
            }
        }

        public IWatchable<uint> VolumeLimit
        {
            get
            {
                return iVolumeLimit;
            }
        }

        public IWatchable<uint> VolumeMax
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

        public void SetBalance(int aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.SetBalance");
                }

                iService.BeginSetBalance(aValue, null);
            }
        }

        public void SetFade(int aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.SetFade");
                }

                iService.BeginSetFade(aValue, null);
            }
        }

        public void SetMute(bool aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.SetMute");
                }

                iService.BeginSetMute(aValue, null);
            }
        }

        public void SetVolume(uint aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.SetVolume");
                }

                iService.BeginSetVolume(aValue, null);
            }
        }

        public void VolumeDec()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.VolumeDec");
                }

                iService.BeginVolumeDec(null);
            }
        }

        public void VolumeInc()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.VolumeInc");
                }

                iService.BeginVolumeInc(null);
            }
        }

        private void HandleVolumeUnityChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iVolumeUnity.Update(iService.PropertyVolumeUnity());
            }
        }

        private void HandleVolumeStepsChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iVolumeSteps.Update(iService.PropertyVolumeSteps());
            }
        }

        private void HandleVolumeMilliDbPerStepChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iVolumeMilliDbPerStep.Update(iService.PropertyVolumeMilliDbPerStep());
            }
        }

        private void HandleVolumeMaxChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iVolumeMax.Update(iService.PropertyVolumeMax());
            }
        }

        private void HandleVolumeLimiteChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iVolumeLimit.Update(iService.PropertyVolumeLimit());
            }
        }

        private void HandleVolumeChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iVolume.Update(iService.PropertyVolume());
            }
        }

        private void HandleMuteChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iMute.Update(iService.PropertyMute());
            }
        }

        private void HandleFadeMaxChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iFadeMax.Update(iService.PropertyFadeMax());
            }
        }

        private void HandleFadeChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iFade.Update(iService.PropertyFade());
            }
        }

        private void HandleBalanceMaxChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iBalanceMax.Update(iService.PropertyBalanceMax());
            }
        }

        private void HandleBalanceChanged()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                iBalance.Update(iService.PropertyBalance());
            }
        }

        private bool iDisposed;
        private object iLock;

        private CpProxyAvOpenhomeOrgVolume1 iService;

        private Watchable<int> iBalance;
        private Watchable<uint> iBalanceMax;
        private Watchable<int> iFade;
        private Watchable<uint> iFadeMax;
        private Watchable<bool> iMute;
        private Watchable<uint> iVolume;
        private Watchable<uint> iVolumeLimit;
        private Watchable<uint> iVolumeMax;
        private Watchable<uint> iVolumeMilliDbPerStep;
        private Watchable<uint> iVolumeSteps;
        private Watchable<uint> iVolumeUnity;
    }

    public class MockServiceOpenHomeOrgVolume1 : IServiceOpenHomeOrgVolume1, IMockable, IDisposable
    {
        public MockServiceOpenHomeOrgVolume1(IWatchableThread aThread, string aId, int aBalance, uint aBalanceMax, int aFade, uint aFadeMax, bool aMute, uint aVolume, uint aVolumeLimit, uint aVolumeMax,
            uint aVolumeMilliDbPerStep, uint aVolumeSteps, uint aVolumeUnity)
        {
            uint volumeLimit = aVolumeLimit;
            if (volumeLimit > aVolumeMax)
            {
                volumeLimit = aVolumeMax;
            }
            iCurrentVolumeLimit = volumeLimit;

            uint volume = aVolume;
            if (volume > aVolumeLimit)
            {
                volume = aVolumeLimit;
            }
            iCurrentVolume = volume;

            iBalance = new Watchable<int>(aThread, string.Format("Balance({0})", aId), aBalance);
            iBalanceMax = new Watchable<uint>(aThread, string.Format("BalanceMax({0})", aId), aBalanceMax);
            iFade = new Watchable<int>(aThread, string.Format("Fade({0})", aId), aFade);
            iFadeMax = new Watchable<uint>(aThread, string.Format("FadeMax({0})", aId), aFadeMax);
            iMute = new Watchable<bool>(aThread, string.Format("Mute({0})", aId), aMute);
            iVolume = new Watchable<uint>(aThread, string.Format("Volume({0})", aId), volume);
            iVolumeLimit = new Watchable<uint>(aThread, string.Format("VolumeLimit({0})", aId), volumeLimit);
            iVolumeMax = new Watchable<uint>(aThread, string.Format("VolumeMax({0})", aId), aVolumeMax);
            iVolumeMilliDbPerStep = new Watchable<uint>(aThread, string.Format("VolumeMilliDbPerStep({0})", aId), aVolumeMilliDbPerStep);
            iVolumeSteps = new Watchable<uint>(aThread, string.Format("VolumeSteps({0})", aId), aVolumeSteps);
            iVolumeUnity = new Watchable<uint>(aThread, string.Format("VolumeUnity({0})", aId), aVolumeUnity);
        }

        public void Dispose()
        {
        }

        public IWatchable<int> Balance
        {
            get
            {
                return iBalance;
            }
        }

        public IWatchable<uint> BalanceMax
        {
            get
            {
                return iBalanceMax;
            }
        }

        public IWatchable<int> Fade
        {
            get
            {
                return iFade;
            }
        }

        public IWatchable<uint> FadeMax
        {
            get
            {
                return iFadeMax;
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                return iMute;
            }
        }

        public IWatchable<uint> Volume
        {
            get
            {
                return iVolume;
            }
        }

        public IWatchable<uint> VolumeLimit
        {
            get
            {
                return iVolumeLimit;
            }
        }

        public IWatchable<uint> VolumeMax
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

        public void SetBalance(int aValue)
        {
            iBalance.Update(aValue);
        }

        public void SetFade(int aValue)
        {
            iFade.Update(aValue);
        }

        public void SetMute(bool aValue)
        {
            iMute.Update(aValue);
        }

        public void SetVolume(uint aValue)
        {
            uint volume = aValue;
            if (volume > iCurrentVolumeLimit)
            {
                volume = iCurrentVolumeLimit;
            }

            if (volume != iCurrentVolume)
            {
                iCurrentVolume = volume;
                iVolume.Update(aValue);
            }
        }

        public void VolumeDec()
        {
            if (iCurrentVolume > 0)
            {
                --iCurrentVolume;
                iVolume.Update(iCurrentVolume);
            }
        }

        public void VolumeInc()
        {
            if (iCurrentVolume < iCurrentVolumeLimit)
            {
                ++iCurrentVolume;
                iVolume.Update(iCurrentVolume);
            }
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
            {
                throw new NotSupportedException();
            }
        }

        private uint iCurrentVolume;
        private uint iCurrentVolumeLimit;

        private Watchable<int> iBalance;
        private Watchable<uint> iBalanceMax;
        private Watchable<int> iFade;
        private Watchable<uint> iFadeMax;
        private Watchable<bool> iMute;
        private Watchable<uint> iVolume;
        private Watchable<uint> iVolumeLimit;
        private Watchable<uint> iVolumeMax;
        private Watchable<uint> iVolumeMilliDbPerStep;
        private Watchable<uint> iVolumeSteps;
        private Watchable<uint> iVolumeUnity;
    }

    public class WatchableVolumeFactory : IWatchableServiceFactory
    {
        public WatchableVolumeFactory(IWatchableThread aThread)
        {
            iThread = aThread;

            iService = null;
        }

        public void Subscribe(WatchableDevice aDevice, Action<IWatchableService> aCallback)
        {
            if (iService == null && iPendingService == null)
            {
                iPendingService = new CpProxyAvOpenhomeOrgVolume1(aDevice.Device);
                iPendingService.SetPropertyInitialEvent(delegate
                {
                    iThread.Schedule(() =>
                    {
                        iService = new WatchableVolume(iThread, string.Format("Volume({0})", aDevice.Udn), iPendingService);
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

        private CpProxyAvOpenhomeOrgVolume1 iPendingService;
        private WatchableVolume iService;
        private IWatchableThread iThread;
    }

    public class WatchableVolume : Volume
    {
        public WatchableVolume(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgVolume1 aService)
            : base(aId, new ServiceOpenHomeOrgVolume1(aThread, aId, aService))
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

        private CpProxyAvOpenhomeOrgVolume1 iCpService;
    }

    public class MockWatchableVolume : Volume, IMockable
    {
        public MockWatchableVolume(IWatchableThread aThread, string aId, int aBalance, uint aBalanceMax, int aFade, uint aFadeMax, bool aMute, uint aVolume, uint aVolumeLimit, uint aVolumeMax,
            uint aVolumeMilliDbPerStep, uint aVolumeSteps, uint aVolumeUnity)
            : base(aId, new MockServiceOpenHomeOrgVolume1(aThread, aId, aBalance, aBalanceMax, aFade, aFadeMax, aMute, aVolume, aVolumeLimit, aVolumeMax, aVolumeMilliDbPerStep, aVolumeSteps, aVolumeUnity))
        {
        }

        public override void Dispose()
        {
        }

        public void Execute(IEnumerable<string> aValue)
        {
            MockServiceOpenHomeOrgProduct1 p = iService as MockServiceOpenHomeOrgProduct1;
            p.Execute(aValue);
        }
    }
}
