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
        private readonly ServiceMediaEndpointOpenHome iService;

        public DeviceMediaEndpointOpenHome(INetwork aNetwork, Uri aUri, string aId, JsonValue aJson, Func<string, Action<string, uint>, IDisposable> aSessionHandler)
            : base(aId)
        {
            var json = aJson as JsonObject;

            var type = json["Type"].Value();
            var name = json["Name"].Value();
            var info = json["Info"].Value();
            var url = json["Url"].Value();
            var artwork = json["Artwork"].Value();
            var manufacturerName = json["ManufacturerName"].Value();
            var manufacturerInfo = json["ManufacturerInfo"].Value();
            var manufacturerUrl = json["ManufacturerUrl"].Value();
            var manufacturerArtwork = json["ManufacturerArtwork"].Value();
            var modelName = json["ModelName"].Value();
            var modelInfo = json["ModelInfo"].Value();
            var modelUrl = json["ModelUrl"].Value();
            var modelArtwork = json["ModelArtwork"].Value();
            var started = DateTime.ParseExact(json["Started"].Value(), "r", System.Globalization.CultureInfo.InvariantCulture);
            var path = json["Path"].Value();

            url = ResolveUri(aUri, url);
            artwork = ResolveUri(aUri, artwork);
            manufacturerUrl = ResolveUri(aUri, manufacturerUrl);
            manufacturerArtwork = ResolveUri(aUri, manufacturerArtwork);
            modelUrl = ResolveUri(aUri, modelUrl);
            modelArtwork = ResolveUri(aUri, modelArtwork);
            
            var uri = ResolveUri(aUri, path);

            var attributes = new List<string>();

            foreach (var attribute in json["Attributes"] as JsonArray)
            {
                attributes.Add(attribute.Value());
            }

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
}

