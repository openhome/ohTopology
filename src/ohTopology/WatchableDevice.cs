using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IWatchableService : IDisposable
    {
        IService Create(IManagableWatchableDevice aDevice);
    }

    public interface IWatchableServiceFactory : IDisposable
    {
        void Subscribe(IWatchableDevice aDevice, Action<IWatchableService> aCallback);
        void Unsubscribe();
    }

    public interface IService : IDisposable
    {
        IWatchableDevice Device { get; }
    }

    public interface IWatchableDevice
    {
        string Udn { get; }
        bool GetAttribute(string aKey, out string aValue);
        void Create<T>(Action<IWatchableDevice, T> aAction) where T : IService;
    }

    public interface IManagableWatchableDevice : IWatchableDevice
    {
        void Unsubscribe<T>() where T : IService;
    }

    public class WatchableDeviceUnordered : WatchableUnordered<IWatchableDevice>
    {
        public WatchableDeviceUnordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<IWatchableDevice>();
        }

        internal new void Add(IWatchableDevice aValue)
        {
            iList.Add(aValue);

            base.Add(aValue);
        }

        internal new void Remove(IWatchableDevice aValue)
        {
            iList.Remove(aValue);

            base.Remove(aValue);
        }

        private List<IWatchableDevice> iList;
    }

    public abstract class WatchableDevice : IManagableWatchableDevice
    {
        public abstract string Udn { get; }
        public abstract CpDevice Device { get; }
        public abstract bool GetAttribute(string aKey, out string aValue);

        public abstract void Create<T>(Action<IWatchableDevice, T> aAction) where T : IService;
        public abstract void Unsubscribe<T>() where T : IService;
    }

    public class DisposableWatchableDevice : WatchableDevice, IDisposable
    {
        public DisposableWatchableDevice(IWatchableThread aThread, IWatchableThread aSubscribeThread, CpDevice aDevice)
        {
            iLock = new object();
            iDisposed = false;
            iThread = aThread;

            iFactories = new Dictionary<Type, IWatchableServiceFactory>();

            // add a factory for each type of watchable service

            iFactories.Add(typeof(ServiceProduct), new WatchableProductFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceVolume), new WatchableVolumeFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceInfo), new WatchableInfoFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceTime), new WatchableTimeFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceSender), new WatchableSenderFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServicePlaylist), new WatchablePlaylistFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceRadio), new WatchableRadioFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceReceiver), new WatchableReceiverFactory(aThread, aSubscribeThread));
            //iFactories.Add(typeof(ServiceContentDirectory), new WatchableContentDirectoryFactory(aThread, aSubscribeThread));

            iServices = new Dictionary<Type, IWatchableService>();
            iServiceRefCount = new Dictionary<Type, uint>();

            iDevice = aDevice;
            iDevice.AddRef();
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("DisposableWatchableDevice.Dispose");
                }

                foreach (IWatchableService s in iServices.Values)
                {
                    s.Dispose();
                }
                iServices.Clear();
                iServices = null;

                iServiceRefCount.Clear();
                iServiceRefCount = null;

                foreach (IWatchableServiceFactory f in iFactories.Values)
                {
                    f.Dispose();
                }
                iFactories.Clear();
                iFactories = null;

                iDevice.RemoveRef();
                iDevice = null;

                iDisposed = true;
            }
        }

        public override string Udn
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("WatchableDevice.Udn");
                    }

                    return iDevice.Udn();
                }
            }
        }

        public override bool GetAttribute(string aKey, out string aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("WatchableDevice.GetAttribute");
                }

                return iDevice.GetAttribute(aKey, out aValue);
            }
        }

        public override void Create<T>(Action<IWatchableDevice, T> aAction)
        {
            lock (iLock)
            {
                IWatchableService service = GetService(typeof(T));
                if (service == null)
                {
                    IWatchableServiceFactory factory = iFactories[typeof(T)];
                    factory.Subscribe(this, (IWatchableService aService) =>
                    {
                        lock (iLock)
                        {
                            iServices.Add(typeof(T), aService);
                            iServiceRefCount.Add(typeof(T), 1);
                        }

                        iThread.Schedule(() =>
                        {
                            aAction(this, (T)aService.Create(this));
                        });
                    });
                }
                else
                {
                    ++iServiceRefCount[typeof(T)];

                    iThread.Schedule(() =>
                    {
                        aAction(this, (T)service.Create(this));
                    });
                }
            }
        }

        private IWatchableService GetService(Type aType)
        {
            IWatchableService service;
            if (iServices.TryGetValue(aType, out service))
            {
                ++iServiceRefCount[aType];

                return service;
            }

            return null;
        }

        public override void Unsubscribe<T>()
        {
            lock (iLock)
            {
                IWatchableService service;
                if (iServices.TryGetValue(typeof(T), out service))
                {
                    --iServiceRefCount[typeof(T)];

                    if (iServiceRefCount[typeof(T)] == 0)
                    {
                        service.Dispose();

                        iServices.Remove(typeof(T));
                        iServiceRefCount.Remove(typeof(T));
                    }
                }
                else
                {
                    // service could be pending subscribe
                    IWatchableServiceFactory factory = iFactories[typeof(T)];
                    factory.Unsubscribe();
                }
            }
        }

        public override CpDevice Device
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("WatchableDevice.Device");
                    }

                    return iDevice;
                }
            }
        }

        protected object iLock;
        protected bool iDisposed;

        private IWatchableThread iThread;

        protected CpDevice iDevice;

        private Dictionary<Type, IWatchableServiceFactory> iFactories;

        private Dictionary<Type, IWatchableService> iServices;
        private Dictionary<Type, uint> iServiceRefCount;
    }

    public class MockWatchableDevice : IManagableWatchableDevice, IMockable, IDisposable
    {
        public MockWatchableDevice(IWatchableThread aThread, IWatchableThread aSubscribeThread, string aUdn)
        {
            iThread = aThread;
            iUdn = aUdn;

            iSubscribeThread = aSubscribeThread;
            iServices = new Dictionary<Type, IWatchableService>();
        }

        public void Dispose()
        {
            foreach (IWatchableService s in iServices.Values)
            {
                s.Dispose();
            }
            iServices.Clear();
            iServices = null;
        }

        public string Udn
        {
            get
            {
                return iUdn;
            }
        }

        public bool GetAttribute(string aKey, out string aValue)
        {
            throw new NotImplementedException();
        }

        public void Create<T>(Action<IWatchableDevice, T> aAction) where T : IService
        {
            iSubscribeThread.Schedule(() =>
            {
                IWatchableService service;
                if (iServices.TryGetValue(typeof(T), out service))
                {
                    iThread.Schedule(() =>
                    {
                        aAction(this, (T)service.Create(this));
                    });
                }
                else
                {
                    throw new NotSupportedException();
                }
            });
        }

        public void Unsubscribe<T>() where T : IService
        {
        }

        public void Add<T>(IWatchableService aService) where T : IService
        {
            iServices.Add(typeof(T), aService);
        }

        internal bool HasService(Type aType)
        {
            return (iServices.ContainsKey(aType));
        }

        public virtual void Execute(IEnumerable<string> aValue)
        {
        }

        private string iUdn;
        private IWatchableThread iThread;
        private IWatchableThread iSubscribeThread; 

        protected Dictionary<Type, IWatchableService> iServices;
    }
}