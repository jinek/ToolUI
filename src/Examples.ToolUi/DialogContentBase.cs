using Avalonia.Controls;

namespace Examples.ToolUi
{
    public abstract class DialogContentBase : UserControl
    {
        public string Title { get; set; }

        public bool OkVisibile { get; set; } = true;
        
        public bool IsError { get; set; }
    }
}