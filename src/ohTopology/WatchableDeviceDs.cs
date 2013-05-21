using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;
using OpenHome.Os;

namespace OpenHome.Av
{
    public class MockWatchableDevice : WatchableDevice, IMockable
    {
        public static MockWatchableDevice CreateDs(INetwork aNetwork, string aUdn)
        {
            return CreateDs(aNetwork, aUdn, "Main Room", "Mock DS", "Info Time Volume Sender");
        }

        public static MockWatchableDevice CreateDs(INetwork aNetwork, string aUdn, string aRoom, string aName, string aAttributes)
        {
            MockWatchableDevice device = new MockWatchableDevice(aUdn);
            // add a factory for each type of watchable service

            // product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            SourceXml xml = new SourceXml(sources.ToArray());

            device.Add<ProxyProduct>(new ServiceProductMock(aNetwork, string.Format("ServiceProduct({0})", aUdn), aRoom, aName, 0, xml, true, aAttributes,
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", ""));

            // volume service
            device.Add<ProxyVolume>(new ServiceVolumeMock(aNetwork, aUdn, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80));

            // info service
            device.Add<ProxyInfo>(new ServiceInfoMock(aNetwork, aUdn, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(string.Empty, string.Empty), new InfoMetatext(string.Empty)));

            // time service
            //Add<ServiceTime>(new MockWatchableTime(aThread, aUdn, 0, 0));

            // sender service
            device.Add<ProxySender>(new ServiceSenderMock(aNetwork, aUdn, aAttributes, string.Empty, false, new SenderMetadata("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Main Room:Mock DS</dc:title><res protocolInfo=\"ohz:*:*:u\">ohz://239.255.255.250:51972/" + aUdn + "</res><upnp:albumArtURI>http://10.2.10.27/images/Icon.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"), "Enabled"));

            // receiver service
            device.Add<ProxyReceiver>(new ServiceReceiverMock(aNetwork, aUdn, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty));

            // radio service
            device.Add<ProxyRadio>(new ServiceRadioMock(aNetwork, aUdn, 0, new List<uint>(), new InfoMetadata(), string.Empty, "Stopped", 100));

            // playlist service
            device.Add<ProxyPlaylist>(new ServicePlaylistMock(aNetwork, aUdn, 0, new List<uint>(), false, false, "Stopped", string.Empty, 1000));

            return device;
        }

        public static MockWatchableDevice CreateDsm(INetwork aNetwork, string aUdn)
        {
            return CreateDsm(aNetwork, aUdn, "Main Room", "Mock Dsm", "Info Time Volume Sender");
        }

        public static MockWatchableDevice CreateDsm(INetwork aNetwork, string aUdn, string aRoom, string aName, string aAttributes)
        {
            MockWatchableDevice device = new MockWatchableDevice(aUdn);
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

            device.Add<ProxyProduct>(new ServiceProductMock(aNetwork, string.Format("ServiceProduct({0})", aUdn), aRoom, aName, 0, xml, true, aAttributes,
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DSM", "",
                "", "Linn High Fidelity System Component", ""));

            // volume service
            device.Add<ProxyVolume>(new ServiceVolumeMock(aNetwork, aUdn, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80));

            // info service
            device.Add<ProxyInfo>(new ServiceInfoMock(aNetwork, aUdn, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(string.Empty, string.Empty), new InfoMetatext(string.Empty)));

            // time service
            //Add<ServiceTime>(new MockWatchableTime(aThread, aUdn, 0, 0));

            // sender service
            device.Add<ProxySender>(new ServiceSenderMock(aNetwork, aUdn, aAttributes, string.Empty, false, new SenderMetadata("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Main Room:Mock DSM</dc:title><res protocolInfo=\"ohz:*:*:u\">ohz://239.255.255.250:51972/" + aUdn + "</res><upnp:albumArtURI>http://10.2.10.27/images/Icon.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"), "Enabled"));

            // receiver service
            device.Add<ProxyReceiver>(new ServiceReceiverMock(aNetwork, aUdn, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty));

            // radio service
            device.Add<ProxyRadio>(new ServiceRadioMock(aNetwork, aUdn, 0, new List<uint>(), new InfoMetadata(), string.Empty, "Stopped", 100));

            // playlist service
            device.Add<ProxyPlaylist>(new ServicePlaylistMock(aNetwork, aUdn, 0, new List<uint>(), false, false, "Stopped", string.Empty, 1000));
            
            return device;
        }

        public MockWatchableDevice(string aUdn)
            : base(aUdn)
        {
        }

        public void Execute(IEnumerable<string> aValue)
        {
            string command = aValue.First().ToLowerInvariant();
            if (command == "product")
            {
                ServiceProductMock p = iServices[typeof(ProxyProduct)] as ServiceProductMock;
                p.Execute(aValue.Skip(1));
            }
            else if (command == "info")
            {
                ServiceInfoMock i = iServices[typeof(ProxyInfo)] as ServiceInfoMock;
                i.Execute(aValue.Skip(1));
            }
            /*else if (command == "time")
            {
                ServiceTimeMock t = iServices[typeof(ProxyTime)] as ServiceTimeMock;
                t.Execute(aValue.Skip(1));
            }*/
            else if (command == "sender")
            {
                ServiceSenderMock s = iServices[typeof(ProxySender)] as ServiceSenderMock;
                s.Execute(aValue.Skip(1));
            }
            else if (command == "volume")
            {
                ServiceVolumeMock v = iServices[typeof(ProxyVolume)] as ServiceVolumeMock;
                v.Execute(aValue.Skip(1));
            }
            else if (command == "playlist")
            {
                ServicePlaylistMock p = iServices[typeof(ProxyPlaylist)] as ServicePlaylistMock;
                p.Execute(aValue.Skip(1));
            }
            else if (command == "radio")
            {
                ServiceRadioMock r = iServices[typeof(ProxyRadio)] as ServiceRadioMock;
                r.Execute(aValue.Skip(1));
            }
            else if (command == "receiver")
            {
                ServiceReceiverMock r = iServices[typeof(ProxyReceiver)] as ServiceReceiverMock;
                r.Execute(aValue.Skip(1));
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

}
