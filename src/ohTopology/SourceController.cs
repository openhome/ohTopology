using System;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ISourceController : IDisposable
    {
        string Name { get; }

        bool HasInfoNext { get; }
        IWatchable<IInfoMetadata> InfoNext { get; }

        IWatchable<bool> HasSourceControl { get; }
        IWatchable<string> TransportState { get; }

        IWatchable<bool> CanPause { get; }
        void Play();
        void Pause();
        void Stop();

        bool CanSkip { get; }
        void Previous();
        void Next();

        //IWatchable<ITime> Create();

        IWatchable<bool> CanSeek { get; }
        void Seek(uint aSeconds);

        /*IWatchable<bool> HasVolume { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Volume { get; }
        void SetMute(bool aMute);
        void SetVolume(uint aSeconds);
        void VolumeInc();
        void VolumeDec();*/
    }

    public class SourceController
    {
        public static ISourceController Create(IWatchableThread aThread, ITopology4Source aSource, Watchable<bool> aHasSourceControl, Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<string> aTransportState, Watchable<bool> aCanPause, Watchable<bool> aCanSkip, Watchable<bool> aCanSeek)
        {
            if (aSource.Type == "Playlist")
            {
                return new SourceControllerPlaylist(aThread, aSource, aHasSourceControl, aHasInfoNext, aInfoNext, aTransportState, aCanPause, aCanSeek, aCanSkip);
            }
            else if (aSource.Type == "Radio")
            {
                return new SourceControllerRadio(aThread, aSource, aHasSourceControl, aHasInfoNext, aInfoNext, aTransportState, aCanPause, aCanSeek, aCanSkip);
            }
            else if (aSource.Type == "Receiver")
            {
                return new SourceControllerReceiver(aThread, aSource, aHasSourceControl, aHasInfoNext, aInfoNext, aTransportState, aCanPause, aCanSeek, aCanSkip);
            }

            return null;
        }
    }
}
