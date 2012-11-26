using System;
using System.Xml;
using System.Collections.Generic;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public interface ITopology2Source
    {
        string Name { get; }
        string Type { get; }
        bool Visible { get; }

        void Update(string aName, string aType, bool aVisible);
    }

    public class Topology2Source : ITopology2Source
    {
        public Topology2Source(string aName, string aType, bool aVisible)
        {
            iName = aName;
            iType = aType;
            iVisible = aVisible;
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

        public void Update(string aName, string aType, bool aVisible)
        {
            iName = aName;
            iType = aType;
            iVisible = aVisible;
        }

        private string iName;
        private string iType;
        private bool iVisible;
    }

    public interface ITopology2Group
    {
        string Id { get; }
        IWatchable<string> Room { get; }
        IWatchable<string> Name { get; }
        IWatchable<bool> Standby { get; }
        IWatchable<uint> SourceIndex { get; }
        IEnumerable<IWatchable<ITopology2Source>> Sources { get; }

        void SetStandby(bool aValue);
        void SetSourceIndex(uint aValue);
    }

    public class Topology2Group : ITopology2Group, IWatcher<string>
    {
        public Topology2Group(IWatchableThread aThread, string aId, Product aProduct)
        {
            iThread = aThread;
            iId = aId;

            iLock = new object();
            iSources = new List<Watchable<ITopology2Source>>();

            iProduct = aProduct;
            iRoom = iProduct.Room;
            iName = iProduct.Name;
            iStandby = iProduct.Standby;
            iSourceIndex = iProduct.SourceIndex;

            iProduct.SourceXml.AddWatcher(this);
        }

        public void RemoveProduct()
        {
            lock (iLock)
            {
                if (iProduct != null)
                {
                    iProduct.SourceXml.RemoveWatcher(this);
                    iProduct.Dispose();
                    iProduct = null;
                }
            }
        }

        public string Id
        {
            get
            {
                return iId;
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
                return iSources;
            }
        }

        public void SetStandby(bool aValue)
        {
            lock (iLock)
            {
                if (iProduct != null)
                {
                    iProduct.SetStandby(aValue);
                }
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            lock (iLock)
            {
                if (iProduct != null)
                {
                    iProduct.SetSourceIndex(aValue);
                }
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

                if (aInitial)
                {
                    iSources.Add(new Watchable<ITopology2Source>(iThread, index.ToString(), new Topology2Source(name, type, visible)));
                }
                else
                {
                    iSources[(int)index].Update(new Topology2Source(name, type, visible));
                }

                ++index;
            }
        }

        private object iLock;

        private string iId;
        private Product iProduct;
        private IWatchable<string> iRoom;
        private IWatchable<string> iName;
        private IWatchable<bool> iStandby;
        private IWatchable<uint> iSourceIndex;
        private IWatchableThread iThread;
        private List<Watchable<ITopology2Source>> iSources;
    }

    public class WatchableTopology2GroupCollection : WatchableCollection<ITopology2Group>
    {
        public WatchableTopology2GroupCollection(IWatchableThread aThread)
            : base(aThread)
        {
            iList = new List<ITopology2Group>();
        }

        public void Add(ITopology2Group aValue)
        {
            uint index = (uint)iList.Count;
            CollectionAdd(aValue, index);
            iList.Add(aValue);
        }

        public void Remove(ITopology2Group aValue)
        {
            uint index = (uint)iList.IndexOf(aValue);
            CollectionRemove(aValue, index);
            iList.Remove(aValue);
        }

        private List<ITopology2Group> iList;
    }

    public class Topology2 : ICollectionWatcher<IWatchableDevice>, ICollectionWatcher<Product>, IDisposable
    {
        public Topology2(ITopology1 aTopology1)
        {
            iDisposed = false;
            iLock = new object();

            iThread = aTopology1.WatchableThread;
            iTopology1 = aTopology1;

            iGroups = new WatchableTopology2GroupCollection(iThread);

            iGroupLookup = new Dictionary<Product, Topology2Group>();
            
            iProducts = new WatchableProductCollection(iThread);

            iTopology1.Devices.AddWatcher(this);
            iProducts.AddWatcher(this);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2.Dispose");
                }

                iProducts.RemoveWatcher(this);
                iProducts.Dispose();
                iProducts = null;

                iGroups.Dispose();
                iGroups = null;

                foreach (Topology2Group g in iGroupLookup.Values)
                {
                    g.RemoveProduct();
                }
                iGroupLookup = null;

                iTopology1.Devices.RemoveWatcher(this);
                iTopology1 = null;

                iDisposed = true;
            }
        }

        public IWatchableCollection<ITopology2Group> Groups
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("Topology2.Groups");
                    }

                    return iGroups;
                }
            }
        }

        public void CollectionOpen()
        {
        }

        public void CollectionClose()
        {
        }

        public void CollectionInitialised()
        {
        }

        public void CollectionAdd(IWatchableDevice aItem, uint aIndex)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2.CollectionAdd");
                }

                iProducts.Add(aItem);
            }
        }

        public void CollectionMove(IWatchableDevice aItem, uint aFrom, uint aTo)
        {
            throw new NotSupportedException();
        }
        

        public void CollectionRemove(IWatchableDevice aItem, uint aIndex)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2.CollectionRemove");
                }

                iProducts.Remove(aItem);
            }
        }

        public void CollectionAdd(Product aItem, uint aIndex)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2.CollectionAdd");
                }

                Topology2Group group = new Topology2Group(iThread, aItem.Id, aItem);

                iGroupLookup.Add(aItem, group);

                iGroups.Add(group);
            }
        }

        public void CollectionMove(Product aItem, uint aFrom, uint aTo)
        {
            throw new NotSupportedException();
        }

        public void CollectionRemove(Product aItem, uint aIndex)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2.CollectionRemove");
                }

                Topology2Group group;
                if (iGroupLookup.TryGetValue(aItem, out group))
                {
                    iGroups.Remove(group);

                    iGroupLookup.Remove(aItem);

                    group.RemoveProduct();
                }
            }
        }

        private object iLock;
        private bool iDisposed;

        private WatchableProductCollection iProducts;
        private IWatchableThread iThread;
        private ITopology1 iTopology1;
        private Dictionary<Product, Topology2Group> iGroupLookup;
        private WatchableTopology2GroupCollection iGroups;
    }
}
