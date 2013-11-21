using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Threading;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IMediaPreset : IDisposable
    {
        uint Index { get; }
        IMediaMetadata Metadata { get; }
        IWatchable<bool> Buffering { get; }
        IWatchable<bool> Playing { get; }
        void Play();
    }

    public interface IWatchableFragment<T>
    {
        uint Index { get; }
        IEnumerable<T> Data { get; }
    }

    public interface IWatchableSnapshot<T>
    {
        uint Total { get; }
        IEnumerable<uint> Alpha { get; } // null if no alpha map
        void Read(uint aIndex, uint aCount, CancellationToken aCancellationToken, Action<IWatchableFragment<T>> aCallback);
    }

    public interface IWatchableContainer<T>
    {
        IWatchable<IWatchableSnapshot<T>> Snapshot { get; }
    }

    // all calls into an instance of this class should be called on the watchable thread
    public class MediaSupervisor<T> : IDisposable
    {
        private CancellationTokenSource iCancellationTokenSource;
        private Watchable<IWatchableSnapshot<T>> iSnapshot;

        public MediaSupervisor(IWatchableThread aThread, IMediaClientSnapshot<T> aClientSnapshot)
        {
            iCancellationTokenSource = new CancellationTokenSource();
            iSnapshot = new Watchable<IWatchableSnapshot<T>>(aThread, "Snapshot", new MediaSnapshot<T>(iCancellationTokenSource.Token, aClientSnapshot));
        }

        public void Dispose()
        {
            iCancellationTokenSource.Cancel();

            MediaSnapshot<T> snapshot = iSnapshot.Value as MediaSnapshot<T>;

            iSnapshot.Dispose();

            snapshot.Dispose();

            iCancellationTokenSource.Dispose();
        }

        public IWatchable<IWatchableSnapshot<T>> Snapshot
        {
            get
            {
                return iSnapshot;
            }
        }

        public void Update(IMediaClientSnapshot<T> aClientSnapshot)
        {
            MediaSnapshot<T> snapshot = iSnapshot.Value as MediaSnapshot<T>;

            iCancellationTokenSource.Cancel();
                
            iCancellationTokenSource.Dispose();

            iCancellationTokenSource = new CancellationTokenSource();
                
            iSnapshot.Update(new MediaSnapshot<T>(iCancellationTokenSource.Token, aClientSnapshot));

            snapshot.Dispose();
        }
    }

    public interface IMediaClientSnapshot<T>
    {
        uint Total { get; }
        IEnumerable<uint> Alpha { get; } // null if no alpha map
        void Read(CancellationToken aToken, uint aIndex, uint aCount, Action<IEnumerable<T>> aCallback);
    }

    internal class MediaSnapshot<T> : IWatchableSnapshot<T>, IDisposable
    {
        private readonly CancellationToken iCancellationToken;
        private readonly IMediaClientSnapshot<T> iSnapshot;

        private bool iDisposed;

        public MediaSnapshot(CancellationToken aCancellationToken, IMediaClientSnapshot<T> aSnapshot)
        {
            iCancellationToken = aCancellationToken;
            iSnapshot = aSnapshot;

            iDisposed = false;
        }

        // IWatchableSnapshot<IMediaPreset>

        public uint Total
        {
            get
            {
                return (iSnapshot.Total);
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                return (iSnapshot.Alpha);
            }
        }

        public void Read(uint aIndex, uint aCount, CancellationToken aCancellationToken, Action<IWatchableFragment<T>> aCallback)
        {
            CancellationTokenLink ct = new CancellationTokenLink(iCancellationToken, aCancellationToken);

            iSnapshot.Read(ct.Token, aIndex, aCount, (values) =>
            {
                if (!iDisposed)
                {
                    aCallback(new WatchableFragment<T>(aIndex, values));
                }
                else
                {
                    Do.Assert(false);
                }
            });
        }

        // IDisposable

        public void Dispose()
        {
            iDisposed = true;
        }
    }

    public class WatchableFragment<T> : IWatchableFragment<T>
    {
        private readonly uint iIndex;
        private readonly IEnumerable<T> iData;

        public WatchableFragment(uint aIndex, IEnumerable<T> aData)
        {
            iIndex = aIndex;
            iData = aData;
        }

        // IWatchableFragment<T>

        public uint Index
        {
            get { return (iIndex); }
        }

        public IEnumerable<T> Data
        {
            get { return (iData); }
        }
    }

    public static class MediaExtensions
    {
        private static readonly string kNsDidl = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        private static readonly string kNsDc = "http://purl.org/dc/elements/1.1/";
        private static readonly string kNsUpnp = "urn:schemas-upnp-org:metadata-1-0/upnp/";

        public static string ToDidlLite(this ITagManager aTagManager, IMediaMetadata aMetadata)
        {
            if (aMetadata == null)
            {
                return string.Empty;
            }
            if (aMetadata[aTagManager.System.Folder] != null)
            {
                return aMetadata[aTagManager.System.Folder].Value;
            }

            XmlDocument document = new XmlDocument();

            XmlElement didl = document.CreateElement("DIDL-Lite", kNsDidl);

            XmlElement container = document.CreateElement("item", kNsDidl);

            XmlElement title = document.CreateElement("dc", "title", kNsDc);
            title.AppendChild(document.CreateTextNode(aMetadata[aTagManager.Audio.Title].Value));

            container.AppendChild(title);

            XmlElement cls = document.CreateElement("upnp", "class", kNsUpnp);
            cls.AppendChild(document.CreateTextNode("object.item.audioItem.musicTrack"));

            container.AppendChild(cls);

            if (aMetadata[aTagManager.Audio.Artwork] != null)
            {
                foreach (var a in aMetadata[aTagManager.Audio.Artwork].Values)
                {
                    XmlElement artwork = document.CreateElement("upnp", "albumArtURI", kNsUpnp);
                    artwork.AppendChild(document.CreateTextNode(a));
                    container.AppendChild(artwork);
                }
            }

            if (aMetadata[aTagManager.Audio.AlbumTitle] != null)
            {
                XmlElement albumtitle = document.CreateElement("upnp", "album", kNsUpnp);
                albumtitle.AppendChild(document.CreateTextNode(aMetadata[aTagManager.Audio.AlbumTitle].Value));
                container.AppendChild(albumtitle);
            }

            if (aMetadata[aTagManager.Audio.Artist] != null)
            {
                foreach (var a in aMetadata[aTagManager.Audio.Artist].Values)
                {
                    XmlElement artist = document.CreateElement("upnp", "artist", kNsUpnp);
                    artist.AppendChild(document.CreateTextNode(a));
                    container.AppendChild(artist);
                }
            }

            if (aMetadata[aTagManager.Audio.AlbumArtist] != null)
            {
                XmlElement albumartist = document.CreateElement("upnp", "artist", kNsUpnp);
                albumartist.AppendChild(document.CreateTextNode(aMetadata[aTagManager.Audio.AlbumArtist].Value));
                XmlAttribute role = document.CreateAttribute("upnp", "role", kNsUpnp);
                role.AppendChild(document.CreateTextNode("albumartist"));
                albumartist.Attributes.Append(role);
                container.AppendChild(albumartist);
            }

            didl.AppendChild(container);

            document.AppendChild(didl);

            return document.OuterXml;
        }

        public static IMediaMetadata FromDidlLite(this ITagManager aTagManager, string aMetadata)
        {
            MediaMetadata metadata = new MediaMetadata();

            if (!string.IsNullOrEmpty(aMetadata))
            {
                XmlDocument document = new XmlDocument();
                XmlNamespaceManager nsManager = new XmlNamespaceManager(document.NameTable);
                nsManager.AddNamespace("didl", kNsDidl);
                nsManager.AddNamespace("upnp", kNsUpnp);
                nsManager.AddNamespace("dc", kNsDc);
                nsManager.AddNamespace("ldl", "urn:linn-co-uk/DIDL-Lite");

                try
                {
                    document.LoadXml(aMetadata);

                    //string c = document.SelectSingleNode("/didl:DIDL-Lite/*/upnp:class", nsManager).FirstChild.Value;
                    XmlNode title = document.SelectSingleNode("/didl:DIDL-Lite/*/dc:title", nsManager);
                    if(title != null)
                    {
                        if (title.FirstChild != null)
                        {
                            metadata.Add(aTagManager.Audio.Title, title.FirstChild.Value);
                        }
                    }

                    XmlNode res = document.SelectSingleNode("/didl:DIDL-Lite/*/didl:res", nsManager);
                    if (res != null)
                    {
                        if (res.FirstChild != null)
                        {
                            metadata.Add(aTagManager.Audio.Uri, res.FirstChild.Value);
                        }
                    }

                    XmlNodeList albumart = document.SelectNodes("/didl:DIDL-Lite/*/upnp:albumArtURI", nsManager);
                    foreach (XmlNode n in albumart)
                    {
                        if (n.FirstChild != null)
                        {
                            metadata.Add(aTagManager.Audio.Artwork, n.FirstChild.Value);
                        }
                    }

                    XmlNode album = document.SelectSingleNode("/didl:DIDL-Lite/*/upnp:album", nsManager);
                    if (album != null)
                    {
                        if (album.FirstChild != null)
                        {
                            metadata.Add(aTagManager.Audio.Album, album.FirstChild.Value);
                        }
                    }

                    XmlNode artist = document.SelectSingleNode("/didl:DIDL-Lite/*/upnp:artist", nsManager);
                    if (artist != null)
                    {
                        if (artist.FirstChild != null)
                        {
                            metadata.Add(aTagManager.Audio.Artist, artist.FirstChild.Value);
                        }
                    }

                    XmlNodeList genre = document.SelectNodes("/didl:DIDL-Lite/*/upnp:genre", nsManager);
                    foreach (XmlNode n in genre)
                    {
                        if (n.FirstChild != null)
                        {
                            metadata.Add(aTagManager.Audio.Genre, n.FirstChild.Value);
                        }
                    }

                    XmlNode albumartist = document.SelectSingleNode("/didl:DIDL-Lite/*/upnp:artist[@role='AlbumArtist']", nsManager);
                    if (albumartist != null)
                    {
                        if (albumartist.FirstChild != null)
                        {
                            metadata.Add(aTagManager.Audio.AlbumArtist, albumartist.FirstChild.Value);
                        }
                    }
                }
                catch (XmlException) { }
            }
            
            metadata.Add(aTagManager.System.Folder, aMetadata);
            
            return metadata;
        }

        public static void Read<T>(this IWatchableSnapshot<T> aSnapshot, uint aIndex, uint aCount, Action<IWatchableFragment<T>> aCallback)
        {
            aSnapshot.Read(aIndex, aCount, CancellationToken.None, aCallback);
        }
    }
}
