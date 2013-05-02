using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IWatchableService : IDisposable
    {
    }

    public interface IWatchableServiceFactory
    {
        void Subscribe(WatchableDevice aDevice, Action<IWatchableService> aCallback);
        void Unsubscribe();
    }

    public interface IWatchableDevice
    {
        string Udn { get; }
        bool GetAttribute(string aKey, out string aValue);
        void Subscribe<T>(Action<IWatchableDevice, T> aCallback) where T : IWatchableService;
        void Unsubscribe<T>() where T : IWatchableService;
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

    public class WatchableDevice : IWatchableDevice
    {
        public WatchableDevice(IWatchableThread aThread, CpDevice aDevice)
        {
            iLock = new object();
            iDisposed = false;
            iThread = aThread;

            iFactories = new Dictionary<Type, IWatchableServiceFactory>();

            // add a factory for each type of watchable service

            iFactories.Add(typeof(Product), new WatchableProductFactory(aThread));
            iFactories.Add(typeof(Volume), new WatchableVolumeFactory(aThread));
            iFactories.Add(typeof(Info), new WatchableInfoFactory(aThread));
            iFactories.Add(typeof(Time), new WatchableTimeFactory(aThread));
            //iFactories.Add(typeof(ContentDirectory), new WatchableContentDirectoryFactory(aThread));

            iServices = new Dictionary<Type, IWatchableService>();
            iServiceRefCount = new Dictionary<Type, uint>();

            iDevice = aDevice;
            iDevice.AddRef();
        }

        public string Udn
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

        public bool GetAttribute(string aKey, out string aValue)
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

        public void Subscribe<T>(Action<IWatchableDevice, T> aCallback) where T : IWatchableService
        {
            lock (iLock)
            {
                IWatchableService service;
                if (iServices.TryGetValue(typeof(T), out service))
                {
                    ++iServiceRefCount[typeof(T)];

                    iThread.Schedule(() =>
                    {
                        aCallback(this, (T)service);
                    });
                }
                else
                {
                    IWatchableServiceFactory factory = iFactories[typeof(T)];
                    factory.Subscribe(this, delegate(IWatchableService aService)
                    {
                        lock (iLock)
                        {
                            iServices.Add(typeof(T), aService);
                            iServiceRefCount.Add(typeof(T), 1);
                        }

                        iThread.Schedule(() =>
                        {
                            aCallback(this, (T)aService);
                        });
                    });
                }
            }
        }

        public void Unsubscribe<T>() where T : IWatchableService
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

        public CpDevice Device
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

    public class DisposableWatchableDevice : WatchableDevice, IDisposable
    {
        public DisposableWatchableDevice(IWatchableThread aThread, CpDevice aDevice)
            : base(aThread, aDevice)
        {
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("DisposableWatchableDevice.Dispose");
                }

                iDevice.RemoveRef();
                iDevice = null;

                iDisposed = true;
            }
        }
    }

    public class MockWatchableDevice : IWatchableDevice, IMockable, IDisposable
    {
        public MockWatchableDevice(IWatchableThread aThread, string aUdn)
        {
            iThread = aThread;
            iUdn = aUdn;
            iServices = new Dictionary<Type, IWatchableService>();
        }

        public void Dispose()
        {
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

        public void Subscribe<T>(Action<IWatchableDevice, T> aCallback) where T : IWatchableService
        {
            IWatchableService service;

            if (iServices.TryGetValue(typeof(T), out service))
            {
                iThread.Schedule(() =>
                {
                    aCallback(this, (T)service);
                });

                return;
            }

            throw new NotSupportedException();
        }

        public void Unsubscribe<T>() where T : IWatchableService
        {
        }

        public void Add<T>(IWatchableService aService) where T : IWatchableService
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

        protected Dictionary<Type, IWatchableService> iServices;

        private IWatchableThread iThread;
        private string iUdn;
    }
}