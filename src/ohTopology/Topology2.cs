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

    class Topology2Source : ITopology2Source
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
        string ProductId { get; }
        IDevice Device { get; }

        IWatchable<string> Room { get; }
        IWatchable<string> Name { get; }
        IWatchable<bool> Standby { get; }
        IWatchable<uint> SourceIndex { get; }
        IEnumerable<IWatchable<ITopology2Source>> Sources { get; }
        IWatchable<string> Registration { get; }

        void SetStandby(bool aValue);
        void SetSourceIndex(uint aValue);
        void SetRegistration(string aValue);
    }

    class Topology2Group : ITopology2Group, IWatcher<string>
    {
        public Topology2Group(IWatchableThread aThread, string aId, IProxyProduct aProduct)
        {
            iDisposed = false;
            iThread = aThread;

            iId = aId;
            iProduct = aProduct;

            iSources = new List<ITopology2Source>();
            iWatchableSources = new List<Watchable<ITopology2Source>>();

            iProduct.SourceXml.AddWatcher(this);
        }

        public void Dispose()
        {
            if (iDisposed)
            {
                throw new ObjectDisposedException("Topology2Group.Dispose");
            }

            iProduct.SourceXml.RemoveWatcher(this);
            iProduct = null;

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
                return iProduct.Attributes;
            }
        }

        public string ProductId
        {
            get
            {
                return iProduct.ProductId;
            }
        }

        public IDevice Device
        {
            get
            {
                return iProduct.Device;
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
                return iProduct.Room;
            }
        }

        public IWatchable<string> Name
        {
            get
            {
                return iProduct.Name;
            }
        }

        public IWatchable<bool> Standby
        {
            get 
            {
                return iProduct.Standby;
            }
        }

        public IWatchable<uint> SourceIndex
        {
            get
            {
                return iProduct.SourceIndex;
            }
        }

        public IWatchable<string> Registration
        {
            get
            {
                return iProduct.Registration;
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

        public void SetRegistration(string aValue)
        {
            if (iProduct != null)
            {
                iProduct.SetRegistration(aValue);
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
        private IProxyProduct iProduct;
        private List<ITopology2Source> iSources;
        private List<Watchable<ITopology2Source>> iWatchableSources;
    }

    public interface ITopology2
    {
        IWatchableUnordered<ITopology2Group> Groups { get; }
        INetwork Network { get; }
    }

    public class Topology2 : ITopology2, IUnorderedWatcher<IProxyProduct>, IDisposable
    {
        public Topology2(ITopology1 aTopology1)
        {
            iDisposed = false;

            iNetwork = aTopology1.Network;
            iTopology1 = aTopology1;

            iGroups = new WatchableUnordered<ITopology2Group>(iNetwork);

            iGroupLookup = new Dictionary<IProxyProduct, Topology2Group>();

            iNetwork.Schedule(() =>
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

            iNetwork.Execute(() =>
            {
                iTopology1.Products.RemoveWatcher(this);
                
                foreach (Topology2Group g in iGroupLookup.Values)
                {
                    g.Dispose();
                }
            });
            iTopology1 = null;

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

        public INetwork Network
        {
            get
            {
                return iNetwork;
            }
        }

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedAdd(IProxyProduct aItem)
        {
            Topology2Group group = new Topology2Group(iNetwork, aItem.Device.Udn, aItem);
            iGroupLookup.Add(aItem, group);
            iGroups.Add(group);
        }

        public void UnorderedRemove(IProxyProduct aItem)
        {
            Topology2Group group;
            if (iGroupLookup.TryGetValue(aItem, out group))
            {
                // schedule higher layer notification
                iGroups.Remove(group);
                iGroupLookup.Remove(aItem);

                group.Dispose();
            }
        }

        public void UnorderedClose()
        {
        }

        private bool iDisposed;

        private INetwork iNetwork;
        private ITopology1 iTopology1;
        private Dictionary<IProxyProduct, Topology2Group> iGroupLookup;
        private WatchableUnordered<ITopology2Group> iGroups;
    }
}
