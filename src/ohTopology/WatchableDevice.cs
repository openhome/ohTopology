using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IService : IDisposable
    {
        Task<T> Create<T>(IWatchableDevice aDevice) where T : IProxy;
        void Subscribe();
        void Unsubscribe();
    }

    public abstract class Service : IService
    {
        protected Service()
        {
            iRefCount = 0;
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
            Task<T> task = Task.Factory.StartNew(() =>
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

        private uint iRefCount;
    }

    // The following will be harmonised with IServiceFactory

    public interface IProxy : IDisposable
    {
        IWatchableDevice Device { get; }
    }

    public interface IWatchableDevice
    {
        string Udn { get; }
        Task<T> Create<T>() where T : IProxy;
    }

    public class WatchableDevice : IWatchableDevice, IDisposable
    {
        public static WatchableDevice Create(INetwork aNetwork, CpDevice aDevice)
        {
            WatchableDevice device = new WatchableDevice(aDevice.Udn());
            string value;
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Product", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyProduct>(new ServiceProductNetwork(aNetwork, string.Format("ServiceProduct({0})", aDevice.Udn()), aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Info", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyInfo>(new ServiceInfoNetwork(aNetwork, string.Format("ServiceInfo{0}", aDevice.Udn()), aDevice));
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
                    device.Add<ProxySender>(new ServiceSenderNetwork(aNetwork, string.Format("ServiceSender{0}", aDevice.Udn()), aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Volume", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyVolume>(new ServiceVolumeNetwork(aNetwork, string.Format("ServiceVolume{0}", aDevice.Udn()), aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Playlist", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyPlaylist>(new ServicePlaylistNetwork(aNetwork, string.Format("ServicePlaylist{0}", aDevice.Udn()), aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Radio", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyRadio>(new ServiceRadioNetwork(aNetwork, string.Format("ServiceRadio{0}", aDevice.Udn()), aDevice));
                }
            }
            if(aDevice.GetAttribute("Upnp.Service.av-openhome-org.Receiver", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<ProxyReceiver>(new ServiceReceiverNetwork(aNetwork, string.Format("ServiceReceiver{0}", aDevice.Udn()), aDevice));
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