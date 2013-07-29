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

        public DeviceMediaEndpointOpenHome(INetwork aNetwork, string aBaseUri, string aId, JsonValue aJson)
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
            var started = DateTime.Parse(json["Started"].Value());
            var path = json["Path"].Value();

            url = ResolveUri(aBaseUri, url);
            artwork = ResolveUri(aBaseUri, artwork);
            manufacturerUrl = ResolveUri(aBaseUri, manufacturerUrl);
            manufacturerArtwork = ResolveUri(aBaseUri, manufacturerArtwork);
            modelUrl = ResolveUri(aBaseUri, modelUrl);
            modelArtwork = ResolveUri(aBaseUri, modelArtwork);
            
            var uri = ResolveUri(aBaseUri, path);

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
                            attributes, uri);

            Add<IProxyMediaEndpoint>(iService);
        }

        private string ResolveUri(string aBaseUri, string aUri)
        {
            try
            {
                var uri = new Uri(aUri);
                return (aUri);
            }
            catch
            {
                var baseUri = new Uri(aBaseUri);
                var uri = new Uri(baseUri, aUri);
                return (uri.ToString());
            }
        }

        // IDisposable

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}

