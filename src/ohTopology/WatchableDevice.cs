using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IWatchableDevice
    {
        string Udn { get; }
        bool GetAttribute(string aKey, out string aValue);
        void Subscribe<T>(Action<IWatchableDevice, T> aCallback) where T : IWatchableService;
        void Unsubscribe<T>() where T : IWatchableService;
    }

    public class ServiceType<T> where T : IWatchableService
    {
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

        public void Subscribe<T>(Action<IWatchableDevice, T> aCallback) where T : IWatchableService
        {
            var type = new ServiceType<T>();

            throw new NotImplementedException();
        }

        public void Unsubscribe<T>() where T : IWatchableService
        {
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

    public class DisposableWatchableDevice : WatchableDevice, IDisposable
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

    public class MockWatchableDevice : IWatchableDevice, IMockable
    {
        public MockWatchableDevice(string aUdn)
        {
            iUdn = aUdn;
            iServices = new Dictionary<Type, IWatchableService>();
        }

        public string Udn
        {
            get
            {
                return iUdn;
            }
        }

        public bool GetAttribute(string aKey, out string aValue)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T>(Action<T> aCallback) where T : IWatchableService
        {
        }

        public void Subscribe<T>(Action<IWatchableDevice, T> aCallback) where T : IWatchableService
        {
            ServiceType<T> type = new ServiceType<T>();

            IWatchableService service;
            if (iServices.TryGetValue(type.GetType(), out service))
            {
                Task task = new Task(new Action(delegate { 
                    aCallback(this, (T)service);
                })
                );
                task.Start();

                return;
            }

            throw new NotSupportedException();
        }

        public void Unsubscribe<T>() where T : IWatchableService
        {
        }

        public void Add<T>(IWatchableService aService) where T : IWatchableService
        {
            ServiceType<T> type = new ServiceType<T>();
            iServices.Add(type.GetType(), aService);
        }

        internal bool HasService(Type aType)
        {
            foreach (Type s in iServices.Keys)
            {
                if (aType == s)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void Execute(IEnumerable<string> aValue)
        {
        }

        protected Dictionary<Type, IWatchableService> iServices;

        private string iUdn;
    }

    public class MockWatchableDs : MockWatchableDevice
    {
        public MockWatchableDs(IWatchableThread aThread, string aUdn)
            : base(aUdn)
        {
            // add a mock product service
            MockWatchableProduct product = new MockWatchableProduct(aThread, aUdn, "Main Room", "Mock DS", 0, "<SourceList><Source><Name>Playlist</Name><Type>Playlist</Type><Visible>true</Visible></Source><Source><Name>Radio</Name><Type>Radio</Type><Visible>true</Visible></Source><Source><Name>UPnP AV</Name><Type>UpnpAv</Type><Visible>false</Visible></Source><Source><Name>Songcast</Name><Type>Receiver</Type><Visible>true</Visible></Source><Source><Name>Net Aux</Name><Type>NetAux</Type><Visible>false</Visible></Source><Source><Name>Analog1</Name><Type>Analog</Type><Visible>true</Visible></Source><Source><Name>Analog2</Name><Type>Analog</Type><Visible>true</Visible></Source><Source><Name>Analog3</Name><Type>Analog</Type><Visible>true</Visible></Source><Source><Name>Phono</Name><Type>Analog</Type><Visible>true</Visible></Source><Source><Name>Front Aux</Name><Type>Analog</Type><Visible>true</Visible></Source><Source><Name>SPDIF1</Name><Type>Digital</Type><Visible>true</Visible></Source><Source><Name>SPDIF2</Name><Type>Digital</Type><Visible>true</Visible></Source><Source><Name>SPDIF3</Name><Type>Digital</Type><Visible>true</Visible></Source><Source><Name>TOSLINK1</Name><Type>Digital</Type><Visible>true</Visible></Source><Source><Name>TOSLINK2</Name><Type>Digital</Type><Visible>true</Visible></Source><Source><Name>TOSLINK3</Name><Type>Digital</Type><Visible>true</Visible></Source></SourceList>", true, "",
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", "");
            Add<Product>(product);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            base.Execute(aValue);

            string command = aValue.First();
            if (command == "product")
            {
                foreach (KeyValuePair<Type, IWatchableService> s in iServices)
                {
                    if (s.Key == new ServiceType<Product>().GetType())
                    {
                        MockWatchableProduct p = s.Value as MockWatchableProduct;
                        p.Execute(aValue.Skip(1));
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
