using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;

namespace TestMediaEndpoint
{
    public class TestMediaEndpointClient : IMediaEndpointClient, IDisposable
    {
        private readonly WatchableThread iWatchableThread;

        public TestMediaEndpointClient()
        {
            iWatchableThread = new WatchableThread(ReportException);
        }

        private void ReportException(Exception aException)
        {
            var aggregate = aException as AggregateException;

            if (aggregate != null)
            {
                Console.WriteLine("WATCHABLE THREAD AGGREGATE EXCEPTION");

                foreach (var exception in aggregate.InnerExceptions)
                {
                    ReportException(exception);
                }
            }
            else
            {
                Console.WriteLine("WATCHABLE THREAD EXCEPTION");
                Console.WriteLine(aException.Message + "\n" + aException.StackTrace);
            }
        }

        // IMediaEndpointClient

        public Task<string> Create(CancellationToken aCancellationToken)
        {
            var tcs = new TaskCompletionSource<string>();
            tcs.SetResult(Guid.NewGuid().ToString());
            Console.WriteLine("Create     : {0}", tcs.Task.Result);
            return (tcs.Task);
        }

        public Task<string> Destroy(CancellationToken aCancellationToken, string aId)
        {
            var tcs = new TaskCompletionSource<string>();
            tcs.SetResult(aId);
            Console.WriteLine("Create     : {0}", aId);
            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> Browse(CancellationToken aCancellationToken, string aSession, IMediaDatum aDatum)
        {
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();

            string id = string.Empty;

            if (aDatum != null)
            {
                id = aDatum.Id;
            }

            Console.WriteLine("Browse     : {0} {1}", aSession, id);

            tcs.SetResult(new TestMediaEndpointClientSnapshot("0", 100, null));
            
            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> List(CancellationToken aCancellationToken, string aSession, ITag aTag)
        {
            Console.WriteLine("List       : {0} {1}", aSession, aTag.FullName);
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();
            tcs.SetResult(new TestMediaEndpointClientSnapshot("0", 100, null));
            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> Link(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue)
        {
            Console.WriteLine("Link       : {0} {1} {2}", aSession, aTag.FullName, aValue);
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();
            tcs.SetResult(new TestMediaEndpointClientSnapshot("0", 100, null));
            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> Match(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue)
        {
            Console.WriteLine("Match      : {0} {1} {2}", aSession, aTag.FullName, aValue);
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();
            tcs.SetResult(new TestMediaEndpointClientSnapshot("0", 100, null));
            return (tcs.Task);
        }

        public Task<IMediaEndpointClientSnapshot> Search(CancellationToken aCancellationToken, string aSession, string aValue)
        {
            Console.WriteLine("Search     : {0} {1}", aSession, aValue);
            var tcs = new TaskCompletionSource<IMediaEndpointClientSnapshot>();
            tcs.SetResult(new TestMediaEndpointClientSnapshot("0", 100, null));
            return (tcs.Task);
        }

        public Task<IEnumerable<IMediaDatum>> Read(CancellationToken aCancellationToken, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount)
        {
            Console.WriteLine("Read       : {0} {1} {2} {3}", aSession, aSnapshot.GetHashCode(), aIndex, aCount);

            var tcs = new TaskCompletionSource<IEnumerable<IMediaDatum>>();

            aCancellationToken.Register(() =>
            {
                tcs.SetCanceled();
            });

            return (tcs.Task);
        }

        // IWatchableThread

        public void Assert()
        {
            iWatchableThread.Assert();
        }

        public void Execute(Action aAction)
        {
            iWatchableThread.Execute(aAction);
        }

        public void Schedule(Action aAction)
        {
            iWatchableThread.Schedule(aAction);
        }

        // IDisposable

        public void Dispose()
        {
            iWatchableThread.Dispose();
        }
    }

    public class TestMediaEndpointClientSnapshot : IMediaEndpointClientSnapshot
    {
        private readonly string iContainer;
        private readonly uint iTotal;
        private readonly IEnumerable<uint> iAlpha;

        public TestMediaEndpointClientSnapshot(string aContainer, uint aTotal, IEnumerable<uint> aAlphaMap)
        {
            iContainer = aContainer;
            iTotal = aTotal;
            iAlpha = aAlphaMap;
        }

        // IMediaEndpointSnapshot Members

        public string Container
        {
            get
            {
                return (iContainer);
            }
        }

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

    class Program
    {
        static int Main(string[] args)
        {
            //TestLinkedTokenSource();
            TestTokenSourceLink();
            TestMediaEndpointSupervisorSesssionHandling();
            TestMediaEndpointSupervisorContainerHandling();
            return (0);
        }

        static void TestTokenSourceLink()
        {
            var a1 = new CancellationTokenSource();
            var a2 = new CancellationTokenSource();
            var alink = new CancellationTokenLink(a1.Token, a2.Token);

            bool throws = false;

            try
            {
                a1.Cancel();
                a1.Dispose();
            }
            catch
            {
                throws = true;
            }

            try
            {
                a2.Cancel();
                a2.Dispose();
            }
            catch
            {
                throws = true;
            }

            Do.Assert(!throws);

            var random = new Random();

            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    var b1 = new CancellationTokenSource();
                    var b2 = new CancellationTokenSource();
                    var blink = new CancellationTokenLink(b1.Token, b2.Token);

                    switch (random.Next(9))
                    {
                        case 0:
                            b1.Cancel();
                            Do.Assert(b1.Token.IsCancellationRequested);
                            Do.Assert(!b2.Token.IsCancellationRequested);
                            Do.Assert(blink.Token.IsCancellationRequested);
                            b1.Dispose();
                            b2.Dispose();
                            break;
                        case 1:
                            b2.Cancel();
                            Do.Assert(!b1.Token.IsCancellationRequested);
                            Do.Assert(b2.Token.IsCancellationRequested);
                            Do.Assert(blink.Token.IsCancellationRequested);
                            b1.Dispose();
                            b2.Dispose();
                            break;
                        case 2:
                            b1.Cancel();
                            b2.Cancel();
                            Do.Assert(b1.Token.IsCancellationRequested);
                            Do.Assert(b2.Token.IsCancellationRequested);
                            Do.Assert(blink.Token.IsCancellationRequested);
                            b1.Dispose();
                            b2.Dispose();
                            break;
                        case 3:
                            b1.Cancel();
                            Do.Assert(b1.Token.IsCancellationRequested);
                            Do.Assert(!b2.Token.IsCancellationRequested);
                            b1.Dispose();
                            b2.Dispose();
                            break;
                        case 4:
                            b2.Cancel();
                            Do.Assert(!b1.Token.IsCancellationRequested);
                            Do.Assert(b2.Token.IsCancellationRequested);
                            b1.Dispose();
                            b2.Dispose();
                            break;
                        case 5:
                            b1.Dispose();
                            b2.Cancel();
                            Do.Assert(b2.Token.IsCancellationRequested);
                            Do.Assert(blink.Token.IsCancellationRequested);
                            b2.Dispose();
                            break;
                        case 7:
                            b2.Dispose();
                            b1.Cancel();
                            Do.Assert(b1.Token.IsCancellationRequested);
                            Do.Assert(blink.Token.IsCancellationRequested);
                            b1.Dispose();
                            break;
                        case 8:
                            b1.Dispose();
                            b2.Dispose();
                            break;
                    }
                }
            }
            catch
            {
                throws = true;
            }

            Do.Assert(!throws);
       }


        static void TestLinkedTokenSource()
        {
            Console.WriteLine("TestLinkedTokenSource");

            var cts1 = new CancellationTokenSource();
            var cts2 = new CancellationTokenSource();
            var ctsl = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token);
            ctsl.Dispose();

            bool throws = false;

            try
            {
                cts1.Cancel();
                cts1.Dispose();
            }
            catch
            {
                throws = true;
            }

            try
            {
                cts2.Cancel();
                cts2.Dispose();
            }
            catch
            {
                throws = true;
            }

            if (throws)
            {
                Do.Assert(false);
            }
        }

        static bool SessionCreateAndDestroy(int aMilliseconds)
        {
            try
            {
                var client = new TestMediaEndpointClient();
                var supervisor = new MediaEndpointSupervisor(client);

                var sessions = new List<Task<IMediaEndpointSession>>();

                var random = new Random();

                client.Execute(() =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        sessions.Add(supervisor.CreateSession());
                    }
                });

                var remaining = new List<Task<IMediaEndpointSession>>();

                foreach (var task in sessions)
                {
                    if (random.Next(2) == 1)
                    {
                        var session = task.Result;

                        client.Execute(() =>
                        {
                            session.Dispose();
                        });
                    }
                    else
                    {
                        remaining.Add(task);
                    }
                }

                Thread.Sleep(aMilliseconds);

                supervisor.Cancel();

                client.Execute(() =>
                {
                    foreach (var task in remaining)
                    {
                        try
                        {
                            task.Result.Dispose();
                        }
                        catch
                        {
                        }
                    }
                });

                Thread.Sleep(aMilliseconds);

                supervisor.Dispose();
                client.Dispose();
            }
            catch
            {
                return (false);
            }

            return (true);
        }

