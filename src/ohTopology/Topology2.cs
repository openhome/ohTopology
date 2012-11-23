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
        public Topology2Group(IWatchableThread aThread, Product aProduct)
        {
            iLock = new object();
            iDisposed = false;
            iSources = new List<Watchable<ITopology2Source>>();

            iThread = aThread;

            iProduct = aProduct;
            iProduct.SourceXml.AddWatcher(this);
        }

        public void Dispose()
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2Group.Dispose");
                }

                if (iProduct != null)
                {
                    iProduct.SourceXml.RemoveWatcher(this);
                    iProduct.Dispose();
                    iProduct = null;
                }

                iSources = null;

                iDisposed = true;
            }
        }

        public void ItemOpen(string aId, string aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                ProcessSourceXml(aValue, true);
            }
        }

        public void ItemClose(string aId, string aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }
            }
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    return;
                }

                ProcessSourceXml(aValue, false);
            }
        }

        public IWatchable<string> Room
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("Topology2Group.Room");
                    }

                    return iProduct.Room;
                }
            }
        }

        public IWatchable<string> Name
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("Topology2Group.Name");
                    }

                    return iProduct.Name;
                }
            }
        }

        public IWatchable<bool> Standby
        {
            get 
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("Topology2Group.Standby");
                    }

                    return iProduct.Standby;
                }
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("Topology2Group.SourceIndex");
                    }

                    return iProduct.SourceIndex;
                }
            }
        }

        public IEnumerable<IWatchable<ITopology2Source>> Sources
        {
            get
            {
                lock (iLock)
                {
                    if (iDisposed)
                    {
                        throw new ObjectDisposedException("Topology2Group.Sources");
                    }

                    return iSources;
                }
            }
        }

        public void SetStandby(bool aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2Group.SetStandby");
                }

                iProduct.SetStandby(aValue);
            }
        }

        public void SetSourceIndex(uint aValue)
        {
            lock (iLock)
            {
                if (iDisposed)
                {
                    throw new ObjectDisposedException("Topology2Group.SetSourceIndex");
                }

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
        private bool iDisposed;

        private Product iProduct;
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
                    g.Dispose();
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

                Console.WriteLine("Collection Add Product");

                Topology2Group group = new Topology2Group(iThread, aItem);

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

                Console.WriteLine("Collection Remove Product");

                Topology2Group group;
                if (iGroupLookup.TryGetValue(aItem, out group))
                {
                    Console.WriteLine("GroupRemoved");

                    iGroupLookup.Remove(aItem);

                    group.Dispose();
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
