using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text;

using System.Net;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public class ServiceMediaEndpointOpenHome : ServiceMediaEndpoint, IMediaEndpointClient
    {
        private readonly string iUri;
        private readonly Func<string, Action<string, uint>, IDisposable> iSessionHandler;

        private readonly Encoding iEncoding;
        private readonly MediaEndpointSupervisor iSupervisor;
        private readonly Dictionary<string, IDisposable> iRefreshHandlers;

        public ServiceMediaEndpointOpenHome(INetwork aNetwork, IDevice aDevice, string aId, string aType, string aName, string aInfo,
            string aUrl, string aArtwork, string aManufacturerName, string aManufacturerInfo, string aManufacturerUrl,
            string aManufacturerArtwork, string aModelName, string aModelInfo, string aModelUrl, string aModelArtwork,
            DateTime aStarted, IEnumerable<string> aAttributes, string aUri, Func<string, Action<string, uint>, IDisposable> aSessionHandler)
            : base (aNetwork, aDevice, aId, aType, aName, aInfo, aUrl, aArtwork, aManufacturerName, aManufacturerInfo,
            aManufacturerUrl, aManufacturerArtwork, aModelName, aModelInfo, aModelUrl, aModelArtwork, aStarted, aAttributes)
        {
            iUri = aUri;
            iSessionHandler = aSessionHandler;

            iEncoding = new UTF8Encoding(false);
            iSupervisor = new MediaEndpointSupervisor(this);
            iRefreshHandlers = new Dictionary<string, IDisposable>();
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return (new ProxyMediaEndpoint(this));
        }

        public override Task<IMediaEndpointSession> CreateSession()
        {
            return (iSupervisor.CreateSession());
        }

        internal string CreateUri(string aFormat, params object[] aArguments)
        {
            var relative = string.Format(aFormat, aArguments);
            return (string.Format("{0}/{1}", iUri, relative));
        }

        internal string ResolveUri(string aValue)
        {
            return (iUri + "/res" + aValue);
        }

        private IMediaEndpointClientSnapshot GetSnapshot(string aFormat, params object[] aArguments)
        {
            var uri = CreateUri(aFormat, aArguments);

            using (var client = new WebClient())
            {
                var session = client.DownloadString(uri);

                var json = JsonParser.Parse(session) as JsonObject;

                var total = GetTotal(json["Total"]);
                var alpha = GetAlpha(json["Alpha"]);

                return (new MediaEndpointSnapshotOpenHome(total, alpha));
            }
        }

        private uint GetTotal(JsonValue aValue)
        {
            var value = aValue as JsonString;
            return (uint.Parse(value.Value()));
        }

        private IEnumerable<uint> GetAlpha(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetAlphaElement(entry));
            }
        }

        private uint GetAlphaElement(JsonValue aValue)
        {
            var value = aValue as JsonString;
            return (uint.Parse(value.Value()));
        }

        private IEnumerable<IMediaDatum> GetData(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetDatum(entry));
            }
        }

        private IMediaDatum GetDatum(JsonValue aValue)
        {
            var value = aValue as JsonObject;

            var id = GetValue(value["Id"]);
            var type = GetType(value["Type"]);

            var datum = new MediaDatum(id, type.ToArray());

            foreach (var entry in value["Metadata"] as JsonArray)
            {
                var values = GetMetadatumValues(entry);
                var tagid = uint.Parse(values.First());
                var tag = iNetwork.TagManager[tagid];
                var resolved = Resolve(tag, values.Skip(1));
                datum.Add(tag, new MediaValue(resolved));
            }

            return (datum);
        }

        private IEnumerable<string> Resolve(ITag aTag, IEnumerable<string> aValues)
        {
            if (aTag == iNetwork.TagManager.Audio.Artwork || aTag == iNetwork.TagManager.Container.Artwork || aTag == iNetwork.TagManager.Audio.Uri)
            {
                return (Resolve(aValues));
            }

            return (aValues);
        }

        private IEnumerable<string> Resolve(IEnumerable<string> aValues)
        {
            foreach (var value in aValues)
            {
                yield return (Resolve(value));
            }
        }

        private string Resolve(string aValue)
        {
            Uri absoluteUri;
            if (Uri.TryCreate(aValue, UriKind.Absolute, out absoluteUri))
            {
                if (!absoluteUri.IsFile)
                {
                    return aValue;
                }
            }
            return ResolveUri(aValue);
        }

        private IEnumerable<string> GetMetadatumValues(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetMetadatumValue(entry));
            }
        }

        private string GetMetadatumValue(JsonValue aValue)
        {
            var value = aValue as JsonString;
            return (value.Value);
        }

        private string GetValue(JsonValue aValue)
        {
            var value = aValue as JsonString;
            return (value.Value);
        }

        private IEnumerable<ITag> GetType(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            foreach (var entry in value)
            {
                yield return (GetTag(entry));
            }
        }

        private ITag GetTag(JsonValue aValue)
        {
            var value = aValue as JsonString;

            var id = uint.Parse(value.Value);

            return (iNetwork.TagManager[id]);
        }


        private string Encode(string aValue)
        {
            var bytes = iEncoding.GetBytes(aValue);
            var value = System.Convert.ToBase64String(bytes);
            return (value);
        }

        // IMediaEndpointClient

        public string Create(CancellationToken aCancellationToken)
        {
            using (var client = new WebClient())
            {
                var uri = CreateUri("create");

                var jsession = client.DownloadString(uri);

                var json = JsonParser.Parse(jsession) as JsonString;

                var session = json.Value();

                var refresh = iSessionHandler("me." + iId + "." + session, (id, seq) =>
                {
                    iSupervisor.Refresh(session);
                });

                iRefreshHandlers.Add(session, refresh);

                return (session);
            }
        }

        public void Destroy(CancellationToken aCancellationToken, string aId)
        {
            using (var client = new WebClient())
            {
                IDisposable refresh;

                if (iRefreshHandlers.TryGetValue(aId, out refresh))
                {
                    iRefreshHandlers.Remove(aId);
                    refresh.Dispose();
                }

                var uri = CreateUri("destroy?session={0}", aId);

                try
                {
                    client.DownloadString(uri);
                }
                catch
                {
                }
            }
        }

        public IMediaEndpointClientSnapshot Browse(CancellationToken aCancellationToken, string aSession, IMediaDatum aDatum)
        {
            if (aDatum == null)
            {
                return (GetSnapshot("browse?session={0}&id={1}", aSession, "0"));
            }

            return (GetSnapshot("browse?session={0}&id={1}", aSession, aDatum.Id));
        }

        public IMediaEndpointClientSnapshot List(CancellationToken aCancellationToken, string aSession, ITag aTag)
        {
            return (GetSnapshot("link?session={0}&tag={1}&val={2}", aSession, aTag.Id));
        }

        public IMediaEndpointClientSnapshot Link(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue)
        {
            return (GetSnapshot("link?session={0}&tag={1}&val={2}", aSession, aTag.Id, Encode(aValue)));
        }

        public IMediaEndpointClientSnapshot Search(CancellationToken aCancellationToken, string aSession, string aValue)
        {
            return (GetSnapshot("search?session={0}&val={1}", aSession, Encode(aValue)));
        }

        public Task<IEnumerable<IMediaDatum>> Read(CancellationToken aCancellationToken, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount)
        {
            var tcs = new TaskCompletionSource<IEnumerable<IMediaDatum>>();

            var uri = CreateUri("read?session={0}&index={1}&count={2}", aSession, aIndex, aCount);

            var client = new WebClient();

            client.Encoding = iEncoding;

            var cancellation = aCancellationToken.Register(() => client.CancelAsync());
            
            client.DownloadStringCompleted += (sender, args) =>
            {
                Console.WriteLine("cancellation dispose");
                cancellation.Dispose();
                Console.WriteLine("cancellation disposed");

                client.Dispose();

                if (aCancellationToken.IsCancellationRequested || args.Error != null)
                {
                    tcs.SetCanceled();
                }
                else
                {
                    var json = JsonParser.Parse(args.Result);

                    if (aCancellationToken.IsCancellationRequested)
                    {
                        tcs.SetCanceled();
                    }
                    else
                    {
                        var data = GetData(json);

                        if (aCancellationToken.IsCancellationRequested)
                        {
                            tcs.SetCanceled();
                        }
                        else
                        {
                            tcs.SetResult(data);
                        }
                    }
                }
            };

            client.DownloadStringAsync(new Uri(uri));

            return (tcs.Task);
        }

        // IDispose

        public override void Dispose()
        {
            iSupervisor.Cancel();

            base.Dispose();

            iSupervisor.Dispose();

            foreach (var entry in iRefreshHandlers)
            {
                entry.Value.Dispose();
            }
        }
    }

    internal class MediaEndpointSnapshotOpenHome : IMediaEndpointClientSnapshot
    {
        private readonly uint iTotal;
        private readonly IEnumerable<uint> iAlpha;

        public MediaEndpointSnapshotOpenHome(uint aTotal, IEnumerable<uint> aAlpha)
        {
            iTotal = aTotal;
            iAlpha = aAlpha;
        }

        // IMediaEndpointClientSnapshot

        public uint Total
        {
            get
            {
                return (iTotal);
            }
        }

        public IEnumerable<uint> Alpha
        {
            get
            {
                return (iAlpha);
            }
        }
    }
}
