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
    public interface IProxyProduct : IProxy
    {
        IWatchable<string> Room { get; }
        IWatchable<string> Name { get; }
        IWatchable<uint> SourceIndex { get; }
        IWatchable<string> SourceXml { get; }
        IWatchable<bool> Standby { get; }
        IWatchable<string> Registration { get; }

        Task SetSourceIndex(uint aValue);
        Task SetSourceIndexByName(string aValue);
        Task SetStandby(bool aValue);
        Task SetRegistration(string aValue);

        string Attributes { get; }
        string ManufacturerImageUri { get; }
        string ManufacturerInfo { get; }
        string ManufacturerName { get; }
        string ManufacturerUrl { get; }
        string ModelImageUri { get; }
        string ModelInfo { get; }
        string ModelName { get; }
        string ModelUrl { get; }
        string ProductImageUri { get; }
        string ProductInfo { get; }
        string ProductUrl { get; }
        string ProductId { get; }
    }

    public abstract class ServiceProduct : Service
    {
        protected ServiceProduct(INetwork aNetwork, IInjectorDevice aDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iRoom = new Watchable<string>(aNetwork, "Room", string.Empty);
            iName = new Watchable<string>(aNetwork, "Name", string.Empty);
            iSourceIndex = new Watchable<uint>(aNetwork, "SourceIndex", 0);
            iSourceXml = new Watchable<string>(aNetwork, "SourceXml", string.Empty);
            iStandby = new Watchable<bool>(aNetwork, "Standby", false);
            iRegistration = new Watchable<string>(aNetwork, "Registration", string.Empty);
        }

        public override void Dispose()
        {
            base.Dispose();

            iRoom.Dispose();
            iRoom = null;

            iName.Dispose();
            iName = null;

            iSourceIndex.Dispose();
            iSourceIndex = null;

            iSourceXml.Dispose();
            iSourceXml = null;

            iStandby.Dispose();
            iStandby = null;

            iRegistration.Dispose();
            iRegistration = null;
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyProduct(this, aDevice);
        }

        // IServiceProduct methods

        public IWatchable<string> Room
        {
            get
            {
                return iRoom;
            }
        }

        public IWatchable<string> Name
        {
            get 
            {
                return iName;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get 
            {
                return iSourceIndex;
            }
        }

        public IWatchable<string> SourceXml
        {
            get
            {
                return iSourceXml;
            }
        }

        public IWatchable<bool> Standby
        {
            get
            {
                return iStandby;
            }
        }

        public IWatchable<string> Registration
        {
            get
            {
                return iRegistration;
            }
        }

        public abstract Task SetSourceIndex(uint aValue);
        public abstract Task SetSourceIndexByName(string aValue);
        public abstract Task SetStandby(bool aValue);
        public abstract Task SetRegistration(string aValue);

        // IProduct methods

        public string Attributes
        {
            get
            {
                return iAttributes;
            }
        }

        public string ManufacturerImageUri
        {
            get
            {
                return iManufacturerImageUri;
            }
        }

        public string ManufacturerInfo
        {
            get
            {
                return iManufacturerInfo;
            }
        }

        public string ManufacturerName
        {
            get
            {
                return iManufacturerName;
            }
        }

        public string ManufacturerUrl
        {
            get
            {
                return iManufacturerUrl;
            }
        }

        public string ModelImageUri
        {
            get
            {
                return iModelImageUri;
            }
        }

        public string ModelInfo
        {
            get
            {
                return iModelInfo;
            }
        }

        public string ModelName
        {
            get
            {
                return iModelName;
            }
        }

        public string ModelUrl
        {
            get
            {
                return iModelUrl;
            }
        }

        public string ProductImageUri
        {
            get
            {
                return iProductImageUri;
            }
        }

        public string ProductInfo
        {
            get
            {
                return iProductInfo;
            }
        }

        public string ProductUrl
        {
            get
            {
                return iProductUrl;
            }
        }

        public string ProductId
        {
            get
            {
                return iProductId;
            }
        }

        protected string iAttributes;
        protected string iManufacturerImageUri;
        protected string iManufacturerInfo;
        protected string iManufacturerName;
        protected string iManufacturerUrl;
        protected string iModelImageUri;
        protected string iModelInfo;
        protected string iModelName;
        protected string iModelUrl;
        protected string iProductImageUri;
        protected string iProductInfo;
        protected string iProductUrl;
        protected string iProductId;

        protected Watchable<string> iRoom;
        protected Watchable<string> iName;
        protected Watchable<uint> iSourceIndex;
        protected Watchable<string> iSourceXml;
        protected Watchable<bool> iStandby;
        protected Watchable<string> iRegistration;
    }

    class ServiceProductNetwork : ServiceProduct
    {
        public ServiceProductNetwork(INetwork aNetwork, IInjectorDevice aDevice, CpDevice aCpDevice, ILog aLog  )
            : base(aNetwork, aDevice, aLog)
        {
            iCpDevice = aCpDevice;
            iCpDevice.AddRef();

            iService = new CpProxyAvOpenhomeOrgProduct1(aCpDevice);
            iServiceConfiguration = new CpProxyLinnCoUkConfiguration1(aCpDevice);

            string value;
            if (aCpDevice.GetAttribute("Upnp.Service.linn-co-uk.Volkano", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    iServiceVolkano = new CpProxyLinnCoUkVolkano1(aCpDevice);
                }
            }

            iService.SetPropertyProductRoomChanged(HandleRoomChanged);
            iService.SetPropertyProductNameChanged(HandleNameChanged);
            iService.SetPropertySourceIndexChanged(HandleSourceIndexChanged);
            iService.SetPropertySourceXmlChanged(HandleSourceXmlChanged);
            iService.SetPropertyStandbyChanged(HandleStandbyChanged);

            iServiceConfiguration.SetPropertyParameterXmlChanged(HandleParameterXmlChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
            iServiceConfiguration.SetPropertyInitialEvent(HandleInitialEventConfiguration);
        }

        public override void Dispose()
        {
            base.Dispose();

            iService.Dispose();
            iService = null;

            iServiceConfiguration.Dispose();
            iServiceConfiguration = null;

            if (iServiceVolkano != null)
            {
                iServiceVolkano.Dispose();
                iServiceVolkano = null;
            }

            iCpDevice.RemoveRef();
        }

        protected override Task OnSubscribe()
        {
            Do.Assert(iSubscribedSource == null);
            Do.Assert(iSubscribedConfigurationSource == null);
            Do.Assert(iSubscribedVolkanoSource == null);

            TaskCompletionSource<bool> volkano = new TaskCompletionSource<bool>();
            iSubscribedSource = new TaskCompletionSource<bool>();
            iSubscribedConfigurationSource = new TaskCompletionSource<bool>();
            iSubscribedVolkanoSource = volkano;

            iService.Subscribe();
            iServiceConfiguration.Subscribe();

            if (iServiceVolkano != null)
            {
                iServiceVolkano.BeginProductId((ptr) =>
                {
                    try
                    {
                        iServiceVolkano.EndProductId(ptr, out iProductId);
                        if (!volkano.Task.IsCanceled)
                        {
                            volkano.SetResult(true);
                        }
                    }
                    catch (ProxyError e)
                    {
                        if (!volkano.Task.IsCanceled)
                        {
                            volkano.SetException(e);
                        }
                    }
                });
            }
            else
            {
                if (!volkano.Task.IsCanceled)
                {
                    volkano.SetResult(true);
                }
            }
                
            return Task.Factory.ContinueWhenAll(
                new Task[] { iSubscribedSource.Task, iSubscribedConfigurationSource.Task, iSubscribedVolkanoSource.Task },
                (tasks) => { Task.WaitAll(tasks); });
        }

        protected override void OnCancelSubscribe()
        {
            if (iSubscribedSource != null)
            {
                iSubscribedSource.TrySetCanceled();
            }
            if (iSubscribedConfigurationSource != null)
            {
                iSubscribedConfigurationSource.TrySetCanceled();
            }
            if (iSubscribedVolkanoSource != null)
            {
                iSubscribedVolkanoSource.TrySetCanceled();
            }
        }

        private void HandleInitialEvent()
        {
            iAttributes = iService.PropertyAttributes();
            iManufacturerImageUri = iService.PropertyManufacturerImageUri();
            iManufacturerInfo = iService.PropertyManufacturerInfo();
            iManufacturerName = iService.PropertyManufacturerName();
            iManufacturerUrl = iService.PropertyManufacturerUrl();
            iModelImageUri = iService.PropertyModelImageUri();
            iModelInfo = iService.PropertyModelInfo();
            iModelName = iService.PropertyModelName();
            iModelUrl = iService.PropertyModelUrl();
            iProductImageUri = iService.PropertyProductImageUri();
            iProductInfo = iService.PropertyProductInfo();
            iProductUrl = iService.PropertyProductUrl();

            if (!iSubscribedSource.Task.IsCanceled)
            {
                iSubscribedSource.SetResult(true);
            }
        }

        private void HandleInitialEventConfiguration()
        {
            string propertyXml = iServiceConfiguration.PropertyParameterXml();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    ParseParameterXml(propertyXml);
                });
            });

            if (!iSubscribedConfigurationSource.Task.IsCanceled)
            {
                iSubscribedConfigurationSource.SetResult(true);
            }
        }

        protected override void OnUnsubscribe()
        {
            if (iService != null)
            {
                iService.Unsubscribe();
            }
            if (iServiceConfiguration != null)
            {
                iServiceConfiguration.Unsubscribe();
            }

            iSubscribedSource = null;
            iSubscribedConfigurationSource = null;
            iSubscribedVolkanoSource = null;
        }

        public override Task SetSourceIndex(uint aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetSourceIndex(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSetSourceIndex(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            taskSource.Task.ContinueWith(t => { iLog.Write("Unobserved exception: {0}\n", t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
            return taskSource.Task;
        }

        public override Task SetSourceIndexByName(string aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetSourceIndexByName(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSetSourceIndexByName(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task SetStandby(bool aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetStandby(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSetStandby(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task SetRegistration(string aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iServiceConfiguration.BeginSetParameter("TuneIn Radio", "Test Mode", "true", (ptr) =>
            {
                try
                {
                    iServiceConfiguration.EndSetParameter(ptr);
                    iServiceConfiguration.BeginSetParameter("TuneIn Radio", "Password", aValue, (ptr2) =>
                    {
                        try
                        {
                            iServiceConfiguration.EndSetParameter(ptr2);
                            iServiceConfiguration.BeginSetParameter("TuneIn Radio", "Test Mode", "false", (ptr3) =>
                            {
                                try
                                {
                                    iServiceConfiguration.EndSetParameter(ptr3);
                                    taskSource.SetResult(true);
                                }
                                catch (Exception e)
                                {
                                    taskSource.SetException(e);
                                }
                            });
                        }
                        catch (Exception e)
                        {
                            taskSource.SetException(e);
                        }
                    });
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        private void HandleRoomChanged()
        {
            string room = iService.PropertyProductRoom();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iRoom.Update(room);
                });
            });
        }

        private void HandleNameChanged()
        {
            string name = iService.PropertyProductName();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iName.Update(name);
                });
            });
        }

        private void HandleSourceIndexChanged()
        {
            uint sourceIndex = iService.PropertySourceIndex();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iSourceIndex.Update(sourceIndex);
                });
            });
        }

        private void HandleSourceXmlChanged()
        {
            string sourceXml = iService.PropertySourceXml();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iSourceXml.Update(sourceXml);
                });
            });
        }

        private void HandleStandbyChanged()
        {
            bool standby = iService.PropertyStandby();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iStandby.Update(standby);
                });
            });
        }

        private void HandleParameterXmlChanged()
        {
            string paramXml = iServiceConfiguration.PropertyParameterXml();
            iNetwork.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    ParseParameterXml(paramXml);
                });
            });
        }

        private void ParseParameterXml(string aParameterXml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(aParameterXml);

            //<ParameterList>
            // ...
            //  <Parameter>
            //    <Target>TuneIn Radio</Target>
            //    <Name>Password</Name>
            //    <Type>string</Type>
            //    <Value></Value>
            //  </Parameter>
            // ...
            //</ParameterList>

            System.Xml.XmlNode registration = document.SelectSingleNode("/ParameterList/Parameter[Target=\"TuneIn Radio\" and Name=\"Password\"]/Value");
            if (registration != null && registration.FirstChild != null)
            {
                iRegistration.Update(registration.FirstChild.Value);
            }
            else
            {
                iRegistration.Update(string.Empty);
            }
        }

        private readonly CpDevice iCpDevice;
        private TaskCompletionSource<bool> iSubscribedSource;
        private TaskCompletionSource<bool> iSubscribedConfigurationSource;
        private TaskCompletionSource<bool> iSubscribedVolkanoSource;
        private CpProxyAvOpenhomeOrgProduct1 iService;
        private CpProxyLinnCoUkConfiguration1 iServiceConfiguration;
        private CpProxyLinnCoUkVolkano1 iServiceVolkano;
    }

    internal class SourceXml
    {
        public class Source
        {
            public Source(string aName, string aType, bool aVisible)
            {
                iName = aName;
                iType = aType;
                iVisible = aVisible;
            }

            public string Name
            {
                get
                {
                    return iName;
                }
                set
                {
                    iName = value;
                }
            }

            public string Type
            {
                get
                {
                    return iType;
                }
            }

            public bool Visible
            {
                get
                {
                    return iVisible;
                }
                set
                {
                    iVisible = value;
                }
            }

            private string iName;
            private string iType;
            private bool iVisible;
        }

        public SourceXml(Source[] aSources)
        {
            iSources = aSources;
            CreateSourceXml();
        }

        public override string ToString()
        {
            return iSourceXml;
        }

        public void UpdateName(uint aIndex, string aName)
        {
            iSources[(int)aIndex].Name = aName;
            CreateSourceXml();
        }

        public void UpdateVisible(uint aIndex, bool aVisible)
        {
            iSources[(int)aIndex].Visible = aVisible;
            CreateSourceXml();
        }

        private void CreateSourceXml()
        {
            XmlDocument doc = new XmlDocument();

            XmlElement sources = doc.CreateElement("SourceList");

            foreach (Source s in iSources)
            {
                XmlElement source = doc.CreateElement("Source");

                XmlElement name = doc.CreateElement("Name");
                XmlElement type = doc.CreateElement("Type");
                XmlElement visible = doc.CreateElement("Visible");

                name.AppendChild(doc.CreateTextNode(s.Name));
                type.AppendChild(doc.CreateTextNode(s.Type));
                visible.AppendChild(doc.CreateTextNode(s.Visible.ToString()));

                source.AppendChild(name);
                source.AppendChild(type);
                source.AppendChild(visible);

                sources.AppendChild(source);
            }

            doc.AppendChild(sources);

            iSourceXml = doc.OuterXml;
        }

        private Source[] iSources;
        private string iSourceXml;
    }

    class ServiceProductMock : ServiceProduct
    {
        public ServiceProductMock(INetwork aNetwork, IInjectorDevice aDevice, string aRoom, string aName, uint aSourceIndex, SourceXml aSourceXmlFactory, bool aStandby,
            string aAttributes, string aManufacturerImageUri, string aManufacturerInfo, string aManufacturerName, string aManufacturerUrl, string aModelImageUri, string aModelInfo, string aModelName,
            string aModelUrl, string aProductImageUri, string aProductInfo, string aProductUrl, string aProductId, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iSourceXmlFactory = aSourceXmlFactory;

            iAttributes = aAttributes;
            iManufacturerImageUri = aManufacturerImageUri;
            iManufacturerInfo = aManufacturerInfo;
            iManufacturerName = aManufacturerName;
            iManufacturerUrl = aManufacturerUrl;
            iModelImageUri = aModelImageUri;
            iModelInfo = aModelInfo;
            iModelName = aModelName;
            iModelUrl = aModelUrl;
            iProductImageUri = aProductImageUri;
            iProductInfo = aProductInfo;
            iProductUrl = aProductUrl;
            iProductId = aProductId;

            iRoom.Update(aRoom);
            iName.Update(aName);
            iSourceIndex.Update(aSourceIndex);
            iSourceXml.Update(iSourceXmlFactory.ToString());
            iStandby.Update(aStandby);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "attributes")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iAttributes = string.Join(" ", value);
            }
            else if (command == "manufacturerimageuri")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerImageUri = string.Join(" ", value);
            }
            else if (command == "manufacturerinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerInfo = string.Join(" ", value);
            }
            else if (command == "manufacturername")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerName = string.Join(" ", value);
            }
            else if (command == "manufacturerurl")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerUrl = string.Join(" ", value);
            }
            else if (command == "modelimageuri")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iModelImageUri = string.Join(" ", value);
            }
            else if (command == "modelinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iModelInfo = string.Join(" ", value);
            }
            else if (command == "modelname")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iManufacturerName = string.Join(" ", value);
            }
            else if (command == "modelurl")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iModelUrl = string.Join(" ", value);
            }
            else if (command == "productimageuri")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iProductImageUri = string.Join(" ", value);
            }
            else if (command == "productinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iProductInfo = string.Join(" ", value);
            }
            else if (command == "producturl")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iProductUrl = string.Join(" ", value);
            }
            else if (command == "productid")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iProductId = string.Join(" ", value);
            }
            else if (command == "room")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iRoom.Update(string.Join(" ", value));
            }
            else if (command == "name")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iName.Update(string.Join(" ", value));
            }
            else if (command == "sourceindex")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iSourceIndex.Update(uint.Parse(value.First()));
            }
            else if (command == "standby")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iStandby.Update(bool.Parse(value.First()));
            }
            else if (command == "registration")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iRegistration.Update(value.First());
            }
            else if (command == "source")
            {
                IEnumerable<string> value = aValue.Skip(1);

                uint index = uint.Parse(value.First());

                value = value.Skip(1);

                string property = value.First();

                value = value.Skip(1);

                if (property == "name")
                {
                    iSourceXmlFactory.UpdateName(index, string.Join(" ", value));
                    iSourceXml.Update(iSourceXmlFactory.ToString());
                }
                else if (property == "visible")
                {
                    iSourceXmlFactory.UpdateVisible(index, bool.Parse(value.First()));
                    iSourceXml.Update(iSourceXmlFactory.ToString());
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public override Task SetSourceIndex(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    iSourceIndex.Update(aValue);
                });
            });
            return task;
        }

        public override Task SetSourceIndexByName(string aValue)
        {
            throw new NotSupportedException();
        }

        public override Task SetStandby(bool aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    iStandby.Update(aValue);
                });
            });
            return task;
        }

        public override Task SetRegistration(string aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                iNetwork.Schedule(() =>
                {
                    iRegistration.Update(aValue);
                });
            });
            return task;
        }

        private SourceXml iSourceXmlFactory;
    }

    public class ProxyProduct : Proxy<ServiceProduct>, IProxyProduct
    {
        public ProxyProduct(ServiceProduct aService, IDevice aDevice)
            : base(aService, aDevice)
        {
        }

        public IWatchable<string> Room
        {
            get { return iService.Room; }
        }

        public IWatchable<string> Name
        {
            get { return iService.Name; }
        }

        public IWatchable<uint> SourceIndex
        {
            get { return iService.SourceIndex; }
        }

        public IWatchable<string> SourceXml
        {
            get { return iService.SourceXml; }
        }

        public IWatchable<bool> Standby
        {
            get { return iService.Standby; }
        }

        public IWatchable<string> Registration
        {
            get { return iService.Registration; }
        }

        public Task SetSourceIndex(uint aValue)
        {
            return iService.SetSourceIndex(aValue);
        }

        public Task SetSourceIndexByName(string aValue)
        {
            return iService.SetSourceIndexByName(aValue);
        }

        public Task SetStandby(bool aValue)
        {
            return iService.SetStandby(aValue);
        }

        public Task SetRegistration(string aValue)
        {
            return iService.SetRegistration(aValue);
        }

        public string Attributes
        {
            get { return iService.Attributes; }
        }

        public string ManufacturerImageUri
        {
            get { return iService.ManufacturerImageUri; }
        }

        public string ManufacturerInfo
        {
            get { return iService.ManufacturerInfo; }
        }

        public string ManufacturerName
        {
            get { return iService.ManufacturerName; }
        }

        public string ManufacturerUrl
        {
            get { return iService.ManufacturerUrl; }
        }

        public string ModelImageUri
        {
            get { return iService.ModelImageUri; }
        }

        public string ModelInfo
        {
            get { return iService.ModelInfo; }
        }

        public string ModelName
        {
            get { return iService.ModelName; }
        }

        public string ModelUrl
        {
            get { return iService.ModelUrl; }
        }

        public string ProductImageUri
        {
            get { return iService.ProductImageUri; }
        }

        public string ProductInfo
        {
            get { return iService.ProductInfo; }
        }

        public string ProductUrl
        {
            get { return iService.ProductUrl; }
        }

        public string ProductId
        {
            get { return iService.ProductId; }
        }
    }
}
