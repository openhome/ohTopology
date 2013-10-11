using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomWatcher : IDisposable
    {
        IStandardRoom Room { get; }
        IWatchable<bool> Active { get; }
        IWatchable<bool> Enabled { get; }
    }

    public abstract class StandardRoomWatcher : IStandardRoomWatcher, IWatcher<IEnumerable<ITopology4Source>>
    {
        protected StandardRoomWatcher(IStandardRoom aRoom)
        {
            iDisposeHandler = new DisposeHandler();

            iRoom = aRoom;
            iSources = aRoom.Sources;
            iNetwork = aRoom.Network;

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(iNetwork, "Active", true);
            iEnabled = new Watchable<bool>(iNetwork, "Enabled", false);

            //iNetwork.Schedule(() =>
            //{
                iSources.AddWatcher(this);
            //});
            iRoom.Join(SetInactive);
        }

        public virtual void Dispose()
        {
            iDisposeHandler.Dispose();

            lock (iLock)
            {
                if (iIsActive)
                {
                    iNetwork.Execute(() =>
                    {
                        iSources.RemoveWatcher(this);
                    });
                    iRoom.Unjoin(SetInactive);
                    iIsActive = false;
                }
            }

            iEnabled.Dispose();
        }

        public IStandardRoom Room
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iRoom;
                }
            }
        }

        public IWatchable<bool> Active
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iActive;
                }
            }
        }

        public IWatchable<bool> Enabled
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iEnabled;
                }
            }
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
        {
            iDisposeHandler.WhenNotDisposed(() =>
            {
                EvaluateEnabledOpen(aValue);
            });
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            iDisposeHandler.WhenNotDisposed(() =>
            {
                EvaluateEnabledUpdate(aValue, aPrevious);
            });
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
            iDisposeHandler.WhenNotDisposed(() =>
            {
                EvaluateEnabledClose(aValue);
                iEnabled.Update(false);
            });
        }

        protected abstract void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue);
        protected abstract void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious);
        protected virtual void EvaluateEnabledClose(IEnumerable<ITopology4Source> aValue) { }
        
        protected void SetEnabled(bool aValue)
        {
            iEnabled.Update(aValue);
        }

        protected virtual void OnSetInactive() { }

        private void SetInactive()
        {
            lock (iLock)
            {
                if (iIsActive)
                {

                    iActive.Update(false);

                    iSources.RemoveWatcher(this);

                    SetEnabled(false);

                    OnSetInactive();

                    iIsActive = false;
                }
            }
        }

        protected readonly DisposeHandler iDisposeHandler;
        protected readonly INetwork iNetwork;

        private readonly IStandardRoom iRoom;
        private readonly IWatchable<IEnumerable<ITopology4Source>> iSources;

        private readonly object iLock;
        private bool iIsActive;
        private readonly Watchable<bool> iActive;
        private readonly Watchable<bool> iEnabled;
    }

    public class StandardRoomWatcherMusic : StandardRoomWatcher
    {
        private WatchableSourceSelectorWatchableSnapshot iWatchableSnapshot;
        private ITopology4Source iSource;
        private IProxyPlaylist iPlaylist;
        private IPlaylistWriter iPlaylistWriter;

        public StandardRoomWatcherMusic(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            DeleteProxy();
        }

        protected override void OnSetInactive()
        {
            using (iDisposeHandler.Lock)
            {
                DeleteProxy();
            }
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableSnapshot.Snapshot;
                }
            }
        }

        public IPlaylistWriter PlaylistWriter
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPlaylistWriter;
                }
            }
        }

        protected override void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue)
        {
            EvaluateEnabled(aValue);
        }

        protected override void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            SetEnabled(false);

            DeleteProxy();

            EvaluateEnabled(aValue);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            SetEnabled(false);

            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Playlist")
                {
                    s.Device.Create<IProxyPlaylist>((playlist) =>
                    {
                        iSource = s;
                        iPlaylist = playlist;
                        iWatchableSnapshot = new WatchableSourceSelectorWatchableSnapshot(iNetwork, s, playlist.Snapshot);
                        iPlaylistWriter = new PlaylistWriter(playlist);
                        SetEnabled(true);
                    });
                    return;
                }
            }
        }

        private void DeleteProxy()
        {
            if (iPlaylist != null)
            {
                iWatchableSnapshot.Dispose();
                iWatchableSnapshot = null;

                iPlaylist.Dispose();
                iPlaylist = null;
                iPlaylistWriter = null;
                iSource = null;
            }
        }
    }

    public class StandardRoomWatcherRadio : StandardRoomWatcher
    {
        private WatchableSourceSelectorWatchableSnapshot iWatchableSnapshot;
        private ITopology4Source iSource;
        private IProxyRadio iRadio;

        public StandardRoomWatcherRadio(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            DeleteProxy();
        }

        protected override void OnSetInactive()
        {
            using (iDisposeHandler.Lock)
            {
                DeleteProxy();
            }
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableSnapshot.Snapshot;
                }
            }
        }

        protected override void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue)
        {
            EvaluateEnabled(aValue);
        }

        protected override void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            SetEnabled(false);

            DeleteProxy();

            EvaluateEnabled(aValue);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Radio")
                {
                    s.Device.Create<IProxyRadio>((radio) =>
                    {
                        iSource = s;
                        iRadio = radio;
                        iWatchableSnapshot = new WatchableSourceSelectorWatchableSnapshot(iNetwork, s, radio.Snapshot);
                        SetEnabled(true);
                    });
                    return;
                }
            }
        }

        private void DeleteProxy()
        {
            if (iRadio != null)
            {
                iWatchableSnapshot.Dispose();
                iWatchableSnapshot = null;

                iRadio.Dispose();
                iRadio = null;
                iSource = null;
            }
        }
    }

    public class StandardRoomWatcherSenders : IStandardRoomWatcher, IWatcher<IEnumerable<ITopology4Source>>, IWatcher<IEnumerable<ISenderMetadata>>
    {
        class SendersMetadataWatcher : IOrderedWatcher<IProxySender>, IWatcher<ISenderMetadata>, IDisposable
        {
            private readonly IWatchableOrdered<IProxySender> iSenders;
            private readonly List<IProxySender> iProxies;
            private readonly Watchable<IEnumerable<ISenderMetadata>> iMetadata;

            public SendersMetadataWatcher(IWatchableThread aThread, IWatchableOrdered<IProxySender> aSenders)
            {
                iSenders = aSenders;
                iProxies = new List<IProxySender>();
                iMetadata = new Watchable<IEnumerable<ISenderMetadata>>(aThread, "Metadata", new List<ISenderMetadata>());

                iSenders.AddWatcher(this);
            }

            public void Dispose()
            {
                iSenders.RemoveWatcher(this);

                foreach (IProxySender s in iProxies)
                {
                    s.Metadata.RemoveWatcher(this);
                }
                iProxies.Clear();

                iMetadata.Dispose();
            }

            public IWatchable<IEnumerable<ISenderMetadata>> Metadata
            {
                get
                {
                    return iMetadata;
                }
            }

            public void OrderedOpen()
            {
            }

            public void OrderedInitialised()
            {
            }

            public void OrderedClose()
            {
            }

            public void OrderedMove(IProxySender aItem, uint aFrom, uint aTo)
            {
            }

            public void OrderedRemove(IProxySender aItem, uint aIndex)
            {
                aItem.Metadata.RemoveWatcher(this);
                iProxies.Remove(aItem);
            }

            public void OrderedAdd(IProxySender aItem, uint aIndex)
            {
                aItem.Metadata.AddWatcher(this);
                iProxies.Add(aItem);
            }

            public void ItemOpen(string aId, ISenderMetadata aValue)
            {
                List<ISenderMetadata> list = new List<ISenderMetadata>(iMetadata.Value);
                list.Insert(InsertIndex(list, aValue), aValue);
                iMetadata.Update(list);
            }

            public void ItemUpdate(string aId, ISenderMetadata aValue, ISenderMetadata aPrevious)
            {
                List<ISenderMetadata> list = new List<ISenderMetadata>(iMetadata.Value);
                list.Remove(aPrevious);
                list.Insert(InsertIndex(list, aValue), aValue);
                iMetadata.Update(list);
            }

            public void ItemClose(string aId, ISenderMetadata aValue)
            {
                List<ISenderMetadata> list = new List<ISenderMetadata>(iMetadata.Value);
                list.Remove(aValue);
                iMetadata.Update(list);
            }

            private int InsertIndex(IList<ISenderMetadata> aList, ISenderMetadata aValue)
            {
                int index = 0;
                foreach (ISenderMetadata m in aList)
                {
                    if (aValue.Name.CompareTo(m.Name) < 0)
                    {
                        break;
                    }
                    ++index;
                }

                return index;
            }
        }

        protected readonly DisposeHandler iDisposeHandler;
        protected readonly INetwork iNetwork;

        private readonly IStandardRoom iRoom;
        private readonly IWatchable<IEnumerable<ITopology4Source>> iSources;
        private readonly SendersMetadataWatcher iSendersMetadataWatcher;

        private readonly object iLock;
        private bool iIsActive;
        private readonly Watchable<bool> iActive;
        private readonly Watchable<bool> iEnabled;

        private IProxyReceiver iReceiver;
        private MediaSupervisor<IMediaPreset> iSupervisor;
        private WatchableSourceSelectorWatchableSnapshot iWatchableSnapshot;

        public StandardRoomWatcherSenders(IStandardHouse aHouse, IStandardRoom aRoom)
        {
            iDisposeHandler = new DisposeHandler();

            iRoom = aRoom;
            iSources = aRoom.Sources;
            iNetwork = aRoom.Network;
            iSendersMetadataWatcher = new SendersMetadataWatcher(iNetwork, aHouse.Senders);

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(iNetwork, "Active", true);
            iEnabled = new Watchable<bool>(iNetwork, "Enabled", false);

            iRoom.Join(SetInactive);

            //iNetwork.Schedule(() =>
            //{
            iSources.AddWatcher(this);
            //});

        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();

            lock (iLock)
            {
                if (iIsActive)
                {
                    iNetwork.Execute(() =>
                    {
                        iSources.RemoveWatcher(this);
                        iSendersMetadataWatcher.Metadata.RemoveWatcher(this);
                    });
                    iRoom.Unjoin(SetInactive);
                    iIsActive = false;
                }
            }

            iEnabled.Dispose();

            iSendersMetadataWatcher.Dispose();

            DeleteProxy();
        }

        private void SetInactive()
        {
            lock (iLock)
            {
                if (iIsActive)
                {

                    iActive.Update(false);

                    iSources.RemoveWatcher(this);
                    iSendersMetadataWatcher.Metadata.RemoveWatcher(this);

                    DeleteProxy();

                    iIsActive = false;
                }
            }
        }

        public IStandardRoom Room
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iRoom;
                }
            }
        }

        public IWatchable<bool> Active
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iActive;
                }
            }
        }

        public IWatchable<bool> Enabled
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iEnabled;
                }
            }
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableSnapshot.Snapshot;
                }
            }
        }

        public void ItemOpen(string aId, IEnumerable<ITopology4Source> aValue)
        {
            EvaluateEnabled(aValue);
        }

        public void ItemUpdate(string aId, IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            SetEnabled(false);

            iSendersMetadataWatcher.Metadata.RemoveWatcher(this);

            DeleteProxy();

            EvaluateEnabled(aValue);
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
            iEnabled.Update(false);

            iSendersMetadataWatcher.Metadata.RemoveWatcher(this);

            DeleteProxy();
        }

        public void ItemOpen(string aId, IEnumerable<ISenderMetadata> aValue)
        {
            iSupervisor.Update(new SendersSnapshot(iNetwork, iReceiver, aValue));
        }

        public void ItemUpdate(string aId, IEnumerable<ISenderMetadata> aValue, IEnumerable<ISenderMetadata> aPrevious)
        {
            iSupervisor.Update(new SendersSnapshot(iNetwork, iReceiver, aValue));
        }

        public void ItemClose(string aId, IEnumerable<ISenderMetadata> aValue)
        {
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            if (iSupervisor != null)
            {
                Console.WriteLine("here");
            }
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Receiver")
                {
                    s.Device.Create<IProxyReceiver>((receiver) =>
                    {
                        iSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new SendersSnapshot(iNetwork, receiver, new List<ISenderMetadata>()));
                        iWatchableSnapshot = new WatchableSourceSelectorWatchableSnapshot(iNetwork, s, iSupervisor.Snapshot);
                        iReceiver = receiver;
                        iSendersMetadataWatcher.Metadata.AddWatcher(this);
                        SetEnabled(true);
                    });
                    return;
                }
            }

            SetEnabled(false);
        }

        private void SetEnabled(bool aValue)
        {
            iEnabled.Update(aValue);
        }

        private void DeleteProxy()
        {
            if (iReceiver != null)
            {
                iWatchableSnapshot.Dispose();
                iWatchableSnapshot = null;

                iSupervisor.Dispose();
                iSupervisor = null;

                iReceiver.Dispose();
                iReceiver = null;
            }
        }
    }

    class MediaPresetSender : IMediaPreset, IWatcher<string>, IWatcher<IInfoMetadata>
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly uint iIndex;
        private readonly uint iId;
        private readonly IMediaMetadata iMetadata;
        private readonly ISenderMetadata iSenderMetadata;
        private readonly IProxyReceiver iReceiver;
        private readonly Watchable<bool> iBuffering;
        private readonly Watchable<bool> iPlaying;
        private IInfoMetadata iCurrentMetadata;
        private string iCurrentTransportState;
        private bool iDisposed;

        public MediaPresetSender(INetwork aNetwork, uint aIndex, uint aId, IMediaMetadata aMetadata, ISenderMetadata aSenderMetadata, IProxyReceiver aReceiver)
        {
            iDisposed = false;
            iDisposeHandler = new DisposeHandler();

            iNetwork = aNetwork;
            iIndex = aIndex;
            iId = aId;
            iMetadata = aMetadata;
            iSenderMetadata = aSenderMetadata;
            iReceiver = aReceiver;

            iBuffering = new Watchable<bool>(iNetwork, "Buffering", false);
            iPlaying = new Watchable<bool>(iNetwork, "Playing", false);
            iNetwork.Schedule(() =>
            {
                if (!iDisposed)
                {
                    iReceiver.Metadata.AddWatcher(this);
                    iReceiver.TransportState.AddWatcher(this);
                }
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();
            iNetwork.Execute(() =>
            {
                iReceiver.Metadata.RemoveWatcher(this);
                iReceiver.TransportState.RemoveWatcher(this);
                iDisposed = true;
            });
            iBuffering.Dispose();
            iPlaying.Dispose();
        }

        public uint Index
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iIndex;
                }
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iMetadata;
                }
            }
        }

        public IWatchable<bool> Buffering
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iBuffering;
                }
            }
        }

        public IWatchable<bool> Playing
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPlaying;
                }
            }
        }

        public void Play()
        {
            using (iDisposeHandler.Lock)
            {
                iReceiver.Play(iSenderMetadata);
            }
        }

        private void EvaluatePlaying()
        {
            iBuffering.Update(iCurrentMetadata.Uri == iSenderMetadata.Uri && iCurrentTransportState == "Buffering");
            iPlaying.Update(iCurrentMetadata.Uri == iSenderMetadata.Uri && iCurrentTransportState == "Playing");
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

        public void ItemOpen(string aId, IInfoMetadata aValue)
        {
            iCurrentMetadata = aValue;
            EvaluatePlaying();
        }

        public void ItemUpdate(string aId, IInfoMetadata aValue, IInfoMetadata aPrevious)
        {
            iCurrentMetadata = aValue;
            EvaluatePlaying();
        }

        public void ItemClose(string aId, IInfoMetadata aValue)
        {
            iPlaying.Update(false);
        }
    }

    class SendersSnapshot : IMediaClientSnapshot<IMediaPreset>
    {
        private readonly IEnumerable<ISenderMetadata> iSendersMetadata;
        private readonly IProxyReceiver iReceiver;
        private readonly INetwork iNetwork;

        public SendersSnapshot(INetwork aNetwork, IProxyReceiver aReceiver, IEnumerable<ISenderMetadata> aSendersMetadata)
        {
            iNetwork = aNetwork;
            iReceiver = aReceiver;
            iSendersMetadata = aSendersMetadata;
        }

        public uint Total
        {
            get
            {
                return ((uint)iSendersMetadata.Count());
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<IMediaPreset> Read(CancellationToken aCancellationToken, uint aIndex, uint aCount)
        {
            uint index = aIndex;
            List<IMediaPreset> presets = new List<IMediaPreset>();
            iSendersMetadata.Skip((int)aIndex).Take((int)aCount).ToList().ForEach(v =>
            {
                string room, name;
                ParseName(v.Name, out room, out name);
                string fullname = string.Format("{0} ({1})", room, name);
                if (room == name || string.IsNullOrEmpty(name))
                {
                    fullname = room;
                }
                MediaMetadata metadata = new MediaMetadata();
                metadata.Add(iNetwork.TagManager.Audio.Title, fullname);
                metadata.Add(iNetwork.TagManager.Audio.Artwork, v.ArtworkUri);
                metadata.Add(iNetwork.TagManager.Audio.Uri, v.Uri);
                presets.Add(new MediaPresetSender(iNetwork, index, index, metadata, v, iReceiver));
                ++index;
            });
            return presets;
        }

        private static bool ParseBrackets(string aMetadata, out string aRoom, out string aName, char aOpen, char aClose)
        {
            int open = aMetadata.IndexOf(aOpen);

            if (open >= 0)
            {
                int close = aMetadata.IndexOf(aClose);

                if (close > -0)
                {
                    int bracketed = close - open - 1;

                    if (bracketed > 1)
                    {
                        aRoom = aMetadata.Substring(0, open).Trim();
                        aName = aMetadata.Substring(open + 1, bracketed).Trim();
                        return (true);
                    }
                }
            }

            aRoom = aMetadata;
            aName = aMetadata;

            return (false);
        }

        private void ParseName(string aMetadata, out string aRoom, out string aName)
        {
            if (ParseBrackets(aMetadata, out aRoom, out aName, '(', ')'))
            {
                return;
            }

            if (ParseBrackets(aMetadata, out aRoom, out aName, '[', ']'))
            {
                return;
            }

            if (ParseBrackets(aMetadata, out aRoom, out aName, '<', '>'))
            {
                return;
            }

            int index = aMetadata.IndexOf(':');

            if (index < 0)
            {
                index = aMetadata.IndexOf('.');
            }

            if (index < 0)
            {
                aRoom = aMetadata;
                aName = aMetadata;
                return;
            }

            string temp = aMetadata.Substring(0, index).Trim();
            aName = aMetadata.Substring(index + 1).Trim();

            index = temp.IndexOf('.');
            if (index < 0)
            {
                aRoom = temp;
            }
            else
            {
                aRoom = temp.Substring(index + 1).Trim();
            }
        }
    }

    public class StandardRoomWatcherExternal : StandardRoomWatcher
    {
        private MediaSupervisor<IMediaPreset> iConfigured;
        private MediaSupervisor<IMediaPreset> iUnconfigured;

        public StandardRoomWatcherExternal(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            iConfigured.Dispose();
            iUnconfigured.Dispose();
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Configured
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iConfigured.Snapshot;
                }
            }
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Unconfigured
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iUnconfigured.Snapshot;
                }
            }
        }

        protected override void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue)
        {
            iConfigured = new MediaSupervisor<IMediaPreset>(iNetwork, new ExternalSnapshot(new List<ITopology4Source>()));
            iUnconfigured = new MediaSupervisor<IMediaPreset>(iNetwork, new ExternalSnapshot(new List<ITopology4Source>()));
            EvaluateEnabled(aValue);
        }

        protected override void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            EvaluateEnabled(aValue);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            SetEnabled(BuildLists(aValue));
        }

        private bool BuildLists(IEnumerable<ITopology4Source> aValue)
        {
            uint cIndex = 0;
            uint uIndex = 0;
            bool hasExternal = false;
            List<ITopology4Source> configured = new List<ITopology4Source>();
            List<ITopology4Source> unconfigured = new List<ITopology4Source>();
            foreach (ITopology4Source s in aValue)
            {
                if (IsExternal(s))
                {
                    if (IsConfigured(s))
                    {
                        configured.Add(s);
                        cIndex++;
                    }
                    else
                    {
                        hasExternal = true;
                        unconfigured.Add(s);
                        uIndex++;
                    }
                }
            }

            iUnconfigured.Update(new ExternalSnapshot(unconfigured));
            iConfigured.Update(new ExternalSnapshot(configured));

            return hasExternal;
        }

        private bool IsExternal(ITopology4Source aSource)
        {
            return (aSource.Type == "Analog" || aSource.Type == "Digital" || aSource.Type == "Hdmi");
        }

        private bool IsConfigured(ITopology4Source aSource)
        {
            if (aSource.Name.StartsWith("HDMI") || aSource.Name.StartsWith("Hdmi"))
            {
                try
                {
                    if (aSource.Name.Length == 4)
                    {
                        return false;
                    }
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
                    if (aSource.Name.Length == 6)
                    {
                        return false;
                    }
                    uint.Parse(aSource.Name.Substring(6));
                    return false;
                }
                catch (FormatException)
                {
                    if (aSource.Name == "Analog Knekt")
                    {
                        return false;
                    }
                    return true;
                }
            }
            if (aSource.Name.StartsWith("SPDIF") || aSource.Name.StartsWith("Spdif"))
            {
                try
                {
                    if (aSource.Name.Length == 5)
                    {
                        return false;
                    }
                    uint.Parse(aSource.Name.Substring(5));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name.StartsWith("TOSLINK") || aSource.Name.StartsWith("Toslink"))
            {
                try
                {
                    if (aSource.Name.Length == 7)
                    {
                        return false;
                    }
                    uint.Parse(aSource.Name.Substring(7));
                    return false;
                }
                catch (FormatException)
                {
                    return true;
                }
            }
            if (aSource.Name.StartsWith("Balanced"))
            {
                try
                {
                    if (aSource.Name.Length == 8)
                    {
                        return false;
                    }
                    uint.Parse(aSource.Name.Substring(8));
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
            if (aSource.Name == "Front Aux")
            {
                return false;
            }

            return true;
        }
    }

    class ExternalSnapshot : IMediaClientSnapshot<IMediaPreset>
    {
        private readonly IList<ITopology4Source> iSources;

        public ExternalSnapshot(IList<ITopology4Source> aSources)
        {
            iSources = aSources;
        }

        public uint Total
        {
            get
            {
                return ((uint)iSources.Count);
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                return null;
            }
        }

        public IEnumerable<IMediaPreset> Read(CancellationToken aCancellationToken, uint aIndex, uint aCount)
        {
            List<IMediaPreset> presets = new List<IMediaPreset>();
            iSources.Skip((int)aIndex).Take((int)aCount).ToList().ForEach(v => presets.Add(v.CreatePreset()));
            return presets;
        }
    }

    public class StandardRoomWatcherDisc : StandardRoomWatcher
    {
        private WatchableSourceSelectorWatchableSnapshot iWatchableSnapshot;
        private ITopology4Source iSource;
        private IProxySdp iSdp;

        public StandardRoomWatcherDisc(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            DeleteProxy();
        }

        protected override void OnSetInactive()
        {
            using (iDisposeHandler.Lock)
            {
                DeleteProxy();
            }
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iWatchableSnapshot.Snapshot;
                }
            }
        }

        protected override void EvaluateEnabledOpen(IEnumerable<ITopology4Source> aValue)
        {
            EvaluateEnabled(aValue);
        }

        protected override void EvaluateEnabledUpdate(IEnumerable<ITopology4Source> aValue, IEnumerable<ITopology4Source> aPrevious)
        {
            SetEnabled(false);

            DeleteProxy();

            EvaluateEnabled(aValue);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Disc")
                {
                    s.Device.Create<IProxySdp>((sdp) =>
                    {
                        iSource = s;
                        iSdp = sdp;
                        iWatchableSnapshot = new WatchableSourceSelectorWatchableSnapshot(iNetwork, s, sdp.Snapshot);
                        SetEnabled(true);
                    });
                    return;
                }
            }
        }

        private void DeleteProxy()
        {
            if (iSdp != null)
            {
                iWatchableSnapshot.Dispose();
                iWatchableSnapshot = null;

                iSdp.Dispose();
                iSdp = null;
                iSource = null;
            }
        }
    }
}
