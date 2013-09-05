using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IService : IMockable, IDisposable
    {
        void Create<T>(Action<T> aCallback) where T : IProxy;
    }

    public abstract class Service : IService, IWatchableThread
    {
        protected readonly INetwork iNetwork;
        private readonly IDevice iDevice;

        private readonly DisposeHandler iDisposeHandler;
        private readonly CancellationTokenSource iCancelSubscribe;
        private readonly ManualResetEvent iSubscribed;
        private readonly List<Task> iTasks;
        private uint iRefCount;

        protected Task iSubscribeTask;

        protected Service(INetwork aNetwork, IDevice aDevice)
        {
            iNetwork = aNetwork;
            iDevice = aDevice;

            iDisposeHandler = new DisposeHandler();
            iCancelSubscribe = new CancellationTokenSource();
            iSubscribed = new ManualResetEvent(true);
            iRefCount = 0;
            iSubscribeTask = null;
            iTasks = new List<Task>();
        }

        public virtual void Dispose()
        {
            iDisposeHandler.Dispose();

            iNetwork.Execute(() =>
            {
                lock (iCancelSubscribe)
                {
                    iCancelSubscribe.Cancel();
                    OnCancelSubscribe();
                }

                // wait for any inflight subscriptions to complete
                if (iSubscribeTask != null)
                {
                    iSubscribeTask.Wait();
                }

                iSubscribed.WaitOne();

                OnUnsubscribe();
            });

            iCancelSubscribe.Dispose();

            if (iRefCount > 0)
            {
                throw new Exception("Disposing of Service with outstanding subscriptions");
            }
        }

        public INetwork Network
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return (iNetwork);
                }
            }
        }

        public IDevice Device
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iDevice;
                }
            }
        }

        public void Create<T>(Action<T> aCallback) where T : IProxy
        {
            using (iDisposeHandler.Lock)
            {
                if (iRefCount == 0)
                {
                    iSubscribeTask = OnSubscribe();
                }
                ++iRefCount;

                if (iSubscribeTask != null)
                {
                    iSubscribed.Reset();

                    iSubscribeTask.ContinueWith((t) =>
                    {
                        bool cancel = false;
                        lock (iCancelSubscribe)
                        {
                            cancel = iCancelSubscribe.IsCancellationRequested;
                        }

                        if (!cancel && !t.IsFaulted)
                        {
                            iNetwork.Schedule(() =>
                            {
                                aCallback((T)OnCreate(iDevice));
                            });
                        }
                        else
                        {
                            Unsubscribe();
                        }

                        iSubscribed.Set();
                    });
                }
                else
                {
                    aCallback((T)OnCreate(iDevice));
                }
            }
        }

        public abstract IProxy OnCreate(IDevice aDevice);

        protected virtual Task OnSubscribe()
        {
            return null;
        }

        protected virtual void OnCancelSubscribe() { }

        public void Unsubscribe()
        {
            if (iRefCount == 0)
            {
                throw new Exception("Service not subscribed");
            }
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

        // IWatchableThread

        public void Assert()
        {
            iNetwork.Assert();
        }

        public void Schedule(Action aAction)
        {
            iNetwork.Schedule(aAction);
        }

        public void Execute(Action aAction)
        {
            iNetwork.Execute(aAction);
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
