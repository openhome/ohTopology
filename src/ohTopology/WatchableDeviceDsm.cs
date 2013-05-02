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
            // add a factory for each type of watchable service

            // product service
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

            Add<Product>(new MockWatchableProduct(aThread, aUdn, "Main Room", "Mock DSM", 0, xml, true, "Info Time Volume Sender",
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DSM", "",
                "", "Linn High Fidelity System Component", ""));

            // volume service
            Add<Volume>(new MockWatchableVolume(aThread, aUdn, this, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80));

            // info service
            Add<Info>(new MockWatchableInfo(aThread, aUdn, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(string.Empty, string.Empty), new InfoMetatext(string.Empty)));

            // time service
            Add<Time>(new MockWatchableTime(aThread, aUdn, 0, 0));

            // receiver service
            Add<Receiver>(new MockWatchableReceiver(aThread, aUdn, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty));

            // sender service
            Add<Sender>(new MockWatchableSender(aThread, aUdn, "Info Time", false, string.Empty, string.Empty, "Enabled"));
        }

        public MockWatchableDsm(IWatchableThread aThread, string aUdn, string aRoom, string aName, string aAttributes)
            : base(aThread, aUdn)
        {
            // add a factory for each type of watchable service

            // product service
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

            Add<Product>(new MockWatchableProduct(aThread, aUdn, aRoom, aName, 0, xml, true, aAttributes,
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DSM", "",
                "", "Linn High Fidelity System Component", ""));

            // volume service
            Add<Volume>(new MockWatchableVolume(aThread, aUdn, this, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80));

            // info service
            Add<Info>(new MockWatchableInfo(aThread, aUdn, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(string.Empty, string.Empty), new InfoMetatext(string.Empty)));

            // time service
            Add<Time>(new MockWatchableTime(aThread, aUdn, 0, 0));

            // receiver service
            Add<Receiver>(new MockWatchableReceiver(aThread, aUdn, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty));

            // sender service
            Add<Sender>(new MockWatchableSender(aThread, aUdn, "Info Time", false, string.Empty, string.Empty, "Enabled"));
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
