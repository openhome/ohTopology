using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IPlaylistWriter
    {
        Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata, bool aPlay);
        Task<uint> InsertNext(string aUri, IMediaMetadata aMetadata, bool aPlay);
        Task<uint> InsertEnd(string aUri, IMediaMetadata aMetadata, bool aPlay);
        Task DeleteId(uint aValue);
        Task DeleteAll();
    }

    class PlaylistWriter : IPlaylistWriter
    {
        private IProxyPlaylist iPlaylist;

        public PlaylistWriter(IProxyPlaylist aPlaylist)
        {
            iPlaylist = aPlaylist;
        }

        public Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata, bool aPlay)
        {
            Task<uint> t1 = Task<uint>.Factory.StartNew(() =>
            {
                Task<uint> t2 = iPlaylist.Insert(aAfterId, aUri, aMetadata);
                t2.ContinueWith((t) =>
                {
                    uint id = t.Result;
                    if (aPlay)
                    {
                        iPlaylist.SeekId(id);
                    }
                });
                return t2.Result;
            });
            return t1;
        }

        public Task<uint> InsertNext(string aUri, IMediaMetadata aMetadata, bool aPlay)
        {
            Task<uint> t1 = Task<uint>.Factory.StartNew(() =>
            {
                Task<uint> t2 = iPlaylist.InsertNext(aUri, aMetadata);
                t2.ContinueWith((t) =>
                {
                    uint id = t.Result;
                    if (aPlay)
                    {
                        iPlaylist.SeekId(id);
                    }
                });
                return t2.Result;
            });
            return t1;
        }

        public Task<uint> InsertEnd(string aUri, IMediaMetadata aMetadata, bool aPlay)
        {
            Task<uint> t1 = Task<uint>.Factory.StartNew(() =>
            {
                Task<uint> t2 = iPlaylist.InsertEnd(aUri, aMetadata);
                t2.ContinueWith((t) =>
                {
                    uint id = t.Result;
                    if (aPlay)
                    {
                        iPlaylist.SeekId(id);
                    }
                });
                return t2.Result;
            });
            return t1;
        }

        public Task Delete(IMediaPreset aMediaPreset)
        {
            return iPlaylist.Delete(aMediaPreset);
        }

        public Task DeleteAll()
        {
            return iPlaylist.DeleteAll();
        }
    }
}
