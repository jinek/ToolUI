using Consolonia.Core;

namespace Examples.ToolUi
{
    internal static class Program
    {
        private static void Main()
        {//setup error handler
            ApplicationStartup.StartConsolonia<App>();
        }
    }
}