        static void TestMediaEndpointSupervisorSesssionHandling()
        {
            Console.WriteLine("TestMediaEndpointSupervisorSesssionHandling");

            for (int i = 0; i < 100; i += 10)
            {
                Do.Assert(SessionCreateAndDestroy(i));
            }
        }

        static bool ContainerCreateAndDestroy(int aMilliseconds)
        {
            try
            {
                var client = new TestMediaEndpointClient();
                var supervisor = new MediaEndpointSupervisor(client);

                Task<IMediaEndpointSession> sessionTask = null;

                client.Execute(() =>
                {
                    sessionTask = supervisor.CreateSession();
                });

                var session = sessionTask.Result;

                for (int i = 0; i < 10; i++)
                {
                    client.Schedule(() =>
                    {
                        session.Browse(null, () =>
                        {
                            if (session.Snapshot.Total >= 100)
                            {
                                for (int j = 0; j < 20; j++)
                                {
                                    session.Snapshot.Read(0, 100);
                                }
                            }
                        });
                    });

                    Thread.Sleep(aMilliseconds);
                }

                client.Execute();

                supervisor.Cancel();

                client.Execute(() =>
                {
                    session.Dispose();
                });

                supervisor.Dispose();

                client.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                return (false);
            }

            return (true);
        }

        static void TestMediaEndpointSupervisorContainerHandling()
        {
            Console.WriteLine("TestMediaEndpointSupervisorContainerHandling");

            for (int i = 0; i < 100; i += 10)
            {
                Do.Assert(ContainerCreateAndDestroy(i));
            }
        }
    }
}
