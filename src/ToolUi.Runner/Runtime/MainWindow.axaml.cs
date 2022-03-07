using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ToolUi.Runner.Forms;

namespace ToolUi.Runner.Runtime
{
    public class MainWindow : Window
    {
        private ObservableCollection<string> OutputList { get; } = new();

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = this;
            ShowDialog();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void ShowDialog()
        {
            await Task.Yield();
            var dialogWindow = new ToolsDialogWindow(Output);
            await dialogWindow.ShowDialogAsync(this);
            Close();
        }

        private void Output(string line)
        {
            if (OutputList.Count >= ClientSize.Height)
                OutputList.Clear();

            if (OutputList.Count <= ClientSize.Height) 
                OutputList.Add(line);
        }
    }
}