using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    // This class should be used to proxy a watchable in a lower layer up to a higher layer.
    // For example, if a L2 object has a Watchable that is not directly required in L3 but
    // needs to be passed up to L4, the a WatchableProxy should be used in L3 to achieve this,
    // rather than making the L2 Watchable object accessible to L4
    public class WatchableProxy<T> : IWatchable<T>, IWatcher<T>, IDisposable
    {
        public WatchableProxy(IWatchable<T> aWatchable)
        {
            iWatchable = aWatchable;
            aWatchable.AddWatcher(this);
        }

        public void Detach()
        {
            // detach this proxy from its wrapped IWatchable<T>
            iWatchable.RemoveWatcher(this);
        }

        public void Dispose()
        {
            if (iProxy == null)
            {
                throw new ObjectDisposedException("WatchableProxy<T>.Dispose");
            }

            iProxy.Dispose();
            iProxy = null;
        }

        // IWatchable<T>

        public string Id
        {
            get { return iProxy.Id; }
        }

        public void AddWatcher(IWatcher<T> aWatcher)
        {
            iProxy.AddWatcher(aWatcher);
        }

        public void RemoveWatcher(IWatcher<T> aWatcher)
        {
            iProxy.RemoveWatcher(aWatcher);
        }

        // IWatcher<T>

        public void ItemOpen(string aId, T aValue)
        {
            iProxy = new Watchable<T>(iWatchable.WatchableThread, iWatchable.Id + "(Proxy)", aValue);
        }

        public void ItemUpdate(string aId, T aValue, T aPrevious)
        {
            iProxy.Update(aValue);
        }

        public void ItemClose(string aId, T aValue)
        {
        }

        public IWatchableThread WatchableThread
        {
            get
            {
                return iProxy.WatchableThread;
            }
        }

        private IWatchable<T> iWatchable;
        private Watchable<T> iProxy;
    }

    public interface IHouse
    {
        IWatchableUnordered<IRoom> Rooms { get; }

        void Refresh();
    }

    public enum EStandby
    {
        eOn,
        eMixed,
        eOff
    }

    public interface IVolume
    {
        bool Enabled { get; }
        bool Mute { get; }
        uint Volume { get; }
    }

    public interface ITime
    {
        bool Enabled { get; }
        uint Duration { get; }
        uint Seconds { get; }
        uint TrackCount { get; }
    }

    public interface IInfo
    {
        bool Enabled { get; }
        uint Bitdepth { get; }
        uint Bitrate { get; }
        string CodecName { get; }
        uint DetailsCount { get; }
        uint Duration { get; }
        bool Lossless { get; }
        string Metadata { get; }
        string Metatext { get; }
        uint MetatextCount { get; }
        uint SampleRate { get; }
        uint TrackCount { get; }
        string Uri { get; }
    }

    public interface IZone
    {
        bool Enabled { get; }
        IRoom Room { get; }
    }

    public interface IRoom
    {
        IWatchable<string> Name { get; }
        IWatchable<EStandby> Standby { get; }
        IWatchable<IVolume> Volume { get; }
        IWatchable<ISource> Current { get; }
        IWatchable<ITime> Time { get; }
        IWatchable<IInfo> Info { get; }
        IWatchableUnordered<IWatchableSource> Sources { get; }
        IWatchable<IZone> Zone { get; }
        IWatchableUnordered<IRoom> Listeners { get; } 

        void SetStandby(uint aIndex, bool aValue);
        void SetMute(bool aValue);
        void SetVolume(uint Value);
        void VolumeInc();
        void VolumeDec();
    }

    public interface IWatchableSource : IWatchable<ISource>
    {
        void Select();
    }

    public interface ISource
    {
        string Name { get; }
        string Group { get; }
        string Type { get; }
    }
}
