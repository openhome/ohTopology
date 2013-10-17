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

    public interface IInjectorDevice : IJoinable, IMockable, IDisposable
    {
        string Udn { get; }
        void Create<T>(Action<T> aCallback, IDevice aDevice) where T : IProxy;
        bool HasService(Type aServiceType);
        bool Wait();
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

    public class InjectorDevice : IInjectorDevice
    {
        private string iUdn;

        private readonly DisposeHandler iDisposeHandler;
        private readonly List<Action> iJoiners;
        protected readonly Dictionary<Type, Service> iServices;

        public InjectorDevice(string aUdn)
        {
            iUdn = aUdn;

            iDisposeHandler = new DisposeHandler();
            iJoiners = new List<Action>();
            iServices = new Dictionary<Type, Service>();
        }

        public void Join(Action aAction)
        {
            using (iDisposeHandler.Lock())
            {
                iJoiners.Add(aAction);
            }
        }

        public void Unjoin(Action aAction)
        {
            using (iDisposeHandler.Lock())
            {
                iJoiners.Remove(aAction);
            }
        }

        public string Udn
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iUdn;
                }
            }
        }

        public void Add<T>(Service aService) where T : IProxy
        {
            using (iDisposeHandler.Lock())
            {
                iServices.Add(typeof(T), aService);
            }
        }

        public bool HasService(Type aServiceType)
        {
            using (iDisposeHandler.Lock())
            {
                return iServices.ContainsKey(aServiceType);
            }
        }

        public void Create<T>(Action<T> aCallback, IDevice aDevice) where T : IProxy
        {
            using (iDisposeHandler.Lock())
            {
                if (!iServices.ContainsKey(typeof(T)))
                {
                    throw new ServiceNotFoundException("Cannot find service of type " + typeof(T) + " on " + iUdn);
                }

                iServices[typeof(T)].Create<T>(aCallback, aDevice);
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
            bool complete = true;

            foreach (Service service in iServices.Values)
            {
                complete &= service.Wait();
            }

            return complete;
        }

        // IMockable

        public void Execute(IEnumerable<string> aValue)
        {
            GetService(aValue.First()).Execute(aValue.Skip(1));
        }

        // IDisposable

        public virtual void Dispose()
        {
            iDisposeHandler.Dispose();

            foreach (Action action in iJoiners)
            {
                action();
            }
            
            foreach (IService s in iServices.Values)
            {
                s.Dispose();
            }
        }
    }
}