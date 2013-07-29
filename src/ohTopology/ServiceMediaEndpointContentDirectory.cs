using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public class ServiceMediaEndpointContentDirectory : ServiceMediaEndpoint
    {
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;

        private readonly List<MediaEndpointSessionContentDirectory> iSessions;
        
        public ServiceMediaEndpointContentDirectory(INetwork aNetwork, IDevice aDevice, string aId, string aType, string aName, string aInfo,
            string aUrl, string aArtwork, string aManufacturerName, string aManufacturerInfo, string aManufacturerUrl,
            string aManufacturerArtwork, string aModelName, string aModelInfo, string aModelUrl, string aModelArtwork,
            DateTime aStarted, IEnumerable<string> aAttributes, CpProxyUpnpOrgContentDirectory1 aUpnpProxy)
            : base (aNetwork, aDevice, aId, aType, aName, aInfo, aUrl, aArtwork, aManufacturerName, aManufacturerInfo,
            aManufacturerUrl, aManufacturerArtwork, aModelName, aModelInfo, aModelUrl, aModelArtwork, aStarted, aAttributes)
        {
            iUpnpProxy = aUpnpProxy;
            iSessions = new List<MediaEndpointSessionContentDirectory>();
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return (new ProxyMediaEndpoint(this));
        }

        public override Task<IMediaEndpointSession> CreateSession()
        {
            return (Task.Factory.StartNew<IMediaEndpointSession>(() =>
            {
                var session = new MediaEndpointSessionContentDirectory(Network, iUpnpProxy, this);

                lock (iSessions)
                {
                    iSessions.Add(session);
                }

                return (session);
            }));
        }

        internal void Refresh()
        {
            lock (iSessions)
            {
                foreach (var session in iSessions)
                {
                    session.Refresh();
                }
            }
        }

        internal void Destroy(MediaEndpointSessionContentDirectory aSession)
        {
            lock (iSessions)
            {
                iSessions.Remove(aSession);
            }
        }

        // IDispose

        public override void Dispose()
        {
            base.Dispose();

            lock (iSessions)
            {
                Do.Assert(iSessions.Count == 0);
            }
        }
    }

    internal class MediaEndpointSessionContentDirectory : IMediaEndpointSession
    {
        private readonly INetwork iNetwork;
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;
        private readonly ServiceMediaEndpointContentDirectory iService;

        private readonly object iLock;
        
        private uint iSequence;

        private MediaEndpointContainerContentDirectory iContainer;

        public MediaEndpointSessionContentDirectory(INetwork aNetwork, CpProxyUpnpOrgContentDirectory1 aUpnpProxy, ServiceMediaEndpointContentDirectory aService)
        {
            iNetwork = aNetwork;
            iUpnpProxy = aUpnpProxy;
            iService = aService;

            iLock = new object();

            iSequence = 0;
        }

        internal void Refresh()
        {
            uint sequence;
            MediaEndpointContainerContentDirectory container;

            lock (iLock)
            {
                sequence = iSequence;
                container = iContainer;
            }

            if (container != null)
            {
                Task.Factory.StartNew(() =>
                {
                    string result;
                    uint numberReturned;
                    uint totalMatches;
                    uint updateId;

                    try
                    {
                        iUpnpProxy.SyncBrowse(container.Id, "BrowseDirectChildren", "", 0, 1, "", out result, out numberReturned, out totalMatches, out updateId);
                    }
                    catch
                    {
                        return;
                    }

                    lock (iLock)
                    {
                        if (iSequence == sequence)
                        {
                            if (container.UpdateId != updateId)
                            {
                                iContainer.Update(updateId, totalMatches);
                            }
                        }
                    }
                });
            }
        }

        private Task<IWatchableContainer<IMediaDatum>> Browse(string aId)
        {
            uint sequence;

            lock (iLock)
            {
                sequence = ++iSequence;

                if (iContainer != null)
                {
                    iContainer.Dispose();
                    iContainer = null;
                }
            }

            return Task.Factory.StartNew<IWatchableContainer<IMediaDatum>>(() =>
            {
                string result;
                uint numberReturned;
                uint totalMatches;
                uint updateId;

                try
                {
                    iUpnpProxy.SyncBrowse(aId, "BrowseDirectChildren", "", 0, 1, "", out result, out numberReturned, out totalMatches, out updateId);
                }
                catch
                {
                    return (null);
                }

                lock (iLock)
                {
                    if (iSequence == sequence)
                    {
                        iContainer = new MediaEndpointContainerContentDirectory(iNetwork, iUpnpProxy, aId, updateId, totalMatches);
                        return (iContainer);
                    }

                    return (null);
                }
            });
        }

        // IMediaServerSession

        public Task<IWatchableContainer<IMediaDatum>> Browse(IMediaDatum aDatum)
        {
            if (aDatum == null)
            {
                return (Browse("0"));
            }

            return (Browse(aDatum.Id));
        }

        public Task<IWatchableContainer<IMediaDatum>> Link(ITag aTag, string aValue)
        {
            throw new NotImplementedException();
        }

        public Task<IWatchableContainer<IMediaDatum>> Search(string aValue)
        {
            throw new NotImplementedException();
        }

        public Task<IWatchableContainer<IMediaDatum>> Query(string aValue)
        {
            throw new NotImplementedException();
        }

        // Disposable

        public void Dispose()
        {
            lock (iLock)
            {
                if (iContainer != null)
                {
                    iContainer.Dispose();
                }
            }

            iService.Destroy(this);
        }
    }

    internal class MediaEndpointContainerContentDirectory : IWatchableContainer<IMediaDatum>, IDisposable
    {
        private readonly INetwork iNetwork;
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;
        private readonly string iId;
        private uint iUpdateId;

        private readonly Watchable<IWatchableSnapshot<IMediaDatum>> iWatchable;

        public MediaEndpointContainerContentDirectory(INetwork aNetwork, CpProxyUpnpOrgContentDirectory1 aUpnpProxy, string aId, uint aUpdateId, uint aTotal)
        {
            iNetwork = aNetwork;
            iUpnpProxy = aUpnpProxy;
            iId = aId;
            iUpdateId = aUpdateId;

            iWatchable = new Watchable<IWatchableSnapshot<IMediaDatum>>(aNetwork, "snapshot", new MediaEndpointSnapshotContentDirectory(iNetwork, iUpnpProxy, iId, aTotal));
        }

        internal string Id
        {
            get
            {
                return (iId);
            }
        }

        internal uint UpdateId
        {
            get
            {
                return (iUpdateId);
            }
        }

        internal void Update(uint aUpdateId, uint aTotal)
        {
            iUpdateId = aUpdateId;

            iNetwork.Schedule(() =>
            {
                iWatchable.Update(new MediaEndpointSnapshotContentDirectory(iNetwork, iUpnpProxy, iId, aTotal));
            });
        }

        // IMediaServerContainer

        public IWatchable<IWatchableSnapshot<IMediaDatum>> Snapshot
        {
            get { return (iWatchable); }
        }

        // IDisposable

        public void Dispose()
        {
            iNetwork.Execute();
            iWatchable.Dispose();
        }
    }


    internal class MediaEndpointSnapshotContentDirectory : IWatchableSnapshot<IMediaDatum>
    {
        private readonly INetwork iNetwork;
        private readonly CpProxyUpnpOrgContentDirectory1 iUpnpProxy;
        private readonly string iId;
        private readonly uint iTotal;

        private readonly IEnumerable<uint> iAlphaMap;

        public MediaEndpointSnapshotContentDirectory(INetwork aNetwork, CpProxyUpnpOrgContentDirectory1 aUpnpProxy, string aId, uint aTotal)
        {
            iNetwork = aNetwork;
            iUpnpProxy = aUpnpProxy;
            iId = aId;
            iTotal = aTotal;

            iAlphaMap = null;
        }

        // IMediaServerSnapshot

        public uint Total
        {
            get { return (iTotal); }
        }

        public IEnumerable<uint> AlphaMap
        {
            get { return (iAlphaMap); }
        }

        public Task<IWatchableFragment<IMediaDatum>> Read(uint aIndex, uint aCount)
        {
            iNetwork.Assert();

            Do.Assert(aIndex + aCount <= iTotal);

            return (Task.Factory.StartNew<IWatchableFragment<IMediaDatum>>(() =>
            {
                string result;
                uint numberReturned;
                uint totalMatches;
                uint updateID;

                try
                {
                    iUpnpProxy.SyncBrowse(iId, "BrowseDirectChildren", "", aIndex, aCount, "", out result, out numberReturned, out totalMatches, out updateID);
                }
                catch
                {
                    return (null);
                }

                return (new MediaEndpointFragmentContentDirectory(iNetwork, aIndex, result));
            }));
        }
    }

    internal class MediaEndpointFragmentContentDirectory : IWatchableFragment<IMediaDatum>
    {
        private const string kNsDidlLite = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        private const string kNsUpnp = "urn:schemas-upnp-org:metadata-1-0/upnp/";
        private const string kNsDc = "http://purl.org/dc/elements/1.1/";
        private const string kNsLinn = "urn:linn-co-uk/DIDL-Lite";

        private readonly INetwork iNetwork;
        private readonly uint iIndex;
        private readonly IEnumerable<IMediaDatum> iData;

        public MediaEndpointFragmentContentDirectory(INetwork aNetwork, uint aIndex, string aDidl)
        {
            iNetwork = aNetwork;
            iIndex = aIndex;
            iData = Parse(aDidl);
        }

        /*
                <DIDL-Lite xmlns="urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:upnp="urn:schemas-upnp-org:metadata-1-0/upnp/" xmlns:dlna="urn:schemas-dlna-org:metadata-1-0/">
         *          <container id="co1" parentID="0" restricted="0"><dc:title>Artist / Album</dc:title><upnp:class>object.container</upnp:class></container>
         *          <container id="co2" parentID="0" restricted="0"><dc:title>Album</dc:title><upnp:class>object.container</upnp:class></container>
         *          <container id="co3" parentID="0" restricted="0"><dc:title>Title</dc:title><upnp:class>object.container</upnp:class></container>
         *          <container id="co4" parentID="0" restricted="0"><dc:title>Composer</dc:title><upnp:class>object.container</upnp:class></container>
         *          <container id="co5" parentID="0" restricted="0"><dc:title>Genre</dc:title><upnp:class>object.container.genre</upnp:class></container>
         *          <container id="co6" parentID="0" restricted="0"><dc:title>Style</dc:title><upnp:class>object.container</upnp:class></container>
         *          <container id="co7" parentID="0" restricted="0"><dc:title>Dynamic Browsing</dc:title><upnp:class>object.container</upnp:class></container>
         *          <container id="co8" parentID="0" restricted="0"><dc:title>Internet Radio [TuneIn Radio]</dc:title><upnp:class>object.container</upnp:class></container>
         *          <container id="co9" parentID="0" restricted="0"><dc:title>Playlists  </dc:title><upnp:class>object.container</upnp:class></container>
         *          <container id="co10" parentID="0" restricted="0"><dc:title>Advanced Search</dc:title><upnp:class>object.container</upnp:class></container>
         *      </DIDL-Lite>
        */

        private IEnumerable<IMediaDatum> Parse(string aDidl)
        {
            var reader = new StringReader(aDidl);

            XDocument xml = null;

            try
            {
                xml = XDocument.Load(reader);
            }
            catch
            {
                yield break;
            }

            foreach (var element in xml.Descendants())
            {
                if (element.Name == XName.Get("item", kNsDidlLite))
                {
                    var datum = ParseItem(element);

                    if (datum != null)
                    {
                        yield return datum;
                    }
                }

                if (element.Name == XName.Get("container", kNsDidlLite))
                {
                    var datum = ParseContainer(element);

                    if (datum != null)
                    {
                        yield return datum;
                    }
                }
            }
        }

        private IMediaDatum ParseItem(XElement aElement)
        {
            var datum = new MediaDatum(null);

            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Audio.Title);
            Convert(aElement, "album", kNsUpnp, datum, iNetwork.TagManager.Audio.AlbumTitle);
            Convert(aElement, "artist", kNsUpnp, datum, iNetwork.TagManager.Audio.Artist);
            Convert(aElement, "res", kNsDidlLite, datum, iNetwork.TagManager.Audio.Uri);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Audio.Artwork);

            return (datum);
        }

        private IMediaDatum ParseContainer(XElement aElement)
        {
            var ids = aElement.Attributes(XName.Get("id"));

            if (ids.Any())
            {
                var id = ids.First().Value;

                var classes = aElement.Descendants(XName.Get("class", kNsUpnp));

                if (classes.Any())
                {
                    var upnpClass = classes.First().Value;

                    switch (upnpClass)
                    {
                        case "object.container.person.musicArtist":
                            return (ParseContainerMusicArtist(aElement, id));
                        case "object.container.album.musicAlbum":
                            return (ParseContainerMusicAlbum(aElement, id));
                        case "object.container.genre.musicGenre":
                            return (ParseContainerMusicGenre(aElement, id));
                        default:
                            return (ParseContainerDefault(aElement, id));
                    }
                }

                return (ParseContainerDefault(aElement, id));
            }

            return (null);
        }

        private IMediaDatum ParseContainerMusicArtist(XElement aElement, string aId)
        {
            var datum = new MediaDatum(aId, iNetwork.TagManager.Audio.Artist);

            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Audio.Artist);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Audio.Artwork);

            return (datum);
        }

        private IMediaDatum ParseContainerMusicAlbum(XElement aElement, string aId)
        {
            var datum = new MediaDatum(aId, iNetwork.TagManager.Audio.AlbumTitle);

            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Audio.AlbumTitle);
            Convert(aElement, "artist", kNsUpnp, datum, iNetwork.TagManager.Audio.AlbumArtist);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Audio.Artwork);

            return (datum);
        }

        private IMediaDatum ParseContainerMusicGenre(XElement aElement, string aId)
        {
            var datum = new MediaDatum(aId, iNetwork.TagManager.Audio.Genre);

            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Audio.Genre);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Audio.Artwork);

            return (datum);
        }

        private IMediaDatum ParseContainerDefault(XElement aElement, string aId)
        {
            var datum = new MediaDatum(aId, iNetwork.TagManager.Container.Title);

            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Container.Title);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Container.Artwork);

            return (datum);
        }

        private void Convert(XElement aElement, string aName, string aNamespace, MediaDatum aDatum, ITag aTag)
        {
            var elements = aElement.Descendants(XName.Get(aName, aNamespace));

            foreach (var element in elements)
            {
                if (element.Value.Length > 0)
                {
                    aDatum.Add(aTag, element.Value);
                }
            }
        }

        // IWatchableFragment<IMediaDatum>

        public uint Index
        {
            get { return (iIndex); }
        }

        public IEnumerable<IMediaDatum> Data
        {
            get { return (iData); }
        }
    }
}
