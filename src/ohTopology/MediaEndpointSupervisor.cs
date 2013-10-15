using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text;

using System.Net;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public interface IMediaEndpointClientSnapshot
    {
        uint Total { get; }
        IEnumerable<uint> Alpha { get; } // null if no alpha map
    }

    public interface IMediaEndpointClient : IWatchableThread
    {
        Task<string> Create(CancellationToken aCancellationToken);
        Task<string> Destroy(CancellationToken aCancellationToken, string aId);
        Task<IMediaEndpointClientSnapshot> Browse(CancellationToken aCancellationToken, string aSession, IMediaDatum aDatum);
        Task<IMediaEndpointClientSnapshot> List(CancellationToken aCancellationToken, string aSession, ITag aTag);
        Task<IMediaEndpointClientSnapshot> Link(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue);
        Task<IMediaEndpointClientSnapshot> Match(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue);
        Task<IMediaEndpointClientSnapshot> Search(CancellationToken aCancellationToken, string aSession, string aValue);
        Task<IEnumerable<IMediaDatum>> Read(CancellationToken aCancellationToken, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount);
    }

    internal class CancellationTokenLink : IDisposable
    {
        private readonly CancellationTokenSource iSource = new CancellationTokenSource();
        private readonly List<CancellationTokenRegistration> iRegistrations;

        public CancellationTokenLink(params CancellationToken[] aTokens)
        {
            iSource = new CancellationTokenSource();

            iRegistrations = new List<CancellationTokenRegistration>();

            foreach (var token in aTokens)
            {
                iRegistrations.Add(token.Register(() => iSource.Cancel()));
            }
        }

        public CancellationToken Token
        {
            get
            {
                return (iSource.Token);
            }
        }

        // IDisposable

        public void Dispose()
        {
            foreach (var registration in iRegistrations)
            {
                registration.Dispose();
            }

            iSource.Dispose();
        }
    }

    internal class MediaEndpointSupervisorSnapshot : IWatchableSnapshot<IMediaDatum>, IDisposable
    {
        private readonly IMediaEndpointClient iClient;
        private readonly MediaEndpointSupervisorSession iSession;
        private readonly IMediaEndpointClientSnapshot iSnapshot;

        private readonly DisposeHandler iDisposeHandler;
        private readonly CancellationTokenSource iCancellationTokenSource;

        private readonly List<Task> iTasks;

        public MediaEndpointSupervisorSnapshot(IMediaEndpointClient aClient, MediaEndpointSupervisorSession aSession, IMediaEndpointClientSnapshot aSnapshot)
        {
            iClient = aClient;
            iSession = aSession;
            iSnapshot = aSnapshot;

            iDisposeHandler = new DisposeHandler();
            iCancellationTokenSource = new CancellationTokenSource();

            iTasks = new List<Task>();
        }

        public void Cancel()
        {
            using (iDisposeHandler.Lock)
            {
                Do.Assert(!iCancellationTokenSource.IsCancellationRequested);
                iCancellationTokenSource.Cancel();
            }
        }

        // IWatchableSnapshot<IMediaDatum>

        public uint Total
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return (iSnapshot.Total);
                }
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return (iSnapshot.Alpha);
                }
            }
        }

        public Task<IWatchableFragment<IMediaDatum>> Read(uint aIndex, uint aCount, CancellationToken aCancellationToken)
        {
            iClient.Assert(); // Must be called on the watchable thread;

            Do.Assert(aIndex + aCount <= iSnapshot.Total);

            using (iDisposeHandler.Lock)
            {
                var tcs = new TaskCompletionSource<IWatchableFragment<IMediaDatum>>();

                if (iCancellationTokenSource.IsCancellationRequested)
                {
                    tcs.SetCanceled();
                    return (tcs.Task);
                }

                if (aCount == 0)
                {
                    tcs.SetResult(new WatchableFragment<IMediaDatum>(aIndex, Enumerable.Empty<IMediaDatum>()));
                    return (tcs.Task);
                }

                Task task = null;

                var ctl = new CancellationTokenLink(iCancellationTokenSource.Token, aCancellationToken);

                lock (iTasks)
                {
                    task = iClient.Read(ctl.Token, iSession.Id, iSnapshot, aIndex, aCount).ContinueWith((t) =>
                    {
                        ctl.Dispose();

                        if (t.IsCanceled || t.IsFaulted)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetResult(new WatchableFragment<IMediaDatum>(aIndex, t.Result));
                        }

                        lock (iTasks)
                        {
                            iTasks.Remove(task);
                        }
                    });

                    iTasks.Add(task);

                    return (tcs.Task);
                }
            }
        }

        // IDisposable

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            Do.Assert(iCancellationTokenSource.IsCancellationRequested);

            Task[] tasks;

            lock (iTasks)
            {
                tasks = iTasks.ToArray();
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch
            {
            }

            lock (iTasks)
            {
                Do.Assert(iTasks.Count == 0);
            }

            iCancellationTokenSource.Dispose();
        }
    }

    internal class MediaEndpointSupervisorSession : IMediaEndpointSession
    {
        private readonly IMediaEndpointClient iClient;
        private readonly string iId;
        private readonly Action<string> iDispose;

        private readonly DisposeHandler iDisposeHandler;

        private Func<CancellationToken, Task<IMediaEndpointClientSnapshot>> iSnapshotFunction;
        private Action iAction;

        private CancellationTokenSource iCancellationTokenSource;

        private Task iTask;

        private MediaEndpointSupervisorSnapshot iSnapshot;

        private uint iSequence;

        public MediaEndpointSupervisorSession(IMediaEndpointClient aClient, string aId, Action<string> aDispose)
        {
            iClient = aClient;
            iId = aId;
            iDispose = aDispose;

            iDisposeHandler = new DisposeHandler();

            iSnapshotFunction = (c) =>
            {
                var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();
                tcs.SetResult(null);
                return (tcs.Task);
            };

            iAction = () =>
            {
                Do.Assert(false);
            };

            iCancellationTokenSource = new CancellationTokenSource();

            iTask = Task.Factory.StartNew(() => { });

            var token = iCancellationTokenSource.Token;

            iSnapshot = new MediaEndpointSupervisorSnapshot(iClient, this, iSnapshotFunction(token).Result);

            iSequence = 0;
        }

        internal string Id
        {
            get
            {
                return (iId);
            }
        }

        internal void Refresh()
        {
            // called on the watchable thread

            // first cancel current snapshot to prevent further activity on it and begin completing outstanding tasks
            
            var previous = iSnapshot;

            iSnapshot = null;

            if (previous != null)
            {
                previous.Cancel();
            }

            try
            {
                iTask.Wait();
            }
            catch
            {
            }

            uint sequence;

            sequence = ++iSequence;

            var token = iCancellationTokenSource.Token;

            iTask = iSnapshotFunction(token).ContinueWith((t) =>
            {
                token.ThrowIfCancellationRequested();

                IMediaEndpointClientSnapshot snapshot;

                try
                {
                    snapshot = t.Result;
                }
                catch
                {
                    throw new OperationCanceledException(token);
                }

                if (snapshot == null)
                {
                    throw new OperationCanceledException(token);
                }

                iClient.Schedule(() =>
                {
                    if (!token.IsCancellationRequested)
                    {
                        if (iSequence != sequence)
                        {
                            return;
                        }

                        Do.Assert(iSnapshot == null);

                        iSnapshot = new MediaEndpointSupervisorSnapshot(iClient, this, snapshot);

                        iAction();

                        if (previous != null)
                        {
                            previous.Dispose();
                        }
                    }
                });
            }, token);
        }

        private void UpdateSnapshot(Func<CancellationToken, Task<IMediaEndpointClientSnapshot>> aSnapshotFunction, Action aAction)
        {
            // called on the watchable thread

            iSnapshotFunction = aSnapshotFunction;
            iAction = aAction;
            Refresh();
        }

        // IMediaEndpointSession

        public IWatchableSnapshot<IMediaDatum> Snapshot
        {
            get
            {
                return (iSnapshot);
            }
        }

        public void Browse(IMediaDatum aDatum, Action aAction)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                UpdateSnapshot((c) => iClient.Browse(c, iId, aDatum), aAction);
            }
        }

        public void List(ITag aTag, Action aAction)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                UpdateSnapshot((c) => iClient.List(c, iId, aTag), aAction);
            }
        }

        public void Link(ITag aTag, string aValue, Action aAction)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                UpdateSnapshot((c) => iClient.Link(c, iId, aTag, aValue), aAction);
            }
        }

        public void Match(ITag aTag, string aValue, Action aAction)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                UpdateSnapshot((c) => iClient.Match(c, iId, aTag, aValue), aAction);
            }
        }

        public void Search(string aValue, Action aAction)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                UpdateSnapshot((c) => iClient.Search(c, iId, aValue), aAction);
            }
        }

        // IDisposable Members

        public void Dispose()
        {
            iClient.Assert(); // must be called on the watchable thread

            iCancellationTokenSource.Cancel();

            try
            {
                iTask.Wait();
            }
            catch
            {
            }

            iDisposeHandler.Dispose();

            if (iSnapshot != null)
            {
                iSnapshot.Cancel();
                iSnapshot.Dispose();
            }

            iCancellationTokenSource.Dispose();

            iDispose(iId);
        }

        public override string ToString()
        {
            return iId;
        }
    }

    // MediaEndpointSupervisor provides all the complicated Task and session handling required by a concrete MediaEndpoint client
    // Instead of writing all this again, construc a MediaEndpointSupervisor with an IMediaEndpointClient that expresses you specific implementation
    // Use Refresh(), or preferably Refresh(string aSession), when the MediaEndpoint for which you are a client has changed its contents.

    public class MediaEndpointSupervisor
    {
        private readonly IMediaEndpointClient iClient;
        private readonly DisposeHandler iDisposeHandler;
        private readonly CancellationTokenSource iCancellationTokenSource;
        private readonly List<Task> iCreateTasks;
        private readonly List<Task> iDestroyTasks;
        private readonly Dictionary<string, MediaEndpointSupervisorSession> iSessions;

        public MediaEndpointSupervisor(IMediaEndpointClient aClient)
        {
            iClient = aClient;
            iDisposeHandler = new DisposeHandler();
            iCancellationTokenSource = new CancellationTokenSource();
            iCreateTasks = new List<Task>();
            iDestroyTasks = new List<Task>();
            iSessions = new Dictionary<string, MediaEndpointSupervisorSession>();
        }

        public void Refresh()
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                lock (iSessions)
                {
                    foreach (var session in iSessions)
                    {
                        session.Value.Refresh();
                    }
                }
            }
        }

        public void Refresh(string aSession)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                lock (iSessions)
                {
                    MediaEndpointSupervisorSession session;

                    if (iSessions.TryGetValue(aSession, out session))
                    {
                        session.Refresh();
                    }
                }
            }
        }

        public void Cancel()
        {
            using (iDisposeHandler.Lock)
            {
                iCancellationTokenSource.Cancel();
            }
        }

        public Task<IMediaEndpointSession> CreateSession()
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                var token = iCancellationTokenSource.Token;

                var task = iClient.Create(token).ContinueWith<IMediaEndpointSession>((t) =>
                {
                    string id;

                    try
                    {
                        id = t.Result;
                    }
                    catch
                    {
                        throw (new OperationCanceledException(token));
                    }

                    lock (iSessions)
                    {
                        token.ThrowIfCancellationRequested();

                        var session = new MediaEndpointSupervisorSession(iClient, id, DestroySession);

                        iSessions.Add(id, session);

                        return (session);
                    }
                }, token);

                Task completion = null;

                lock (iCreateTasks)
                {
                    completion = task.ContinueWith((t) =>
                    {
                        try
                        {
                            t.Wait();
                        }
                        catch
                        {
                        }

                        lock (iCreateTasks)
                        {
                            iCreateTasks.Remove(completion);
                        }
                    });

                    iCreateTasks.Add(completion);
                }

                return (task);
            }
        }

        private void DestroySession(string aId)
        {
            // called on the watchable thread

            lock (iSessions)
            {
                iSessions.Remove(aId);
            }

            var task = Task.Factory.StartNew(() =>
            {
                var token = iCancellationTokenSource.Token;

                token.ThrowIfCancellationRequested();

                try
                {
                    iClient.Destroy(token, aId);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    throw (new OperationCanceledException());
                }
            });

            Task completion = null;

            lock (iDestroyTasks)
            {
                completion = task.ContinueWith((t) =>
                {
                    try
                    {
                        t.Wait();
                    }
                    catch
                    {
                    }

                    lock (iDestroyTasks)
                    {
                        iDestroyTasks.Remove(completion);
                    }
                });

                iDestroyTasks.Add(completion);
            }
        }

        // IDispose

        public void Dispose()
        {
            // users of the supervisor must close it, then indicate that their endpoint has disappeared, then dispose their supervisor
            // this gives clients the opportunity to dispose all their sessions in advance of the supervisor itself being disposed

            iDisposeHandler.Dispose();

            Do.Assert(iCancellationTokenSource.IsCancellationRequested);

            // now guaranteed that no more sessions are being created

            Task[] tasks;

            lock (iCreateTasks)
            {
                tasks = iCreateTasks.ToArray();
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch
            {
            }

            lock (iCreateTasks)
            {
                Do.Assert(iCreateTasks.Count == 0);
            }

            lock (iSessions)
            {
                Do.Assert(iSessions.Count == 0);
            }

            lock (iDestroyTasks)
            {
                tasks = iDestroyTasks.ToArray();
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch
            {
            }

            lock (iDestroyTasks)
            {
                Do.Assert(iDestroyTasks.Count == 0);
            }

            iCancellationTokenSource.Dispose();
        }
    }
}
