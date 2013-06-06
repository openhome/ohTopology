using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;

using OpenHome.Os.App;

namespace OpenHome.Av
{
    public interface IMockable
    {
        void Execute(IEnumerable<string> aValue);
    }

    public class Mockable : IMockable
    {
        public Mockable()
        {
            iMockables = new Dictionary<string, IMockable>();
        }

        public void Add(string aId, IMockable aMockable)
        {
            iMockables.Add(aId, aMockable);
        }

        public void Remove(string aId)
        {
            iMockables.Remove(aId);
        }
    
        public void Execute(IEnumerable<string> aValue)
        {
            iMockables[aValue.First()].Execute(aValue.Skip(1));
        }

        private Dictionary<string, IMockable> iMockables;
    }

    public class MockableStream
    {
        public MockableStream(TextReader aTextReader, IMockable aMockable)
        {
            iTextReader = aTextReader;
            iMockable = aMockable;
        }

        public void Start()
        {
            while (true)
            {
                string line = iTextReader.ReadLine();

                if (line == null)
                {
                    break;
                }

                var commands = Tokeniser.Parse(line);

                if (commands.Any())
                {
                    iMockable.Execute(commands);
                }
            }
        }

        private TextReader iTextReader;
        private IMockable iMockable;
    }

    public class MockableScriptRunner
    {
        public class AssertError : Exception
        {
            public AssertError() : base("ASSERT") { }
        }

        public MockableScriptRunner()
        {
            iResultQueue = new Queue<string>();
        }

