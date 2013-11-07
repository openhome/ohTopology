using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;

using OpenHome.Os.App;
using OpenHome.Net.ControlPoint.Proxies;
using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    class MediaPresetRadio : IMediaPreset, IWatcher<uint>, IWatcher<string>
    {
        private readonly uint iIndex;
        private readonly uint iId;
        private readonly IMediaMetadata iMetadata;
        private readonly string iUri;
        private readonly ServiceRadio iRadio;
        private readonly Watchable<bool> iBuffering;
        private readonly Watchable<bool> iPlaying;

        private uint iCurrentId;
        private string iCurrentTransportState;

        public MediaPresetRadio(IWatchableThread aThread, uint aIndex, uint aId, IMediaMetadata aMetadata, string aUri, ServiceRadio aRadio)
        {
            iIndex = aIndex;
            iId = aId;
            iMetadata = aMetadata;
            iUri = aUri;
            iRadio = aRadio;

            iBuffering = new Watchable<bool>(aThread, "Buffering", false);
            iPlaying = new Watchable<bool>(aThread, "Playing", false);
            iRadio.Id.AddWatcher(this);
            iRadio.TransportState.AddWatcher(this);
        }

        public void Dispose()
        {
            iRadio.Id.RemoveWatcher(this);
            iRadio.TransportState.RemoveWatcher(this);
            iBuffering.Dispose();
            iPlaying.Dispose();
        }

        public uint Index
        {
            get
            {
                return iIndex;
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<bool> Buffering
        {
            get
            {
                return iBuffering;
            }
        }

        public IWatchable<bool> Playing
        {
            get
            {
                return iPlaying;
            }
        }

        public void Play()
        {
            if (iId > 0)
            {
                iRadio.SetId(iId, iUri).ContinueWith((t) =>
                {
                    iRadio.Play();
                });
            }
        }

        private void EvaluatePlaying()
        {
            iBuffering.Update(iCurrentId == iId && iCurrentTransportState == "Buffering");
            iPlaying.Update(iCurrentId == iId && iCurrentTransportState == "Playing");
        }

        public void ItemOpen(string aId, uint aValue)
        {
            iCurrentId = aValue;
            EvaluatePlaying();
        }

        public void ItemUpdate(string aId, uint aValue, uint aPrevious)
        {
            iCurrentTransportState = string.Empty;
            iCurrentId = aValue;
            EvaluatePlaying();
        }

        public void ItemClose(string aId, uint aValue)
        {
            iPlaying.Update(false);
        }

        public void ItemOpen(string aId, string aValue)
        {
            iCurrentTransportState = aValue;
            EvaluatePlaying();
        }

        public void ItemUpdate(string aId, string aValue, string aPrevious)
        {
            iCurrentTransportState = aValue;
            EvaluatePlaying();
        }

        public void ItemClose(string aId, string aValue)
        {
            iPlaying.Update(false);
        }
    }

    public interface IProxyRadio : IProxy
    {
        IWatchable<uint> Id { get; }
        IWatchable<string> TransportState { get; }
        IWatchable<IInfoMetadata> Metadata { get; }

        Task Play();
        Task Pause();
        Task Stop();
        Task SeekSecondAbsolute(uint aValue);
        Task SeekSecondRelative(int aValue);

        Task SetId(uint aId, string aUri);
        Task SetChannel(string aUri, IMediaMetadata aMetadata);

        IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot { get; }

        uint ChannelsMax { get; }
        string ProtocolInfo { get; }
    }

    public abstract class ServiceRadio : Service
    {
        protected ServiceRadio(INetwork aNetwork, IInjectorDevice aDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iId = new Watchable<uint>(aNetwork, "Id", 0);
            iTransportState = new Watchable<string>(aNetwork, "TransportState", string.Empty);
            iMetadata = new Watchable<IInfoMetadata>(aNetwork, "Metadata", InfoMetadata.Empty);
        }

        public override void Dispose()
        {
            base.Dispose();

            iId.Dispose();
            iId = null;

            iTransportState.Dispose();
            iTransportState = null;

            iMetadata.Dispose();
            iMetadata = null;
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return new ProxyRadio(this, aDevice);
        }
        
        public IWatchable<uint> Id
        {
            get
            {
                return iId;
            }
        }
        
        public IWatchable<string> TransportState
        {
            get
            {
                return iTransportState;
            }
        }
        
        public IWatchable<IInfoMetadata> Metadata
        {
            get
            {
                return iMetadata;
            }
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iMediaSupervisor.Snapshot;
            }
        }

        public uint ChannelsMax
        {
            get
            {
                return iChannelsMax;
            }
        }

        public string ProtocolInfo
        {
            get
            {
                return iProtocolInfo;
            }
        }

        public abstract Task Play();
        public abstract Task Pause();
        public abstract Task Stop();
        public abstract Task SeekSecondAbsolute(uint aValue);
        public abstract Task SeekSecondRelative(int aValue);
        public abstract Task SetId(uint aId, string aUri);
        public abstract Task SetChannel(string aUri, IMediaMetadata aMetadata);
        
        protected uint iChannelsMax;
        protected string iProtocolInfo;

        protected Watchable<uint> iId;
        protected Watchable<string> iTransportState;
        protected Watchable<IInfoMetadata> iMetadata;

        protected MediaSupervisor<IMediaPreset> iMediaSupervisor;
    }

    class ServiceRadioNetwork : ServiceRadio
    {
        public ServiceRadioNetwork(INetwork aNetwork, IInjectorDevice aDevice, CpDevice aCpDevice, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iCpDevice = aCpDevice;
            iCpDevice.AddRef();

            iService = new CpProxyAvOpenhomeOrgRadio1(aCpDevice);

            iService.SetPropertyIdChanged(HandleIdChanged);
            iService.SetPropertyIdArrayChanged(HandleIdArrayChanged);
            iService.SetPropertyMetadataChanged(HandleMetadataChanged);
            iService.SetPropertyTransportStateChanged(HandleTransportStateChanged);

            iService.SetPropertyInitialEvent(HandleInitialEvent);
        }

        public override void Dispose()
        {
            base.Dispose();

            Do.Assert(iCacheSession == null);

            iService.Dispose();
            iService = null;

            iCpDevice.RemoveRef();
        }

        protected override Task OnSubscribe()
        {
            Do.Assert(iSubscribedSource == null);

            iSubscribedSource = new TaskCompletionSource<bool>();

            iCacheSession = Network.IdCache.CreateSession(string.Format("Radio({0})", Device.Udn), ReadList);

            iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new RadioSnapshot(iNetwork, iCacheSession, new List<uint>(), this));

            iService.Subscribe();

            return iSubscribedSource.Task.ContinueWith((t) => { });
        }

        protected override void OnCancelSubscribe()
        {
            if (iSubscribedSource != null)
            {
                iSubscribedSource.TrySetCanceled();
            }
        }

        private void HandleInitialEvent()
        {
            iChannelsMax = iService.PropertyChannelsMax();
            iProtocolInfo = iService.PropertyProtocolInfo();

            if (!iSubscribedSource.Task.IsCanceled)
            {
                iSubscribedSource.SetResult(true);
            }
        }

        protected override void OnUnsubscribe()
        {
            if (iService != null)
            {
                iService.Unsubscribe();
            }

            if (iMediaSupervisor != null)
            {
                iMediaSupervisor.Dispose();
                iMediaSupervisor = null;
            }

            if (iCacheSession != null)
            {
                iCacheSession.Dispose();
                iCacheSession = null;
            }

            iSubscribedSource = null;
        }

        public override Task Play()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginPlay((ptr) =>
            {
                try
                {
                    iService.EndPlay(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task Pause()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginPause((ptr) =>
            {
                try
                {
                    iService.EndPause(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task Stop()
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginStop((ptr) =>
            {
                try
                {
                    iService.EndStop(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task SeekSecondAbsolute(uint aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSeekSecondAbsolute(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSeekSecondAbsolute(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task SeekSecondRelative(int aValue)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSeekSecondRelative(aValue, (ptr) =>
            {
                try
                {
                    iService.EndSeekSecondRelative(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task SetId(uint aId, string aUri)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetId(aId, aUri, (ptr) =>
            {
                try
                {
                    iService.EndSetId(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        public override Task SetChannel(string aUri, IMediaMetadata aMetadata)
        {
            TaskCompletionSource<bool> taskSource = new TaskCompletionSource<bool>();
            iService.BeginSetChannel(aUri, iNetwork.TagManager.ToDidlLite(aMetadata), (ptr) =>
            {
                try
                {
                    iService.EndSetChannel(ptr);
                    taskSource.SetResult(true);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });
            return taskSource.Task.ContinueWith((t) => { });
        }

        private Task<IEnumerable<IIdCacheEntry>> ReadList(IEnumerable<uint> aIdList)
        {
            TaskCompletionSource<IEnumerable<IIdCacheEntry>> taskSource = new TaskCompletionSource<IEnumerable<IIdCacheEntry>>();

            string idList = string.Empty;
            foreach(uint id in aIdList)
            {
                idList += string.Format("{0} ", id);
            }
            idList.Trim(' ');

            iService.BeginReadList(idList, (ptr) =>
            {
                try
                {
                    string channelList;
                    iService.EndReadList(ptr, out channelList);

                    List<IIdCacheEntry> entries = new List<IIdCacheEntry>();

                    XmlDocument document = new XmlDocument();
                    document.LoadXml(channelList);

                    foreach (uint id in aIdList)
                    {
                        if (id > 0)
                        {
                            XmlNode n = document.SelectSingleNode(string.Format("/ChannelList/Entry[Id={0}]/Metadata", id));
                            IMediaMetadata metadata = iNetwork.TagManager.FromDidlLite(n.InnerText);
                            string uri = metadata[iNetwork.TagManager.Audio.Uri].Value;
                            entries.Add(new IdCacheEntry(metadata, uri));
                        }
                    }

                    taskSource.SetResult(entries);
                }
                catch (Exception e)
                {
                    taskSource.SetException(e);
                }
            });

            return taskSource.Task.ContinueWith((t) => { return t.Result; });
        }

        private void HandleIdChanged()
        {
            uint id = iService.PropertyId();
            Network.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iId.Update(id);
                });
            });
        }

        private void HandleIdArrayChanged()
        {
            IList<uint> idArray = ByteArray.Unpack(iService.PropertyIdArray());
            Network.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iCacheSession.SetValid(idArray.Where(v => v != 0).ToList());
                    iMediaSupervisor.Update(new RadioSnapshot(Network, iCacheSession, idArray, this));
                });
            });
        }

        private void HandleMetadataChanged()
        {
            IMediaMetadata metadata = iNetwork.TagManager.FromDidlLite(iService.PropertyMetadata());
            string uri = iService.PropertyUri();
            Network.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iMetadata.Update(
                        new InfoMetadata(
                            metadata,
                            uri
                        ));
                });
            });
        }

        private void HandleTransportStateChanged()
        {
            string transportState = iService.PropertyTransportState();
            Network.Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iTransportState.Update(transportState);
                });
            });
        }

        private readonly CpDevice iCpDevice;
        private TaskCompletionSource<bool> iSubscribedSource;
        private CpProxyAvOpenhomeOrgRadio1 iService;
        private IIdCacheSession iCacheSession;
    }

    class RadioSnapshot : IMediaClientSnapshot<IMediaPreset>
    {
        private readonly INetwork iNetwork;
        private readonly IIdCacheSession iCacheSession;
        private readonly IList<uint> iIdArray;
        private readonly ServiceRadio iRadio;

        public RadioSnapshot(INetwork aNetwork, IIdCacheSession aCacheSession, IList<uint> aIdArray, ServiceRadio aRadio)
        {
            iNetwork = aNetwork;
            iCacheSession = aCacheSession;
            iIdArray = aIdArray;
            iRadio = aRadio;
        }

        public uint Total
        {
            get
            {
                return ((uint)iIdArray.Where(v => v != 0).Count());
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                return null;
            }
        }

        public void Read(CancellationToken aCancellationToken, uint aIndex, uint aCount, Action<IEnumerable<IMediaPreset>> aCallback)
        {
            Do.Assert(aIndex + aCount <= Total);

            List<uint> idList = new List<uint>();
            for (uint i = aIndex; i < aIndex + aCount; ++i)
            {
                idList.Add(iIdArray.Where(v => v != 0).ElementAt((int)i));
            }

            List<IMediaPreset> presets = new List<IMediaPreset>();
            IEnumerable<IIdCacheEntry> entries = new List<IIdCacheEntry>();
            try
            {
                entries = iCacheSession.Entries(idList).Result;
            }
            catch
            {
            }

            iNetwork.Schedule(() =>
            {
                uint index = aIndex;
                foreach (IIdCacheEntry e in entries)
                {
                    uint id = iIdArray.Where(v => v != 0).ElementAt((int)index);
                    presets.Add(new MediaPresetRadio(iNetwork, (uint)(iIdArray.IndexOf(id) + 1), id, e.Metadata, e.Uri, iRadio));
                    ++index;
                }

                aCallback(presets);
            });
        }
    }

    class ServiceRadioMock : ServiceRadio, IMockable
    {
        private IIdCacheSession iCacheSession;
        private IList<IMediaMetadata> iPresets;
        private List<uint> iIdArray;

        public ServiceRadioMock(INetwork aNetwork, IInjectorDevice aDevice, uint aId, IList<IMediaMetadata> aPresets,
            IInfoMetadata aMetadata, string aProtocolInfo, string aTransportState, uint aChannelsMax, ILog aLog)
            : base(aNetwork, aDevice, aLog)
        {
            iChannelsMax = aChannelsMax;
            iProtocolInfo = aProtocolInfo;

            iIdArray = new List<uint>();
            uint id = 1;
            foreach (IMediaMetadata m in aPresets)
            {
                if (m == null)
                {
                    iIdArray.Add(0);
                }
                else
                {
                    iIdArray.Add(id);
                }
                ++id;
            }
            iPresets = aPresets;
            
            iId.Update(aId);
            iMetadata.Update(aMetadata);
            iTransportState.Update(aTransportState);
        }

        public override void Dispose()
        {
            if (iMediaSupervisor != null)
            {
                iMediaSupervisor.Dispose();
                iMediaSupervisor = null;
            }

            if (iCacheSession != null)
            {
                iCacheSession.Dispose();
                iCacheSession = null;
            }

            base.Dispose();
        }

        protected override Task OnSubscribe()
        {
            iCacheSession = Network.IdCache.CreateSession(string.Format("Radio({0})", Device.Udn), ReadList);
            iCacheSession.SetValid(iIdArray.Where(v => v != 0).ToList());

            iMediaSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new RadioSnapshot(iNetwork, iCacheSession, iIdArray, this));

            return base.OnSubscribe();
        }

        protected override void OnUnsubscribe()
        {
            if (iMediaSupervisor != null)
            {
                iMediaSupervisor.Dispose();
                iMediaSupervisor = null;
            } 
            
            if (iCacheSession != null)
            {
                iCacheSession.Dispose();
                iCacheSession = null;
            }

            base.OnUnsubscribe();
        }

        public override Task Play()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iTransportState.Update("Playing");
                });
            });
            return task;
        }

        public override Task Pause()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iTransportState.Update("Paused");
                });
            });
            return task;
        }

        public override Task Stop()
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iTransportState.Update("Stopped");
                });
            });
            return task;
        }

        public override Task SeekSecondAbsolute(uint aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
            });
            return task;
        }

        public override Task SeekSecondRelative(int aValue)
        {
            Task task = Task.Factory.StartNew(() =>
            {
            });
            return task;
        }

        public override Task SetId(uint aId, string aUri)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iId.Update(aId);
                });
            });
            return task;
        }

        public override Task SetChannel(string aUri, IMediaMetadata aMetadata)
        {
            Task task = Task.Factory.StartNew(() =>
            {
                Network.Schedule(() =>
                {
                    iMetadata.Update(new InfoMetadata(aMetadata, aUri));
                });
            });
            return task;
        }

        private Task<IEnumerable<IIdCacheEntry>> ReadList(IEnumerable<uint> aIdList)
        {
            Task<IEnumerable<IIdCacheEntry>> task = Task<IEnumerable<IIdCacheEntry>>.Factory.StartNew(() =>
            {
                List<IdCacheEntry> entries = new List<IdCacheEntry>();
                lock(iIdArray)
                {
                    foreach (uint id in aIdList)
                    {
                        IMediaMetadata metadata = iPresets[iIdArray.IndexOf(id)];
                        entries.Add(new IdCacheEntry(metadata, metadata[Network.TagManager.Audio.Uri].Value)); 
                    }
                }
                return entries;
            });
            return task;
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "channelsmax")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iChannelsMax = uint.Parse(value.First());
            }
            else if (command == "protocolinfo")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iProtocolInfo = string.Join(" ", value);
            }
            else if (command == "id")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iId.Update(uint.Parse(value.First()));
            }
            else if (command == "presets")
            {
                throw new NotImplementedException();
            }
            else if (command == "metadata")
            {
                IEnumerable<string> value = aValue.Skip(1);
                if (value.Count() != 2)
                {
                    throw new NotSupportedException();
                }

                /*XmlDocument document = new XmlDocument();
                XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
                nsManager.AddNamespace("didl", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
                nsManager.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
                nsManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
                nsManager.AddNamespace("ldl", "urn:linn-co-uk/DIDL-Lite");

                XmlNode didl = document.CreateElement("DIDL-Lite", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");

                XmlNode item = document.CreateElement("item", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");

                XmlNode title = document.CreateElement("dc:title", "http://purl.org/dc/elements/1.1/");
                title.AppendChild(document.CreateTextNode(value.ElementAt(0)));
                item.AppendChild(title);

                XmlNode c = document.CreateElement("upnp:class", "urn:schemas-upnp-org:metadata-1-0/upnp/");
                c.AppendChild(document.CreateTextNode("object.item.audioItem"));
                item.AppendChild(c);

                XmlNode res = document.CreateElement("res", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
                res.AppendChild(document.CreateTextNode(value.ElementAt(1)));
                item.AppendChild(res);

                didl.AppendChild(item);

                document.AppendChild(didl);*/

                IInfoMetadata metadata = new InfoMetadata(Network.TagManager.FromDidlLite(value.ElementAt(0)), value.ElementAt(1));
                iMetadata.Update(metadata);
            }
            else if (command == "transportstate")
            {
                IEnumerable<string> value = aValue.Skip(1);
                iTransportState.Update(value.First());
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public class ProxyRadio : Proxy<ServiceRadio>, IProxyRadio
    {
        public ProxyRadio(ServiceRadio aService, IDevice aDevice)
            : base(aService, aDevice)
        {
        }

        public IWatchable<uint> Id
        {
            get { return iService.Id; }
        }

        public IWatchable<string> TransportState
        {
            get { return iService.TransportState; }
        }

        public IWatchable<IInfoMetadata> Metadata
        {
            get { return iService.Metadata; }
        }

        public uint ChannelsMax
        {
            get { return iService.ChannelsMax; }
        }

        public string ProtocolInfo
        {
            get { return iService.ProtocolInfo; }
        }

        public Task Play()
        {
            return iService.Play();
        }

        public Task Pause()
        {
            return iService.Pause();
        }

        public Task Stop()
        {
            return iService.Stop();
        }

        public Task SeekSecondAbsolute(uint aValue)
        {
            return iService.SeekSecondAbsolute(aValue);
        }

        public Task SeekSecondRelative(int aValue)
        {
            return iService.SeekSecondRelative(aValue);
        }

        public Task SetId(uint aId, string aUri)
        {
            return iService.SetId(aId, aUri);
        }

        public Task SetChannel(string aUri, IMediaMetadata aMetadata)
        {
            return iService.SetChannel(aUri, aMetadata);
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                return iService.Snapshot;
            }
        }
    }
}
