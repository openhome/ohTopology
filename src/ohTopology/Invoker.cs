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
        public InvokerPlaylist()
        {
        }

        public void Play(IEnumerable<ITopology4Source> aSources, string aUri, string aMetadata)
        {
        }
    }

    internal class InvokerRadio : IInvoker
    {
        public InvokerRadio()
        {
        }

        public void Play(IEnumerable<ITopology4Source> aSources, string aUri, string aMetadata)
        {
        }
    }

    internal class InvokerReceiver : IInvoker
    {
        public InvokerReceiver()
        {
        }

        public void Play(IEnumerable<ITopology4Source> aSources, string aUri, string aMetadata)
        {
        }
    }

    internal class InvokerExternal : IInvoker
    {
        public InvokerExternal()
        {
        }

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
