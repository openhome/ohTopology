using System;
using System.Threading.Tasks;

namespace OpenHome.Av
{
    public interface IPlaylistWriter
    {
        Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata);
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

        public Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata)
        {
            return iPlaylist.Insert(aAfterId, aUri, aMetadata);
        }

        public Task DeleteId(uint aValue)
        {
            return iPlaylist.DeleteId(aValue);
        }

        public Task DeleteAll()
        {
            return iPlaylist.DeleteAll();
        }
    }
}
