using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using OpenHome;
using OpenHome.Net.Core;
using OpenHome.Av;
using OpenHome.Os;
using OpenHome.Os.App;

namespace StressMediaEndpoint
{
    public abstract class Test : IDisposable
    {
        protected readonly IWatchableThread iWatchableThread;
        protected readonly INetwork iNetwork;
        protected readonly IProxyMediaEndpoint iMediaEndpoint;

        private Task<IMediaEndpointSession> iTask;
        protected IMediaEndpointSession iSession;

        protected Test(IWatchableThread aWatchableThread, INetwork aNetwork, IProxyMediaEndpoint aMediaEndpoint)
        {
            iWatchableThread = aWatchableThread;
            iNetwork = aNetwork;
            iMediaEndpoint = aMediaEndpoint;

            iWatchableThread.Execute(() =>
            {
                iTask = iMediaEndpoint.CreateSession();
            });
        }

        public void Run()
        {
            iSession = iTask.Result;
            DoRun();
        }

        public abstract void DoRun();

        // IDisposable

        public void Dispose()
        {
        }
    }

    public class TestRapidBrowsing : Test
    {
        private int iCount;
        private readonly Random iRandom;
        private readonly WatchableTimer iTimer;
        private readonly EventWaitHandle iDone;

        public TestRapidBrowsing(IWatchableThread aWatchableThread, INetwork aNetwork, IProxyMediaEndpoint aMediaEndpoint)
            : base (aWatchableThread, aNetwork, aMediaEndpoint)
        {
            iCount = 300;
            iRandom = new Random();
            iTimer = new WatchableTimer(iWatchableThread, TimerExpired);
            iDone = new ManualResetEvent(false);
            Console.WriteLine("Test Rapid Browsing");
        }

        public override void DoRun()
        {
            iTimer.FireIn(0);
            iDone.WaitOne();
            Console.WriteLine("Test completed successfully");
        }

        private void TimerExpired()
        {
            iSession.Browse(null, () =>
            {
            });

            if (--iCount > 0)
            {
                iTimer.FireIn((uint)iRandom.Next(5));
            }
            else
            {
                iTimer.Dispose();
                iSession.Dispose();
                iDone.Set();
            }
        }
    }

    public class TestBrowseWholeTree : Test
    {
        private readonly EventWaitHandle iDone;
        private readonly Queue<IMediaDatum> iQueue;

        private readonly int iIndex;

        public TestBrowseWholeTree(IWatchableThread aWatchableThread, INetwork aNetwork, IProxyMediaEndpoint aMediaEndpoint, int aIndex)
            : base(aWatchableThread, aNetwork, aMediaEndpoint)
        {
            iDone = new ManualResetEvent(false);
            iQueue = new Queue<IMediaDatum>();
            iIndex = aIndex;
        }

        public override void DoRun()
        {
            iQueue.Enqueue(null);
            Schedule(null);
            iDone.WaitOne();
            Console.WriteLine("Test completed successfully");
        }

        private void Browse(IMediaDatum aValue)
        {
            var title = aValue[iNetwork.TagManager.Container.Title];

            if (title != null)
            {
                Console.WriteLine("Browse {0}", title.Value);
            }

            Console.WriteLine("Browse {0}", aValue.Id);

            lock (iQueue)
            {
                iQueue.Enqueue(aValue);
            }
        }

        public void Schedule(IMediaDatum aValue)
        {
            iWatchableThread.Schedule(() =>
            {
                iSession.Browse(aValue, () =>
                {
                    iSession.Snapshot.Read(0, iSession.Snapshot.Total, ReadComplete);
                });
            });
        }

        private void SignalComplete()
        {
            iWatchableThread.Schedule(() =>
            {
                iSession.Dispose();
                iDone.Set();
            });
        }

        private void ReadComplete(IWatchableFragment<IMediaDatum> aFragment)
        {
            IMediaDatum container;

            lock (iQueue)
            {
                container = iQueue.Dequeue();
            }

            if (container == null)
            {
                Browse(aFragment.Data.ElementAt(iIndex));
            }
            else
            {
                foreach (var entry in aFragment.Data)
                {
                    if (entry.Type.Count() > 0)
                    {
                        Browse(entry);
                    }
                }
            }

            lock (iQueue)
            {
                if (iQueue.Count > 0)
                {
                    Schedule(iQueue.Peek());
                }
                else
                {
                    SignalComplete();
                }
            }
        }
    }

    public class TestBrowseTree : Test
    {
        private readonly EventWaitHandle iDone;
        private readonly Queue<IMediaDatum> iQueue;

