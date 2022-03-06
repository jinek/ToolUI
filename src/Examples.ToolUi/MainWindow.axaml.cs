using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Examples.ToolUi
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = this;
            
            ShowDialog();
        }

        private ObservableCollection<string> OutputList { get; } = new();

        private async void ShowDialog()
        {
            await Task.Yield();
            var dialogWindow = new ToolsDialogWindow(Output);
            await dialogWindow.ShowDialogAsync(this.FindControl<ListBox>("Output"));
            Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Output(string line)
        {
            if (OutputList.Count >= ClientSize.Height)
                OutputList.Clear();

            if (OutputList.Count <= ClientSize.Height) OutputList.Add(line);
        }
    }
}