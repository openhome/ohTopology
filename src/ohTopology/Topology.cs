using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
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
