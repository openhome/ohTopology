using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenHome.Av
{
    public interface IService : IMockable, IDisposable
    {
        void Create<T>(Action<T> aCallback) where T : IProxy;
    }

    public abstract class Service : IService
    {
        private readonly INetwork iNetwork;
        private readonly IDevice iDevice;
        private uint iRefCount;
        protected Task iSubscribeTask;
        private List<Task> iTasks;

        protected Service(INetwork aNetwork, IDevice aDevice)
        {
            iNetwork = aNetwork;
            iDevice = aDevice;
            iRefCount = 0;
            iSubscribeTask = new Task(() => { });
            iTasks = new List<Task>();
        }

        public INetwork Network
        {
            get
            {
                return (iNetwork);
            }
        }

        public IDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public virtual void Dispose()
        {
            if (iRefCount > 0)
            {
                throw new Exception("Disposing of Service with outstanding subscriptions");
            }
        }

        public void Create<T>(Action<T> aCallback) where T : IProxy
        {
            if (iRefCount == 0)
            {
                iSubscribeTask = OnSubscribe();
            }
            ++iRefCount;

            if (iSubscribeTask != null)
            {
                iSubscribeTask.ContinueWith((t) =>
                {
                    iNetwork.Schedule(() =>
                    {
                        aCallback((T)OnCreate(iDevice));
                    });
                });
            }
            else
            {
                aCallback((T)OnCreate(iDevice));
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

        protected Task Start(Action aAction)
        {
            var task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    aAction();
                });
            });
            
            lock (iTasks)
            {
                iTasks.Add(task);
            }

            return (task);
        }

        protected Task<T> Start<T>(Func<T> aFunction)
        {
            var task = Task.Factory.StartNew<T>(() =>
            {
                T value = default(T);

                iNetwork.Execute(() =>
                {
                    value = aFunction();
                });

                return (value);
            });

            lock (iTasks)
            {
                iTasks.Add(task);
            }

            return (task);
        }

        public bool Wait()
        {
            bool complete = false;
            Task[] tasks = null;

            lock (iTasks)
            {
                tasks = iTasks.ToArray();
                iTasks.Clear();
                complete = (tasks.Length == 0);
            }

            Task.WaitAll(tasks);

            return complete;
        }

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
        protected readonly T iService;

        protected Proxy(T aService)
        {
            iService = aService;
        }

        // IProxy

        public IDevice Device
        {
            get
            {
                return (iService.Device);
            }
        }

        // IDisposable

        public void Dispose()
        {
            iService.Unsubscribe();
        }
    }
}
