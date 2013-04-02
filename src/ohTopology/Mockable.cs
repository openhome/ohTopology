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
            bool wait = false;

            while (true)
            {
                string line = aStream.ReadLine();
            
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
                        aMockable.Execute(commands.Skip(1));

                        wait = true;
                    }
                    else if (command == "expect")
                    {
                        if (wait)
                        {
                            aThread.WaitComplete();
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
						Assert(iResultQueue.Count == 0);
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
            //Console.WriteLine(aValue);
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
}
