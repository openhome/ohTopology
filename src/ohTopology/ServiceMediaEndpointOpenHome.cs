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

        private readonly HttpClient iClient;

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

            iSupervisor = new MediaEndpointSupervisor(this, aLog);

            iClient = new HttpClient(iNetwork);
        }

        public override IProxy OnCreate(IDevice aDevice)
        {
            return (new ProxyMediaEndpoint(this, aDevice));
        }

        public override void CreateSession(Action<IMediaEndpointSession> aCallback)
        {
            iSupervisor.CreateSession(aCallback);
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

        private void Fetch(CancellationToken aCancellationToken, string aUri, Action<string> aJsonParser)
        {
            //Console.WriteLine("REQUEST   : {0}", aUri);

            iClient.Read(aUri, aCancellationToken, (buffer) =>
            {
                if (buffer != null)
                {
                    var value = iEncoding.GetString(buffer);

                    try
                    {
                        aJsonParser(value);
                    }
                    catch
                    {
                    }
                }
            });
        }

        private void GetSnapshot(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aFormat, params object[] aArguments)
        {
            Fetch(aCancellationToken, CreateUri(aFormat, aArguments), (value) =>
            {
                Task.Factory.StartNew(() =>
                {
                    var json = JsonParser.Parse(value) as JsonObject;
                    var total = GetTotal(json["Total"]);
                    var alpha = GetAlpha(json["Alpha"]);
                    var snapshot = new MediaEndpointSnapshotOpenHome(total, alpha);

                    iNetwork.Schedule(() =>
                    {
                        if (!aCancellationToken.IsCancellationRequested)
                        {
                            aCallback(snapshot);
                        }
                    });
                });
            });
        }

        private uint GetTotal(JsonValue aValue)
        {
            var value = aValue as JsonInteger;
            return ((uint)value.Value);
        }

        private IEnumerable<uint> GetAlpha(JsonValue aValue)
        {
            var value = aValue as JsonArray;

            if (value != null)
            {
                return GetAlpha(value);
            }

            return (null);
        }

        private IEnumerable<uint> GetAlpha(JsonArray aValue)
        {
            foreach (var entry in aValue)
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

        public void Create(CancellationToken aCancellationToken, Action<string> aCallback)
        {
            Fetch(aCancellationToken, CreateUri("create"), (value) =>
            {
                Task.Factory.StartNew(() =>
                {
                    var json = JsonParser.Parse(value) as JsonString;

                    var session = json.Value;

                    iNetwork.Schedule(() =>
                    {
                        if (!aCancellationToken.IsCancellationRequested)
                        {
                            iSessionHandler("me." + iId + "." + session, (id, seq) =>
                            {
                                iSupervisor.Refresh(session);
                            });

                            aCallback(session);
                        }
                    });
                });
            });
        }

        public void Destroy(CancellationToken aCancellationToken, Action<string> aCallback, string aId)
        {
            Fetch(aCancellationToken, CreateUri("destroy?session={0}", aId), (value) =>
            {
                aCallback(aId);
            });
        }

        public void Browse(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, IMediaDatum aDatum)
        {
            if (aDatum == null)
            {
                GetSnapshot(aCancellationToken, aCallback, "browse?session={0}&id={1}", aSession, "0");
            }
            else
            {
                GetSnapshot(aCancellationToken, aCallback, "browse?session={0}&id={1}", aSession, aDatum.Id);
            }
        }

        public void List(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, ITag aTag)
        {
            GetSnapshot(aCancellationToken, aCallback, "list?session={0}&tag={1}", aSession, aTag.Id);
        }

        public void Link(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, ITag aTag, string aValue)
        {
            GetSnapshot(aCancellationToken, aCallback, "link?session={0}&tag={1}&val={2}", aSession, aTag.Id, Encode(aValue));
        }

        public void Match(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, ITag aTag, string aValue)
        {
            GetSnapshot(aCancellationToken, aCallback, "match?session={0}&tag={1}&val={2}", aSession, aTag.Id, Encode(aValue));
        }

        public void Search(CancellationToken aCancellationToken, Action<IMediaEndpointClientSnapshot> aCallback, string aSession, string aValue)
        {
            GetSnapshot(aCancellationToken, aCallback, "search?session={0}&val={1}", aSession, Encode(aValue));
        }

        public void Read(CancellationToken aCancellationToken, Action<IWatchableFragment<IMediaDatum>> aCallback, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount)
        {
            Fetch(aCancellationToken, CreateUri("read?session={0}&index={1}&count={2}", aSession, aIndex, aCount), (value) =>
            {
                Task.Factory.StartNew(() =>
                {
                    var json = JsonParser.Parse(value);

                    var fragment = new WatchableFragment<IMediaDatum>(aIndex, GetData(json));

                    iNetwork.Schedule(() =>
                    {
                        if (!aCancellationToken.IsCancellationRequested)
                        {
                            aCallback(fragment);
                        }
                    });
                });
            });
        }

        // IDispose

        public override void Dispose()
        {
            iSupervisor.Cancel();

            base.Dispose();

            iSupervisor.Dispose();

            iClient.Dispose();
        }
    }

    internal class MediaEndpointSnapshotOpenHome : IMediaEndpointClientSnapshot
    {
        private readonly uint iTotal;
        private readonly uint[] iAlpha;

        public MediaEndpointSnapshotOpenHome(uint aTotal, IEnumerable<uint> aAlpha)
        {
            iTotal = aTotal;

            if (aAlpha != null)
            {
                iAlpha = aAlpha.ToArray();
            }
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
