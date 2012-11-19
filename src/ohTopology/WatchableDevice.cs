using System;

namespace OpenHome.Net.ControlPoint
{
    public interface IWatchableDevice : IDisposable
    {
        string Udn { get; }
        bool GetAttribute(string aKey, out string aValue);
    }

    public class WatchableDevice : IWatchableDevice
    {
        public WatchableDevice(CpDevice aDevice)
        {
            iLock = new object();
            iDisposed = false;

            iDevice = aDevice;
            iDevice.AddRef();
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("WatchableDevice.Dispose");
                }

                iDevice.RemoveRef();
                iDevice = null;

                iDisposed = true;
            }
        }

        public string Udn
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("WatchableDevice.Udn");
                    }

                    return iDevice.Udn();
                }
            }
        }

        public bool GetAttribute(string aKey, out string aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("WatchableDevice.GetAttribute");
                }

                return iDevice.GetAttribute(aKey, out aValue);
            }
        }

        private object iLock;
        private bool iDisposed;

        private CpDevice iDevice;
    }

    public class MockWatchableDevice : IWatchableDevice
    {
        public MockWatchableDevice(string aUdn)
        {
            iLock = new object();
            iDisposed = false;

            iUdn = aUdn;
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("MockWatchableDevice.Dispose");
                }

                iDisposed = true;
            }
        }

        public string Udn
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("MockWatchableDevice.Udn");
                    }
                    return iUdn;
                }
            }
        }

        public bool GetAttribute(string aKey, out string aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("MockWatchableDevice.GetAttribute");
                }

                throw new NotImplementedException();
            }
        }

        private object iLock;
        private bool iDisposed;

        private string iUdn;
    }
}
