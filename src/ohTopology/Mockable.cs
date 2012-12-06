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

        public void Add(string aId, IMockable aValue)
        {
            iMockables.Add(aId, aValue);
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
                IEnumerable<string> command = line.Trim().ToLowerInvariant().Split(' ');

                if (command.First() == "exit")
                {
                    break;
                }

                if (command.First() != string.Empty)
                {
                    iMockable.Execute(command);
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

        public void Run(IWatchableThread aThread, TextReader aTextReader, IMockable aMockable)
        {
            bool wait = false;
            while (true)
            {
                string line = aTextReader.ReadLine();
                
                if (line == "exit")
                {
                    break;
                }

                if (!string.IsNullOrEmpty(line))
                {
                    IEnumerable<string> command = line.Trim().ToLowerInvariant().Split(' ');

                    if (command.First() == "mock")
                    {
                        IEnumerable<string> value = command.Skip(1);
                        
                        aMockable.Execute(value);

                        wait = true;
                    }
                    else if (command.First() == "expect")
                    {
                        if (wait)
                        {
                            aThread.WaitComplete();
                            wait = false;
                        }

                        string expected = line.Substring("expect".Length + 1);

                        if (expected == "empty")
                        {
                            Assert(iResultQueue.Count == 0);
                        }
                        else
                        {
                            string result = iResultQueue.Dequeue();

                            Assert(result == expected);
                        }
                    }
                    else if(command.First() == "wait")
                    {
                        aThread.WaitComplete();
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
        }

        private void Assert(bool aExpression)
        {
            if (!aExpression)
            {
                StackTrace st = new StackTrace(true);
                StackFrame sf = st.GetFrame(1);

                Console.WriteLine(sf.GetFileName(), sf.GetFileLineNumber());

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
