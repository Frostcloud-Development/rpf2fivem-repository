using System;
using System.Threading;
using System.Windows.Forms;
using Sentry;

namespace rpf2fivem
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            // Init the Sentry SDK
            SentrySdk.Init(o =>
            {
                // Tells which project in Sentry to send events to:
                o.Dsn = "https://8d64d12f888336dfbe68b0b8f290be1c@o4509386247503872.ingest.de.sentry.io/4509386253795408";
                // When configuring for the first time, to see what the SDK is doing:
                o.Debug = true;

                o.TracesSampleRate = 1.0;
                o.IsGlobalModeEnabled = true;
                o.Release = Properties.Resources.Sentry_version;
                o.AutoSessionTracking = true;
                o.Environment = Properties.Resources.Sentry_enviroment;
                o.StackTraceMode = Sentry.StackTraceMode.Enhanced;
            });
            // Configure WinForms to throw exceptions so Sentry can capture them.
            
            //warms threadpool which will shorten Parralel.ForEach creation time a bit, it works cause people need to choose files first and the system will warm up in the background
            ThreadPool.SetMinThreads(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

            Application.Run(new Main());
        }
    }
}
