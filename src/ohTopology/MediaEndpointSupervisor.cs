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
    public interface IMediaEndpointSnapshot
    {
        uint Total { get; }
        IEnumerable<uint> AlphaMap { get; } // null if no alpha map
        Task<IWatchableFragment<IMediaDatum>> Read(uint aIndex, uint aCount);
    }

    public interface IMediaEndpointClient
    {
        string CreateSession(CancellationToken aCancellationToken);
        void DestroySession(CancellationToken aCancellationToken, string aId);
        IMediaEndpointSnapshot Browse(CancellationToken aCancellationToken, IMediaDatum aDatum);
        IMediaEndpointSnapshot Link(CancellationToken aCancellationToken, ITag aTag, string aValue);
        IMediaEndpointSnapshot Search(CancellationToken aCancellationToken, string aValue);
    }

    internal class MediaEndpointSupervisorSession : IMediaEndpointSession
    {
        private readonly IMediaEndpointClient iClient;
        private readonly string iId;
        private readonly Action<string> iDispose;

        private readonly DisposeHandler iDisposeHandler;

        public MediaEndpointSupervisorSession(IMediaEndpointClient aClient, string aId, Action<string> aDispose)
        {
            iClient = aClient;
            iId = aId;
            iDispose = aDispose;

            iDisposeHandler = new DisposeHandler();
        }

        // IMediaEndpointSession

        public Task<IWatchableContainer<IMediaDatum>> Browse(IMediaDatum aDatum)
        {
            using (iDisposeHandler.Lock)
            {
                throw new NotImplementedException();
            }
        }

        public Task<IWatchableContainer<IMediaDatum>> Link(ITag aTag, string aValue)
        {
            using (iDisposeHandler.Lock)
            {
                throw new NotImplementedException();
            }
        }

        public Task<IWatchableContainer<IMediaDatum>> Search(string aValue)
        {
            using (iDisposeHandler.Lock)
            {
                throw new NotImplementedException();
            }
        }

        // IDisposable Members

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iDispose(iId);
        }
    }

    public class MediaEndpointSupervisor
    {
        private readonly IMediaEndpointClient iClient;
        private readonly DisposeHandler iDisposeHandler;
        private readonly CancellationTokenSource iCancellationSource;
        private readonly List<Task> iTasks;
        private readonly Dictionary<string, MediaEndpointSupervisorSession> iSessions;
        
        public MediaEndpointSupervisor(IMediaEndpointClient aClient)
        {
            iClient = aClient;
            iDisposeHandler = new DisposeHandler();
            iCancellationSource = new CancellationTokenSource();
            iTasks = new List<Task>();
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

        public void Cancel()
        {
            iCancellationSource.Cancel();
        }

        public Task<IMediaEndpointSession> CreateSession()
        {
            using (iDisposeHandler.Lock)
            {
                var task = Task.Factory.StartNew<IMediaEndpointSession>(() =>
                {
                    var id = iClient.CreateSession(CancellationToken);

                    var session = new MediaEndpointSupervisorSession(iClient, id, DestroySession);

                    lock (iSessions)
                    {
                        iSessions.Add(id, session);
                    }

                    return (session);
                });

                lock (iTasks)
                {
                    Task completion = null;
                    
                    completion = task.ContinueWith((t) =>
                    {
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

        private void DestroySession(string aId)
        {
            lock (iSessions)
            {
                iSessions.Remove(aId);
            }

            var task = Task.Factory.StartNew(() =>
            {
                iClient.DestroySession(CancellationToken, aId);
            });

            lock (iTasks)
            {
                Task completion = null;

                completion = task.ContinueWith((t) =>
                {
                    lock (iTasks)
                    {
                        iTasks.Remove(completion);
                    }
                });

                iTasks.Add(completion);
            }
        }

        // IDispose

        public void Dispose()
        {
            Cancel();

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

            lock (iSessions)
            {
                Do.Assert(iSessions.Count == 0);
            }
        }
    }

    /*
    internal class MediaEndpointSessionOpenHome : IMediaEndpointSession
    {
        private readonly INetwork iNetwork;
        private readonly ServiceMediaEndpointOpenHome iService;

        private readonly DisposeHandler iDisposeHandler;

        private readonly Encoding iEncoding;

        private readonly object iLock;

        private readonly string iId;
        
        private uint iSequence;

        private Task<IWatchableContainer<IMediaDatum>> iTask;

        private MediaEndpointContainerOpenHome iContainer;

        public MediaEndpointSessionOpenHome(INetwork aNetwork, ServiceMediaEndpointOpenHome aService)
        {
            iNetwork = aNetwork;
            iService = aService;

            iDisposeHandler = new DisposeHandler();

            iEncoding = new UTF8Encoding(false);

            iLock = new object();

            using (var client = new WebClient())
            {
                try
                {
                    var create = iService.CreateUri("create");

                    var session = client.DownloadString(create);

                    var json = JsonParser.Parse(session) as JsonString;

                    iId = json.Value();
                }
                catch
                {
                }

                iSequence = 0;
            }

            iTask = Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                return (null);
            });
        }

        private string Encode(string aValue)
        {
            var bytes = iEncoding.GetBytes(aValue);
            var value = System.Convert.ToBase64String(bytes);
            return (value);
        }

        internal void Refresh()
        {
        }

        private Task<IWatchableContainer<IMediaDatum>> UpdateLocked(Func<MediaEndpointContainerOpenHome> aContainer)
        {
            if (iId == null)
            {
                return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
                {
                    return (null);
                }));
            }

            uint sequence;

            sequence = ++iSequence;

            if (iContainer != null)
            {
                iContainer.Dispose();
                iContainer = null;
            }

            return (Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                lock (iLock)
                {
                    if (iSequence == sequence)
                    {
                        iContainer = aContainer();
                        return (iContainer);
                    }

                    return (null);
                }
            }));
        }

        // IMediaEndpointSession

        public Task<IWatchableContainer<IMediaDatum>> Browse(IMediaDatum aDatum)
        {
            using (iDisposeHandler.Lock)
            {
                lock (iLock)
                {
                    if (aDatum == null)
                    {
                        iTask = UpdateLocked(() =>
                        {
                            return (new MediaEndpointContainerOpenHomeBrowse(iNetwork, iService, iId, "0"));
                        });
                    }
                    else
                    {
                        iTask = UpdateLocked(() =>
                        {
                            return (new MediaEndpointContainerOpenHomeBrowse(iNetwork, iService, iId, aDatum.Id));
                        });
                    }

                    return (iTask);
                }
            }
        }

        public Task<IWatchableContainer<IMediaDatum>> Link(ITag aTag, string aValue)
        {
            using (iDisposeHandler.Lock)
            {
                lock (iLock)
                {
                    iTask = UpdateLocked(() =>
                    {
                        return (new MediaEndpointContainerOpenHomeLink(iNetwork, iService, iId, aTag, Encode(aValue)));
                    });

                    return (iTask);
                }
            }
        }

        public Task<IWatchableContainer<IMediaDatum>> Search(string aValue)
        {
            using (iDisposeHandler.Lock)
            {
                lock (iLock)
                {
                    iTask = UpdateLocked(() =>
                    {
                        return (new MediaEndpointContainerOpenHomeSearch(iNetwork, iService, iId, Encode(aValue)));
                    });

                    return (iTask);
                }
            }
        }

        // Disposable

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            iTask.Wait();

            if (iContainer != null)
            {
                iContainer.Dispose();
            }

            if (iId != null)
            {
                using (var client = new WebClient())
                {
                    var destroy = iService.CreateUri("destroy?session={0}", iId);

                    try
                    {
                        client.DownloadString(destroy);
                    }
                    catch
                    {
                        // common if disposing session because the endpoint has disappeared
                        // could try and distinguish between disposing when the endpoint has disappeared
                        // and disposing because the client is no longer interested in the session
                    }
                }
            }

            iService.Destroy(this);
        }
    }

    internal class MediaEndpointContainerOpenHome : IWatchableContainer<IMediaDatum>, IDisposable
    {
        private readonly INetwork iNetwork;
        private readonly ServiceMediaEndpointOpenHome iService;
        private readonly string iSession;

        private readonly Uri iUriRefresh;

        private readonly Watchable<IWatchableSnapshot<IMediaDatum>> iWatchableSnapshot;

        protected MediaEndpointContainerOpenHome(INetwork aNetwork, ServiceMediaEndpointOpenHome aService, string aSession, string aFormat, params object[] aArguments)
        {
            iNetwork = aNetwork;
            iService = aService;
            iSession = aSession;

            var snapshot = GetSnapshot(aFormat, aArguments);

            iWatchableSnapshot = new Watchable<IWatchableSnapshot<IMediaDatum>>(aNetwork, "snapshot", snapshot);
        }

        private IWatchableSnapshot<IMediaDatum> GetSnapshot(string aFormat, params object[] aArguments)
        {
            var uri = iService.CreateUri(aFormat, aArguments);

            using (var client = new WebClient())
            {
                try
                {
                    var session = client.DownloadString(uri);

                    var json = JsonParser.Parse(session) as JsonObject;

                    var total = GetTotal(json["Total"]);
                    var alpha = GetAlpha(json["Alpha"]);

                    return (new MediaEndpointSnapshotOpenHome(iNetwork, iService, iSession, total, alpha));
                }
                catch
                {
                    return (null);
                }
            }
        }

        private uint GetTotal(JsonValue aValue)
        {
            var value = aValue as JsonString;
            return (uint.Parse(value.Value()));
        }

        private IEnumerable<uint> GetAlpha(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetAlphaElement(entry));
            }
        }

        private uint GetAlphaElement(JsonValue aValue)
        {
            var value = aValue as JsonString;
            return (uint.Parse(value.Value()));
        }

        internal void Update(uint aSequence)
        {
            iWatchableSnapshot.Update(GetSnapshot("refresh?session=" + iSession));
        }

        // IMediaServerContainer

        public IWatchable<IWatchableSnapshot<IMediaDatum>> Snapshot
        {
            get { return (iWatchableSnapshot); }
        }

        // IDisposable

        public void Dispose()
        {
            iNetwork.Execute();
            iWatchableSnapshot.Dispose();
        }
    }

    internal class MediaEndpointContainerOpenHomeBrowse : MediaEndpointContainerOpenHome
    {
        public MediaEndpointContainerOpenHomeBrowse(INetwork aNetwork, ServiceMediaEndpointOpenHome aService, string aSession, string aId)
            : base (aNetwork, aService, aSession, "browse?session={0}&id={1}", aSession, aId)
        {
        }
    }

    internal class MediaEndpointContainerOpenHomeLink : MediaEndpointContainerOpenHome
    {
        public MediaEndpointContainerOpenHomeLink(INetwork aNetwork, ServiceMediaEndpointOpenHome aService, string aSession, ITag aTag, string aValue)
            : base(aNetwork, aService, aSession, "link?session={0}&tag={1}&val={2}", aSession, aTag.Id, aValue)
        {
        }
    }

    internal class MediaEndpointContainerOpenHomeSearch : MediaEndpointContainerOpenHome
    {
        public MediaEndpointContainerOpenHomeSearch(INetwork aNetwork, ServiceMediaEndpointOpenHome aService, string aSession, string aValue)
            : base(aNetwork, aService, aSession, "search?session={0}&val={1}", aSession, aValue)
        {
        }
    }

    internal class MediaEndpointSnapshotOpenHome : IWatchableSnapshot<IMediaDatum>
    {
        private readonly INetwork iNetwork;
        private readonly ServiceMediaEndpointOpenHome iService;
        private readonly string iSession;
        private readonly uint iTotal;
        private readonly IEnumerable<uint> iAlpha;

        public MediaEndpointSnapshotOpenHome(INetwork aNetwork, ServiceMediaEndpointOpenHome aService, string aSession, uint aTotal, IEnumerable<uint> aAlpha)
        {
            iNetwork = aNetwork;
            iService = aService;
            iSession = aSession;
            iTotal = aTotal;
            iAlpha = aAlpha;
        }

        // IMediaServerSnapshot<IMediaDatum>

        public uint Total
        {
            get
            {
                return (iTotal);
            }
        }

        public IEnumerable<uint> AlphaMap
        {
            get
            {
                return (iAlpha);
            }
        }

        public Task<IWatchableFragment<IMediaDatum>> Read(uint aIndex, uint aCount)
        {
            iNetwork.Assert();

            Do.Assert(aIndex + aCount <= iTotal);

            return (Task.Factory.StartNew<IWatchableFragment<IMediaDatum>>(() =>
            {
                return (new MediaEndpointFragmentOpenHome(iNetwork, iService, iSession, iTotal, aIndex, aCount));
            }));
        }
    }

    internal class MediaEndpointFragmentOpenHome : IWatchableFragment<IMediaDatum>
    {
        private readonly INetwork iNetwork;
        private readonly ServiceMediaEndpointOpenHome iService;
        private readonly string iSession;
        private readonly uint iTotal;
        private readonly uint iIndex;

        private readonly IEnumerable<IMediaDatum> iData;

        public MediaEndpointFragmentOpenHome(INetwork aNetwork, ServiceMediaEndpointOpenHome aService, string aSession, uint aTotal, uint aIndex, uint aCount)
        {
            iNetwork = aNetwork;
            iService = aService;
            iSession = aSession;
            iTotal = aTotal;
            iIndex = aIndex;

            var uri = iService.CreateUri("/read?session={0}&index={1}&count={2}", iSession, iIndex, aCount);

            using (var client = new WebClient())
            {
                try
                {
                    var session = client.DownloadString(uri);

                    var json = JsonParser.Parse(session);

                    iData = GetData(json);
                }
                catch
                {
                    iData = new List<IMediaDatum>();
                }
            }
        }

        private IEnumerable<IMediaDatum> GetData(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetDatum(entry));
            }
        }

        private IMediaDatum GetDatum(JsonValue aValue)
        {
            var value = aValue as JsonObject;

            var id = GetValue(value["Id"]);
            var type = GetType(value["Type"]);

            var datum = new MediaDatum(id, type.ToArray());

            foreach (var entry in value["Metadata"] as JsonArray)
            {
                var values = GetMetadatumValues(entry);
                var tagid = uint.Parse(values.First());
                var tag = iNetwork.TagManager[tagid];
                var resolced = Resolve(tag, values.Skip(1));
                datum.Add(tag, new MediaValue(values.Skip(1)));
            }

            return (datum);
        }

        private IEnumerable<string> Resolve(ITag aTag, IEnumerable<string> aValues)
        {
            if (aTag == iNetwork.TagManager.Audio.Artwork || aTag == iNetwork.TagManager.Container.Artwork)
            {
                return (ResolveUri(aValues));
            }

            return (aValues);
        }

        private IEnumerable<string> ResolveUri(IEnumerable<string> aValues)
        {
            foreach (var value in aValues)
            {
                yield return (ResolveUri(value));
            }
        }

        private string ResolveUri(string aValue)
        {
            try
            {
                var uri = new Uri(aValue);
                return (aValue);
            }
            catch
            {
                return (iService.ResolveUri(aValue));
            }
        }

        private IEnumerable<string> GetMetadatumValues(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetMetadatumValue(entry));
            }
        }

        private string GetMetadatumValue(JsonValue aValue)
        {
            var value = aValue as JsonString;
            return (value.Value);
        }

        private string GetValue(JsonValue aValue)
        {
            var value = aValue as JsonString;
            return (value.Value);
        }

        private IEnumerable<ITag> GetType(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetTag(entry));
            }
        }

        private ITag GetTag(JsonValue aValue)
        {
            var value = aValue as JsonString;

            var id = uint.Parse(value.Value);

            return (iNetwork.TagManager[id]);
        }

        // IWatchableFragment<IMediaDatum>

        public uint Index
        {
            get { return (iIndex); }
        }

        public IEnumerable<IMediaDatum> Data
        {
            get { return (iData); }
        }
    }
    */
}
