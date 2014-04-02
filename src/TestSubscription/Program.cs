using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenHome.Av;

namespace TestSubscription
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    class TestDevice : IDevice
    {
        private string iUdn;
        public TestDevice()
        {
            iUdn = Guid.NewGuid().ToString();
        }

        public string Udn
        {
            get { return iUdn; }
        }

        public void Create<T>(Action<T> aCallback) where T : IProxy
        {
            aCallback((T)new TestProxy(this));
        }

        public void Join(Action aAction)
        {
        }

        public void Unjoin(Action aAction)
        {
        }
    }

    class TestProxy : IProxy
    {
        private string iID = Guid.NewGuid().ToString();
        private IDevice iDevice;
        public TestProxy(IDevice aDevice)
        {
            iDevice = aDevice;
            Console.WriteLine("Created proxy {0} for device {1}");
        }

        public IDevice Device
        {
            get { return iDevice; }
        }

        public void Dispose()
        {
        }
    }

    class TestService : Service
    {
        public override IProxy OnCreate(IDevice aDevice)
        {
            throw new NotImplementedException();
        }
    }
}
