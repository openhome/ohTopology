using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IDevice : IJoinable
    {
        string Udn { get; }
        void Create<T>(Action<T> aCallback) where T : IProxy;
    }

    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException()
        {
        }

        public ServiceNotFoundException(string aMessage)
            : base(aMessage)
        {
        }

        public ServiceNotFoundException(string aMessage, Exception aInnerException)
            : base(aMessage, aInnerException)
        {
        }
    }

    public class Device : IDevice, IMockable, IDisposable
    {
        public Device(string aUdn)
        {
            iUdn = aUdn;
            iDisposed = false;

            iJoiners = new List<Action>();
            iServices = new Dictionary<Type, Service>();
        }

        public virtual void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Device.Dispose");
            }

            List<Action> linked = new List<Action>(iJoiners);
            foreach (Action a in linked)
            {
                a();
            }
            if (iJoiners.Count > 0)
            {
                throw new Exception("Device joiners > 0");
            }
            iJoiners = null;

            foreach (IService s in iServices.Values)
            {
                s.Dispose();
            }
            iServices.Clear();
            iServices = null;

            iDisposed = true;
        }

        public void Join(Action aAction)
        {
            iJoiners.Add(aAction);
        }

        public void UnJoin(Action aAction)
        {
            iJoiners.Remove(aAction);
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
            try
            {
                iServices[typeof(T)].Create<T>(aCallback);
            }
            catch (KeyNotFoundException)
            {
                throw new ServiceNotFoundException("Cannot find service of type " + typeof(T) + " on " + iUdn);
            }
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
                throw new ServiceNotFoundException();
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
        private List<Action> iJoiners;

        protected Dictionary<Type, Service> iServices;
    }
}