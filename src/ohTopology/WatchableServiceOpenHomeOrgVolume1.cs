using System;
using System.Linq;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IServiceOpenHomeOrgVolume1
    {
        IWatchable<int> Balance { get; }
        IWatchable<int> Fade { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Value { get; }
        IWatchable<uint> VolumeLimit { get; }
        IWatchable<uint> VolumeMilliDbPerStep { get; }
        IWatchable<uint> VolumeSteps { get; }
        IWatchable<uint> VolumeUnity { get; }

        void SetBalance(int aValue, Action aAction);
        void SetFade(int aValue, Action aAction);
        void SetMute(bool aValue, Action aAction);
        void SetVolume(uint aValue, Action aAction);
        void VolumeDec(Action aAction);
        void VolumeInc(Action aAction);
    }

    public interface IVolume : IServiceOpenHomeOrgVolume1
    {
        uint BalanceMax { get; }
        uint FadeMax { get; }
        uint VolumeMax { get; }
    }

    public abstract class Volume : IVolume, IWatchableService
    {
        public abstract void Dispose();

        public IService Create(IManagableWatchableDevice aDevice)
        {
            return new ServiceVolume(aDevice, this);
        }

        internal abstract IServiceOpenHomeOrgVolume1 Service { get; }

        public IWatchable<int> Balance
        {
            get
            {
                return Service.Balance;
            }
        }

        public IWatchable<int> Fade
        {
            get
            {
                return Service.Fade;
            }
        }

        public IWatchable<bool> Mute
        {
            get
            {
                return Service.Mute;
            }
        }

        public IWatchable<uint> Value
        {
            get
            {
                return Service.Value;
            }
        }

        public IWatchable<uint> VolumeLimit
        {
            get
            {
                return Service.VolumeLimit;
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
                return Service.VolumeMilliDbPerStep;
            }
        }

        public IWatchable<uint> VolumeSteps
        {
            get
            {
                return Service.VolumeSteps;
            }
        }

        public IWatchable<uint> VolumeUnity
        {
            get
            {
                return Service.VolumeUnity;
            }
        }

        public void SetBalance(int aValue, Action aAction)
        {
            Service.SetBalance(aValue, aAction);
        }

        public void SetFade(int aValue, Action aAction)
        {
            Service.SetFade(aValue, aAction);
        }

        public void SetMute(bool aValue, Action aAction)
        {
            Service.SetMute(aValue, aAction);
        }

        public void SetVolume(uint aValue, Action aAction)
        {
            Service.SetVolume(aValue, aAction);
        }

        public void VolumeDec(Action aAction)
        {
            Service.VolumeDec(aAction);
        }

        public void VolumeInc(Action aAction)
        {
            Service.VolumeInc(aAction);
        }

        protected uint iBalanceMax;
        protected uint iFadeMax;
        protected uint iVolumeMax;
    }

    public class ServiceOpenHomeOrgVolume1 : IServiceOpenHomeOrgVolume1, IDisposable
    {
        public ServiceOpenHomeOrgVolume1(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgVolume1 aService)
        {
            iThread = aThread;

            iLock = new object();
            iDisposed = false;

            lock (iLock)
            {
                iService = aService;

                iService.SetPropertyBalanceChanged(HandleBalanceChanged);
                iService.SetPropertyFadeChanged(HandleFadeChanged);
                iService.SetPropertyMuteChanged(HandleMuteChanged);
                iService.SetPropertyVolumeChanged(HandleVolumeChanged);
                iService.SetPropertyVolumeLimitChanged(HandleVolumeLimitChanged);
                iService.SetPropertyVolumeMilliDbPerStepChanged(HandleVolumeMilliDbPerStepChanged);
                iService.SetPropertyVolumeStepsChanged(HandleVolumeStepsChanged);
                iService.SetPropertyVolumeUnityChanged(HandleVolumeUnityChanged);

                iBalance = new Watchable<int>(iThread, string.Format("Balance({0})", aId), iService.PropertyBalance());
                iFade = new Watchable<int>(iThread, string.Format("Fade({0})", aId), iService.PropertyFade());
                iMute = new Watchable<bool>(iThread, string.Format("Mute({0})", aId), iService.PropertyMute());
                iValue = new Watchable<uint>(iThread, string.Format("Value({0})", aId), iService.PropertyVolume());
                iVolumeLimit = new Watchable<uint>(iThread, string.Format("VolumeLimit({0})", aId), iService.PropertyVolumeLimit());
                iVolumeMilliDbPerStep = new Watchable<uint>(aThread, string.Format("VolumeMilliDbPerStep({0})", aId), iService.PropertyVolumeMilliDbPerStep());
                iVolumeSteps = new Watchable<uint>(iThread, string.Format("VolumeSteps({0})", aId), iService.PropertyVolumeSteps());
                iVolumeUnity = new Watchable<uint>(iThread, string.Format("VolumeUnity({0})", aId), iService.PropertyVolumeUnity());
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

                iService.Dispose();
                iService = null;

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
            get{
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

        public void SetBalance(int aValue, Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.SetBalance");
                }

                iService.BeginSetBalance(aValue, (IntPtr) =>
                {
                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void SetFade(int aValue, Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.SetFade");
                }

                iService.BeginSetFade(aValue, (IntPtr) =>
                {
                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void SetMute(bool aValue, Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.SetMute");
                }

                iService.BeginSetMute(aValue, (IntPtr) =>
                {
                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void SetVolume(uint aValue, Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.SetVolume");
                }

                iService.BeginSetVolume(aValue, (IntPtr) =>
                {
                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void VolumeDec(Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.VolumeDec");
                }

                iService.BeginVolumeDec((IntPtr) =>
                {
                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
            }
        }

        public void VolumeInc(Action aAction)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("ServiceOpenHomeOrgVolume1.VolumeInc");
                }

                iService.BeginVolumeInc((IntPtr) =>
                {
                    iThread.Schedule(() =>
                    {
                        if (aAction != null)
                        {
                            aAction();
                        }
                    });
                });
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

        private void HandleVolumeLimitChanged()
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

                iValue.Update(iService.PropertyVolume());
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

        private IWatchableThread iThread;
        private CpProxyAvOpenhomeOrgVolume1 iService;

        private Watchable<int> iBalance;
        private Watchable<int> iFade;
        private Watchable<bool> iMute;
        private Watchable<uint> iValue;
        private Watchable<uint> iVolumeLimit;
        private Watchable<uint> iVolumeMilliDbPerStep;
        private Watchable<uint> iVolumeSteps;
        private Watchable<uint> iVolumeUnity;
    }

    public class MockServiceOpenHomeOrgVolume1 : IServiceOpenHomeOrgVolume1, IMockable, IDisposable
    {
        public MockServiceOpenHomeOrgVolume1(IWatchableThread aThread, string aId, int aBalance, uint aBalanceMax, int aFade, uint aFadeMax, bool aMute, uint aValue, uint aVolumeLimit, uint aVolumeMax,
            uint aVolumeMilliDbPerStep, uint aVolumeSteps, uint aVolumeUnity)
        {
            iThread = aThread;

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

            iBalance = new Watchable<int>(iThread, string.Format("Balance({0})", aId), aBalance);
            iFade = new Watchable<int>(iThread, string.Format("Fade({0})", aId), aFade);
            iMute = new Watchable<bool>(iThread, string.Format("Mute({0})", aId), aMute);
            iValue = new Watchable<uint>(iThread, string.Format("Value({0})", aId), value);
            iVolumeLimit = new Watchable<uint>(iThread, string.Format("VolumeLimit({0})", aId), volumeLimit);
            iVolumeMilliDbPerStep = new Watchable<uint>(iThread, string.Format("VolumeMilliDbPerStep({0})", aId), aVolumeMilliDbPerStep);
            iVolumeSteps = new Watchable<uint>(iThread, string.Format("VolumeSteps({0})", aId), aVolumeSteps);
            iVolumeUnity = new Watchable<uint>(iThread, string.Format("VolumeUnity({0})", aId), aVolumeUnity);
        }

        public void Dispose()
        {
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

        public void SetBalance(int aValue, Action aAction)
        {
            iBalance.Update(aValue);
        }

        public void SetFade(int aValue, Action aAction)
        {
            iFade.Update(aValue);
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void SetMute(bool aValue, Action aAction)
        {
            iMute.Update(aValue);
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void SetVolume(uint aValue, Action aAction)
        {
            uint value = aValue;
            if (value > iCurrentVolumeLimit)
            {
                value = iCurrentVolumeLimit;
            }

            if (value != iCurrentVolume)
            {
                iCurrentVolume = value;
                iValue.Update(aValue);
            }
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void VolumeDec(Action aAction)
        {
            if (iCurrentVolume > 0)
            {
                --iCurrentVolume;
                iValue.Update(iCurrentVolume);
            }
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
        }

        public void VolumeInc(Action aAction)
        {
            if (iCurrentVolume < iCurrentVolumeLimit)
            {
                ++iCurrentVolume;
                iValue.Update(iCurrentVolume);
            }
            iThread.Schedule(() =>
            {
                if (aAction != null)
                {
                    aAction();
                }
            });
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
                VolumeInc(null);
            }
            else if (command == "volumedec")
            {
                VolumeDec(null);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private IWatchableThread iThread;

        private uint iCurrentVolume;
        private uint iCurrentVolumeLimit;

        private Watchable<int> iBalance;
        private Watchable<int> iFade;
        private Watchable<bool> iMute;
        private Watchable<uint> iValue;
        private Watchable<uint> iVolumeLimit;
        private Watchable<uint> iVolumeMilliDbPerStep;
        private Watchable<uint> iVolumeSteps;
        private Watchable<uint> iVolumeUnity;
    }

    public class WatchableVolumeFactory : IWatchableServiceFactory
    {
        public WatchableVolumeFactory(IWatchableThread aThread, IWatchableThread aSubscribeThread)
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
                    iPendingService = new CpProxyAvOpenhomeOrgVolume1(d.Device);
                    iPendingService.SetPropertyInitialEvent(delegate
                    {
                        lock (iLock)
                        {
                            if (iPendingService != null)
                            {
                                iService = new WatchableVolume(iThread, string.Format("Volume({0})", aDevice.Udn), iPendingService);
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

                    if (iService != null)
                    {
                        iService.Dispose();
                        iService = null;
                    }
                }
            });
        }

        private object iLock;
        private bool iDisposed;
        private IWatchableThread iSubscribeThread;
        private CpProxyAvOpenhomeOrgVolume1 iPendingService;
        private WatchableVolume iService;
        private IWatchableThread iThread;
    }

    public class WatchableVolume : Volume
    {
        public WatchableVolume(IWatchableThread aThread, string aId, CpProxyAvOpenhomeOrgVolume1 aService)
        {
            iBalanceMax = aService.PropertyBalanceMax();
            iFadeMax = aService.PropertyFadeMax();
            iVolumeMax = aService.PropertyVolumeMax();

            iService = new ServiceOpenHomeOrgVolume1(aThread, aId, aService);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgVolume1 Service
        {
            get
            {
                return iService;
            }
        }

        private ServiceOpenHomeOrgVolume1 iService;
    }

    public class MockWatchableVolume : Volume, IMockable
    {
        public MockWatchableVolume(IWatchableThread aThread, string aId, int aBalance, uint aBalanceMax, int aFade, uint aFadeMax, bool aMute, uint aVolume, uint aVolumeLimit, uint aVolumeMax,
            uint aVolumeMilliDbPerStep, uint aVolumeSteps, uint aVolumeUnity)
        {
            iBalanceMax = aBalanceMax;
            iFadeMax = aFadeMax;
            iVolumeMax = aVolumeMax;

            iService = new MockServiceOpenHomeOrgVolume1(aThread, aId, aBalance, aBalanceMax, aFade, aFadeMax, aMute, aVolume, aVolumeLimit, aVolumeMax, aVolumeMilliDbPerStep, aVolumeSteps, aVolumeUnity);
        }

        public override void Dispose()
        {
            iService.Dispose();
            iService = null;
        }

        internal override IServiceOpenHomeOrgVolume1 Service
        {
            get
            {
                return iService;
            }
        }

        public void Execute(IEnumerable<string> aValue)
        {
            iService.Execute(aValue);
        }

        private MockServiceOpenHomeOrgVolume1 iService;
    }

    public class ServiceVolume : IVolume, IService
    {
        public ServiceVolume(IManagableWatchableDevice aDevice, IVolume aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        public void Dispose()
        {
            iDevice.Unsubscribe<ServiceVolume>();
            iDevice = null;
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

        public void SetBalance(int aValue, Action aAction)
        {
            iService.SetBalance(aValue, aAction);
        }

        public void SetFade(int aValue, Action aAction)
        {
            iService.SetFade(aValue, aAction);
        }

        public void SetMute(bool aValue, Action aAction)
        {
            iService.SetMute(aValue, aAction);
        }

        public void SetVolume(uint aValue, Action aAction)
        {
            iService.SetVolume(aValue, aAction);
        }

        public void VolumeDec(Action aAction)
        {
            iService.VolumeDec(aAction);
        }

        public void VolumeInc(Action aAction)
        {
            iService.VolumeInc(aAction);
        }

        private IManagableWatchableDevice iDevice;
        private IVolume iService;
    }
}
