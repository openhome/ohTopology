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
        void Create<T>(Action<T> aCallback, IDevice aDevice) where T : IProxy;
    }

    public abstract class Service : IService, IWatchableThread
    {
        protected readonly INetwork iNetwork;
        private readonly IInjectorDevice iDevice;
        protected readonly ILog iLog;

        private readonly CancellationTokenSource iCancelSubscribe;
        private readonly List<Task> iTasks;
        private uint iRefCount;

        protected readonly DisposeHandler iDisposeHandler;
        protected Task iSubscribeTask;

        protected Service(INetwork aNetwork, IInjectorDevice aDevice, ILog aLog)
        {
            iNetwork = aNetwork;
            iDevice = aDevice;
            iLog = aLog;

            iDisposeHandler = new DisposeHandler();
            iCancelSubscribe = new CancellationTokenSource();
            iRefCount = 0;
            iSubscribeTask = null;
            iTasks = new List<Task>();
        }

        public virtual void Dispose()
        {
            Assert();

            iDisposeHandler.Dispose();
            
            iCancelSubscribe.Cancel();
            OnCancelSubscribe();

            // wait for any inflight subscriptions to complete

            if (iSubscribeTask != null)
            {
                try
                {
                    iSubscribeTask.Wait();
                }
                catch (AggregateException e)
                {
                    HandleAggregate(e);
                }
            }

            OnUnsubscribe();

            iCancelSubscribe.Dispose();

            iNetwork.Schedule(() =>
            {
                Do.Assert(iRefCount == 0);
            });
        }

        private void HandleAggregate(AggregateException aException)
        {
            aException.Handle((x) =>
            {
                if (x is ProxyError)
                {
                    return true;
                }
                if (x is TaskCanceledException)
                {
                    return true;
                }
                if (x is AggregateException)
                {
                    // will throw if aggregate contains an unhandled case
                    HandleAggregate(x as AggregateException);
                    return true;
                }
                return false;
            });
        }

        public IInjectorDevice Device
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iDevice;
                }
            }
        }

        public void Create<T>(Action<T> aCallback, IDevice aDevice) where T : IProxy
        {
            Assert();

            using (iDisposeHandler.Lock())
            {
                if (iRefCount == 0)
                {
                    Do.Assert(iSubscribeTask == null);
                    iSubscribeTask = OnSubscribe();
                }
                ++iRefCount;

                if (iSubscribeTask != null)
                {
                    iSubscribeTask = iSubscribeTask.ContinueWith((t) =>
                    {
                        iNetwork.Schedule(() =>
                        {
                            // we must access t.Exception property to supress finalized task exceptions
                            if (t.Exception == null && !iCancelSubscribe.IsCancellationRequested)
                            {
                                aCallback((T)OnCreate(aDevice));
                            }
                            else
                            {
                                --iRefCount;
                                if (iRefCount == 0)
                                {
                                    iSubscribeTask = null;
                                }
                            }
                        });
                    });
                }
                else
                {
                    aCallback((T)OnCreate(aDevice));
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
            Do.Assert(iRefCount != 0);
            --iRefCount;
            if (iRefCount == 0)
            {
                OnUnsubscribe();
                if (iSubscribeTask != null)
                {
                    try
                    {
                        iSubscribeTask.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        HandleAggregate(ex);
                    }
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
            Task[] tasks;

            lock (iTasks)
            {
                tasks = iTasks.ToArray();
                iTasks.Clear();
            }

            Task.WaitAll(tasks);

            return (tasks.Length == 0);
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
        private readonly IDevice iDevice;

        protected Proxy(T aService, IDevice aDevice)
        {
            iService = aService;
            iDevice = aDevice;
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
}
