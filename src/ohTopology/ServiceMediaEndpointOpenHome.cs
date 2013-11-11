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
using System.Web;

using OpenHome.Os.App;

using OpenHome.Net.ControlPoint;
using OpenHome.Net.ControlPoint.Proxies;


namespace OpenHome.Av
{
    public class ServiceMediaEndpointOpenHome : ServiceMediaEndpoint, IMediaEndpointClient
    {
        private readonly string iUri;
        private readonly Action<string, Action<string, uint>> iSessionHandler;

        private readonly Encoding iEncoding;
        private readonly MediaEndpointSupervisor iSupervisor;

        public ServiceMediaEndpointOpenHome(INetwork aNetwork, IInjectorDevice aDevice, string aId, string aType, string aName, string aInfo,
            string aUrl, string aArtwork, string aManufacturerName, string aManufacturerInfo, string aManufacturerUrl,
            string aManufacturerArtwork, string aModelName, string aModelInfo, string aModelUrl, string aModelArtwork,
            DateTime aStarted, IEnumerable<string> aAttributes, string aUri, Action<string, Action<string, uint>> aSessionHandler, ILog aLog)
            : base (aNetwork, aDevice, aId, aType, aName, aInfo, aUrl, aArtwork, aManufacturerName, aManufacturerInfo,
            aManufacturerUrl, aManufacturerArtwork, aModelName, aModelInfo, aModelUrl, aModelArtwork, aStarted, aAttributes, aLog)
        {
            iUri = aUri;
            iSessionHandler = aSessionHandler;

            iEncoding = new UTF8Encoding(false);
            iSupervisor = new MediaEndpointSupervisor(this);
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return (new ProxyMediaEndpoint(this, aDevice));
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

        private Task<T> Fetch<T>(CancellationToken aCancellationToken, string aUri, Func<string, T> aJsonParser)
        {
            var tcs = new TaskCompletionSource<T>();

            //Console.WriteLine("REQUEST   : {0}", aUri);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(aUri);

                    // Use a default WebProxy to avoid proxy authentication errors
                    request.Proxy = new WebProxy();
                    request.KeepAlive = false;
                    request.Timeout = 5000; // milliseconds
                    request.ReadWriteTimeout = 5000;

                    request.BeginGetResponse((result) =>
                    {
                        try
                        {
                            var response = request.EndGetResponse(result);

                            if (aCancellationToken.IsCancellationRequested)
                            {
                                response.Close();
                                tcs.SetCanceled();
                            }
                            else
                            {
                                var count = int.Parse(response.Headers["Content-Length"]);

                                if (count >= 0)
                                {
                                    var buffer = new byte[count];

                                    var stream = response.GetResponseStream();

                                    Fetch(stream, aCancellationToken, buffer, 0, count, () =>
                                    {
                                        try
                                        {
                                            var value = iEncoding.GetString(buffer);
                                            var json = aJsonParser(value);
                                            response.Close();
                                            tcs.SetResult(json);
                                        }
                                        catch
                                        {
                                            response.Close();
                                            tcs.SetCanceled();
                                        }
                                    },
                                    () =>
                                    {
                                        response.Close();
                                        tcs.SetCanceled();
                                    });
                                }
                                else
                                {
                                    response.Close();
                                    tcs.SetCanceled();
                                }
                            }
                        }
                        catch
                        {
                            tcs.SetCanceled();
                        }

                    }, null);
                }
                catch (Exception)
                {
                    tcs.SetCanceled();
                }
            });

