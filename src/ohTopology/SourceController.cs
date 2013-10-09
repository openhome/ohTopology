using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ISourceController : IDisposable
    {
        IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot { get; }

        void Play();
        void Pause();
        void Stop();

        void Previous();
        void Next();

        void Seek(uint aSeconds);

        void SetRepeat(bool aValue);
        void SetShuffle(bool aValue);
    }

    public class SourceController
    {
        public static ISourceController Create(IWatchableThread aThread, ITopology4Source aSource, Watchable<bool> aHasSourceControl,
            Watchable<bool> aHasInfoNext, Watchable<IInfoMetadata> aInfoNext, Watchable<bool> aHasContainer, Watchable<string> aTransportState, Watchable<bool> aCanPause,
            Watchable<bool> aCanSkip, Watchable<bool> aCanSeek, Watchable<bool> aHasPlayMode, Watchable<bool> aShuffle, Watchable<bool> aRepeat)
        {
            if (aSource.Type == "Playlist")
            {
                return new SourceControllerPlaylist(aThread, aSource, aHasSourceControl, aHasInfoNext, aInfoNext, aHasContainer, aTransportState, aCanPause, aCanSkip, aCanSeek, aHasPlayMode, aShuffle, aRepeat);
            }
            else if (aSource.Type == "Radio")
            {
                return new SourceControllerRadio(aSource, aHasSourceControl, aHasInfoNext, aInfoNext, aHasContainer, aTransportState, aCanPause, aCanSkip, aCanSeek, aHasPlayMode, aShuffle, aRepeat);
            }
            else if (aSource.Type == "Receiver")
            {
                return new SourceControllerReceiver(aSource, aHasSourceControl, aHasInfoNext, aInfoNext, aHasContainer, aTransportState, aCanPause, aCanSkip, aCanSeek, aHasPlayMode, aShuffle, aRepeat);
            }
            else if (aSource.Type == "Disc")
            {
                return new SourceControllerDisc(aSource, aHasSourceControl, aHasInfoNext, aInfoNext, aHasContainer, aTransportState, aCanPause, aCanSkip, aCanSeek, aHasPlayMode, aShuffle, aRepeat);
            }
            /*else if (aSource.Type == "NetAux" || aSource.Type == "UpnpAv" || aSource.Type == "Analog" || aSource.Type == "Digital" || aSource.Type == "Hdmi")
            {
                return new SourceControllerExternal(aSource, aHasSourceControl, aHasInfoNext, aInfoNext, aTransportState, aCanPause, aCanSeek, aCanSkip, aHasPlayMode, aShuffle, aRepeat);
            }*/

            return null;
        }
    }
}
