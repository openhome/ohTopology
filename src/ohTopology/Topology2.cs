using System;
using System.Xml;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public interface ITopology2Source
    {
        uint Index { get; }
        string Name { get; }
        string Type { get; }
        bool Visible { get; }
    }

    public class Topology2Source : ITopology2Source
    {
        public Topology2Source(uint aIndex, string aName, string aType, bool aVisible)
        {
            Index = aIndex;
            Name = aName;
            Type = aType;
            Visible = aVisible;
        }

        public uint Index
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public string Type
        {
            get;
            private set;
        }

        public bool Visible
        {
            get;
            private set;
        }
    }

    public interface ITopology2Group
    {
        string Id { get; }
        string Attributes { get; }
        IWatchableDevice Device { get; }

        IWatchable<string> Room { get; }
        IWatchable<string> Name { get; }
        IWatchable<bool> Standby { get; }
        IWatchable<uint> SourceIndex { get; }
        IEnumerable<IWatchable<ITopology2Source>> Sources { get; }

        void SetStandby(bool aValue);
        void SetSourceIndex(uint aValue);
    }

    public class Topology2Group : ITopology2Group, IWatcher<string>, ITopologyObject
    {
        public Topology2Group(IWatchableThread aThread, string aId, ServiceProduct aProduct)
        {
            iDisposed = false;
            iThread = aThread;

            iId = aId;
            iProduct = aProduct;
            iAttributes = aProduct.Attributes;
            iDevice = aProduct.Device;

            // create proxies for watchables that are to be passed straight through this layer
            iRoom = new WatchableProxy<string>(iProduct.Room);
            iName = new WatchableProxy<string>(iProduct.Name);
            iStandby = new WatchableProxy<bool>(iProduct.Standby);
            iSourceIndex = new WatchableProxy<uint>(iProduct.SourceIndex);

            iSources = new List<ITopology2Source>();
            iWatchableSources = new List<Watchable<ITopology2Source>>();

            iProduct.SourceXml.AddWatcher(this);
        }

        public void Detach()
        {
            // detach all watchers from the L1 watchables
            iRoom.Detach();
            iName.Detach();
            iStandby.Detach();
            iSourceIndex.Detach();

            iProduct.SourceXml.RemoveWatcher(this);
            iProduct = null;
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology2Group.Dispose");
            }

            // dispose of all created watchables
            iRoom.Dispose();
            iName.Dispose();
            iStandby.Dispose();
            iSourceIndex.Dispose();

            foreach (Watchable<ITopology2Source> s in iWatchableSources)
            {
                s.Dispose();
            }

            iDisposed = true;
        }

        public string Id
        {
            get
            {
                return iId;
            }
        }

        public string Attributes
        {
            get
            {
                return iAttributes;
            }
        }

        public IWatchableDevice Device
        {
            get
            {
                return iDevice;
            }
        }

        public void ItemOpen(string aId, string aValue)
        {
            ProcessSourceXml(aValue, true);
        }

        public void ItemClose(string aId, string aValue)
        {
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            ProcessSourceXml(aValue, false);
        }

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

        public IWatchable<bool> Standby
        {
            get 
            {
                return iStandby;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get
            {
                return iSourceIndex;
            }
        }

        public IEnumerable<IWatchable<ITopology2Source>> Sources
        {
            get
            {
                return iWatchableSources;
            }
        }

        public void SetStandby(bool aValue)
        {
            if (iProduct != null)
            {
                iProduct.SetStandby(aValue);
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            if (iProduct != null)
            {
                iProduct.SetSourceIndex(aValue);
            }
        }

        private void ProcessSourceXml(string aSourceXml, bool aInitial)
        {
            uint index = 0;

            XmlDocument document = new XmlDocument();
            document.LoadXml(aSourceXml);

            XmlNodeList sources = document.SelectNodes("SourceList/Source");
            foreach (XmlNode s in sources)
            {
                XmlNode nameNode = s.SelectSingleNode("Name");
                XmlNode typeNode = s.SelectSingleNode("Type");
                XmlNode visibleNode = s.SelectSingleNode("Visible");

                string name = string.Empty;
                string type = string.Empty;
                bool visible = false;
                if (nameNode != null && nameNode.FirstChild != null)
                {
                    name = nameNode.FirstChild.Value;
                }
                if (typeNode != null && typeNode.FirstChild != null)
                {
                    type = typeNode.FirstChild.Value;
                }
                if (visibleNode != null && visibleNode.FirstChild != null)
                {
                    string value = visibleNode.FirstChild.Value;
                    try
                    {
                        visible = bool.Parse(value);
                    }
                    catch (FormatException)
                    {
                        try
                        {
                            visible = uint.Parse(value) > 0;
                        }
                        catch (FormatException)
                        {
                            visible = false;
                        }
                    }
                }

                ITopology2Source source = new Topology2Source(index, name, type, visible);

                if (aInitial)
                {
                    iSources.Add(source);
                    iWatchableSources.Add(new Watchable<ITopology2Source>(iThread, string.Format("{0}({1})", iId, index.ToString()), source));
                }
                else
                {
                    ITopology2Source oldSource = iSources[(int)index];
                    if (oldSource.Name != source.Name ||
                        oldSource.Visible != source.Visible ||
                        oldSource.Index != source.Index ||
                        oldSource.Type != source.Type)
                    {
                        iSources[(int)index] = source;
                        iWatchableSources[(int)index].Update(source);
                    }
                }

                ++index;
            }
        }

        private bool iDisposed;
        private IWatchableThread iThread;
        private string iId;
        private ServiceProduct iProduct;
        private string iAttributes;
        private IWatchableDevice iDevice;
        private WatchableProxy<string> iRoom;
        private WatchableProxy<string> iName;
        private WatchableProxy<bool> iStandby;
        private WatchableProxy<uint> iSourceIndex;
        private List<ITopology2Source> iSources;
        private List<Watchable<ITopology2Source>> iWatchableSources;
    }

    public interface ITopology2
    {
        IWatchableUnordered<ITopology2Group> Groups { get; }
    }

    public class Topology2 : ITopology2, IUnorderedWatcher<ServiceProduct>, IDisposable
    {
        public Topology2(IWatchableThread aThread, ITopology1 aTopology1)
        {
            iDisposed = false;

            iThread = aThread;
            iTopology1 = aTopology1;

            iGroups = new WatchableUnordered<ITopology2Group>(aThread);

            iGroupLookup = new Dictionary<ServiceProduct, Topology2Group>();

            iThread.Schedule(() =>
            {
                iTopology1.Products.AddWatcher(this);
            });
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology2.Dispose");
            }

            iThread.Wait(() =>
            {
                iTopology1.Products.RemoveWatcher(this);
                
                foreach (Topology2Group g in iGroupLookup.Values)
                {
                    g.Detach();
                }
            });
            iTopology1 = null;

            foreach (Topology2Group g in iGroupLookup.Values)
            {
                g.Dispose();
            }
            iGroupLookup = null;

            iGroups.Dispose();
            iGroups = null;

            iDisposed = true;
        }

        public IWatchableUnordered<ITopology2Group> Groups
        {
            get
            {
                return iGroups;
            }
        }

        public void UnorderedOpen()
        {
        }

        public void UnorderedClose()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedAdd(ServiceProduct aItem)
        {
            Topology2Group group = new Topology2Group(iThread, aItem.Device.Udn, aItem);
            iGroupLookup.Add(aItem, group);
            iGroups.Add(group);
        }

        public void UnorderedRemove(ServiceProduct aItem)
        {
            Topology2Group group;
            if (iGroupLookup.TryGetValue(aItem, out group))
            {
                // schedule higher layer notification
                iGroups.Remove(group);
                iGroupLookup.Remove(aItem);

                // immediately detach Topology2Group from L1 since the Product object is about to be disposed
                group.Detach();

                // schedule Topology2Group disposal
                iThread.Schedule(() =>
                {
                    group.Dispose();
                });
            }
        }

        private bool iDisposed;

        private IWatchableThread iThread;
        private ITopology1 iTopology1;
        private Dictionary<ServiceProduct, Topology2Group> iGroupLookup;
        private WatchableUnordered<ITopology2Group> iGroups;
    }
}
