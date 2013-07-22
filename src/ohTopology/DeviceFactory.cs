using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using OpenHome.Net.ControlPoint;
using OpenHome.Os.App;
using OpenHome.Os;

namespace OpenHome.Av
{
    public static class DeviceFactory
    {
        public static Device Create(INetwork aNetwork, CpDevice aDevice)
        {
            Device device = new Device(aDevice.Udn());
            string value;
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Product", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyProduct>(new ServiceProductNetwork(aNetwork, device, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Info", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyInfo>(new ServiceInfoNetwork(aNetwork, device, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Time", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyTime>(new ServiceTimeNetwork(aNetwork, device, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Sender", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxySender>(new ServiceSenderNetwork(aNetwork, device, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Volume", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyVolume>(new ServiceVolumeNetwork(aNetwork, device, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Playlist", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyPlaylist>(new ServicePlaylistNetwork(aNetwork, device, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Radio", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyRadio>(new ServiceRadioNetwork(aNetwork, device, aDevice));
                }
            }
            if (aDevice.GetAttribute("Upnp.Service.av-openhome-org.Receiver", out value))
            {
                if (uint.Parse(value) == 1)
                {
                    device.Add<IProxyReceiver>(new ServiceReceiverNetwork(aNetwork, device, aDevice));
                }
            }
            return device;
        }

        public static Device CreateDs(INetwork aNetwork, string aUdn)
        {
            return CreateDs(aNetwork, aUdn, "Main Room", "Mock DS", "Info Time Volume Sender");
        }

        public static Device CreateDs(INetwork aNetwork, string aUdn, string aRoom, string aName, string aAttributes)
        {
            Device device = new Device(aUdn);
            // add a factory for each type of watchable service

            // product service
            List<SourceXml.Source> sources = new List<SourceXml.Source>();
            sources.Add(new SourceXml.Source("Playlist", "Playlist", true));
            sources.Add(new SourceXml.Source("Radio", "Radio", true));
            sources.Add(new SourceXml.Source("UPnP AV", "UpnpAv", false));
            sources.Add(new SourceXml.Source("Songcast", "Receiver", true));
            sources.Add(new SourceXml.Source("Net Aux", "NetAux", false));
            SourceXml xml = new SourceXml(sources.ToArray());

            device.Add<IProxyProduct>(new ServiceProductMock(aNetwork, device, aRoom, aName, 0, xml, true, aAttributes,
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DS", "",
                "", "Linn High Fidelity System Component", "", aUdn));

            // volume service
            device.Add<IProxyVolume>(new ServiceVolumeMock(aNetwork, device, aUdn, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80));

            // info service
            device.Add<IProxyInfo>(new ServiceInfoMock(aNetwork, device, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(aNetwork.TagManager.FromDidlLite(string.Empty), string.Empty), new InfoMetatext(aNetwork.TagManager.FromDidlLite(string.Empty))));

            // time service
            device.Add<IProxyTime>(new ServiceTimeMock(aNetwork, device, 0, 0));

            // sender service
            device.Add<IProxySender>(new ServiceSenderMock(aNetwork, device, aAttributes, string.Empty, false, new SenderMetadata("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Main Room:Mock DS</dc:title><res protocolInfo=\"ohz:*:*:u\">ohz://239.255.255.250:51972/" + aUdn + "</res><upnp:albumArtURI>http://10.2.10.27/images/Icon.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"), "Enabled"));

            // receiver service
            device.Add<IProxyReceiver>(new ServiceReceiverMock(aNetwork, device, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty));

            // radio service
            List<IMediaMetadata> presets = new List<IMediaMetadata>();
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Linn Radio (Variety)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"40000\">http://opml.radiotime.com/Tune.ashx?id=s122119&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s122119q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Linn Jazz (Jazz)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"40000\">http://opml.radiotime.com/Tune.ashx?id=s122120&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s122120q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));
            presets.Add(null);
            presets.Add(null);
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Linn Classical (Classical)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"40000\">http://opml.radiotime.com/Tune.ashx?id=s122116&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s122116q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>BBC World Service (World News)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"4000\">http://opml.radiotime.com/Tune.ashx?id=s50646&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s50646q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Sky Radio News (World News)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"4000\">http://opml.radiotime.com/Tune.ashx?id=s81093&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s81093q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));

            device.Add<IProxyRadio>(new ServiceRadioMock(aNetwork, device, 0, presets, InfoMetadata.Empty, string.Empty, "Stopped", 100));

            // playlist service
            List<IMediaMetadata> tracks = new List<IMediaMetadata>();
            device.Add<IProxyPlaylist>(new ServicePlaylistMock(aNetwork, device, 0, tracks, false, false, "Stopped", string.Empty, 1000));

            return device;
        }

        public static Device CreateDsm(INetwork aNetwork, string aUdn)
        {
            return CreateDsm(aNetwork, aUdn, "Main Room", "Mock Dsm", "Info Time Volume Sender");
        }

        public static Device CreateDsm(INetwork aNetwork, string aUdn, string aRoom, string aName, string aAttributes)
        {
            Device device = new Device(aUdn);
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

            device.Add<IProxyProduct>(new ServiceProductMock(aNetwork, device, aRoom, aName, 0, xml, true, aAttributes,
                "", "Linn Products Ltd", "Linn", "http://www.linn.co.uk",
                "", "Linn High Fidelity System Component", "Mock DSM", "",
                "", "Linn High Fidelity System Component", "", aUdn));

            // volume service
            device.Add<IProxyVolume>(new ServiceVolumeMock(aNetwork, device, aUdn, 0, 15, 0, 0, false, 50, 100, 100, 1024, 100, 80));

            // info service
            device.Add<IProxyInfo>(new ServiceInfoMock(aNetwork, device, new InfoDetails(0, 0, string.Empty, 0, false, 0), new InfoMetadata(aNetwork.TagManager.FromDidlLite(string.Empty), string.Empty), new InfoMetatext(aNetwork.TagManager.FromDidlLite(string.Empty))));

            // time service
            device.Add<IProxyTime>(new ServiceTimeMock(aNetwork, device, 0, 0));

            // sender service
            device.Add<IProxySender>(new ServiceSenderMock(aNetwork, device, aAttributes, string.Empty, false, new SenderMetadata("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Main Room:Mock DSM</dc:title><res protocolInfo=\"ohz:*:*:u\">ohz://239.255.255.250:51972/" + aUdn + "</res><upnp:albumArtURI>http://10.2.10.27/images/Icon.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"), "Enabled"));

            // receiver service
            device.Add<IProxyReceiver>(new ServiceReceiverMock(aNetwork, device, string.Empty, "ohz:*:*:*,ohm:*:*:*,ohu:*.*.*", "Stopped", string.Empty));

            // radio service
            List<IMediaMetadata> presets = new List<IMediaMetadata>();
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Linn Radio (Variety)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"40000\">http://opml.radiotime.com/Tune.ashx?id=s122119&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s122119q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Linn Jazz (Jazz)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"40000\">http://opml.radiotime.com/Tune.ashx?id=s122120&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s122120q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));
            presets.Add(null);
            presets.Add(null);
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Linn Classical (Classical)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"40000\">http://opml.radiotime.com/Tune.ashx?id=s122116&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s122116q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>BBC World Service (World News)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"4000\">http://opml.radiotime.com/Tune.ashx?id=s50646&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s50646q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));
            presets.Add(aNetwork.TagManager.FromDidlLite("<DIDL-Lite xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:upnp=\"urn:schemas-upnp-org:metadata-1-0/upnp/\" xmlns=\"urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/\"><item id=\"\" parentID=\"\" restricted=\"True\"><dc:title>Sky Radio News (World News)</dc:title><res protocolInfo=\"*:*:*:*\" bitrate=\"4000\">http://opml.radiotime.com/Tune.ashx?id=s81093&amp;formats=mp3,wma,aac,wmvideo,ogg&amp;partnerId=ah2rjr68&amp;username=linnproducts&amp;c=ebrowse</res><upnp:albumArtURI>http://d1i6vahw24eb07.cloudfront.net/s81093q.png</upnp:albumArtURI><upnp:class>object.item.audioItem</upnp:class></item></DIDL-Lite>"));

            device.Add<IProxyRadio>(new ServiceRadioMock(aNetwork, device, 0, presets, InfoMetadata.Empty, string.Empty, "Stopped", 100));

            // playlist service
            List<IMediaMetadata> tracks = new List<IMediaMetadata>();
            device.Add<IProxyPlaylist>(new ServicePlaylistMock(aNetwork, device, 0, tracks, false, false, "Stopped", string.Empty, 1000));
            
            return device;
        }

        public static Device CreateMediaServer(INetwork aNetwork, string aUdn, string aResourceRoot)
        {
            return (new DeviceMediaServerMock(aNetwork, aUdn, aResourceRoot));
        }
    }
}
