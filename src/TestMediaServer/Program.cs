using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenHome.Os;
using OpenHome.Os.App;
using OpenHome.Av;

namespace TestMediaServer
{
    class Program
    {
        class ExceptionReporter : IExceptionReporter
        {
            public void ReportException(Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(-1);
            }
        }

        static void Main(string[] args)
        {
            ExceptionReporter reporter = new ExceptionReporter();
            WatchableThread watchablethread = new WatchableThread(reporter);
            WatchableThread subscribeThread = new WatchableThread(reporter);

            //var ms = new MockWatchableMediaServer(watchablethread, subscribeThread, "ABCDEFGHIJKLM", ".");
        }
    }
}
