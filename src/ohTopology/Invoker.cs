using System;

namespace OpenHome.Av
{
    interface IInvoker
    {
        void Play(string aUri, string aMetadata);
    }

    /*internal class InvokerPlaylist : IInvoker
    {
        public InvokerPlaylist(ITopology4Source aSource)
        {
            aSource.Device.Create<ServicePlaylist>((IWatchableDevice aDevice, ServicePlaylist playlist) =>
            {
            });
        }

        public void Play(string aUri, string aMetadata)
        {
            throw new NotImplementedException();
        }
    }

    internal class InvokerRadio : IInvoker
    {
    }

    internal class InvokerReceiver : IInvoker
    {
    }

    internal class InvokerExternal : IInvoker
    {
    }*/
}
