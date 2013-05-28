using System;
using System.Collections.Generic;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class BrowserExternalSource : IWatcher<IEnumerable<ITopology4Source>>, IDisposable
    {
        private IStandardRoom iRoom;
        private WatchableOrdered<ITopology4Source> iConfigured;
        private WatchableOrdered<ITopology4Source> iUnconfigured;

        public BrowserExternalSource(IStandardRoom aRoom)
        {
            iRoom = aRoom;

            iConfigured = new WatchableOrdered<ITopology4Source>(aRoom.WatchableThread);
            iUnconfigured = new WatchableOrdered<ITopology4Source>(aRoom.WatchableThread);

            iRoom.Sources.AddWatcher(this);
        }

        public void Dispose()
        {
            iRoom.Sources.RemoveWatcher(this);

            iConfigured.Dispose();
            iUnconfigured.Dispose();
        }

        public string Name
        {
            get
            {
                return iRoom.Name;
            }
        }

        public IWatchableOrdered<ITopology4Source> Configured
        {
            get
            {
                return iConfigured;
            }
        }

        public IWatchableOrdered<ITopology4Source> Unconfigured
        {
            get
            {
                return iUnconfigured;
            }
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
        {
            BuildLists(aValue);
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            BuildLists(aValue);
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
        }

        private void BuildLists(IEnumerable<ITopology4Source> aValue)
        {
            iUnconfigured.Clear();
            iConfigured.Clear();

            uint cIndex = 0;
            uint uIndex = 0;
            foreach (ITopology4Source s in aValue)
            {
                if (IsExternal(s))
                {
                    if (IsConfigured(s))
                    {
                        iConfigured.Add(s, cIndex);
                        cIndex++;
                    }
                    else
                    {
                        iUnconfigured.Add(s, uIndex);
                        uIndex++;
                    }
                }
            }
        }

        private bool IsExternal(ITopology4Source aSource)
        {
            return (aSource.Type == "Analog" || aSource.Type == "Digital" || aSource.Type == "Hdmi");
        }

        private bool IsConfigured(ITopology4Source aSource)
        {
            if (aSource.Name.StartsWith("Hdmi"))
            {
                try
                {
                    uint.Parse(aSource.Name.Substring(4));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name.StartsWith("Analog"))
            {
                try
                {
                    uint.Parse(aSource.Name.Substring(6));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name.StartsWith("SPDIF"))
            {
                try
                {
                    uint.Parse(aSource.Name.Substring(5));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name.StartsWith("TOSLINK"))
            {
                try
                {
                    uint.Parse(aSource.Name.Substring(7));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name == "Phono")
            {
                return false;
            }

            return true;
        }
    }
}
