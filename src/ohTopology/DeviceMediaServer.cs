using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

using System.Xml;
using System.Xml.Linq;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using OpenHome.Os.App;
using OpenHome.MediaServer;
using OpenHome.Http;
using OpenHome.Http.Owin;

using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public interface IMockMediaServerUriProvider
    {
        string GetArtworkUri(IMediaMetadata aMetadata);
        string GetAudioUri(IMediaMetadata aMetadata);
    }

    public class DeviceMediaServerMock : Device, IMockMediaServerUriProvider
    {
        private readonly INetwork iNetwork;
        private readonly IEnumerable<IMediaMetadata> iMetadata;
        private readonly HttpFramework iHttpFramework;
        private readonly List<Tuple<Color, Color>> iColors;

        public DeviceMediaServerMock(string aUdn, INetwork aNetwork, string aAppRoot)
            : base(aUdn)
        {
            iNetwork = aNetwork;
            iMetadata = ReadMetadata(aNetwork, aAppRoot);
            iHttpFramework = new HttpFramework(10);

            iColors = new List<Tuple<Color, Color>>();
            Add(Color.Black, Color.White);
            Add(Color.White, Color.Black);
            Add(Color.Cyan, Color.DarkBlue);
            Add(Color.DarkGoldenrod, Color.LightGray);
            Add(Color.OrangeRed, Color.PaleGreen);
            Add(Color.Orchid, Color.DarkGray);
            Add(Color.DarkKhaki, Color.LightSeaGreen);
            Add(Color.LightYellow, Color.DarkBlue);
            Add(Color.LightSteelBlue, Color.Orange);
            Add(Color.Olive, Color.Moccasin);
            Add(Color.MistyRose, Color.Navy);


            Console.WriteLine(iHttpFramework.Port);

            iHttpFramework.AddHttpHandler("artwork", HandleRequestArtwork);
            iHttpFramework.AddHttpHandler("audio", HandleRequestAudio);
            iHttpFramework.Open();

            Add<IProxyMediaServer>(new ServiceMediaServerMock(aNetwork, new string[] {"Browse", "Query"},
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                iMetadata, this));

            // content directory service
            //MockWatchableContentDirectory contentDirectory = new MockWatchableContentDirectory(aThread, aUdn, 0, "");
            //Add<ContentDirectory>(contentDirectory);
        }

        private void Add(Color aBackground, Color aForeground)
        {
            iColors.Add(new Tuple<Color, Color>(aForeground, aBackground));
        }

        private IEnumerable<IMediaMetadata> ReadMetadata(INetwork aNetwork, string aAppRoot)
        {
            var path = Path.Combine(aAppRoot, "MockMediaServer.zip");

            using (var file = File.Open(path, FileMode.Open))
            {
                var zip = new ZipFile(file);

                var entries = zip.GetEnumerator();

                entries.MoveNext();

                var entry = entries.Current as ZipEntry;

                Do.Assert(entry.Name == "MockMediaServer.xml");

                Stream stream = zip.GetInputStream(entry);

                return (ReadMetadata(aNetwork, stream));
            }
        }

        private IEnumerable<IMediaMetadata> ReadMetadata(INetwork aNetwork, Stream aStream)
        {
            var reader = XmlReader.Create(aStream);

            var xml = XDocument.Load(reader);

            var items = from item in xml.Descendants("item") select new
            {
                Metadata = item.Descendants("metadatum")
            };

            var results = new List<IMediaMetadata>();

            foreach (var item in items)
            {
                var metadata = new MediaMetadata();

                var xmetadata = from metadatum in item.Metadata select new
                {
                    Tag = metadatum.Attribute("tag"),
                    Values = metadatum.Descendants("value")
                };

                foreach (var metadatum in xmetadata)
                {
                    ITag tag = aNetwork.TagManager.Audio[metadatum.Tag.Value];

                    if (tag != null)
                    {
                        foreach (var value in metadatum.Values)
                        {
                            metadata.Add(tag, value.Value);
                        }
                    }
                }

                results.Add(metadata);
            }

            return (results);
        }

        private Task HandleRequestArtwork(IHttpEnvironment aEnvironment)
        {
            return Task.Factory.StartNew(() => { ProcessRequestArtwork(aEnvironment); });
        }

        private void ProcessRequestArtwork(IHttpEnvironment aEnvironment)
        {
            var album = aEnvironment.Pop;

            if (album == null)
            {
                aEnvironment.FlushSendNotFound();
                return;
            }

            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Album].Value == album);

            if (!tracks.Any())
            {
                aEnvironment.FlushSendNotFound();
                return;
            }

            var artist = tracks.First()[iNetwork.TagManager.Audio.AlbumArtist];

            if (artist == null)
            {
                aEnvironment.FlushSendNotFound();
                return;
            }

            var title = tracks.First()[iNetwork.TagManager.Audio.AlbumTitle];

            if (title == null)
            {
                aEnvironment.FlushSendNotFound();
                return;
            }

            aEnvironment.FlushSend(GetAlbumArtworkPng(artist.Value, title.Value), "image/png");
        }

        private byte[] GetAlbumArtworkPng(string aArtist, string aTitle)
        {
            var combined = aArtist + " " + aTitle;

            var hash = combined.GetHashCode();
            var rem = hash % iColors.Count;
            var index = rem < 0 ? -rem : rem;
            var colors = iColors[index];
            var background = colors.Item1;
            var foreground = colors.Item2;

            Bitmap bitmap = new Bitmap(500, 500);

            Graphics graphics = Graphics.FromImage(bitmap);

            using (var brush = new SolidBrush(background))
            {
                graphics.FillRectangle(brush, graphics.ClipBounds);
            }

            using (var font = new Font(FontFamily.GenericSansSerif, 20))
            {
                using (var brush = new SolidBrush(foreground))
                {
                    graphics.DrawString(aArtist, font, brush, 10, 10);
                    graphics.DrawString(aTitle, font, brush, 10, 50);
                }
            }

            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);

                return (stream.ToArray());
            }
        }

        private Task HandleRequestAudio(IHttpEnvironment aEnvironment)
        {
            return Task.Factory.StartNew(() => { ProcessRequestAudio(aEnvironment); });
        }

        private void ProcessRequestAudio(IHttpEnvironment aEnvironment)
        {
            aEnvironment.FlushSendNotFound();
        }

        // IMockMediaServerUriProvider

        public string GetArtworkUri(IMediaMetadata aMetadata)
        {
            var album = aMetadata[iNetwork.TagManager.Audio.Album];

            if (album != null)
            {
                return ("http://localhost:" + iHttpFramework.Port + "/artwork/" + album.Value);
            }

            return (null);
        }

        public string GetAudioUri(IMediaMetadata aMetadata)
        {
            return (null);
        }
    }
}
