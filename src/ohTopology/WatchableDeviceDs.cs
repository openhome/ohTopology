using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;
using OpenHome.Os;

namespace OpenHome.Av
{
    public class MockWatchableDs : MockWatchableDevice
    {
        public MockWatchableDs(IWatchableThread aThread, IWatchableThread aSubscribeThread, string aUdn)
            : base(aThread, aSubscribeThread, aUdn)
        {
            // add a factory for each type of watchable service

            // product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            SourceXml xml = new SourceXml(sources.ToArray());

            Add<ServiceProduct>(new MockWatchableProduct(aThread, aUdn, "Main Room", "Mock DS", 0, xml, true, "Info Time Volume Sender",
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", ""));

            // volume service
            Add<ServiceVolume>(new MockWatchableVolume(aThread, aUdn, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80));

            // info service
            Add<ServiceInfo>(new MockWatchableInfo(aThread, aUdn, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(string.Empty, string.Empty), new InfoMetatext(string.Empty)));

            // time service
            Add<ServiceTime>(new MockWatchableTime(aThread, aUdn, 0, 0));

            // receiver service
            Add<ServiceReceiver>(new MockWatchableReceiver(aThread, aUdn, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty));

            // sender service
            Add<ServiceSender>(new MockWatchableSender(aThread, aUdn, "Info Time", false, string.Empty, string.Empty, "Enabled"));

            // radio service
            Add<ServiceRadio>(new MockWatchableRadio(aThread, aUdn, 0, new byte[0], string.Empty, string.Empty, "Stopped", string.Empty));
        }

        public MockWatchableDs(IWatchableThread aThread, IWatchableThread aSubscribeThread, string aUdn, string aRoom, string aName, string aAttributes)
            : base(aThread, aSubscribeThread, aUdn)
        {
            // add a factory for each type of watchable service

            // product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            SourceXml xml = new SourceXml(sources.ToArray());

            Add<ServiceProduct>(new MockWatchableProduct(aThread, aUdn, aRoom, aName, 0, xml, true, aAttributes,
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", ""));

            // volume service
            Add<ServiceVolume>(new MockWatchableVolume(aThread, aUdn, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80));

            // info service
            Add<ServiceInfo>(new MockWatchableInfo(aThread, aUdn, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(string.Empty, string.Empty), new InfoMetatext(string.Empty)));

            // time service
            Add<ServiceTime>(new MockWatchableTime(aThread, aUdn, 0, 0));

            // receiver service
            Add<ServiceReceiver>(new MockWatchableReceiver(aThread, aUdn, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty));

            // sender service
            Add<ServiceSender>(new MockWatchableSender(aThread, aUdn, "Info Time", false, string.Empty, string.Empty, "Enabled"));

            // radio service
            Add<ServiceRadio>(new MockWatchableRadio(aThread, aUdn, 0, new byte[0], string.Empty, string.Empty, "Stopped", string.Empty));
        }

        public override void Execute(IEnumerable<string> aValue)
        {
            base.Execute(aValue);

            string command = aValue.First().ToLowerInvariant();
            if (command == "product")
            {
                Type key = typeof(ServiceProduct);
                foreach (KeyValuePair<Type, IWatchableService> s in iServices)
                {
                    if (s.Key == key)
                    {
                        MockWatchableProduct p = s.Value as MockWatchableProduct;
                        p.Execute(aValue.Skip(1));
                    }
                }
            }
            else if (command == "info")
            {
                Type key = typeof(ServiceInfo);
                foreach (KeyValuePair<Type, IWatchableService> s in iServices)
                {
                    if (s.Key == key)
                    {
                        MockWatchableInfo i = s.Value as MockWatchableInfo;
                        i.Execute(aValue.Skip(1));
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
