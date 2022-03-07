using Avalonia.Controls;

namespace ToolUi.Runner.Dialog
{
    public abstract class DialogContentBase : UserControl
    {
        public string Title { get; set; }

        public bool OkVisible { get; set; } = true;
        
        public bool IsError { get; set; }
    }
}