            return (tcs.Task);
        }

        private void Fetch(Stream aStream, CancellationToken aCancellationToken, byte[] aBuffer, int aOffset, int aCount, Action aSuccess, Action aFail)
        {
            try
            {
                aStream.BeginRead(aBuffer, aOffset, aCount, (result) =>
                {
                    try
                    {
                        if (aCancellationToken.IsCancellationRequested)
                        {
                            aFail();
                        }
                        else
                        {
                            var count = aStream.EndRead(result);

                            if (count == aCount)
                            {
                                aSuccess();
                            }
                            else if (aCount == 0)
                            {
                                aFail();
                            }
                            else
                            {
                                Fetch(aStream, aCancellationToken, aBuffer, aOffset + count, aCount - count, aSuccess, aFail);
                            }
                        }
                    }
                    catch
                    {
                        aFail();
                    }
                }, null);
            }
            catch
            {
                aFail();
            }
        }

        private Task<IMediaEndpointClientSnapshot> GetSnapshot(CancellationToken aCancellationToken, string aFormat, params object[] aArguments)
        {
            return (Fetch<IMediaEndpointClientSnapshot>(aCancellationToken, CreateUri(aFormat, aArguments), (value) =>
            {
                var json = JsonParser.Parse(value) as JsonObject;
                var total = GetTotal(json["Total"]);
                var alpha = GetAlpha(json["Alpha"]);
                return (new MediaEndpointSnapshotOpenHome(total, alpha));
            }));
        }

        private uint GetTotal(JsonValue aValue)
        {
            var value = aValue as JsonInteger;
            return ((uint)value.Value);
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
            var value = aValue as JsonInteger;
            return ((uint)value.Value);
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

        public Task<string> Create(CancellationToken aCancellationToken)
        {
            return (Fetch<string>(aCancellationToken, CreateUri("create"), (value) =>
            {
                var json = JsonParser.Parse(value) as JsonString;

                var session = json.Value;

                iSessionHandler("me." + iId + "." + session, (id, seq) =>
                {
                    iSupervisor.Refresh(session);
                });

                return (session);
            }));
        }

        public Task<string> Destroy(CancellationToken aCancellationToken, string aId)
        {
            return (Fetch<string>(aCancellationToken, CreateUri("destroy?session={0}", aId), (value) =>
            {
                return (aId);
            }));
        }

        public Task<IMediaEndpointClientSnapshot> Browse(CancellationToken aCancellationToken, string aSession, IMediaDatum aDatum)
        {
            if (aDatum == null)
            {
                return (GetSnapshot(aCancellationToken, "browse?session={0}&id={1}", aSession, "0"));
            }

            return (GetSnapshot(aCancellationToken, "browse?session={0}&id={1}", aSession, aDatum.Id));
        }

        public Task<IMediaEndpointClientSnapshot> List(CancellationToken aCancellationToken, string aSession, ITag aTag)
        {
            return (GetSnapshot(aCancellationToken, "list?session={0}&tag={1}", aSession, aTag.Id));
        }

        public Task<IMediaEndpointClientSnapshot> Link(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue)
        {
            return (GetSnapshot(aCancellationToken, "link?session={0}&tag={1}&val={2}", aSession, aTag.Id, Encode(aValue)));
        }

        public Task<IMediaEndpointClientSnapshot> Match(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue)
        {
            return (GetSnapshot(aCancellationToken, "match?session={0}&tag={1}&val={2}", aSession, aTag.Id, Encode(aValue)));
        }

        public Task<IMediaEndpointClientSnapshot> Search(CancellationToken aCancellationToken, string aSession, string aValue)
        {
            return (GetSnapshot(aCancellationToken, "search?session={0}&val={1}", aSession, Encode(aValue)));
        }

        public Task<IEnumerable<IMediaDatum>> Read(CancellationToken aCancellationToken, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount)
        {
            return (Fetch<IEnumerable<IMediaDatum>>(aCancellationToken, CreateUri("read?session={0}&index={1}&count={2}", aSession, aIndex, aCount), (value) =>
            {
                var json = JsonParser.Parse(value);
                return (GetData(json));
            }));
        }

        // IDispose

        public override void Dispose()
        {
            iSupervisor.Cancel();

            base.Dispose();

            iSupervisor.Dispose();
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