        public void Run(IWatchableThread aThread, TextReader aStream, IMockable aMockable)
        {
            bool wait = true;

            string lastline = aStream.ReadLine();
            while (true)
            {
                string line = lastline;
                lastline = aStream.ReadLine();
                while (lastline != null && lastline != string.Empty && !lastline.StartsWith("//") && !lastline.StartsWith("mock") && !lastline.StartsWith("expect") && !lastline.StartsWith("empty") && !lastline.StartsWith("break"))
                {
                    line += "\n" + lastline;
                    lastline = aStream.ReadLine();
                }
            
                if (line == null)
                {
                    break;
                }

                if (line.StartsWith("//"))
                {
                    continue;
                }

                var commands = Tokeniser.Parse(line);

                if (commands.Any())
                {
                    string command = commands.First().ToLowerInvariant();

                    if (command == "mock")
                    {
                        Console.WriteLine(line);

                        aThread.Execute(() =>
                        {
                            aMockable.Execute(commands.Skip(1));
                        });

                        wait = true;
                    }
                    else if (command == "expect")
                    {
                        if (wait)
                        {
                            aThread.Wait();
                            wait = false;
                        }

                        string expected = line.Substring("expect".Length + 1);

                        try
                        {
                            string result = iResultQueue.Dequeue();

                            Assert(result, expected);
                        }
                        catch (InvalidOperationException)
                        {
                            Console.WriteLine(string.Format("Failed\nExpected: {0} but queue was empty", expected));
                            throw;
                        }
                    }
					else if (command == "empty")
					{
                        aThread.Wait();
						Assert(iResultQueue.Count == 0);
					}
                    else if (command == "break")
                    {
                        // dummy command to allow adding break points - add a VS break point to the line below and add a "break" line
                        // in the test script
                        string cmd = command;
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }
        }

        public void Result(string aValue)
        {
            iResultQueue.Enqueue(aValue);
            Console.WriteLine(aValue);
        }

        private void Assert(string aActual, string aExpected)
        {
            if (aActual != aExpected)
            {
                Console.WriteLine(string.Format("Failed\nExpected: {0}\nReceived: {1}", aExpected, aActual));
                throw new AssertError();
            }
            else
            {
                Console.Write('.');
            }
        }

        private void Assert(bool aExpression)
        {
            if (!aExpression)
            {
                Console.WriteLine("Failed");
                throw new AssertError();
            }
            else
            {
                Console.Write('.');
            }
        }

        private Queue<string> iResultQueue;
    }

    public class ResultWatcherFactory : IDisposable
    {
        private readonly MockableScriptRunner iRunner;
        private readonly Dictionary<string, List<IDisposable>> iWatchers;

        public ResultWatcherFactory(MockableScriptRunner aRunner)
        {
            iRunner = aRunner;
            iWatchers = new Dictionary<string, List<IDisposable>>();
        }

        public void Create<T>(string aId, IWatchable<T> aWatchable, Func<T, string> aFunction)
        {
            List<IDisposable> watchers;

            if (!iWatchers.TryGetValue(aId, out watchers))
            {
                watchers = new List<IDisposable>();
                iWatchers.Add(aId, watchers);
            }

            watchers.Add(new ResultWatcher<T>(iRunner, aId, aWatchable, aFunction));
        }

        public void Create<T>(string aId, IWatchableUnordered<T> aWatchable, Func<T, string> aFunction)
        {
            List<IDisposable> watchers;

            if (!iWatchers.TryGetValue(aId, out watchers))
            {
                watchers = new List<IDisposable>();
                iWatchers.Add(aId, watchers);
            }

            watchers.Add(new ResultUnorderedWatcher<T>(iRunner, aId, aWatchable, aFunction));
        }

        public void Create<T>(string aId, IWatchableOrdered<T> aWatchable, Func<T, string> aFunction)
        {
            List<IDisposable> watchers;

            if (!iWatchers.TryGetValue(aId, out watchers))
            {
                watchers = new List<IDisposable>();
                iWatchers.Add(aId, watchers);
            }

            watchers.Add(new ResultOrderedWatcher<T>(iRunner, aId, aWatchable, aFunction));
        }

        public void Destroy(string aId)
        {
            iWatchers[aId].ForEach(w => w.Dispose());
            iWatchers.Remove(aId);
        }

        // IDisposable

        public void Dispose()
        {
            foreach (var entry in iWatchers)
            {
                entry.Value.ForEach(w => w.Dispose());
            }
        }
    }

    public class ResultWatcher<T> : IWatcher<T>, IDisposable
    {
        private readonly MockableScriptRunner iRunner;
        private readonly string iId;
        private IWatchable<T> iWatchable;
        private readonly Func<T, string> iFunction;

        public ResultWatcher(MockableScriptRunner aRunner, string aId, IWatchable<T> aWatchable, Func<T, string> aFunction)
        {
            iRunner = aRunner;
            iId = aId;
            iWatchable = aWatchable;
            iFunction = aFunction;

            iWatchable.AddWatcher(this);
        }

        // IWatcher<T>

        public void ItemOpen(string aId, T aValue)
        {
            iRunner.Result(iId + " open " + iFunction(aValue));
        }

        public void ItemUpdate(string aId, T aValue, T aPrevious)
        {
            iRunner.Result(iId + " update " + iFunction(aValue));
        }

        public void ItemClose(string aId, T aValue)
        {
            //iRunner.Result(iUdn + " " + iFunction(aValue, "close"));
        }

        // IDisposable

        public void Dispose()
        {
            iWatchable.RemoveWatcher(this);
        }
    }

    public class ResultUnorderedWatcher<T> : IUnorderedWatcher<T>, IDisposable
    {
        private readonly MockableScriptRunner iRunner;
        private readonly string iId;
        private IWatchableUnordered<T> iWatchable;
        private readonly Func<T, string> iFunction;

        public ResultUnorderedWatcher(MockableScriptRunner aRunner, string aId, IWatchableUnordered<T> aWatchable, Func<T, string> aFunction)
        {
            iRunner = aRunner;
            iId = aId;
            iWatchable = aWatchable;
            iFunction = aFunction;

            iWatchable.AddWatcher(this);
        }

        // IUnorderedWatcher<T>

        public void UnorderedOpen()
        {
        }

        public void UnorderedInitialised()
        {
        }

        public void UnorderedAdd(T aItem)
        {
            iRunner.Result(iId + " add " + iFunction(aItem));
        }

        public void UnorderedRemove(T aItem)
        {
            iRunner.Result(iId + " remove " + iFunction(aItem));
        }

        public void UnorderedClose()
        {
        }

        // IDisposable

        public void Dispose()
        {
            iWatchable.RemoveWatcher(this);
        }
    }

    public class ResultOrderedWatcher<T> : IOrderedWatcher<T>, IDisposable
    {
        private readonly MockableScriptRunner iRunner;
        private readonly string iId;
        private IWatchableOrdered<T> iWatchable;
        private readonly Func<T, string> iFunction;

        public ResultOrderedWatcher(MockableScriptRunner aRunner, string aId, IWatchableOrdered<T> aWatchable, Func<T, string> aFunction)
        {
            iRunner = aRunner;
            iId = aId;
            iWatchable = aWatchable;
            iFunction = aFunction;

            iWatchable.AddWatcher(this);
        }

        // IOrderedWatcher<T>

        public void OrderedOpen()
        {
        }

        public void OrderedInitialised()
        {
        }

        public void OrderedAdd(T aItem, uint aIndex)
        {
            iRunner.Result(iId + " add " + iFunction(aItem));
        }

        public void OrderedMove(T aItem, uint aFrom, uint aTo)
        {
            iRunner.Result(iId + " moved from " + aFrom + " to " + aTo + " " + iFunction(aItem));
        }

        public void OrderedRemove(T aItem, uint aIndex)
        {
            iRunner.Result(iId + " remove " + iFunction(aItem));
        }

        public void OrderedClose()
        {
        }

        // IDisposable

        public void Dispose()
        {
            iWatchable.RemoveWatcher(this);
        }
    }

    public static class MocakbleExtensions
    {
        public static void Execute(this IMockable aMockable, string aValue)
        {
            aMockable.Execute(Tokeniser.Parse(aValue));
        }
    }

}
