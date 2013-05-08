using System;
using System.Collections.Generic;

namespace OpenHome.Av
{
    interface IInvoker
    {
        void Play(IEnumerable<ITopology4Source> aSources, string aUri, string aMetadata);
    }

    internal class Invoker
    {
        public static void Play(IEnumerable<ITopology4Source> aSources, string aMode, string aUri, string aMetadata)
        {
            IInvoker invoker = null;

            if (aMode == "Playlist")
            {
                invoker = new InvokerPlaylist();
            }
            else if (aMode == "Radio")
            {
                invoker = new InvokerRadio();
            }
            else if (aMode == "Receiver")
            {
                invoker = new InvokerReceiver();
            }
            else if (aMode == "External")
            {
                invoker = new InvokerExternal();
            }

            invoker.Play(aSources, aUri, aMetadata);
        }
    }

    internal class InvokerPlaylist : IInvoker
    {
        public void Play(IEnumerable<ITopology4Source> aSources, string aUri, string aMetadata)
        {
            uint id = uint.Parse(aUri);
            foreach (ITopology4Source s in aSources)
            {
                if (s.Type == "Playlist")
                {
                    s.Device.Create<ServicePlaylist>((IWatchableDevice device, ServicePlaylist playlist) =>
                    {
                        playlist.SeekId(id, null);
                        playlist.Dispose();
                    });
                    return;
                }
            }
        }
    }

    internal class InvokerRadio : IInvoker
    {
        public void Play(IEnumerable<ITopology4Source> aSources, string aUri, string aMetadata)
        {
            foreach (ITopology4Source s in aSources)
            {
                if (s.Type == "Radio")
                {
                    s.Device.Create<ServiceRadio>((IWatchableDevice device, ServiceRadio radio) =>
                    {
                        radio.SetChannel(aUri, aMetadata, () => { radio.Play(null); });
                        radio.Dispose();
                    });
                    return;
                }
            }
        }
    }

    internal class InvokerReceiver : IInvoker
    {
        public void Play(IEnumerable<ITopology4Source> aSources, string aUri, string aMetadata)
        {
            foreach (ITopology4Source s in aSources)
            {
                if (s.Type == "Receiver")
                {
                    s.Device.Create<ServiceReceiver>((IWatchableDevice device, ServiceReceiver receiver) =>
                    {
                        receiver.SetSender(aUri, aMetadata, () => { receiver.Play(null); });
                        receiver.Dispose();
                    });
                    return;
                }
            }
        }
    }

    internal class InvokerExternal : IInvoker
    {
        public void Play(IEnumerable<ITopology4Source> aSources, string aUri, string aMetadata)
        {
            uint index = uint.Parse(aUri);
            foreach (ITopology4Source s in aSources)
            {
                if (s.Index == index)
                {
                    s.Select();
                    return;
                }
            }
        }
    }
}
