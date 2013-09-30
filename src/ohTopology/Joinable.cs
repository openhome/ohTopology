using System;

namespace OpenHome.Av
{
    public interface IJoinable
    {
        void Join(Action aAction);
        void Unjoin(Action aAction);
    }
}
