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
    public class TestMediaEndpointClient : IMediaEndpointClient
    {
        private readonly WatchableThread iWatchableThread;

        public TestMediaEndpointClient()
        {
            iWatchableThread = new WatchableThread(ReportException);
        }

        private void ReportException(Exception aException)
        {
            Console.WriteLine(aException.Message + "\n" + aException.StackTrace);
        }

        public void Wait()
        {
            iWatchableThread.Wait();
        }

        // IMediaEndpointClient

        public string Create(CancellationToken aCancellationToken)
        {
            var id = Guid.NewGuid().ToString();
            Console.WriteLine("Create     : {0}", id);
            return (id);
        }

        public void Destroy(CancellationToken aCancellationToken, string aId)
        {
            Console.WriteLine("Destroy    : {0}", aId);
        }

        public IMediaEndpointClientSnapshot Browse(CancellationToken aCancellationToken, string aSession, IMediaDatum aDatum)
        {
            string id = string.Empty;

            if (aDatum != null)
            {
                id = aDatum.Id;
            }

            Console.WriteLine("Browse     : {0} {1}", aSession, id);

            return (new TestMediaEndpointClientSnapshot("0", 100, null));
        }

        public IMediaEndpointClientSnapshot List(CancellationToken aCancellationToken, string aSession, ITag aTag)
        {
            Console.WriteLine("List       : {0} {1}", aSession, aTag.FullName);
            return (new TestMediaEndpointClientSnapshot("0", 100, null));
        }

        public IMediaEndpointClientSnapshot Link(CancellationToken aCancellationToken, string aSession, ITag aTag, string aValue)
        {
            Console.WriteLine("Link       : {0} {1} {2}", aSession, aTag.FullName, aValue);
            return (new TestMediaEndpointClientSnapshot("0", 100, null));
        }

        public IMediaEndpointClientSnapshot Search(CancellationToken aCancellationToken, string aSession, string aValue)
        {
            Console.WriteLine("Search     : {0} {1}", aSession, aValue);
            return (new TestMediaEndpointClientSnapshot("0", 100, null));
        }

        public IEnumerable<IMediaDatum> Read(CancellationToken aCancellationToken, string aSession, IMediaEndpointClientSnapshot aSnapshot, uint aIndex, uint aCount)
        {
            Console.WriteLine("Read       : {0} {1} {2} {3}", aSession, aSnapshot.GetHashCode(), aIndex, aCount);
            return (null);
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
            TestMediaEndpointSupervisorSesssionHandling();
            TestMediaEndpointSupervisorContainerHandling();
            return (0);
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

                supervisor.Close();

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
            }
            catch
            {
                return (false);
            }

            return (true);
        }

        static void TestMediaEndpointSupervisorSesssionHandling()
        {
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

                IDisposable watcher = session.Snapshot.CreateWatcher((s) =>
                {
                    if (s.Total >= 100)
                    {
                        for (int j = 0; j < 20; j++)
                        {
                            s.Read(0, 100).ContinueWith((t) =>
                            {
                                try
                                {
                                    t.Wait();
                                }
                                catch
                                {
                                }
                            });
                        }
                    }
                });


                for (int i = 0; i < 10; i++)
                {
                    client.Schedule(() =>
                    {
                        session.Browse(null);
                    });

                    Thread.Sleep(aMilliseconds);
                }

                client.Wait();

                supervisor.Close();

                watcher.Dispose();

                client.Execute(() =>
                {
                    session.Dispose();
                });

                supervisor.Dispose();
            }
            catch
            {
                return (false);
            }

            return (true);
        }

        static void TestMediaEndpointSupervisorContainerHandling()
        {
            for (int i = 0; i < 100; i += 10)
            {
                Do.Assert(ContainerCreateAndDestroy(i));
            }
        }
    }
}
