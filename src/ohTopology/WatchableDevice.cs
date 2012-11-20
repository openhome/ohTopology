using System;

namespace OpenHome.Net.ControlPoint
{
    public interface IWatchableDevice
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

        public CpDevice Device
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("WatchableDevice.Device");
                    }

                    return iDevice;
                }
            }
        }

        protected object iLock;
        protected bool iDisposed;

        protected CpDevice iDevice;
    }

    internal class DisposableWatchableDevice : WatchableDevice, IDisposable
    {
        public DisposableWatchableDevice(CpDevice aDevice)
            : base(aDevice)
        {
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("DisposableWatchableDevice.Dispose");
                }

                iDevice.RemoveRef();
                iDevice = null;

                iDisposed = true;
            }
        }
    }

    public class MockWatchableDevice : IWatchableDevice
    {
        public MockWatchableDevice(string aUdn)
        {
            iLock = new object();
            iDisposed = false;

            iUdn = aUdn;
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

        protected object iLock;
        protected bool iDisposed;

        private string iUdn;
    }

    internal class DisposableMockWatchableDevice : MockWatchableDevice, IDisposable
    {
        public DisposableMockWatchableDevice(string aUdn)
            : base(aUdn)
        {
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("DisposableMockWatchableDevice.Dispose");
                }

                iDisposed = true;
            }
        }
    }
}
