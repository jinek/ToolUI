using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Markup.Xaml;
using ToolUi.Runner.Dialog;

namespace ToolUi.Runner.Forms
{
    public class HelpText : DialogContentBase, IDialogContent<object>
    {
        public HelpText()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public event Action<object> ResultChanged;

        private void Navigate(string url)
        {
            Task.Run(() => { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); });
        }
    }
}