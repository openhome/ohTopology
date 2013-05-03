using System;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface ISourceController : IDisposable
    {
        string Name { get; }

        bool HasInfoNext { get; }
        IWatchable<IInfoNext> InfoNext { get; }

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

        IWatchable<bool> HasVolume { get; }
        IWatchable<bool> Mute { get; }
        IWatchable<uint> Volume { get; }
        void SetMute(bool aMute);
        void SetVolume(uint aSeconds);
        void VolumeInc();
        void VolumeDec();
    }

    public class SourceControllerRadio : ISourceController
    {
        public SourceControllerRadio(IWatchableThread aThread, ITopology4Source aSource)
        {
            iSource = aSource;

            iCanPause = new Watchable<bool>(aThread, string.Format("CanPause({0}:{1})", aSource.Group, aSource.Name), false);
            iCanSeek = new Watchable<bool>(aThread, string.Format("CanSeek({0}:{1})", aSource.Group, aSource.Name), false);
            iCanSkip = new Watchable<bool>(aThread, string.Format("CanSkip({0}:{1})", aSource.Group, aSource.Name), false);
        }

        public void Dispose()
        {
            iCanPause.Dispose();
            iCanPause = null;

            iCanSeek.Dispose();
            iCanSeek = null;

            iCanSkip.Dispose();
            iCanSkip = null;
        }

        public string Name
        {
            get
            {
                return iSource.Name;
            }
        }

        public bool HasInfoNext
        {
            get
            {
                return false;
            }
        }

        public IWatchable<IInfoNext> InfoNext
        {
            get { throw new NotSupportedException(); }
        }

        public IWatchable<string> TransportState
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<bool> CanPause
        {
            get
            {
                return iCanPause;
            }
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public bool CanSkip
        {
            get { throw new NotImplementedException(); }
        }

        public void Previous()
        {
            throw new NotImplementedException();
        }

        public void Next()
        {
            throw new NotImplementedException();
        }

        public IWatchable<bool> CanSeek
        {
            get { throw new NotImplementedException(); }
        }

        public void Seek(uint aSeconds)
        {
            throw new NotImplementedException();
        }

        public IWatchable<bool> HasVolume
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<bool> Mute
        {
            get { throw new NotImplementedException(); }
        }

        public IWatchable<uint> Volume
        {
            get { throw new NotImplementedException(); }
        }

        public void SetMute(bool aMute)
        {
            throw new NotImplementedException();
        }

        public void SetVolume(uint aSeconds)
        {
            throw new NotImplementedException();
        }

        public void VolumeInc()
        {
            throw new NotImplementedException();
        }

        public void VolumeDec()
        {
            throw new NotImplementedException();
        }

        private ITopology4Source iSource;

        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<bool> iCanSkip;
    }
}
