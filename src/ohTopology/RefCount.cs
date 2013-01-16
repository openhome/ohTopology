using System;

namespace OpenHome.Av
{
    public interface IRefCount
    {
        void AddRef();
        void RemoveRef();
    }

    public abstract class RefCount : IRefCount
    {
        protected RefCount()
        {
            AddRef();
        }

        public void AddRef()
        {
            ++iRefCount;
        }

        public void RemoveRef()
        {
            --iRefCount;
            if (iRefCount == 0)
            {
                CleanUp();
            }
        }

        protected abstract void CleanUp();

        private uint iRefCount;
    }
}
