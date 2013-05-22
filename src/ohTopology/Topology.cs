using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    // Interface for objects that are passed through the topology layers. Implements the
    // IDisposable interface - the Dispose() method should dispose of all watchables created
    // for this object. At the point that this method is called, the upper layers should
    // have been notified that this object is about to be removed.
    public interface ITopologyObject : IDisposable
    {
        // This method is intended to detach the object from its lower layer dependencies
        // which will primarily involve removing watchers from the lower layer's watchables
        void Detach();
    }

    public enum EStandby
    {
        eOn,
        eMixed,
        eOff
    }
}
