using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    class StandardRoomWatcherMediaPreset : IMediaPreset
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly IMediaPreset iSource;
        private readonly IMediaPreset iPreset;

        public StandardRoomWatcherMediaPreset(ITopology4Source aSource, IMediaPreset aPreset)
        {
            iDisposeHandler = new DisposeHandler();
            iSource = aSource.CreatePreset();
            iPreset = aPreset;
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();
            iSource.Dispose();
            iPreset.Dispose();
        }

        public uint Index
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPreset.Index;
                }
            }
        }

        public IMediaMetadata Metadata
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPreset.Metadata;
                }
            }
        }

        public IWatchable<bool> Buffering
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPreset.Buffering;
                }
            }
        }

        public IWatchable<bool> Playing
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPreset.Playing;
                }
            }
        }

        public void Play()
        {
            using (iDisposeHandler.Lock)
            {
                iSource.Play();
                iPreset.Play();
            }
        }
    }

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
                    iRoom.UnJoin(SetInactive);
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

                    iRoom.UnJoin(SetInactive);
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
        private IProxyPlaylist iPlaylist;
        private IPlaylistWriter iPlaylistWriter;

        public StandardRoomWatcherMusic(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            if (iPlaylist != null)
            {
                iPlaylist.Dispose();
                iPlaylist = null;
                iPlaylistWriter = null;
            }
        }

        protected override void OnSetInactive()
        {
            using (iDisposeHandler.Lock)
            {
                if (iPlaylist != null)
                {
                    iPlaylist.Dispose();
                    iPlaylist = null;
                    iPlaylistWriter = null;
                }
            }
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iPlaylist.Snapshot;
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

            if (iPlaylist != null)
            {
                iPlaylist.Dispose();
                iPlaylist = null;
                iPlaylistWriter = null;
            }

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
                        iPlaylist = playlist;
                        iPlaylistWriter = new PlaylistWriter(playlist);
                        SetEnabled(true);
                    });
                    return;
                }
            }
        }
    }

    public class StandardRoomWatcherRadio : StandardRoomWatcher
    {
        private IProxyRadio iRadio;

        public StandardRoomWatcherRadio(IStandardRoom aRoom)
            : base(aRoom)
        {
        }

        public override void Dispose()
        {
            base.Dispose();

            if (iRadio != null)
            {
                iRadio.Dispose();
                iRadio = null;
            }
        }

        protected override void OnSetInactive()
        {
            using (iDisposeHandler.Lock)
            {
                if (iRadio != null)
                {
                    iRadio.Dispose();
                    iRadio = null;
                }
            }
        }

        public IWatchable<IWatchableSnapshot<IMediaPreset>> Snapshot
        {
            get
            {
                using (iDisposeHandler.Lock)
                {
                    return iRadio.Snapshot;
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

            if (iRadio != null)
            {
                iRadio.Dispose();
                iRadio = null;
            }

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
                        iRadio = radio;
                        SetEnabled(true);
                    });
                    return;
                }
            }
        }
    }

    public class StandardRoomWatcherSenders : IStandardRoomWatcher, IWatcher<IEnumerable<ITopology4Source>>
    {
        protected readonly DisposeHandler iDisposeHandler;
        protected readonly INetwork iNetwork;

        private readonly IStandardRoom iRoom;
        private readonly IWatchable<IEnumerable<ITopology4Source>> iSources;

        private readonly object iLock;
        private bool iIsActive;
        private readonly Watchable<bool> iActive;
        private readonly Watchable<bool> iEnabled;

        private IProxyReceiver iReceiver;
        private MediaSupervisor<IMediaPreset> iSupervisor;
        private IWatchableOrdered<IProxySender> iSenders;

        public StandardRoomWatcherSenders(IStandardHouse aHouse, IStandardRoom aRoom)
        {
            iDisposeHandler = new DisposeHandler();

            iRoom = aRoom;
            iSources = aRoom.Sources;
            iNetwork = aRoom.Network;
            iSenders = aHouse.Senders;

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
                    });
                    iRoom.UnJoin(SetInactive);
                    iIsActive = false;
                }
            }

            iEnabled.Dispose();

            if (iReceiver != null)
            {
                iSupervisor.Dispose();
                iSupervisor = null;
                iReceiver.Dispose();
                iReceiver = null;
            }
        }

        private void SetInactive()
        {
            lock (iLock)
            {
                if (iIsActive)
                {

                    iActive.Update(false);

                    iSources.RemoveWatcher(this);

                    if (iReceiver != null)
                    {
                        iSupervisor.Dispose();
                        iSupervisor = null;
                        iReceiver.Dispose();
                        iReceiver = null;
                    }

                    iRoom.UnJoin(SetInactive);
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
                    return iSupervisor.Snapshot;
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

            if (iReceiver != null)
            {
                iSupervisor.Dispose();
                iSupervisor = null;
                iReceiver.Dispose();
                iReceiver = null;
            }

            EvaluateEnabled(aValue);
        }

        public void ItemClose(string aId, IEnumerable<ITopology4Source> aValue)
        {
            iEnabled.Update(false);
        }

        private void EvaluateEnabled(IEnumerable<ITopology4Source> aValue)
        {
            foreach (ITopology4Source s in aValue)
            {
                if (s.Type == "Receiver")
                {
                    s.Device.Create<IProxyReceiver>((receiver) =>
                    {
                        Do.Assert(iSupervisor == null);
                        iSupervisor = new MediaSupervisor<IMediaPreset>(iNetwork, new SendersSnapshot(iNetwork, receiver, iSenders.Values));
                        iReceiver = receiver;
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
                iReceiver.SetSender(iSenderMetadata);
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
        private readonly IEnumerable<IProxySender> iSenders;
        private readonly IProxyReceiver iReceiver;
        private readonly INetwork iNetwork;

        public SendersSnapshot(INetwork aNetwork, IProxyReceiver aReceiver, IEnumerable<IProxySender> aSenders)
        {
            iNetwork = aNetwork;
            iReceiver = aReceiver;
            iSenders = aSenders;
        }

        public uint Total
        {
            get
            {
                return ((uint)iSenders.Count());
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
            iSenders.Skip((int)aIndex).Take((int)aCount).ToList().ForEach(v =>
            {
                MediaMetadata metadata = new MediaMetadata();
                metadata.Add(iNetwork.TagManager.Audio.Title, v.Metadata.Value.Name);
                metadata.Add(iNetwork.TagManager.Audio.Artwork, v.Metadata.Value.ArtworkUri);
                metadata.Add(iNetwork.TagManager.Audio.Uri, v.Metadata.Value.Uri);
                presets.Add(new MediaPresetSender(iNetwork, index, index, metadata, v.Metadata.Value, iReceiver));
                ++index;
            });
            return presets;
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
            if (aSource.Name.StartsWith("Hdmi"))
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
}