        Action<IMediaEndpointSession, System.Action> iCallback;

        public TestBrowseTree(IWatchableThread aWatchableThread, INetwork aNetwork, IProxyMediaEndpoint aMediaEndpoint, Action<IMediaEndpointSession, System.Action> aCallback)
            : base(aWatchableThread, aNetwork, aMediaEndpoint)
        {
            iDone = new ManualResetEvent(false);
            iQueue = new Queue<IMediaDatum>();
            iCallback = aCallback;
        }

        public override void DoRun()
        {
            Begin();
            iDone.WaitOne();
            Console.WriteLine("Test completed successfully");
        }

        private void Browse(IMediaDatum aValue)
        {
            var title = aValue[iNetwork.TagManager.Container.Title];

            if (title != null)
            {
                Console.WriteLine("Browse {0}", title.Value);
            }

            Console.WriteLine("Browse {0}", aValue.Id);

            lock (iQueue)
            {
                iQueue.Enqueue(aValue);
            }
        }

        public void Begin()
        {
            iQueue.Enqueue(null);

            iWatchableThread.Schedule(() =>
            {
                iCallback(iSession, () =>
                {
                    iSession.Snapshot.Read(0, iSession.Snapshot.Total, ReadComplete);
                });
            });
        }

        public void Schedule(IMediaDatum aValue)
        {
            iWatchableThread.Schedule(() =>
            {
                iSession.Browse(aValue, () =>
                {
                    iSession.Snapshot.Read(0, iSession.Snapshot.Total, ReadComplete);
                });
            });
        }

        private void SignalComplete()
        {
            iWatchableThread.Schedule(() =>
            {
                iSession.Dispose();
                iDone.Set();
            });
        }

        private void ReadComplete(IWatchableFragment<IMediaDatum> aFragment)
        {
            foreach (var entry in aFragment.Data)
            {
                if (entry.Type.Count() > 0)
                {
                    Browse(entry);
                }
                else
                {
                    Console.WriteLine("Media {0}", entry.Id);
                }
            }

            lock (iQueue)
            {
                iQueue.Dequeue();

                if (iQueue.Count > 0)
                {
                    Schedule(iQueue.Peek());
                }
                else
                {
                    SignalComplete();
                }
            }
        }
    }

    public class Tests : IDisposable
    {
        private readonly IWatchableThread iWatchableThread;
        private readonly INetwork iNetwork;
        private readonly IProxyMediaEndpoint iMediaEndpoint;

        public Tests(IWatchableThread aWatchableThread, INetwork aNetwork, IProxyMediaEndpoint aMediaEndpoint)
        {
            iWatchableThread = aWatchableThread;
            iNetwork = aNetwork;
            iMediaEndpoint = aMediaEndpoint;

        }

        public void Run()
        {
            TestRapidBrowsing();
            TestBrowseWholeTree();
            TestSearch();
            TestMatch();
            TestLink();
            //TestList();
        }

        private void TestRapidBrowsing()
        {
            var test = new TestRapidBrowsing(iWatchableThread, iNetwork, iMediaEndpoint);
            test.Run();
        }

        private void TestBrowseWholeTree()
        {
            //var test1 = new TestBrowseWholeTree(iWatchableThread, iNetwork, iMediaEndpoint, 0);
            
            //test1.Run();

            var test2 = new TestBrowseWholeTree(iWatchableThread, iNetwork, iMediaEndpoint, 5);
            
            test2.Run();
        }

        private void TestSearch()
        {
            var test3 = new TestBrowseTree(iWatchableThread, iNetwork, iMediaEndpoint, (session, action) =>
            {
                session.Search("love", action);
            });

            test3.Run();
        }

        private void TestMatch()
        {
            var test1 = new TestBrowseTree(iWatchableThread, iNetwork, iMediaEndpoint, (session, action) =>
            {
                session.Match(iNetwork.TagManager.Audio.AlbumTitle, "green", action);
            });

            test1.Run();
        }

        private void TestLink()
        {
            var test1 = new TestBrowseTree(iWatchableThread, iNetwork, iMediaEndpoint, (session, action) =>
            {
                session.Link(iNetwork.TagManager.Audio.Artist, "Madonna", action);
            });

            test1.Run();
        }

        private void TestList()
        {
            var test1 = new TestBrowseTree(iWatchableThread, iNetwork, iMediaEndpoint, (session, action) =>
            {
                session.List(iNetwork.TagManager.Audio.Artist, action);
            });

            test1.Run();
        }

        // IDisposable

        public void Dispose()
        {
        }
    }
}
