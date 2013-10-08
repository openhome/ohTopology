using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IIdCacheEntry
    {
        IMediaMetadata Metadata { get; }
        string Uri { get; }
    }

    public interface IIdCacheSession : IDisposable
    {
        void SetValid(IList<uint> aValid);
        Task<IEnumerable<IIdCacheEntry>> Entries(IEnumerable<uint> aIds);
    }

    public interface IIdCache
    {
        IIdCacheSession CreateSession(string aId, Func<IEnumerable<uint>, Task<IEnumerable<IIdCacheEntry>>> aFunction);
    }

    class IdCache : IIdCache, IDisposable
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly uint iMaxCacheEntries;
        private readonly Dictionary<Hash, Dictionary<uint, IdCacheEntrySession>> iCache;
        private readonly List<IdCacheEntrySession> iLastAccessed;
        private readonly List<Hash> iSessions;
        private uint iCacheEntries;

        public IdCache(uint aMaxCacheEntries)
        {
            iMaxCacheEntries = aMaxCacheEntries;

            iDisposeHandler = new DisposeHandler();
            iCacheEntries = 0;
            iCache = new Dictionary<Hash, Dictionary<uint, IdCacheEntrySession>>();
            iLastAccessed = new List<IdCacheEntrySession>();
            iSessions = new List<Hash>();
        }

        public void Dispose()
        {
            if (iSessions.Count > 0)
            {
                throw new Exception("IdCache disposed with active sessions");
            }

            iDisposeHandler.Dispose();
            iCache.Clear();
            iLastAccessed.Clear();
            iCacheEntries = 0;
        }

        public IIdCacheSession CreateSession(string aId, Func<IEnumerable<uint>, Task<IEnumerable<IIdCacheEntry>>> aFunction)
        {
            using (iDisposeHandler.Lock)
            {
                Hash hash = Hash.Create(aId);

                lock (iCache)
                {
                    iSessions.Add(hash);
                    if (!iCache.ContainsKey(hash))
                    {
                        iCache.Add(hash, new Dictionary<uint, IdCacheEntrySession>());
                    }
                }

                return new IdCacheSession(hash, aFunction, this);
            }
        }

        internal void DestroySession(Hash aSessionId)
        {
            using (iDisposeHandler.Lock)
            {
                lock (iCache)
                {
                    iSessions.Remove(aSessionId);
                }
            }
        }

        internal void SetValid(Hash aSessionId, IList<uint> aValid)
        {
            using (iDisposeHandler.Lock)
            {
                lock (iCache)
                {
                    Dictionary<uint, IdCacheEntrySession> c = iCache[aSessionId];
                    List<uint> keys = new List<uint>(c.Keys);
                    foreach (uint k in keys)
                    {
                        if (!aValid.Contains(k))
                        {
                            c.Remove(k);
                            --iCacheEntries;
                        }
                    }
                }
            }
        }

        internal IIdCacheEntry Entry(Hash aSessionId, uint aId)
        {
            using (iDisposeHandler.Lock)
            {
                lock (iCache)
                {
                    IdCacheEntrySession entry;
                    if (iCache[aSessionId].TryGetValue(aId, out entry))
                    {
                        iLastAccessed.Remove(entry);
                        iLastAccessed.Add(entry);
                        return entry;
                    }
                    return null;
                }
            }
        }

        internal IIdCacheEntry AddEntry(Hash aSessionId, uint aId, IIdCacheEntry aEntry)
        {
            using (iDisposeHandler.Lock)
            {
                IdCacheEntrySession entry;

                lock (iCache)
                {
                    if (!iCache[aSessionId].TryGetValue(aId, out entry))
                    {
                        entry = new IdCacheEntrySession(aSessionId, aId, aEntry);

                        if (iCacheEntries == iMaxCacheEntries)
                        {
                            RemoveEntry();
                        }

                        iCache[aSessionId].Add(aId, entry);
                        iLastAccessed.Add(entry);
                        ++iCacheEntries;
                    }
                }

                return entry;
            }
        }

        private void RemoveEntry()
        {
            using (iDisposeHandler.Lock)
            {
                lock (iCache)
                {
                    IdCacheEntrySession entry = iLastAccessed[0];
                    iCache[entry.SessionId].Remove(entry.Id);
                    iLastAccessed.RemoveAt(0);
                    --iCacheEntries;
                }
            }
        }
    }

    class IdCacheSession : IIdCacheSession
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly Hash iSessionId;
        private readonly Func<IList<uint>, Task<IEnumerable<IIdCacheEntry>>> iFunction;
        private readonly IdCache iCache;
        private readonly Semaphore iSemaphoreHigh;
        private readonly Queue<Task<IEnumerable<IIdCacheEntry>>> iQueueHigh;
        private readonly Semaphore iSemaphoreLow;
        private readonly Queue<Task<IEnumerable<IIdCacheEntry>>> iQueueLow;
        private readonly Task iTask;

        public IdCacheSession(Hash aSessionId, Func<IList<uint>, Task<IEnumerable<IIdCacheEntry>>> aFunction, IdCache aCache)
        {
            iDisposeHandler = new DisposeHandler();

            iSessionId = aSessionId;
            iFunction = aFunction;
            iCache = aCache;

            iSemaphoreHigh = new Semaphore(0, 2000);
            iQueueHigh = new Queue<Task<IEnumerable<IIdCacheEntry>>>();
            iSemaphoreLow = new Semaphore(0, 2000);
            iQueueLow = new Queue<Task<IEnumerable<IIdCacheEntry>>>();

            iTask = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    int result = Semaphore.WaitAny(new WaitHandle[] { iSemaphoreHigh, iSemaphoreLow });
                    Task<IEnumerable<IIdCacheEntry>> job = null;
                    switch (result)
                    {
                        case 0:
                            lock (iQueueHigh)
                            {
                                job = iQueueHigh.Dequeue();
                            }
                            break;
                        case 1:
                            lock (iQueueLow)
                            {
                                job = iQueueLow.Dequeue();
                            }
                            break;
                        default:
                            Do.Assert(true);
                            break;
                    }

                    if (job != null)
                    {
                        job.Start();
                        job.Wait();
                    }
                    else
                    {
                        break;
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();
            iCache.DestroySession(iSessionId);

            lock(iQueueLow)
            {
                iQueueLow.Enqueue(null);
            }
            iSemaphoreLow.Release();

            iTask.Wait();
        }

        public void SetValid(IList<uint> aValid)
        {
            using (iDisposeHandler.Lock)
            {
                if (aValid.Count > 0)
                {
                    iCache.SetValid(iSessionId, aValid);
                    foreach (uint id in aValid)
                    {
                        lock (iQueueLow)
                        {
                            iQueueLow.Enqueue(CreateJob(new List<uint>(new uint[] { id })));
                        }
                        iSemaphoreLow.Release();
                    }
                }
            }
        }

        public Task<IEnumerable<IIdCacheEntry>> Entries(IEnumerable<uint> aIds)
        {
            using (iDisposeHandler.Lock)
            {
                Task<IEnumerable<IIdCacheEntry>> task = Task<IEnumerable<IIdCacheEntry>>.Factory.StartNew(() =>
                {
                    Task<IEnumerable<IIdCacheEntry>> job = CreateJob(aIds);

                    lock (iQueueHigh)
                    {
                        iQueueHigh.Enqueue(job);
                    }
                    iSemaphoreHigh.Release();

                    return job.Result;
                });
                return task;
            }
        }

        private Task<IEnumerable<IIdCacheEntry>> CreateJob(IEnumerable<uint> aIds)
        {
            Task<IEnumerable<IIdCacheEntry>> task = new Task<IEnumerable<IIdCacheEntry>>(() =>
            {
                List<IIdCacheEntry> entries = new List<IIdCacheEntry>();
                List<uint> ids = new List<uint>();

                // find all entries currently in cache and build a list of ids required to be fetched
                foreach (uint id in aIds)
                {
                    IIdCacheEntry entry = iCache.Entry(iSessionId, id);
                    if (entry == null)
                    {
                        ids.Add(id);
                    }
                    entries.Add(entry);
                }

                if (ids.Count == 0)
                {
                    return entries;
                }

                // fetch missing ids
                IEnumerable<IIdCacheEntry> result = iFunction(ids).Result;

                // add retrieved ids to cache
                uint index = 0;
                foreach (IIdCacheEntry e in result)
                {
                    uint id = ids.ElementAt((int)index);
                    IIdCacheEntry entry = iCache.AddEntry(iSessionId, id, e);
                    entries[aIds.ToList().IndexOf(ids[(int)index])] = entry;
                    ++index;
                }

                return entries;
            });
            return task;
        }
    }

    class IdCacheEntry : IIdCacheEntry
    {
        private readonly IMediaMetadata iMetadata;
        private readonly string iUri;

        public IdCacheEntry(IMediaMetadata aMetadata, string aUri)
        {
            iMetadata = aMetadata;
            iUri = aUri;
        }

        public IMediaMetadata Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public string Uri
        {
            get
            {
                return iUri;
            }
        }
    }

    class IdCacheEntrySession : IIdCacheEntry
    {
        private readonly Hash iSessionId;
        private readonly uint iId;
        private readonly IIdCacheEntry iCacheEntry;

        public IdCacheEntrySession(Hash aSessionId, uint aId, IIdCacheEntry aCacheEntry)
        {
            iSessionId = aSessionId;
            iId = aId;
            iCacheEntry = aCacheEntry;
        }

        public Hash SessionId
        {
            get
            {
                return iSessionId;
            }
        }

        public uint Id
        {
            get
            {
                return iId;
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                return iCacheEntry.Metadata;
            }
        }

        public string Uri
        {
            get
            {
                return iCacheEntry.Uri;
            }
        }
    }
}
