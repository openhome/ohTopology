using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;

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
        private object iRefCountLock;

        protected Task iSubscribeTask;

        protected Service(INetwork aNetwork, IDevice aDevice)
        {
            iNetwork = aNetwork;
            iDevice = aDevice;

            iDisposeHandler = new DisposeHandler();
            iCancelSubscribe = new CancellationTokenSource();
            iSubscribed = new ManualResetEvent(true);
            iRefCount = 0;
            iRefCountLock = new object();
            iSubscribeTask = null;
            iTasks = new List<Task>();
        }

        public virtual void Dispose()
        {
            iNetwork.Assert();

            iDisposeHandler.Dispose();

            lock (iCancelSubscribe)
            {
                iCancelSubscribe.Cancel();
                OnCancelSubscribe();
            }

            // wait for any inflight subscriptions to complete
            if (iSubscribeTask != null)
            {
                try
                {
                    iSubscribeTask.Wait();
                }
                catch (AggregateException e)
                {
                    e.Handle((x) =>
                    {
                        if (x is ProxyError)
                        {
                            return true;
                        }

                        return false;
                    });
                }
            }

            iSubscribed.WaitOne();

            OnUnsubscribe();

            iCancelSubscribe.Dispose();

            iNetwork.Schedule(() =>
            {
                Do.Assert(iRefCount == 0);
            });
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
                lock (iRefCountLock)
                {
                    if (iRefCount == 0)
                    {
                        Do.Assert(iSubscribeTask == null);
                        iSubscribeTask = OnSubscribe();
                    }
                    ++iRefCount;
                }

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
                            lock (iRefCountLock)
                            {
                                --iRefCount;
                                if (iRefCount == 0)
                                {
                                    iSubscribeTask = null;
                                }
                            }
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
            lock (iRefCountLock)
            {
                if (iRefCount == 0)
                {
                    throw new Exception("Service not subscribed");
                }
                --iRefCount;
                if (iRefCount == 0)
                {
                    OnUnsubscribe();
                    iSubscribeTask = null;
                }
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
