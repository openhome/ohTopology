using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IDevice
    {
        string Udn { get; }
        void Create<T>(Action<T> aCallback) where T : IProxy;
    }

    public class Device : IDevice, IMockable, IDisposable
    {
        public Device(string aUdn)
        {
            iUdn = aUdn;

            iDisposed = false;
            iServices = new Dictionary<Type, Service>();
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

        public void Add<T>(Service aService) where T : IProxy
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

        public bool Wait()
        {
            bool complete = false;

            foreach (Service s in iServices.Values)
            {
                complete |= s.Wait();
            }

            return complete;
        }

        // IMockable

        public void Execute(IEnumerable<string> aValue)
        {
            GetService(aValue.First()).Execute(aValue.Skip(1));
        }

        private string iUdn;
        private bool iDisposed;

        protected Dictionary<Type, Service> iServices;
    }
}