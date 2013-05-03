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

    public class SourceController
    {
        public static ISourceController Create(IWatchableThread aThread, ITopology4Source aSource)
        {
            if (aSource.Type == "Radio")
            {
                Console.WriteLine("Create RADIO source controller");
                return new SourceControllerRadio(aThread, aSource);
            }

            return null;
        }
    }

    public class SourceControllerRadio : IWatcher<string>, ISourceController
    {
        public SourceControllerRadio(IWatchableThread aThread, ITopology4Source aSource)
        {
            iLock = new object();
            iDisposed = false;

            iSource = aSource;

            iCanPause = new Watchable<bool>(aThread, string.Format("CanPause({0}:{1})", aSource.Group, aSource.Name), false);
            iCanSeek = new Watchable<bool>(aThread, string.Format("CanSeek({0}:{1})", aSource.Group, aSource.Name), false);
            iTransportState = new Watchable<string>(aThread, string.Format("TransportState({0}:{1})", aSource.Group, aSource.Name), string.Empty);

            aSource.Device.Create<ServiceRadio>((IWatchableDevice device, ServiceRadio radio) =>
            {
                lock (iLock)
                {
                    if (!iDisposed)
                    {
                        iRadio = radio;

                        iRadio.TransportState.AddWatcher(this);
                    }
                    else
                    {
                        radio.Dispose();
                    }
                }
            });
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("SourceControllerRadio.Dispose");
                }

                if (iRadio != null)
                {
                    iRadio.TransportState.RemoveWatcher(this);

                    iRadio.Dispose();
                    iRadio = null;
                }

                iCanPause.Dispose();
                iCanPause = null;

                iCanSeek.Dispose();
                iCanSeek = null;

                iDisposed = true;
            }
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
            get 
            {
                return iTransportState;
            }
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
            iRadio.Play();
        }

        public void Pause()
        {
            iRadio.Pause();
        }

        public void Stop()
        {
            iRadio.Stop();
        }

        public bool CanSkip
        {
            get
            {
                return true;
            }
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
            get
            {
                return iCanSeek;
            }
        }

        public void Seek(uint aSeconds)
        {
            iRadio.SeekSecondsAbsolute(aSeconds);
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
        
        public void ItemOpen(string aId, string aValue)
        {
            iTransportState.Update(aValue);
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iTransportState.Update(aValue);
        }

        public void ItemClose(string aId, string aValue)
        {
        }

        private object iLock;
        private bool iDisposed;

        private ITopology4Source iSource;
        private ServiceRadio iRadio;

        private Watchable<bool> iCanPause;
        private Watchable<bool> iCanSeek;
        private Watchable<string> iTransportState;
    }
}
