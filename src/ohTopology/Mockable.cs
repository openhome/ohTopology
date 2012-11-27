using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
}
