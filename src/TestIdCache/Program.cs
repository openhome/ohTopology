using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using OpenHome.Os.App;

using OpenHome.Av;

namespace TestIdCache
{
    class CacheWatcher
    {
        private class IdCacheEntry : IIdCacheEntry
        {
            private IMediaMetadata iMetadata;
            private string iUri;

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

        private uint iCount;

        public CacheWatcher()
        {
            iCount = 0;
        }

        public uint Count
        {
            get
            {
                return iCount;
            }
        }

        public Task<IEnumerable<IIdCacheEntry>> Entries(IEnumerable<uint> aIds)
        {
            return Task<IEnumerable<IIdCacheEntry>>.Factory.StartNew(() =>
            {
                List<IIdCacheEntry> results = new List<IIdCacheEntry>();
                foreach (uint id in aIds)
                {
                    iCount++;
                    MediaMetadata metadata = new MediaMetadata();
                    string uri = id.ToString();
                    results.Add(new IdCacheEntry(metadata, uri));
                }
                return results;
            });
        }
    }

    class Program
    {
        public class AssertError : Exception
        {
            public AssertError() : base("ASSERT") { }
        }

        static int Main(string[] args)
        {
            Network network = new Network(50);
            IIdCache cache = network.IdCache;
            CacheWatcher watcher = new CacheWatcher();

            IIdCacheSession session = cache.CreateSession("test", watcher.Entries);

            IEnumerable<IIdCacheEntry> result;
            uint index;

            // get 4 entries from cache and check our handler is called 4 times and results are as expected
            result = session.Entries(new List<uint>(new uint[] { 0, 1, 2, 3 })).Result;

            Assert(watcher.Count == 4);

            index = 0;
            foreach (IIdCacheEntry e in result)
            {
                Assert(e.Uri == index.ToString());
                ++index;
            }

            // get same 4 entries from cache and check our handler is not called and results are as expected
            result = session.Entries(new List<uint>(new uint[] { 0, 1, 2, 3 })).Result;

            Assert(watcher.Count == 4);

            index = 0;
            foreach (IIdCacheEntry e in result)
            {
                Assert(e.Uri == index.ToString());
                ++index;
            }

            // fill cache and see if we re-fetch the original list correctly
            List<uint> list = new List<uint>();
            for (uint i = 0; i < 50; ++i)
            {
                list.Add(i + 4);
            }
            result = session.Entries(list).Result;

            Assert(watcher.Count == 54);

            result = session.Entries(new List<uint>(new uint[] { 0, 1, 2, 3 })).Result;

            Assert(watcher.Count == 58);

            index = 0;
            foreach (IIdCacheEntry e in result)
            {
                Assert(e.Uri == index.ToString());
                ++index;
            }

            session.Dispose();

            network.Dispose();

            return 0;
        }

        private static void Assert(bool aExpression)
        {
            if (!aExpression)
            {
                Console.WriteLine("Failed");
                throw new AssertError();
            }
            else
            {
                Console.Write('.');
            }
        }
    }
}
