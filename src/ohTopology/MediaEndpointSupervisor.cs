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
        string CreateSession(CancellationToken aCancellationToken);
        void DestroySession(CancellationToken aCancellationToken, string aId);
        IMediaEndpointClientSnapshot Browse(CancellationToken aCancellationToken, string aSession, IMediaDatum aDatum);
        IMediaEndpointClientSnapshot Link(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue);
        IMediaEndpointClientSnapshot Search(CancellationToken aCancellationToken, string aSession, string aValue);
        IEnumerable<IMediaDatum> Read(CancellationToken aCancellationToken, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount);
    }

    public class MediaEndpointClientSnapshot : IMediaEndpointClientSnapshot
    {
        private readonly string iContainer;
        private readonly uint iTotal;
        private readonly IEnumerable<uint> iAlpha;

        public MediaEndpointClientSnapshot(string aContainer, uint aTotal, IEnumerable<uint> aAlphaMap)
        {
            iContainer = aContainer;
            iTotal = aTotal;
            iAlpha = aAlphaMap;
        }

        // IMediaEndpointSnapshot Members

        public string Container
        {
            get
            {
                return (iContainer);
            }
        }

        public uint Total
        {
            get
            {
                return (iTotal);
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                return (iAlpha);
            }
        }
    }

    internal class MediaEndpointSupervisorSnapshot : IWatchableSnapshot<IMediaDatum>, IDisposable
    {
        private readonly IMediaEndpointClient iClient;
        private readonly MediaEndpointSupervisorSession iSession;
        private readonly CancellationToken iCancellationToken;
        private readonly IMediaEndpointClientSnapshot iSnapshot;

        private readonly DisposeHandler iDisposeHandler;

        private readonly List<Task> iTasks;

        public MediaEndpointSupervisorSnapshot(IMediaEndpointClient aClient, MediaEndpointSupervisorSession aSession, CancellationToken aCancellationToken, IMediaEndpointClientSnapshot aSnapshot)
        {
            iClient = aClient;
            iSession = aSession;
            iCancellationToken = aCancellationToken;
            iSnapshot = aSnapshot;

            iDisposeHandler = new DisposeHandler();

            iTasks = new List<Task>();
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

        public Task<IWatchableFragment<IMediaDatum>> Read(uint aIndex, uint aCount)
        {
            iClient.Assert(); // Must be called on the watchable thread;

            Do.Assert(aIndex + aCount <= iSnapshot.Total);

            using (iDisposeHandler.Lock)
            {
                var task = Task.Factory.StartNew<IWatchableFragment<IMediaDatum>>(() =>
                {
                    iCancellationToken.ThrowIfCancellationRequested();

                    var data = iClient.Read(iCancellationToken, iSession.Id, iSnapshot, aIndex, aCount);

                    return (new WatchableFragment<IMediaDatum>(aIndex, data));
                });

                lock (iTasks)
                {
                    Task completion = null;

                    completion = task.ContinueWith((t) =>
                    {
                        try
                        {
                            t.Wait();
                        }
                        catch
                        {
                        }

                        lock (iTasks)
                        {
                            iTasks.Remove(completion);
                        }
                    });

                    iTasks.Add(completion);
                }

                return (task);
            }
        }

        // IDisposable

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            Task[] tasks;

            lock (iTasks)
            {
                tasks = iTasks.ToArray();
            }

            Task.WaitAll(tasks);

            lock (iTasks)
            {
                Do.Assert(iTasks.Count == 0);
            }
        }
    }

    internal class MediaEndpointSupervisorContainer : IWatchableContainer<IMediaDatum>, IDisposable
    {
        private readonly IMediaEndpointClient iClient;
        private readonly MediaEndpointSupervisorSession iSession;

        private readonly DisposeHandler iDisposeHandler;
        private readonly CancellationTokenSource iCancellationSource;
        private MediaEndpointSupervisorSnapshot iSnapshot;
        private readonly Watchable<IWatchableSnapshot<IMediaDatum>> iWatchableSnapshot;

        public MediaEndpointSupervisorContainer(IMediaEndpointClient aClient, MediaEndpointSupervisorSession aSession, IMediaEndpointClientSnapshot aSnapshot)
        {
            iClient = aClient;
            iSession = aSession;

            iDisposeHandler = new DisposeHandler();
            iCancellationSource = new CancellationTokenSource();
            iSnapshot = new MediaEndpointSupervisorSnapshot(iClient, iSession, iCancellationSource.Token, aSnapshot);
            iWatchableSnapshot = new Watchable<IWatchableSnapshot<IMediaDatum>>(iClient, "Snapshot", iSnapshot);
        }

        // IWatchableContainer<IMediaDatum>

        public IWatchable<IWatchableSnapshot<IMediaDatum>> Snapshot
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return (iWatchableSnapshot);
                }
            }
        }

        // IDisposable

        public void Dispose()
        {
            // called on the watchable thread

            iCancellationSource.Cancel();
            iDisposeHandler.Dispose();
            iWatchableSnapshot.Dispose();
            iSnapshot.Dispose();
        }
    }

    internal class MediaEndpointSupervisorSession : IMediaEndpointSession
    {
        private readonly IMediaEndpointClient iClient;
        private readonly CancellationToken iCancellationToken;
        private readonly string iId;
        private readonly Action<string> iDispose;

        private readonly DisposeHandler iDisposeHandler;

        private readonly List<Task> iTasks;

        private readonly object iLock;

        private uint iSequence;

        private MediaEndpointSupervisorContainer iContainer;

        public MediaEndpointSupervisorSession(IMediaEndpointClient aClient, CancellationToken aCancellationToken, string aId, Action<string> aDispose)
        {
            iClient = aClient;
            iCancellationToken = aCancellationToken;
            iId = aId;
            iDispose = aDispose;

            iDisposeHandler = new DisposeHandler();

            iTasks = new List<Task>();

            iLock = new object();

            iSequence = 0;
        }

        internal string Id
        {
            get
            {
                return (iId);
            }
        }

        private Task<IWatchableContainer<IMediaDatum>> UpdateContainer(Func<IMediaEndpointClientSnapshot> aFunction)
        {
            // called on the watchable thread

            uint sequence;

            lock (iLock)
            {
                if (iContainer != null)
                {
                    iContainer.Dispose();
                    iContainer = null;
                }

                sequence = ++iSequence;
            }

            var task = Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                iCancellationToken.ThrowIfCancellationRequested();

                lock (iLock)
                {
                    if (iSequence == sequence)
                    {
                        iContainer = new MediaEndpointSupervisorContainer(iClient, this, aFunction());

                        return (iContainer);
                    }

                    throw (new OperationCanceledException());
                }
            });

            lock (iTasks)
            {
                Task completion = null;

                completion = task.ContinueWith((t) =>
                {
                    try
                    {
                        t.Wait();
                    }
                    catch
                    {
                    }

                    lock (iTasks)
                    {
                        iTasks.Remove(completion);
                    }
                });

                iTasks.Add(completion);
            }

            return (task);
        }

        // IMediaEndpointSession

        public Task<IWatchableContainer<IMediaDatum>> Browse(IMediaDatum aDatum)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                return (UpdateContainer(() => iClient.Browse(iCancellationToken, iId, aDatum)));
            }
        }

        public Task<IWatchableContainer<IMediaDatum>> Link(ITag aTag, string aValue)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                return (UpdateContainer(() => iClient.Link(iCancellationToken, iId, aTag, aValue)));
            }
        }

        public Task<IWatchableContainer<IMediaDatum>> Search(string aValue)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                return (UpdateContainer(() => iClient.Search(iCancellationToken, iId, aValue)));
            }
        }

        // IDisposable Members

        public void Dispose()
        {
            iClient.Assert(); // must be called on the watchable thread

            iDisposeHandler.Dispose();

            lock (iLock)
            {
                if (iContainer != null)
                {
                    iContainer.Dispose();
                }
            }

            iDispose(iId);
        }
    }

    public class MediaEndpointSupervisor
    {
        private readonly IMediaEndpointClient iClient;
        private readonly DisposeHandler iDisposeHandler;
        private readonly CancellationTokenSource iCancellationSource;
        private readonly List<Task> iCreateTasks;
        private readonly List<Task> iDestroyTasks;
        private readonly Dictionary<string, MediaEndpointSupervisorSession> iSessions;
        
        public MediaEndpointSupervisor(IMediaEndpointClient aClient)
        {
            iClient = aClient;
            iDisposeHandler = new DisposeHandler();
            iCancellationSource = new CancellationTokenSource();
            iCreateTasks = new List<Task>();
            iDestroyTasks = new List<Task>();
            iSessions = new Dictionary<string, MediaEndpointSupervisorSession>();
        }

        private CancellationToken CancellationToken
        {
            get
            {
                var token = iCancellationSource.Token;
                token.ThrowIfCancellationRequested();
                return (token);
            }
        }

        public void Close()
        {
            iCancellationSource.Cancel();
        }

        public Task<IMediaEndpointSession> CreateSession()
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock)
            {
                var task = Task.Factory.StartNew<IMediaEndpointSession>(() =>
                {
                    var token = CancellationToken;

                    var id = iClient.CreateSession(token);

                    var session = new MediaEndpointSupervisorSession(iClient, token, id, DestroySession);

                    lock (iSessions)
                    {
                        iSessions.Add(id, session);
                    }

                    return (session);
                });

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
                iClient.DestroySession(CancellationToken, aId);
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

            Do.Assert(iCancellationSource.IsCancellationRequested);

            iDisposeHandler.Dispose();

            // now guaranteed that no more sessions are being created

            Task[] tasks;

            lock (iCreateTasks)
            {
                tasks = iCreateTasks.ToArray();
            }

            Task.WaitAll(tasks);

            lock (iCreateTasks)
            {
                Do.Assert(iCreateTasks.Count == 0);
            }

            Do.Assert(iSessions.Count == 0);

            lock (iDestroyTasks)
            {
                tasks = iDestroyTasks.ToArray();
            }

            Task.WaitAll(tasks);

            lock (iDestroyTasks)
            {
                Do.Assert(iDestroyTasks.Count == 0);
            }
        }
    }
}
