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
            iIndex = aIndex;
            iName = aName;
            iType = aType;
            iVisible = aVisible;
        }

        public uint Index
        {
            get
            {
                return iIndex;
            }
        }

        public string Name
        {
            get
            {
                return iName;
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
        }

        private string iName;
        private string iType;
        private bool iVisible;
        private uint iIndex;
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

    public class Topology2Group : ITopology2Group, IWatcher<string>, IDisposable
    {
        public Topology2Group(IWatchableThread aThread, string aId, Product aProduct)
        {
            iThread = aThread;
            iId = aId;

            iDisposed = false;
            iSources = new List<ITopology2Source>();
            iWatchableSources = new List<Watchable<ITopology2Source>>();

            iProduct = aProduct;
            iDevice = aProduct.Device;
            iAttributes = aProduct.Attributes;
            iRoom = iProduct.Room;
            iName = iProduct.Name;
            iStandby = iProduct.Standby;
            iSourceIndex = iProduct.SourceIndex;

            iProduct.SourceXml.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology2Group.Dispose");
            }

            iProduct.SourceXml.RemoveWatcher(this);
            iProduct.Dispose();
            iProduct = null;

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

        private string iId;
        private string iAttributes;
        private Product iProduct;
        private IWatchableDevice iDevice;
        private IWatchable<string> iRoom;
        private IWatchable<string> iName;
        private IWatchable<bool> iStandby;
        private IWatchable<uint> iSourceIndex;
        private IWatchableThread iThread;
        private List<ITopology2Source> iSources;
        private List<Watchable<ITopology2Source>> iWatchableSources;
    }

    public class WatchableTopology2GroupUnordered : WatchableUnordered<ITopology2Group>
    {
        public WatchableTopology2GroupUnordered(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<ITopology2Group>();
        }

        public new void Add(ITopology2Group aValue)
        {
            iList.Add(aValue); 
            base.Add(aValue);
        }

        public new void Remove(ITopology2Group aValue)
        {
            iList.Remove(aValue);
            base.Remove(aValue);
        }

        private List<ITopology2Group> iList;
    }

    public interface ITopology2
    {
        IWatchableUnordered<ITopology2Group> Groups { get; }
    }

    public class Topology2 : ITopology2, IUnorderedWatcher<Product>, IDisposable
    {
        public Topology2(IWatchableThread aThread, ITopology1 aTopology1)
        {
            iDisposed = false;

            iThread = aThread;
            iTopology1 = aTopology1;

            iGroups = new WatchableTopology2GroupUnordered(aThread);

            iGroupLookup = new Dictionary<Product, Topology2Group>();

            iTopology1.Products.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology2.Dispose");
            }

            iTopology1.Products.RemoveWatcher(this);
            iTopology1 = null;

            iGroups.Dispose();
            iGroups = null;

            foreach (Topology2Group g in iGroupLookup.Values)
            {
                g.Dispose();
            }
            iGroupLookup = null;

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

        public void UnorderedAdd(Product aItem)
        {
            Topology2Group group = new Topology2Group(iThread, aItem.Id, aItem);

            iGroupLookup.Add(aItem, group);

            iGroups.Add(group);
        }

        public void UnorderedRemove(Product aItem)
        {
            Topology2Group group;
            if (iGroupLookup.TryGetValue(aItem, out group))
            {
                iGroups.Remove(group);

                iGroupLookup.Remove(aItem);

                // schedule the disposale for the group for after all watchers of the group collection have been notified
                iThread.Schedule(() =>
                {
                    group.Dispose();
                });
            }
        }

        private bool iDisposed;

        private IWatchableThread iThread;
        private ITopology1 iTopology1;
        private Dictionary<Product, Topology2Group> iGroupLookup;
        private WatchableTopology2GroupUnordered iGroups;
    }
}
