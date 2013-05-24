using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IService : IMockable, IDisposable
    {
        void Create<T>(IDevice aDevice, Action<T> aCallback) where T : IProxy;
    }

    public abstract class Service : IService
    {
        private readonly INetwork iNetwork;
        private uint iRefCount;
        protected Task iSubscribeTask;

        protected Service(INetwork aNetwork)
        {
            iNetwork = aNetwork;
            iRefCount = 0;
            iSubscribeTask = new Task(() => { });
        }

        public INetwork Network
        {
            get
            {
                return (iNetwork);
            }
        }

        public virtual void Dispose()
        {
            if (iRefCount > 0)
            {
                throw new Exception("Disposing of Service with outstanding subscriptions");
            }
        }

        public void Create<T>(IDevice aDevice, Action<T> aCallback) where T : IProxy
        {
            if (iRefCount == 0)
            {
                iSubscribeTask = OnSubscribe();
            }
            ++iRefCount;

            if (iSubscribeTask != null)
            {
                Task.Factory.StartNew(() =>
                {
                    iSubscribeTask.Wait();
                    Network.Schedule(() =>
                    {
                        aCallback((T)OnCreate(aDevice));
                    });
                });
            }
            else
            {
                aCallback((T)OnCreate(aDevice));
            }
        }

        public abstract IProxy OnCreate(IDevice aDevice);

        protected virtual Task OnSubscribe()
        {
            return null;
        }

        public void Unsubscribe()
        {
            --iRefCount;
            if (iRefCount == 0)
            {
                OnUnsubscribe();
            }
        }

        protected virtual void OnUnsubscribe() { }

        // IMockable

        public virtual void Execute(IEnumerable<string> aCommand)
        {
        }
    }

    public interface IProxy : IDisposable
    {
        IDevice Device { get; }
    }

    public class Proxy<T> where T : Service
    {
        private readonly IDevice iDevice;
        protected readonly T iService;

        protected Proxy(IDevice aDevice, T aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        // IProxy

        public IDevice Device
        {
            get
            {
                return (iDevice);
            }
        }

        // IDisposable

        public void Dispose()
        {
            iService.Unsubscribe();
        }
    }

    public interface IDevice
    {
        string Udn { get; }
        void Create<T>(Action<T> aCallback) where T : IProxy;
    }

    public class Device : IDevice, IMockable, IDisposable
    {
        public static Device Create(INetwork aNetwork, CpDevice aDevice)
        {
            Device device = new Device(aDevice.Udn());
            string value;
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Product", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyProduct>(new ServiceProductNetwork(aNetwork, aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Info", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyInfo>(new ServiceInfoNetwork(aNetwork, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Time", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyTime>(new ServiceTimeNetwork(aNetwork, aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Sender", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxySender>(new ServiceSenderNetwork(aNetwork, aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Volume", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyVolume>(new ServiceVolumeNetwork(aNetwork, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Playlist", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyPlaylist>(new ServicePlaylistNetwork(aNetwork, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Radio", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyRadio>(new ServiceRadioNetwork(aNetwork, aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Receiver", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyReceiver>(new ServiceReceiverNetwork(aNetwork, aDevice));
                }
            }
            return device;
        }

        public Device(string aUdn)
        {
            iUdn = aUdn;

            iDisposed = false;
            iServices = new Dictionary<Type, IService>();
        }

        public virtual void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("WatchableDevice.Dispose");
            }

            foreach (IService s in iServices.Values)
            {
                s.Dispose();
            }
            iServices.Clear();
            iServices = null;

            iDisposed = true;
        }

        public string Udn
        {
            get
            {
                return iUdn;
            }
        }

        public void Add<T>(IService aService) where T : IProxy
        {
            iServices.Add(typeof(T), aService);
        }

        public bool HasService(Type aServiceType)
        {
            return iServices.ContainsKey(aServiceType);
        }

        public void Create<T>(Action<T> aCallback) where T : IProxy
        {
            iServices[typeof(T)].Create<T>(this, aCallback);
        }

        private IService GetService(string aType)
        {
            if (aType == "product")
            {
                return iServices[typeof(IProxyProduct)];
            }
            else if (aType == "info")
            {
                return iServices[typeof(IProxyInfo)];
            }
            else if (aType == "time")
            {
                return iServices[typeof(IProxyTime)];
            }
            else if (aType == "sender")
            {
                return iServices[typeof(IProxySender)];
            }
            else if (aType == "volume")
            {
                return iServices[typeof(IProxyVolume)];
            }
            else if (aType == "playlist")
            {
                return iServices[typeof(IProxyPlaylist)];
            }
            else if (aType == "radio")
            {
                return iServices[typeof(IProxyRadio)];
            }
            else if (aType == "receiver")
            {
                return iServices[typeof(IProxyReceiver)];
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        // IMockable

        public void Execute(IEnumerable<string> aValue)
        {
            GetService(aValue.First()).Execute(aValue.Skip(1));
        }

        private string iUdn;
        private bool iDisposed;

        protected Dictionary<Type, IService> iServices;
    }
}