using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.LogicalTree;

namespace ToolUi.Runner.Runtime
{
    public static class Helpers
    {
        public static T GetElement<T>(this ILogical visual, [CallerMemberName] string elementName = null)
            where T : class, ILogical
        {
            return visual.Get<T>(elementName);
        }

        public static void HandleErrors(this Task task)
        {
            task.ContinueWith(
                task1 =>
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                        throw new InvalidOperationException("Exception happened in unobserved Task (case 1)", task1.Exception));
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}