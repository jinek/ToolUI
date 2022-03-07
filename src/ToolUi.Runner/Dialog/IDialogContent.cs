using System;

namespace ToolUi.Runner.Dialog
{
    public interface IDialogContent<out TResult>
    {
        public event Action<TResult> ResultChanged;
    }
}