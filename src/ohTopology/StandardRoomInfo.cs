using System;
using System.Threading.Tasks;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IStandardRoomInfo : IDisposable
    {
        string Name { get; }
        IWatchable<bool> Active { get; }
        IWatchable<RoomDetails> Details { get; }
        IWatchable<RoomMetadata> Metadata { get; }
        IWatchable<RoomMetatext> Metatext { get; }
    }

    internal class InfoWatcher : IWatcher<IInfoDetails>, IWatcher<IInfoMetadata>, IWatcher<IInfoMetatext>, IDisposable
    {
        public InfoWatcher(INetwork aNetwork, ITopology4Source aSource, Watchable<RoomDetails> aDetails, Watchable<RoomMetadata> aMetadata, Watchable<RoomMetatext> aMetatext)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aNetwork;
            iSource = aSource;
            iDetails = aDetails;
            iMetadata = aMetadata;
            iMetatext = aMetatext;
            iDisposed = false;

            iSource.Device.Create<IProxyInfo>((info) =>
            {
                if (!iDisposed)
                {
                    iInfo = info;

                    iInfo.Details.AddWatcher(this);
                    iInfo.Metadata.AddWatcher(this);
                    iInfo.Metatext.AddWatcher(this);
                }
                else
                {
                    info.Dispose();
                }
            });
        }

        public void Dispose()
        {
            iDisposeHandler.Dispose();
            
            if (iInfo != null)
            {
                iInfo.Details.RemoveWatcher(this);
                iInfo.Metadata.RemoveWatcher(this);
                iInfo.Metatext.RemoveWatcher(this);

                iInfo.Dispose();
                iInfo = null;
            }

            iDisposed = true;
        }

        public void ItemOpen(string aId, IInfoDetails aValue)
        {
            iDetails.Update(new RoomDetails(aValue));
        }

        public void ItemUpdate(string aId, IInfoDetails aValue, IInfoDetails aPrevious)
        {
            iDetails.Update(new RoomDetails(aValue));
        }

        public void ItemClose(string aId, IInfoDetails aValue)
        {
            iDetails.Update(new RoomDetails());
        }

        private bool IsExternal(ITopology4Source aSource)
        {
            return (aSource.Type == "Analog" || aSource.Type == "Digital" || aSource.Type == "Hdmi");
        }

        public void ItemOpen(string aId, IInfoMetadata aValue)
        {
            if (IsExternal(iSource))
            {
                MediaMetadata metadata = new MediaMetadata();
                foreach (var m in aValue.Metadata)
                {
                    metadata.Add(m.Key, m.Value);
                }
                metadata.Add(iNetwork.TagManager.Audio.Artwork, "external://" + iSource.Name);
                iMetadata.Update(new RoomMetadata(new InfoMetadata(metadata, string.Empty)));
            }
            else
            {
                iMetadata.Update(new RoomMetadata(aValue));
            }
        }

        public void ItemUpdate(string aId, IInfoMetadata aValue, IInfoMetadata aPrevious)
        {
            ItemOpen(aId, aValue);
        }

        public void ItemClose(string aId, IInfoMetadata aValue)
        {
            iMetadata.Update(new RoomMetadata());
        }

        public void ItemOpen(string aId, IInfoMetatext aValue)
        {
            iMetatext.Update(new RoomMetatext(aValue));
        }

        public void ItemUpdate(string aId, IInfoMetatext aValue, IInfoMetatext aPrevious)
        {
            iMetatext.Update(new RoomMetatext(aValue));
        }

        public void ItemClose(string aId, IInfoMetatext aValue)
        {
            iMetatext.Update(new RoomMetatext());
        }

        private readonly DisposeHandler iDisposeHandler;
        private bool iDisposed;
        private IProxyInfo iInfo;

        private readonly INetwork iNetwork;
        private readonly ITopology4Source iSource;
        private readonly Watchable<RoomDetails> iDetails;
        private readonly Watchable<RoomMetadata> iMetadata;
        private readonly Watchable<RoomMetatext> iMetatext;
    }

    internal class StandardRoomInfo : IWatcher<ITopology4Source>, IStandardRoomInfo
    {
        private readonly DisposeHandler iDisposeHandler;
        private readonly INetwork iNetwork;
        private readonly IStandardRoom iRoom;
        private readonly IWatchable<ITopology4Source> iSource;

        private readonly object iLock;
        private bool iIsActive;
        private readonly Watchable<bool> iActive;

        private InfoWatcher iInfoWatcher;
        private readonly Watchable<RoomDetails> iDetails;
        private readonly Watchable<RoomMetadata> iMetadata;
        private readonly Watchable<RoomMetatext> iMetatext;

        public StandardRoomInfo(IStandardRoom aRoom)
        {
            iDisposeHandler = new DisposeHandler();
            iNetwork = aRoom.Network;
            iRoom = aRoom;
            iSource = aRoom.Source;

            iLock = new object();
            iIsActive = true;
            iActive = new Watchable<bool>(iNetwork, "Active", true);

            iDetails = new Watchable<RoomDetails>(iNetwork, "Details", new RoomDetails());
            iMetadata = new Watchable<RoomMetadata>(iNetwork, "Metadata", new RoomMetadata());
            iMetatext = new Watchable<RoomMetatext>(iNetwork, "Metatext", new RoomMetatext());

            iSource.AddWatcher(this);
            iRoom.Join(SetInactive);
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
                        iSource.RemoveWatcher(this);
                    });
                    iRoom.Unjoin(SetInactive);
                    iIsActive = false;
                }
            }

            Do.Assert(iInfoWatcher == null);

            iDetails.Dispose();
            iMetadata.Dispose();
            iMetatext.Dispose();
            iActive.Dispose();
        }

        private void SetInactive()
        {
            lock (iLock)
            {
                if (iIsActive)
                {
                    iIsActive = false;

                    iActive.Update(false);

                    iSource.RemoveWatcher(this);
                }
            }
        }

        public string Name
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iRoom.Name;
                }
            }
        }

        public IWatchable<bool> Active
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iActive;
                }
            }
        }

        public IWatchable<RoomDetails> Details
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iDetails;
                }
            }
        }

        public IWatchable<RoomMetadata> Metadata
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iMetadata;
                }
            }
        }

        public IWatchable<RoomMetatext> Metatext
        {
            get
            {
                using (iDisposeHandler.Lock())
                {
                    return iMetatext;
                }
            }
        }

        public void ItemOpen(string aId, ITopology4Source aValue)
        {
            if (aValue.HasInfo)
            {
                iInfoWatcher = new InfoWatcher(iNetwork, aValue, iDetails, iMetadata, iMetatext);
            }
        }

        public void ItemUpdate(string aId, ITopology4Source aValue, ITopology4Source aPrevious)
        {
            if ((aPrevious.HasInfo && !aValue.HasInfo) || (aPrevious.HasInfo && aValue.HasInfo && aPrevious.Device != aValue.Device))
            {
                iInfoWatcher.Dispose();
                iInfoWatcher = null;
            }

            if ((!aPrevious.HasInfo && aValue.HasInfo) || (aPrevious.HasInfo && aValue.HasInfo && aPrevious.Device != aValue.Device))
            {
                Do.Assert(iInfoWatcher == null);
                iInfoWatcher = new InfoWatcher(iNetwork, aValue, iDetails, iMetadata, iMetatext);
            }
        }

        public void ItemClose(string aId, ITopology4Source aValue)
        {
            if (aValue.HasInfo)
            {
                iInfoWatcher.Dispose();
                iInfoWatcher = null;
            }

            Do.Assert(iInfoWatcher == null);
        }
    }
}
