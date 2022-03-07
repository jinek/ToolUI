using System;
using System.Threading.Tasks;
using Consolonia.Core;

namespace ToolUi.Runner
{
    internal static class Program
    {
        private static void Main()
        {
            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                Environment.FailFast("Exception in unobserved task",
                    new InvalidOperationException("Exception happened in unobserved Task (case 2)",
                        args.Exception));
            };

            //todo: setup error handler
            ApplicationStartup.StartConsolonia<App>();
        }
    }
}