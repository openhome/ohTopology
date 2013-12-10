using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Os;

namespace OpenHome.Av
{
    public interface IProxyVolkano : IProxy
    {
        string MacAddress { get; }
        string SoftwareVersion { get; }
        bool SoftwareUpdateAvailable { get; }
        string SoftwareUpdateVersion { get; }
        string ProductId { get; }
    }

    public abstract class ServiceVolkano : Service
    {
        protected ServiceVolkano(INetwork aNetwork, IInjectorDevice aDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyVolkano(this, aDevice);
        }

        // Volkano methods

        public string MacAddress {
            get {
                return iMacAddress;
            }
        }

        public string SoftwareVersion {
            get {
                return iSoftwareVersion;
            }
        }

        public bool SoftwareUpdateAvailable {
            get {
                return iSoftwareUpdateAvailable;
            }
        }

        public string SoftwareUpdateVersion {
            get {
                return iSoftwareUpdateVersion;
            }
        }

        public string ProductId {
            get {
                return iProductId;
            }
        }

        protected string iMacAddress;
        protected string iSoftwareVersion;
        protected bool iSoftwareUpdateAvailable;
        protected string iSoftwareUpdateVersion;
        protected string iProductId;
    }

    class ServiceVolkanoNetwork : ServiceVolkano
    {
        public ServiceVolkanoNetwork(INetwork aNetwork, IInjectorDevice aDevice, CpDevice aCpDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iCpDevice = aCpDevice;
            iCpDevice.AddRef();

            iService = new CpProxyLinnCoUkVolkano1(aCpDevice);
        }

        public override void Dispose()
        {
            base.Dispose();

            iService.Dispose();
            iService = null;

            iCpDevice.RemoveRef();
        }

        protected override Task OnSubscribe()
        {
            Do.Assert(iSubscribedSourceProductId == null);
            Do.Assert(iSubscribedSourceMacAddress == null);
            Do.Assert(iSubscribedSourceSoftwareVersion == null);
            Do.Assert(iSubscribedSourceSoftwareUpdate == null);

            iSubscribedSourceProductId = new TaskCompletionSource<bool>();
            iSubscribedSourceMacAddress = new TaskCompletionSource<bool>();
            iSubscribedSourceSoftwareVersion = new TaskCompletionSource<bool>();
            iSubscribedSourceSoftwareUpdate = new TaskCompletionSource<bool>();

            iService.BeginProductId((ptr) =>
            {
                try
                {
                    iService.EndProductId(ptr, out iProductId);
                    if (!iSubscribedSourceProductId.Task.IsCanceled)
                    {
                        iSubscribedSourceProductId.SetResult(true);
                    }
                }
                catch (ProxyError e)
                {
                    if (!iSubscribedSourceProductId.Task.IsCanceled)
                    {
                        iSubscribedSourceProductId.SetException(e);
                    }
                }
            });

            iService.BeginMacAddress((ptr) => {
                try {
                    iService.EndMacAddress(ptr, out iMacAddress);
                    if (!iSubscribedSourceMacAddress.Task.IsCanceled) {
                        iSubscribedSourceMacAddress.SetResult(true);
                    }
                }
                catch (ProxyError e) {
                    if (!iSubscribedSourceMacAddress.Task.IsCanceled) {
                        iSubscribedSourceMacAddress.SetException(e);
                    }
                }
            });

            iService.BeginSoftwareVersion((ptr) => {
                try {
                    iService.EndSoftwareVersion(ptr, out iSoftwareVersion);
                    if (!iSubscribedSourceSoftwareVersion.Task.IsCanceled) {
                        iSubscribedSourceSoftwareVersion.SetResult(true);
                    }
                }
                catch (ProxyError e) {
                    if (!iSubscribedSourceSoftwareVersion.Task.IsCanceled) {
                        iSubscribedSourceSoftwareVersion.SetException(e);
                    }
                }
            });

            iService.BeginSoftwareUpdate((ptr) => {
                try {
                    iService.EndSoftwareUpdate(ptr, out iSoftwareUpdateAvailable, out iSoftwareUpdateVersion);
                    if (!iSubscribedSourceSoftwareUpdate.Task.IsCanceled) {
                        iSubscribedSourceSoftwareUpdate.SetResult(true);
                    }
                }
                catch (ProxyError e) {
                    if (!iSubscribedSourceSoftwareUpdate.Task.IsCanceled) {
                        iSubscribedSourceSoftwareUpdate.SetException(e);
                    }
                }
            });
                
            return Task.Factory.ContinueWhenAll(
                new Task[] { iSubscribedSourceProductId.Task, iSubscribedSourceMacAddress.Task, iSubscribedSourceSoftwareVersion.Task, iSubscribedSourceSoftwareUpdate.Task },
                (tasks) => { Task.WaitAll(tasks); });
        }

