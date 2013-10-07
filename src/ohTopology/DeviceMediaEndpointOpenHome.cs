using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;

using System.Xml;
using System.Xml.Linq;

using System.Net;

using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

using OpenHome.Os.App;

using OpenHome.Http;
using OpenHome.Http.Owin;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    internal class DeviceMediaEndpointOpenHome : Device
    {
        private int kVersionMin = 1;
        private int kVersionMax = 1;

        private readonly ServiceMediaEndpointOpenHome iService;

        public DeviceMediaEndpointOpenHome(INetwork aNetwork, Uri aUri, string aId, JsonValue aJson, Func<string, Action<string, uint>, IDisposable> aSessionHandler)
            : base(aId)
        {
            var json = aJson as JsonObject;

            if (json != null)
            {
                try
                {
                    var type = json.GetStringValue("Type");
                    var name = json.GetStringValue("Name");
                    var info = json.GetStringValue("Info");
                    var url = json.GetStringValue("Url");
                    var artwork = json.GetStringValue("Artwork");
                    var manufacturerName = json.GetStringValue("ManufacturerName");
                    var manufacturerInfo = json.GetStringValue("ManufacturerInfo");
                    var manufacturerUrl = json.GetStringValue("ManufacturerUrl");
                    var manufacturerArtwork = json.GetStringValue("ManufacturerArtwork");
                    var modelName = json.GetStringValue("ModelName");
                    var modelInfo = json.GetStringValue("ModelInfo");
                    var modelUrl = json.GetStringValue("ModelUrl");
                    var modelArtwork = json.GetStringValue("ModelArtwork");
                    var started = json.GetDateValue("Started");
                    var path = json.GetStringValue("Path");
                    var attributes = json.GetStringArrayValues("Attributes");
                    var version = json.GetIntegerValue("Version");

                    if (version >= kVersionMin && version <= kVersionMax)
                    {
                        url = ResolveUri(aUri, url);
                        artwork = ResolveUri(aUri, artwork);
                        manufacturerUrl = ResolveUri(aUri, manufacturerUrl);
                        manufacturerArtwork = ResolveUri(aUri, manufacturerArtwork);
                        modelUrl = ResolveUri(aUri, modelUrl);
                        modelArtwork = ResolveUri(aUri, modelArtwork);

                        var uri = ResolveUri(aUri, path);



                        iService = new ServiceMediaEndpointOpenHome(aNetwork, this, aId, type,
                                        name, info, url, artwork,
                                        manufacturerName, manufacturerInfo, manufacturerUrl, manufacturerArtwork,
                                        modelName, modelInfo, modelUrl, modelArtwork,
                                        started,
                                        attributes,
                                        uri,
                                        aSessionHandler);

                        Add<IProxyMediaEndpoint>(iService);
                    }
                }
                catch
                {
                }
            }
        }

        private string ResolveUri(Uri aBaseUri, string aUri)
        {
            Uri absoluteUri;
            if (Uri.TryCreate(aUri, UriKind.Absolute, out absoluteUri))
            {
                if (!absoluteUri.IsFile)
                {
                    return aUri;
                }
            }
            absoluteUri = new Uri(aBaseUri, aUri);
            return absoluteUri.ToString();
        }

        // IDisposable

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    public static class JsonExtensions
    {
        public static string GetStringValue(this JsonObject aValue, string aKey)
        {
            var value = aValue[aKey] as JsonString;
            
            if (value != null)
            {
                return (value.Value);
            }

            throw (new ArgumentException());
        }

        public static long GetIntegerValue(this JsonObject aValue, string aKey)
        {
            var value = aValue[aKey] as JsonInteger;

            if (value != null)
            {
                return (value.Value);
            }

            throw (new ArgumentException());
        }

        public static DateTime GetDateValue(this JsonObject aValue, string aKey)
        {
            var value = aValue[aKey] as JsonString;

            if (value != null)
            {
                try
                {
                    return (DateTime.ParseExact(value.Value, "r", System.Globalization.CultureInfo.InvariantCulture));
                }
                catch
                {
                }
            }

            throw (new ArgumentException());
        }

        public static IEnumerable<string> GetStringArrayValues(this JsonObject aValue, string aKey)
        {
            var value = aValue[aKey] as JsonArray;

            if (value == null)
            {
                throw (new ArgumentException());
            }

            foreach (var entry in value)
            {
                var svalue = entry as JsonString;

                if (svalue != null)
                {
                    yield return (svalue.Value);
                }
            }
        }
    }
}

