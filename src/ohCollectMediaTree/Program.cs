using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Web;

using System.Net;
using System.Net.Sockets;

using OpenHome.Os.App;
using OpenHome.MediaServer;

using OpenHome.Av;

namespace ohCollectMediaTree
{
    class Program : IDisposable
    {
        static void Main(string[] aArgs)
        {
            if (aArgs.Length == 3)
            {
                try
                {
                    using (var program = new Program(aArgs[0], aArgs[1], aArgs[2]))
                    {
                        //program.Run();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                return;
            }

            Console.WriteLine("ohCollectMediaTree [ip-address] [query-port] [http-port] [folder]");
        }

        private readonly IPEndPoint iQueryEndpoint;
        private readonly IPEndPoint iHttpEndpoint;
        private readonly ITagManager iTagManager;

        public Program(string aAddress, string aQueryPort, string aHttpPort)
        {
            var address = IPAddress.Parse(aAddress);
            iQueryEndpoint = new IPEndPoint(address, int.Parse(aQueryPort));
            iHttpEndpoint = new IPEndPoint(address, int.Parse(aHttpPort));
            iTagManager = new TagManager();
        }

        /*public void Run()
        {
            Console.WriteLine("Collecting ... ");

            var items = new List<IMediaMetadata>();

            using (var client = new TcpClient())
            {
                client.Connect(iQueryEndpoint);

                using (var stream = client.GetStream())
                {
                    Encoding encoding = new UTF8Encoding(false, true);

                    var writer = new StreamWriter(stream, encoding);
                    var reader = new StreamReader(stream, encoding, false);

                    writer.Write("audio\n");
                    writer.Flush();
                    
                    var first = reader.ReadLine();

                    Do.Assert(first == "[");

                    while (true)
                    {
                        var start = reader.ReadLine();

                        if (start == "{")
                        {
                            var metadata = new MediaMetadata();

                            while (true)
                            {
                                var line = reader.ReadLine();

                                if (line == "}")
                                {
                                    break;
                                }

                                var split = line.Split('=');

                                ITag tag = iTagManager.Audio[split[0].Trim()];

                                metadata.Add(tag, split[1].Trim());
                            }

                            items.Add(metadata);

                            continue;
                        }

                        break;
                    }

                    reader.Dispose();
                    writer.Dispose();
                }
            }

            Console.WriteLine("{0} raw items collected", items.Count);

            Console.WriteLine("Checking for artwork ...");

            List<string> artworkAlbums = new List<string>();
            List<string> rejectedAlbums = new List<string>();

            List<IMediaMetadata> artworkItems = new List<IMediaMetadata>();

            foreach (var item in items)
            {
                var album = item[iTagManager.Audio.Album];

                if (album != null)
                {
                    if (!artworkAlbums.Contains(album.Value) && !rejectedAlbums.Contains(album.Value))
                    {
                        var uri = "http://" + iHttpEndpoint + "/artwork/" + album.Value;

                        using (var client = new WebClient())
                        {
                            try
                            {
                                client.DownloadData(uri);
                            }
                            catch
                            {
                                Console.WriteLine("Failed to download artwork for {0}", album.Value);
                                rejectedAlbums.Add(album.Value);
                                continue;
                            }
                        }

                        artworkAlbums.Add(album.Value);
                    }

                    artworkItems.Add(item);
                }
            }

            Console.WriteLine("{0} items in {1} albums successfully collected", artworkItems.Count, artworkAlbums.Count);

            if (artworkAlbums.Count > 1000)
            {
                Console.WriteLine("Randomly selecting 1000 albums");

                var randomAlbums = new List<string>(artworkAlbums.OrderBy(x => Guid.NewGuid()).Take(1000));

                List<IMediaMetadata> finalItems = new List<IMediaMetadata>();

                foreach (var item in artworkItems)
                {
                    if (randomAlbums.Contains(item[iTagManager.Audio.Album].Value))
                    {
                        finalItems.Add(item);
                    }
                }

                using (var file = File.Create("MockMediaServer.xml"))
                {
                    finalItems.Serialise(file);
                }

                Console.WriteLine("{0} items in {1} albums successfully collected", finalItems.Count, randomAlbums.Count);
            }
            else
            {
                Console.WriteLine("Not enough albums for a mock media server");
            }

        }*/

        // IDisposable

        public void Dispose()
        {
        }
    }

    /*public static class MetadataExtensions
    {
        public static void Serialise(this IEnumerable<IMediaMetadata> aItems, Stream aStream)
        {
            var settings = new XmlWriterSettings();
            settings.CloseOutput = false;
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.Encoding = new UTF8Encoding(false, true);
            settings.Indent = true;
            settings.IndentChars = " ";

            using (var writer = XmlWriter.Create(aStream))
            {
                writer.WriteStartElement("items");

                foreach (var entry in aItems)
                {
                    writer.WriteStartElement("item");

                    foreach (var kvp in entry)
                    {
                        writer.WriteStartElement("metadatum");
                        writer.WriteAttributeString("tag", kvp.Key.Name);

                        foreach (var value in kvp.Value.Values)
                        {
                            writer.WriteElementString("value", value);
                        }

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }
    }*/
}