        protected override void OnCancelSubscribe()
        {
            if (iSubscribedSourceProductId != null)
            {
                iSubscribedSourceProductId.TrySetCanceled();
            }
            if (iSubscribedSourceMacAddress != null) {
                iSubscribedSourceMacAddress.TrySetCanceled();
            }
            if (iSubscribedSourceSoftwareVersion != null)
            {
                iSubscribedSourceSoftwareVersion.TrySetCanceled();
            }
            if (iSubscribedSourceSoftwareUpdate != null) {
                iSubscribedSourceSoftwareUpdate.TrySetCanceled();
            }
        }

        protected override void OnUnsubscribe()
        {
            iSubscribedSourceProductId = null;
            iSubscribedSourceMacAddress = null;
            iSubscribedSourceSoftwareVersion = null;
            iSubscribedSourceSoftwareUpdate = null;
        }

        private readonly CpDevice iCpDevice;
        private TaskCompletionSource<bool> iSubscribedSourceProductId;
        private TaskCompletionSource<bool> iSubscribedSourceMacAddress;
        private TaskCompletionSource<bool> iSubscribedSourceSoftwareVersion;
        private TaskCompletionSource<bool> iSubscribedSourceSoftwareUpdate;
        private CpProxyLinnCoUkVolkano1 iService;
    }

    class ServiceVolkanoMock : ServiceVolkano
    {
        public ServiceVolkanoMock(INetwork aNetwork, IInjectorDevice aDevice, string aProductId, string aMacAddress, string aSoftwareVersion, bool aSoftwareUpdateAvailable, string aSoftwareUpdateVersion, ILog aLog)
            : base(aNetwork, aDevice, aLog) {
            iProductId = aProductId;
            iMacAddress = aMacAddress;
            iSoftwareVersion = aSoftwareVersion;
            iSoftwareUpdateAvailable = aSoftwareUpdateAvailable;
            iSoftwareUpdateVersion = aSoftwareUpdateVersion;
        }

        public override void Execute(IEnumerable<string> aValue) {
            string command = aValue.First().ToLowerInvariant();
            if (command == "productid") {
                IEnumerable<string> value = aValue.Skip(1);
                iProductId = string.Join(" ", value);
            }
            else if (command == "macaddress") {
                IEnumerable<string> value = aValue.Skip(1);
                iMacAddress = string.Join(" ", value);
            }
            else if (command == "softwareversion") {
                IEnumerable<string> value = aValue.Skip(1);
                iSoftwareVersion = string.Join(" ", value);
            }
            else if (command == "softwareupdateavailable") {
                IEnumerable<string> value = aValue.Skip(1);
                iSoftwareUpdateAvailable = bool.Parse(value.First());
            }
            else if (command == "softwareupdateversion") {
                IEnumerable<string> value = aValue.Skip(1);
                iSoftwareUpdateVersion = string.Join(" ", value);
            }
            else {
                throw new NotSupportedException();
            }
        }
    }

    public class ProxyVolkano : Proxy<ServiceVolkano>, IProxyVolkano
    {
        public ProxyVolkano(ServiceVolkano aService, IDevice aDevice)
            : base(aService, aDevice)
        {
        }

        public string MacAddress {
            get {
                return iService.MacAddress;
            }
        }

        public string SoftwareVersion {
            get {
                return iService.SoftwareVersion;
            }
        }

        public bool SoftwareUpdateAvailable {
            get {
                return iService.SoftwareUpdateAvailable;
            }
        }

        public string SoftwareUpdateVersion {
            get {
                return iService.SoftwareUpdateVersion;
            }
        }

        public string ProductId {
            get {
                return iService.ProductId;
            }
        }
    }
}
