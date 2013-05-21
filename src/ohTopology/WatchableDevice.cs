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
        Task<T> Create<T>(IWatchableDevice aDevice) where T : IProxy;
        void Subscribe();
        void Unsubscribe();
    }

    public abstract class Service : IService
    {
        private readonly INetwork iNetwork;

        private uint iRefCount;

        protected Service(INetwork aNetwork)
        {
            iNetwork = aNetwork;
            iRefCount = 0;
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

        public Task<T> Create<T>(IWatchableDevice aDevice) where T : IProxy
        {
            Task<T> task = Task.Factory.StartNew<T>(() =>
            {
                Subscribe();
                return (T)OnCreate(aDevice);
            });
            return task;
        }

        public abstract IProxy OnCreate(IWatchableDevice aDevice);

        public void Subscribe()
        {
            if (iRefCount == 0)
            {
                OnSubscribe();
            }
            ++iRefCount;
        }

        protected abstract void OnSubscribe();

        public void Unsubscribe()
        {
            --iRefCount;
            if (iRefCount == 0)
            {
                OnUnsubscribe();
            }
        }

        protected abstract void OnUnsubscribe();

        // IMockable

        public virtual void Execute(IEnumerable<string> aCommand)
        {
        }
    }

    public interface IProxy : IDisposable
    {
        IWatchableDevice Device { get; }
    }

    public class Proxy<T> where T : Service
    {
        private readonly IWatchableDevice iDevice;
        protected readonly T iService;

        protected Proxy(IWatchableDevice aDevice, T aService)
        {
            iDevice = aDevice;
            iService = aService;
        }

        // IProxy

        public IWatchableDevice Device
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

    public interface IWatchableDevice
    {
        string Udn { get; }
        Task<T> Create<T>() where T : IProxy;
    }

    public class WatchableDevice : IWatchableDevice, IMockable, IDisposable
    {
        public static WatchableDevice Create(INetwork aNetwork, CpDevice aDevice)
        {
            WatchableDevice device = new WatchableDevice(aDevice.Udn());
            string value;
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Product", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyProduct>(new ServiceProductNetwork(aNetwork, aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Info", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyInfo>(new ServiceInfoNetwork(aNetwork, aDevice));
                }
            }
            /*if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Time", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyTime>(new ServiceTime(aNetwork, string.Format("ServiceTime{0}", aDevice.Udn()), aDevice));
                }
            }*/
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Sender", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxySender>(new ServiceSenderNetwork(aNetwork, aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Volume", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyVolume>(new ServiceVolumeNetwork(aNetwork, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Playlist", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyPlaylist>(new ServicePlaylistNetwork(aNetwork, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Radio", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyRadio>(new ServiceRadioNetwork(aNetwork, aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Receiver", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyReceiver>(new ServiceReceiverNetwork(aNetwork, aDevice));
                }
            }
            return device;
        }

        public WatchableDevice(string aUdn)
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

        public Task<T> Create<T>() where T : IProxy
        {
            return iServices[typeof(T)].Create<T>(this);
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

    /*internal class RealWatchableDevice : WatchableDevice
    {
        public RealWatchableDevice(INetwork aNetwork, CpDevice aDevice)
            : base(aNetwork.WatchableSubscribeThread)
        {
            /*iFactories = new Dictionary<Type, IWatchableServiceFactory>();

            // add a factory for each type of watchable service

            string value;
            if (iDevice.GetAttribute("Upnp.Service.av-openhome-org.Product", out value))
            {
                iFactories.Add(typeof(ServiceProduct), new WatchableProductFactory(aThread, aSubscribeThread));
            }
            iFactories.Add(typeof(ServiceVolume), new WatchableVolumeFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceInfo), new WatchableInfoFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceTime), new WatchableTimeFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceSender), new WatchableSenderFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServicePlaylist), new WatchablePlaylistFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceRadio), new WatchableRadioFactory(aThread, aSubscribeThread));
            iFactories.Add(typeof(ServiceReceiver), new WatchableReceiverFactory(aThread, aSubscribeThread));
            //iFactories.Add(typeof(ServiceMediaServer), new WatchableMediaServerFactory(aThread, aSubscribeThread));

            iServices = new Dictionary<Type, IWatchableService>();
            iServiceRefCount = new Dictionary<Type, uint>();

            iDevice = aDevice;
            iDevice.AddRef();
        }
    }*/
}