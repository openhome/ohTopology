using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public class ServiceMediaEndpointContentDirectory : ServiceMediaEndpoint, IMediaEndpointClient
    {
        private const string kNsDidlLite = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        private const string kNsUpnp = "urn:schemas-upnp-org:metadata-1-0/upnp/";
        private const string kNsDc = "http://purl.org/dc/elements/1.1/";
        private const string kNsLinn = "urn:linn-co-uk/DIDL-Lite";

        private readonly CpProxyUpnpOrgContentDirectory1 iProxy;

        private readonly MediaEndpointSupervisor iSupervisor;

        public ServiceMediaEndpointContentDirectory(INetwork aNetwork, IInjectorDevice aDevice, string aId, string aType, string aName, string aInfo,
            string aUrl, string aArtwork, string aManufacturerName, string aManufacturerInfo, string aManufacturerUrl,
            string aManufacturerArtwork, string aModelName, string aModelInfo, string aModelUrl, string aModelArtwork,
            DateTime aStarted, IEnumerable<string> aAttributes, CpProxyUpnpOrgContentDirectory1 aProxy, ILog aLog)
            : base (aNetwork, aDevice, aId, aType, aName, aInfo, aUrl, aArtwork, aManufacturerName, aManufacturerInfo,
            aManufacturerUrl, aManufacturerArtwork, aModelName, aModelInfo, aModelUrl, aModelArtwork, aStarted, aAttributes, aLog)
        {
            iProxy = aProxy;

            iSupervisor = new MediaEndpointSupervisor(this, aLog);
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return (new ProxyMediaEndpoint(this, aDevice));
        }

        public override void CreateSession(Action<IMediaEndpointSession> aCallback)
        {
            iSupervisor.CreateSession(aCallback);
        }

        internal void Refresh()
        {
            Schedule(() =>
            {
                iDisposeHandler.WhenNotDisposed(() =>
                {
                    iSupervisor.Refresh();
                });
            });
        }

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

            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Container.Title);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Container.Artwork);
            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Audio.Artist);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Audio.Artwork);

            return (datum);
        }

        private IMediaDatum ParseContainerMusicAlbum(XElement aElement, string aId)
        {
            var datum = new MediaDatum(aId, iNetwork.TagManager.Audio.AlbumTitle);

            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Container.Title);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Container.Artwork);
            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Audio.AlbumTitle);
            Convert(aElement, "artist", kNsUpnp, datum, iNetwork.TagManager.Audio.AlbumArtist);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Audio.Artwork);

            return (datum);
        }

        private IMediaDatum ParseContainerMusicGenre(XElement aElement, string aId)
        {
            var datum = new MediaDatum(aId, iNetwork.TagManager.Audio.Genre);

            Convert(aElement, "title", kNsDc, datum, iNetwork.TagManager.Container.Title);
            Convert(aElement, "albumArtURI", kNsUpnp, datum, iNetwork.TagManager.Container.Artwork);
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


        // IMediaEndpointClient

        public void Create(CancellationToken aCancellationToken, Action<string> aCallback)
        {
            iNetwork.Schedule(() =>
            {
                aCallback(Guid.NewGuid().ToString());
            });
        }

        public void Destroy(CancellationToken aCancellationToken, Action<string> aCallback, string aId)
        {
            iNetwork.Schedule(() =>
            {
                aCallback(aId);
            });
        }

        public void Browse(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, IMediaDatum aDatum)
        {
            string id = "0";

            if (aDatum != null)
            {
                id = aDatum.Id;
            }

            iProxy.BeginBrowse(id, "BrowseDirectChildren", "", 0, 1, "", (r) =>
            {
                string result;
                uint returned;
                uint total;
                uint update;

                try
                {
                    iProxy.EndBrowse(r, out result, out returned, out total, out update);

                    var snapshot = new MediaEndpointSnapshotContentDirectory(id, total);

                    iNetwork.Schedule(() =>
                    {
                        if (!aCancellationToken.IsCancellationRequested)
                        {
                            aCallback(snapshot);
                        }
                    });
                }
                catch
                {
                }
            });
        }

        public void List(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, ITag aTag)
        {
            throw new InvalidOperationException();
        }

        public void Link(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, ITag aTag, string aValue)
        {
            throw new InvalidOperationException();
        }

        public void Match(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, ITag aTag, string aValue)
        {
            throw new InvalidOperationException();
        }

        public void Search(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, string aValue)
        {
            throw new InvalidOperationException();
        }

        public void Read(CancellationToken aCancellationToken, Action<IWatchableFragment<IMediaDatum>> aCallback, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount)
        {
            var snapshot = aSnapshot as MediaEndpointSnapshotContentDirectory;

            iProxy.BeginBrowse(snapshot.Id, "BrowseDirectChildren", "*", aIndex, aCount, "", (r) =>
            {
                string result;
                uint returned;
                uint total;
                uint update;

                try
                {
                    iProxy.EndBrowse(r, out result, out returned, out total, out update);

                    var fragment = new WatchableFragment<IMediaDatum>(aIndex, Parse(result));

                    iNetwork.Schedule(() =>
                    {
                        if (!aCancellationToken.IsCancellationRequested)
                        {
                            aCallback(fragment);
                        }
                    });
                }
                catch
                {
                }
            });
        }

        // IDispose

        public override void Dispose()
        {
            iSupervisor.Cancel();

            base.Dispose();

            iSupervisor.Dispose();
        }
    }


    internal class MediaEndpointSnapshotContentDirectory : IMediaEndpointClientSnapshot
    {
        private readonly string iId;
        private readonly uint iTotal;
        private readonly IEnumerable<uint> iAlphaMap;

        public MediaEndpointSnapshotContentDirectory(string aId, uint aTotal)
        {
            iId = aId;
            iTotal = aTotal;
            iAlphaMap = null;
        }

        internal string Id
        {
            get
            {
                return (iId);
            }
        }

        // IMediaEndpointClientSnapshot

        public uint Total
        {
            get
            {
                return (iTotal);
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                return (iAlphaMap); 
            }
        }
    }
}
