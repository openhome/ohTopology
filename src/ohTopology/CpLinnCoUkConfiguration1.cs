using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using OpenHome.Net.Core;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Net.ControlPoint.Proxies
{
    public interface ICpProxyLinnCoUkConfiguration1 : ICpProxy, IDisposable
    {
        void SyncConfigurationXml(out String aConfigurationXml);
        void BeginConfigurationXml(CpProxy.CallbackAsyncComplete aCallback);
        void EndConfigurationXml(IntPtr aAsyncHandle, out String aConfigurationXml);
        void SyncParameterXml(out String aParameterXml);
        void BeginParameterXml(CpProxy.CallbackAsyncComplete aCallback);
        void EndParameterXml(IntPtr aAsyncHandle, out String aParameterXml);
        void SyncSetParameter(String aTarget, String aName, String aValue);
        void BeginSetParameter(String aTarget, String aName, String aValue, CpProxy.CallbackAsyncComplete aCallback);
        void EndSetParameter(IntPtr aAsyncHandle);
        void SetPropertyConfigurationXmlChanged(System.Action aConfigurationXmlChanged);
        String PropertyConfigurationXml();
        void SetPropertyParameterXmlChanged(System.Action aParameterXmlChanged);
        String PropertyParameterXml();
    }

    internal class SyncConfigurationXmlLinnCoUkConfiguration1 : SyncProxyAction
    {
        private CpProxyLinnCoUkConfiguration1 iService;
        private String iConfigurationXml;

        public SyncConfigurationXmlLinnCoUkConfiguration1(CpProxyLinnCoUkConfiguration1 aProxy)
        {
            iService = aProxy;
        }
        public String ConfigurationXml()
        {
            return iConfigurationXml;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndConfigurationXml(aAsyncHandle, out iConfigurationXml);
        }
    };

    internal class SyncParameterXmlLinnCoUkConfiguration1 : SyncProxyAction
    {
        private CpProxyLinnCoUkConfiguration1 iService;
        private String iParameterXml;

        public SyncParameterXmlLinnCoUkConfiguration1(CpProxyLinnCoUkConfiguration1 aProxy)
        {
            iService = aProxy;
        }
        public String ParameterXml()
        {
            return iParameterXml;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndParameterXml(aAsyncHandle, out iParameterXml);
        }
    };

    internal class SyncSetParameterLinnCoUkConfiguration1 : SyncProxyAction
    {
        private CpProxyLinnCoUkConfiguration1 iService;

        public SyncSetParameterLinnCoUkConfiguration1(CpProxyLinnCoUkConfiguration1 aProxy)
        {
            iService = aProxy;
        }
        protected override void CompleteRequest(IntPtr aAsyncHandle)
        {
            iService.EndSetParameter(aAsyncHandle);
        }
    };

    /// <summary>
    /// Proxy for the linn.co.uk:Configuration:1 UPnP service
    /// </summary>
    public class CpProxyLinnCoUkConfiguration1 : CpProxy, IDisposable, ICpProxyLinnCoUkConfiguration1
    {
        private OpenHome.Net.Core.Action iActionConfigurationXml;
        private OpenHome.Net.Core.Action iActionParameterXml;
        private OpenHome.Net.Core.Action iActionSetParameter;
        private PropertyString iConfigurationXml;
        private PropertyString iParameterXml;
        private System.Action iConfigurationXmlChanged;
        private System.Action iParameterXmlChanged;
        private Mutex iPropertyLock;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>Use CpProxy::[Un]Subscribe() to enable/disable querying of state variable and reporting of their changes.</remarks>
        /// <param name="aDevice">The device to use</param>
        public CpProxyLinnCoUkConfiguration1(CpDevice aDevice)
            : base("linn-co-uk", "Configuration", 1, aDevice)
        {
            OpenHome.Net.Core.Parameter param;
            List<String> allowedValues = new List<String>();

            iActionConfigurationXml = new OpenHome.Net.Core.Action("ConfigurationXml");
            param = new ParameterString("aConfigurationXml", allowedValues);
            iActionConfigurationXml.AddOutputParameter(param);

            iActionParameterXml = new OpenHome.Net.Core.Action("ParameterXml");
            param = new ParameterString("aParameterXml", allowedValues);
            iActionParameterXml.AddOutputParameter(param);

            iActionSetParameter = new OpenHome.Net.Core.Action("SetParameter");
            param = new ParameterString("aTarget", allowedValues);
            iActionSetParameter.AddInputParameter(param);
            param = new ParameterString("aName", allowedValues);
            iActionSetParameter.AddInputParameter(param);
            param = new ParameterString("aValue", allowedValues);
            iActionSetParameter.AddInputParameter(param);

            iConfigurationXml = new PropertyString("ConfigurationXml", ConfigurationXmlPropertyChanged);
            AddProperty(iConfigurationXml);
            iParameterXml = new PropertyString("ParameterXml", ParameterXmlPropertyChanged);
            AddProperty(iParameterXml);
            
            iPropertyLock = new Mutex();
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aConfigurationXml"></param>
        public void SyncConfigurationXml(out String aConfigurationXml)
        {
            SyncConfigurationXmlLinnCoUkConfiguration1 sync = new SyncConfigurationXmlLinnCoUkConfiguration1(this);
            BeginConfigurationXml(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aConfigurationXml = sync.ConfigurationXml();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndConfigurationXml().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginConfigurationXml(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionConfigurationXml, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionConfigurationXml.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aConfigurationXml"></param>
        public void EndConfigurationXml(IntPtr aAsyncHandle, out String aConfigurationXml)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aConfigurationXml = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aParameterXml"></param>
        public void SyncParameterXml(out String aParameterXml)
        {
            SyncParameterXmlLinnCoUkConfiguration1 sync = new SyncParameterXmlLinnCoUkConfiguration1(this);
            BeginParameterXml(sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
            aParameterXml = sync.ParameterXml();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndParameterXml().</remarks>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginParameterXml(CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionParameterXml, aCallback);
            int outIndex = 0;
            invocation.AddOutput(new ArgumentString((ParameterString)iActionParameterXml.OutputParameter(outIndex++)));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        /// <param name="aParameterXml"></param>
        public void EndParameterXml(IntPtr aAsyncHandle, out String aParameterXml)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
            uint index = 0;
            aParameterXml = Invocation.OutputString(aAsyncHandle, index++);
        }

        /// <summary>
        /// Invoke the action synchronously
        /// </summary>
        /// <remarks>Blocks until the action has been processed
        /// on the device and sets any output arguments</remarks>
        /// <param name="aTarget"></param>
        /// <param name="aName"></param>
        /// <param name="aValue"></param>
        public void SyncSetParameter(String aTarget, String aName, String aValue)
        {
            SyncSetParameterLinnCoUkConfiguration1 sync = new SyncSetParameterLinnCoUkConfiguration1(this);
            BeginSetParameter(aTarget, aName, aValue, sync.AsyncComplete());
            sync.Wait();
            sync.ReportError();
        }

        /// <summary>
        /// Invoke the action asynchronously
        /// </summary>
        /// <remarks>Returns immediately and will run the client-specified callback when the action
        /// later completes.  Any output arguments can then be retrieved by calling
        /// EndSetParameter().</remarks>
        /// <param name="aTarget"></param>
        /// <param name="aName"></param>
        /// <param name="aValue"></param>
        /// <param name="aCallback">Delegate to run when the action completes.
        /// This is guaranteed to be run but may indicate an error</param>
        public void BeginSetParameter(String aTarget, String aName, String aValue, CallbackAsyncComplete aCallback)
        {
            Invocation invocation = iService.Invocation(iActionSetParameter, aCallback);
            int inIndex = 0;
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetParameter.InputParameter(inIndex++), aTarget));
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetParameter.InputParameter(inIndex++), aName));
            invocation.AddInput(new ArgumentString((ParameterString)iActionSetParameter.InputParameter(inIndex++), aValue));
            iService.InvokeAction(invocation);
        }

        /// <summary>
        /// Retrieve the output arguments from an asynchronously invoked action.
        /// </summary>
        /// <remarks>This may only be called from the callback set in the above Begin function.</remarks>
        /// <param name="aAsyncHandle">Argument passed to the delegate set in the above Begin function</param>
        public void EndSetParameter(IntPtr aAsyncHandle)
        {
			uint code;
			string desc;
            if (Invocation.Error(aAsyncHandle, out code, out desc))
            {
                throw new ProxyError(code, desc);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the ConfigurationXml state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkConfiguration1 instance will not overlap.</remarks>
        /// <param name="aConfigurationXmlChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyConfigurationXmlChanged(System.Action aConfigurationXmlChanged)
        {
            lock (iPropertyLock)
            {
                iConfigurationXmlChanged = aConfigurationXmlChanged;
            }
        }

        private void ConfigurationXmlPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iConfigurationXmlChanged);
            }
        }

        /// <summary>
        /// Set a delegate to be run when the ParameterXml state variable changes.
        /// </summary>
        /// <remarks>Callbacks may be run in different threads but callbacks for a
        /// CpProxyLinnCoUkConfiguration1 instance will not overlap.</remarks>
        /// <param name="aParameterXmlChanged">The delegate to run when the state variable changes</param>
        public void SetPropertyParameterXmlChanged(System.Action aParameterXmlChanged)
        {
            lock (iPropertyLock)
            {
                iParameterXmlChanged = aParameterXmlChanged;
            }
        }

        private void ParameterXmlPropertyChanged()
        {
            lock (iPropertyLock)
            {
                ReportEvent(iParameterXmlChanged);
            }
        }

        /// <summary>
        /// Query the value of the ConfigurationXml property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the ConfigurationXml property</returns>
        public String PropertyConfigurationXml()
        {
            PropertyReadLock();
            String val = iConfigurationXml.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Query the value of the ParameterXml property.
        /// </summary>
        /// <remarks>This function is threadsafe and can only be called if Subscribe() has been
        /// called and a first eventing callback received more recently than any call
        /// to Unsubscribe().</remarks>
        /// <returns>Value of the ParameterXml property</returns>
        public String PropertyParameterXml()
        {
            PropertyReadLock();
            String val = iParameterXml.Value();
            PropertyReadUnlock();
            return val;
        }

        /// <summary>
        /// Must be called for each class instance.  Must be called before Core.Library.Close().
        /// </summary>
        public void Dispose()
        {
            lock (this)
            {
                if (iHandle == IntPtr.Zero)
                    return;
                DisposeProxy();
                iHandle = IntPtr.Zero;
            }
            iActionConfigurationXml.Dispose();
            iActionParameterXml.Dispose();
            iActionSetParameter.Dispose();
            iConfigurationXml.Dispose();
            iParameterXml.Dispose();
        }
    }
}

