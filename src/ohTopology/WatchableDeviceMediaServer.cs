using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;
using OpenHome.MediaServer;

namespace OpenHome.Av
{
    public class MockWatchableMediaServer : WatchableDeviceMock
    {
        private readonly ITagManager iTagManager;
        private readonly List<IMediaMetadata> iMetadata;

        public MockWatchableMediaServer(IWatchableThread aThread, IWatchableThread aSubscribeThread, string aUdn, string aAppRoot)
            : base(aSubscribeThread, aThread, aUdn)
        {
            Add<IServiceMediaServer>(new ServiceFactoryMediaServerMock(aThread, new string[] {"browse", "query"},
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org",
                "", "OpenHome", "OpenHome", "http://www.openhome.org"));

            iTagManager = new TagManager();
            iMetadata = ReadMetadata(aAppRoot);

            // content directory service
            //MockWatchableContentDirectory contentDirectory = new MockWatchableContentDirectory(aThread, aUdn, 0, "");
            //Add<ContentDirectory>(contentDirectory);
        }

        private List<IMediaMetadata> ReadMetadata(string aAppRoot)
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

                return (ReadMetadata(stream));
            }
        }

        private List<IMediaMetadata> ReadMetadata(Stream aStream)
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
                    ITag tag = iTagManager.Audio[metadatum.Tag.Value];

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



        // IMockable

        public override void Execute(IEnumerable<string> aValue)
        {
        }
    }
}
