using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public interface IPlaylistWriter
    {
        Task<uint> Insert(uint aAfterId, string aUri, IMediaMetadata aMetadata, bool aPlay);
        Task<uint> InsertNext(string aUri, IMediaMetadata aMetadata, bool aPlay);
        Task<uint> InsertEnd(string aUri, IMediaMetadata aMetadata, bool aPlay);
        Task MakeRoomForInsert(uint aCount);
        Task Delete(IMediaPreset aValue);
        Task DeleteAll();
    }

    class PlaylistWriter : IPlaylistWriter, IDisposable
    {
        private readonly IWatchableThread iThread;
        private readonly IProxyPlaylist iPlaylist;
        private IMediaPreset iSourcePreset;

        public PlaylistWriter(IWatchableThread aThread, ITopology4Source aSource, IProxyPlaylist aPlaylist)
        {
            iThread = aThread;
            iSourcePreset = aSource.CreatePreset();
            iPlaylist = aPlaylist;
        }

        public void Dispose()
        {
            iSourcePreset.Dispose();
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
                        iThread.Schedule(() =>
                        {
                            iSourcePreset.Play();
                            iPlaylist.SeekId(id);
                        });
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
                        iThread.Schedule(() =>
                        {
                            iSourcePreset.Play();
                            iPlaylist.SeekId(id);
                        });
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
                        iThread.Schedule(() =>
                        {
                            iSourcePreset.Play();
                            iPlaylist.SeekId(id);
                        });
                    }
                });
                return t2.Result;
            });
            return t1;
        }

        public Task MakeRoomForInsert(uint aCount)
        {
            return iPlaylist.MakeRoomForInsert(aCount);
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
