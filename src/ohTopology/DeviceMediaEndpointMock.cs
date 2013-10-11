using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using OpenHome.Http;

using OpenHome.Os;
using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public interface IDeviceMediaEndpointMockUriProvider
    {
        string GetArtworkUri(IMediaMetadata aMetadata);
        string GetAudioUri(IMediaMetadata aMetadata);
    }

    public class DeviceMediaEndpointMock : InjectorDevice, IDeviceMediaEndpointMockUriProvider
    {
        private readonly INetwork iNetwork;
        private readonly IEnumerable<IMediaMetadata> iMetadata;
        private readonly HttpFramework iHttpFramework;
        private readonly List<Tuple<string, string>> iColors;

        public DeviceMediaEndpointMock(INetwork aNetwork, string aUdn, string aResourceRoot, ILog aLog)
            : base(aUdn)
        {
            iNetwork = aNetwork;
            iMetadata = ReadMetadata(aNetwork, aResourceRoot);
            iHttpFramework = new HttpFramework(HandleRequest, 10);

            iColors = new List<Tuple<string, string>>();
            Add("000000", "FFFFFF");
            Add("FFFFFF", "000000");
            Add("00FFFF", "00008B");
            Add("B8860B", "D3D3D3");
            Add("FF4500", "98FB98");
            Add("DA70D6", "A9A9A9");
            Add("BDB76B", "20B2AA");
            Add("FFFFE0", "00008B");
            Add("B0C4DE", "FFA500");
            Add("808000", "FFE4B5");
            Add("FFE4E1", "000080");


            Console.WriteLine("Port: " + iHttpFramework.Port);

            Add<IProxyMediaEndpoint>(new ServiceMediaEndpointMock(aNetwork, this, "mock", "Music",
                "Mock", "Mock", "http://www.openhome.org", "",
                "OpenHome", "OpenHome", "http://www.openhome.org", "",
                "OpenHome", "OpenHome", "http://www.openhome.org", "",
                DateTime.Now,
                new string[] { "Browse", "Link", "Search" },
                iMetadata, this, aLog));
        }

        private void Add(string aBackground, string aForeground)
        {
            iColors.Add(new Tuple<string, string>(aForeground, aBackground));
        }

        private IEnumerable<IMediaMetadata> ReadMetadata(INetwork aNetwork, string aResourceRoot)
        {
            ZipConstants.DefaultCodePage = Encoding.Default.CodePage; // without this linux fails to unzip due to missing codepage 850

            var path = Path.Combine(aResourceRoot, "MockMediaServer.zip");

            using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
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

            var items = from item in xml.Descendants("item")
                        select new
                            {
                                Metadata = item.Descendants("metadatum")
                            };

            var results = new List<IMediaMetadata>();

            foreach (var item in items)
            {
                var metadata = new MediaMetadata();

                var xmetadata = from metadatum in item.Metadata
                                select new
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

        private Task HandleRequest(IDictionary<string, object> aEnvironment)
        {
            var request = new HttpRequest(aEnvironment);

            var query = Lri.ParseQuery(request.Query);
            var lri = new Lri(request.Path, query);
            Console.WriteLine("HandleRequest: " + lri.FullPathAndQuery());
            if (lri.Any())
            {
                var segment = lri.First();

                switch (segment)
                {
                    case "artwork":
                        return HandleRequestArtwork(request, lri.Pop());
                    case "audio":
                        return HandleRequestAudio(request, lri.Pop());
                    default:
                        break;
                }
            }

            return (Task.Factory.StartNew(() =>
            {
                request.SendNotFound();
            }));
        }

        private Task HandleRequestArtwork(IHttpRequest aRequest, ILri aLri)
        {
            return Task.Factory.StartNew(() => { ProcessRequestArtwork(aRequest, aLri); });
        }

        private void ProcessRequestArtwork(IHttpRequest aRequest, ILri aLri)
        {

            Console.WriteLine("ProcessRequestArtwork: " + aLri.FullPathAndQuery());
            if (!aLri.Any())
            {
                aRequest.SendNotFound();
                return;
            }

            var album = aLri.First();

            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Album].Value == album);

            if (!tracks.Any())
            {
                aRequest.SendNotFound();
                return;
            }

            var artist = tracks.First()[iNetwork.TagManager.Audio.AlbumArtist];

            if (artist == null)
            {
                aRequest.SendNotFound();
                return;
            }

            var title = tracks.First()[iNetwork.TagManager.Audio.AlbumTitle];

            if (title == null)
            {
                aRequest.SendNotFound();
                return;
            }

            try
            {
                aRequest.Send(GetAlbumArtworkPng(artist.Value, title.Value), "image/png");
            }
            catch
            {
                aRequest.SendNotFound();
            }
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


            var url = string.Format("http://dummyimage.com/500x500/{0}/{1}.png&text={2}+-+{3}", background, foreground, Encode(aArtist), Encode(aTitle));

            var request = System.Net.HttpWebRequest.Create(url) as System.Net.HttpWebRequest;
            request.Timeout = 1000;
            request.ReadWriteTimeout = 1000;

            using (var response = request.GetResponse())
            {
                using (var s = response.GetResponseStream())
                {
                    int count = 0;
                    byte[] buf = new byte[1024];
                    List<byte> result = new List<byte>();
                    do
                    {
                        count = s.Read(buf, 0, buf.Length);
                        if (count != 0)
                        {
                            result.AddRange(buf.Take(count));
                        }
                    }
                    while (count > 0);
                    return result.ToArray();
                }
            }
        }

        private string Encode(string aString)
        {
            return aString.Replace("+", "0x2B")
                          .Replace("#", "0x23")
                          .Replace("%", "0x25")
                          .Replace("&", "0x26")
                          .Replace(" ", "+");
        }

        private Task HandleRequestAudio(IHttpRequest aRequest, ILri aLri)
        {
            return Task.Factory.StartNew(() => { ProcessRequestAudio(aRequest, aLri); });
        }

        private void ProcessRequestAudio(IHttpRequest aRequest, ILri aLri)
        {
            aRequest.SendNotFound();
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


/*
﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using OpenHome.Http;

using OpenHome.Os;
using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;

namespace OpenHome.Av
{
    public interface IDeviceMediaEndpointMockUriProvider
    {
        string GetArtworkUri(IMediaMetadata aMetadata);
        string GetAudioUri(IMediaMetadata aMetadata);
    }

    public class DeviceMediaEndpointMock : Device, IDeviceMediaEndpointMockUriProvider
    {
        private readonly INetwork iNetwork;
        private readonly IEnumerable<IMediaMetadata> iMetadata;
        private readonly HttpFramework iHttpFramework;
        private readonly List<Tuple<Color, Color>> iColors;

        public DeviceMediaEndpointMock(INetwork aNetwork, string aUdn, string aResourceRoot)
            : base(aUdn)
        {
            iNetwork = aNetwork;
            iMetadata = ReadMetadata(aNetwork, aResourceRoot);
            iHttpFramework = new HttpFramework(HandleRequest, 10);

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


            Console.WriteLine("Port: " + iHttpFramework.Port);

            Add<IProxyMediaEndpoint>(new ServiceMediaEndpointMock(aNetwork, this, "mock", "Music",
                "Mock", "Mock", "http://www.openhome.org", "",
                "OpenHome", "OpenHome", "http://www.openhome.org", "",
                "OpenHome", "OpenHome", "http://www.openhome.org", "",
                DateTime.Now,
                new string[] {"Browse", "Link", "Link:audio.artist", "Link:audio.album", "Link:audio.genre", "Search"},
                iMetadata, this));
        }

        private void Add(Color aBackground, Color aForeground)
        {
            iColors.Add(new Tuple<Color, Color>(aForeground, aBackground));
        }

        private IEnumerable<IMediaMetadata> ReadMetadata(INetwork aNetwork, string aResourceRoot)
        {
            ZipConstants.DefaultCodePage = Encoding.Default.CodePage; // without this linux fails to unzip due to missing codepage 850

            var path = Path.Combine(aResourceRoot, "MockMediaServer.zip");

            using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
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

        private Task HandleRequest(IDictionary<string, object> aEnvironment)
        {
            var request = new HttpRequest(aEnvironment);

            var query = Lri.ParseQuery(request.Query);
            var lri = new Lri(request.Path, query);

            if (lri.Any())
            {
                var segment = lri.First();

                switch (segment)
                {
                    case "artwork":
                        return HandleRequestArtwork(request, lri.Pop());
                    case "audio":
                        return HandleRequestAudio(request, lri.Pop());
                    default:
                        break;
                }
            }

            return (Task.Factory.StartNew(() =>
            {
                request.SendNotFound();
            }));
        }

        private Task HandleRequestArtwork(IHttpRequest aRequest, ILri aLri)
        {
            return Task.Factory.StartNew(() => { ProcessRequestArtwork(aRequest, aLri); });
        }

        private void ProcessRequestArtwork(IHttpRequest aRequest, ILri aLri)
        {
            if (!aLri.Any())
            {
                aRequest.SendNotFound();
                return;
            }

            var album = aLri.First();

            var tracks = iMetadata.Where(m => m[iNetwork.TagManager.Audio.Album].Value == album);

            if (!tracks.Any())
            {
                aRequest.SendNotFound();
                return;
            }

            var artist = tracks.First()[iNetwork.TagManager.Audio.AlbumArtist];

            if (artist == null)
            {
                aRequest.SendNotFound();
                return;
            }

            var title = tracks.First()[iNetwork.TagManager.Audio.AlbumTitle];

            if (title == null)
            {
                aRequest.SendNotFound();
                return;
            }

            aRequest.Send(GetAlbumArtworkPng(artist.Value, title.Value), "image/png");
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

        private Task HandleRequestAudio(IHttpRequest aRequest, ILri aLri)
        {
            return Task.Factory.StartNew(() => { ProcessRequestAudio(aRequest, aLri); });
        }

        private void ProcessRequestAudio(IHttpRequest aRequest, ILri aLri)
        {
            aRequest.SendNotFound();
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
*/
