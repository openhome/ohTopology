using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IRadioPreset
    {
        string Metadata { get; }
    }

    public class RadioPreset : IRadioPreset
    {
        public static readonly RadioPreset Empty = new RadioPreset();
        private string iMetadata;

        private RadioPreset()
        {
            iMetadata = "null";
        }

        public RadioPreset(string aMetadata)
        {
            iMetadata = aMetadata;
        }

        public string Metadata
        {
            get
            {
                return iMetadata;
            }
        }
    }

    public class BrowserRadioPreset : IWatcher<IEnumerable<ITopology4Source>>, IWatcher<IEnumerable<uint>>, IDisposable
    {
        private IStandardRoom iRoom;
        private IProxyRadio iRadio;
        private Watchable<IEnumerable<IRadioPreset>> iPresets;

        public BrowserRadioPreset(IStandardRoom aRoom)
        {
            iRoom = aRoom;
            iPresets = new Watchable<IEnumerable<IRadioPreset>>(iRoom.WatchableThread, "Presets", new List<IRadioPreset>());
            iRoom.Sources.AddWatcher(this);
        }

        public void Dispose()
        {
            iRoom.WatchableThread.Execute(() =>
            {
                iRoom.Sources.RemoveWatcher(this);
            });
            iRoom = null;

            if (iRadio != null)
            {
                iRadio.IdArray.RemoveWatcher(this);
                iRadio.Dispose();
                iRadio = null;
            }
        }

        public string Name
        {
            get
            {
                return iRoom.Name;
            }
        }

        public IWatchable<IEnumerable<IRadioPreset>> Presets
        {
            get
            {
                return iPresets;
            }
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Radio")
                {
                    s.Device.Create<IProxyRadio>((IProxyRadio radio) =>
                    {
                        iRadio = radio;
                        iRadio.IdArray.AddWatcher(this);
                    });
                    return;
                }
            }
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
        }

        public void ItemOpen(string aId, IEnumerable<uint> aValue)
        {
            BuildPresetList(aValue);
        }

        public void ItemUpdate(string aId, IEnumerable<uint> aValue, IEnumerable<uint> aPrevious)
        {
            BuildPresetList(aValue);
        }

        public void ItemClose(string aId, IEnumerable<uint> aValue)
        {
        }

        private void BuildPresetList(IEnumerable<uint> aValue)
        {
            string idList = string.Empty;
            foreach (uint id in aValue)
            {
                idList += id.ToString() + " ";
            }
            Task<string> task = iRadio.ReadList(idList);
            task.ContinueWith((t) =>
            {
                List<IRadioPreset> presets = new List<IRadioPreset>();
                string result = t.Result;

                XmlDocument document = new XmlDocument();
                document.LoadXml(result);

                foreach (uint id in aValue)
                {
                    if (id > 0)
                    {
                        XmlNode n = document.SelectSingleNode(string.Format("/ChannelList/Entry[Id={0}]/Metadata", id));
                        presets.Add(new RadioPreset(n.InnerText));
                    }
                    else
                    {
                        presets.Add(RadioPreset.Empty);
                    }
                }

                iRoom.WatchableThread.Schedule(() =>
                {
                    iPresets.Update(presets);
                });
            });
        }
    }
}
