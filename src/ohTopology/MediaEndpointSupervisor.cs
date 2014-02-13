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
        void Create(CancellationToken aCancellationToken, Action<string> aCallback);
        void Destroy(CancellationToken aCancellationToken, Action<string> aCallback, string aId);
        void Browse(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, IMediaDatum aDatum);
        void List(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, ITag aTag);
        void Link(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, ITag aTag, string aValue);
        void Match(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, ITag aTag, string aValue);
        void Search(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, string aValue);
        void Read(CancellationToken aCancellationToken, Action<IWatchableFragment<IMediaDatum>> aCallback, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount);
    }

    public class CancellationTokenLink
    {
        private readonly CancellationTokenSource iSource;

        public CancellationTokenLink(params CancellationToken[] aTokens)
        {
            iSource = new CancellationTokenSource();

            foreach (var token in aTokens)
            {
                token.Register(() => 
                {
                    iSource.Cancel();
                });
            }
        }

        public CancellationToken Token
        {
            get
            {
                return (iSource.Token);
            }
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
        private ILog iLog;

        public MediaEndpointSupervisorSnapshot(IMediaEndpointClient aClient, MediaEndpointSupervisorSession aSession, IMediaEndpointClientSnapshot aSnapshot, ILog aLog)
        {
            iLog = aLog;
            iClient = aClient;
            iSession = aSession;
            iSnapshot = aSnapshot;

            iDisposeHandler = new DisposeHandler();
            iCancellationTokenSource = new CancellationTokenSource();

            iTasks = new List<Task>();
        }

        public void Cancel()
        {
            using (iDisposeHandler.Lock())
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
                using (iDisposeHandler.Lock())
                {
                    return (iSnapshot.Total);
                }
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return (iSnapshot.Alpha);
                }
            }
        }

        public void Read(uint aIndex, uint aCount, CancellationToken aCancellationToken, Action<IWatchableFragment<IMediaDatum>> aCallback)
        {
            iClient.Assert(); // Must be called on the watchable thread;
            var s = new Stopwatch();
            s.Start();
            Do.Assert(aIndex + aCount <= iSnapshot.Total);

            using (iDisposeHandler.Lock())
            {
                if (iCancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                if (aCount == 0)
                {
                    aCallback(new WatchableFragment<IMediaDatum>(aIndex, Enumerable.Empty<IMediaDatum>()));
                }
                else
                {
                    var ctl = new CancellationTokenLink(iCancellationTokenSource.Token, aCancellationToken);

                    iClient.Read(ctl.Token, (fragment) =>
                    {
                        s.Stop();
                        iLog.Write("Snapshot Read ({0}) took {1} milliseconds\n", iSession.Id, s.Milliseconds);
                        aCallback(fragment);
                    }, iSession.Id, iSnapshot, aIndex, aCount);
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

            Task.WaitAll(tasks);

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

        private Action<CancellationToken, Action<IMediaEndpointClientSnapshot>> iSnapshotFunction;
        private Action iAction;

        private CancellationTokenSource iCancellationTokenSource;

        private Task iTask;

        private MediaEndpointSupervisorSnapshot iSnapshot;
        private MediaEndpointSupervisorSnapshot iPrevious;

        private uint iSequence;

        private ILog iLog;

        public MediaEndpointSupervisorSession(IMediaEndpointClient aClient, string aId, Action<string> aDispose, ILog aLog)
        {
            iLog = aLog;
            iClient = aClient;
            iId = aId;
            iDispose = aDispose;

            iDisposeHandler = new DisposeHandler();

            iSnapshotFunction = (token, snapshot) =>
            {
            };

            iAction = () =>
            {
                Do.Assert(false);
            };

            iCancellationTokenSource = new CancellationTokenSource();

            iTask = Task.Factory.StartNew(() => { });

            iSnapshot = new MediaEndpointSupervisorSnapshot(iClient, this, null, iLog);

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

            if (iSnapshot != null)
            {
                iPrevious = iSnapshot;
                iSnapshot = null;
                iCancellationTokenSource.Cancel();
                iCancellationTokenSource.Dispose();
                iCancellationTokenSource = new CancellationTokenSource();
                iPrevious.Cancel();
            }

            uint sequence;

            sequence = ++iSequence;

            iSnapshotFunction(iCancellationTokenSource.Token, (snapshot) =>
            {
                if (iSequence != sequence)
                {
                    return;
                }

                Do.Assert(iSnapshot == null);

                iSnapshot = new MediaEndpointSupervisorSnapshot(iClient, this, snapshot, iLog);

                iAction();

                if (iPrevious != null)
                {
                    iPrevious.Dispose();
                }
            });
        }

        private void UpdateSnapshot(Action<CancellationToken, Action<IMediaEndpointClientSnapshot>> aSnapshotFunction, Action aAction)
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

            var s = new Stopwatch();
            s.Start();
            using (iDisposeHandler.Lock())
            {
                UpdateSnapshot((token, callback) => iClient.Browse(token, callback, iId, aDatum), () =>
                {
                    s.Stop();
                    iLog.Write("Browse ({0}) took {1} milliseconds\n", this.Id, s.Milliseconds);
                    aAction();
                });
            }
        }

        public void List(ITag aTag, Action aAction)
        {
            iClient.Assert(); // must be called on the watchable thread

            var s = new Stopwatch();
            s.Start();
            using (iDisposeHandler.Lock())
            {
                UpdateSnapshot((token, callback) => iClient.List(token, callback, iId, aTag), () =>
                {
                    s.Stop();
                    iLog.Write("List ({0}) took {1} milliseconds\n", this.Id, s.Milliseconds);
                    aAction();
                });
            }
        }

        public void Link(ITag aTag, string aValue, Action aAction)
        {
            iClient.Assert(); // must be called on the watchable thread

            var s = new Stopwatch();
            s.Start();
            using (iDisposeHandler.Lock())
            {
                UpdateSnapshot((token, callback) => iClient.Link(token, callback, iId, aTag, aValue), () =>
                {
                    s.Stop();
                    iLog.Write("Link ({0}) took {1} milliseconds\n", this.Id, s.Milliseconds);
                    aAction();
                });
            }
        }

        public void Match(ITag aTag, string aValue, Action aAction)
        {
            iClient.Assert(); // must be called on the watchable thread

            var s = new Stopwatch();
            s.Start();
            using (iDisposeHandler.Lock())
            {
                UpdateSnapshot((token, callback) => iClient.Match(token, callback, iId, aTag, aValue), () =>
                {
                    s.Stop();
                    iLog.Write("Match ({0}) took {1} milliseconds\n", this.Id, s.Milliseconds);
                    aAction();
                });
            }
        }

        public void Search(string aValue, Action aAction)
        {
            iClient.Assert(); // must be called on the watchable thread
            var s = new Stopwatch();
            s.Start();
            using (iDisposeHandler.Lock())
            {
                UpdateSnapshot((token, callback) => iClient.Search(token, callback, iId, aValue), () => {                    
                    s.Stop();
                    iLog.Write("Search ({0}) took {1} milliseconds\n", this.Id, s.Milliseconds);
                    aAction();
                });
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
        private readonly Dictionary<string, MediaEndpointSupervisorSession> iSessions;
        private ILog iLog;

        public MediaEndpointSupervisor(IMediaEndpointClient aClient, ILog aLog)
        {
            iClient = aClient;
            iLog = aLog;
            iDisposeHandler = new DisposeHandler();
            iCancellationTokenSource = new CancellationTokenSource();
            iSessions = new Dictionary<string, MediaEndpointSupervisorSession>();
        }

        public void Refresh()
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock())
            {
                foreach (var session in iSessions)
                {
                    session.Value.Refresh();
                }
            }
        }

        public void Refresh(string aSession)
        {
            iClient.Assert(); // must be called on the watchable thread

            using (iDisposeHandler.Lock())
            {
                MediaEndpointSupervisorSession session;

                if (iSessions.TryGetValue(aSession, out session))
                {
                    session.Refresh();
                }
            }
        }

        public void Cancel()
        {
            using (iDisposeHandler.Lock())
            {
                iCancellationTokenSource.Cancel();
            }
        }

        public void CreateSession(Action<IMediaEndpointSession> aCallback)
        {
            iClient.Assert(); // must be called on the watchable thread
            Stopwatch s = new Stopwatch();
            s.Start();
            using (iDisposeHandler.Lock())
            {
                var token = iCancellationTokenSource.Token;

                iClient.Create(iCancellationTokenSource.Token, (session) =>
                {
                    var newSession = new MediaEndpointSupervisorSession(iClient, session, DestroySession, iLog);
                    iSessions.Add(session, newSession);
                    s.Stop();
                    iLog.Write("CreateSession ({0}) took {1} milliseconds\n", session, s.Milliseconds);
                    aCallback(newSession);
                });
            }
        }

        private void DestroySession(string aId)
        {
            // already guaranteed to be on the watchable thread because it
            // is only called from Session the dispose method

            iSessions.Remove(aId);

            iClient.Destroy(iCancellationTokenSource.Token, (session) => {}, aId);
        }

        // IDispose

        public void Dispose()
        {
            // users of the supervisor must close it, then indicate that their endpoint has disappeared, then dispose their supervisor
            // this gives clients the opportunity to dispose all their sessions in advance of the supervisor itself being disposed

            iDisposeHandler.Dispose();

            Do.Assert(iCancellationTokenSource.IsCancellationRequested);

            iClient.Schedule(() =>
            {
                Do.Assert(iSessions.Count == 0);
            });

            iCancellationTokenSource.Dispose();
        }
    }
}
