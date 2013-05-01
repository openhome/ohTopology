using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;

namespace OpenHome.Av
{
    public class MockWatchableDsm : MockWatchableDevice
    {
        public MockWatchableDsm(IWatchableThread aThread, string aUdn)
            : base(aThread, aUdn)
        {
            // add a mock product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            sources.Add(new SourceXml.Source("Analog1", "Analog", true));
            sources.Add(new SourceXml.Source("Analog2", "Analog", true));
            sources.Add(new SourceXml.Source("Phono", "Analog", true));
            sources.Add(new SourceXml.Source("SPDIF1", "Digital", true));
            sources.Add(new SourceXml.Source("SPDIF2", "Digital", true));
            sources.Add(new SourceXml.Source("TOSLINK1", "Digital", true));
            SourceXml xml = new SourceXml(sources.ToArray());

            // product service
            MockWatchableProduct product = new MockWatchableProduct(aThread, aUdn, this, "Main Room", "Mock DS", 0, xml, true, "Info Time Volume Sender",
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", "");
            Add<Product>(product);

            // volume service
            MockWatchableVolume volume = new MockWatchableVolume(aThread, aUdn, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80);
            Add<Volume>(volume);

            // info service
            MockWatchableInfo info = new MockWatchableInfo(aThread, aUdn, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(string.Empty, string.Empty), new InfoMetatext(string.Empty));
            Add<Info>(info);

            // time service
            MockWatchableTime time = new MockWatchableTime(aThread, aUdn, 0, 0);
            Add<Time>(time);

            // receiver service
            MockWatchableReceiver receiver = new MockWatchableReceiver(aThread, aUdn, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty);
            Add<Receiver>(receiver);

            // sender service
            MockWatchableSender sender = new MockWatchableSender(aThread, aUdn, "Info Time", false, string.Empty, string.Empty, "Enabled");
            Add<Sender>(sender);
        }

        public MockWatchableDsm(IWatchableThread aThread, string aUdn, string aRoom, string aName, string aAttributes)
            : base(aThread, aUdn)
        {
            // add a mock product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            sources.Add(new SourceXml.Source("Analog1", "Analog", true));
            sources.Add(new SourceXml.Source("Analog2", "Analog", true));
            sources.Add(new SourceXml.Source("Phono", "Analog", true));
            sources.Add(new SourceXml.Source("SPDIF1", "Digital", true));
            sources.Add(new SourceXml.Source("SPDIF2", "Digital", true));
            sources.Add(new SourceXml.Source("TOSLINK1", "Digital", true));
            sources.Add(new SourceXml.Source("TOSLINK2", "Digital", true));
            SourceXml xml = new SourceXml(sources.ToArray());

            // product service
            MockWatchableProduct product = new MockWatchableProduct(aThread, aUdn, this, aRoom, aName, 0, xml, true, aAttributes,
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DSM", "",
                "", "Linn High Fidelity System Component", "");
            Add<Product>(product);

            // volume service
            MockWatchableVolume volume = new MockWatchableVolume(aThread, aUdn, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80);
            Add<Volume>(volume);

            // info service
            MockWatchableInfo info = new MockWatchableInfo(aThread, aUdn, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(string.Empty, string.Empty), new InfoMetatext(string.Empty));
            Add<Info>(info);

            // time service
            MockWatchableTime time = new MockWatchableTime(aThread, aUdn, 0, 0);
            Add<Time>(time);

            // receiver service
            MockWatchableReceiver receiver = new MockWatchableReceiver(aThread, aUdn, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty);
            Add<Receiver>(receiver);

            // sender service
            MockWatchableSender sender = new MockWatchableSender(aThread, aUdn, string.Empty, false, string.Empty, string.Empty, "Enabled");
            Add<Sender>(sender);
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            base.Execute(aValue);

            Type key = typeof(Product);
            string command = aValue.First().ToLowerInvariant();
            if (command == "product")
            {
                foreach (KeyValuePair<Type, IWatchableService> s in iServices)
                {
                    if (s.Key == key)
                    {
                        MockWatchableProduct p = s.Value as MockWatchableProduct;
                        p.Execute(aValue.Skip(1));
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
