using Avalonia.Markup.Xaml;
using Consolonia.Themes.TurboVision.Templates.Controls.Dialog;
using ToolUi.Runner.Data;

namespace ToolUi.Runner.Forms
{
    public class SearchDetails : DialogWindow
    {
        public SearchDetails(SearchRow searchRow) : this()
        {
            SearchRow = searchRow;
            DataContext = this;
        }

        public SearchDetails()
        {
            InitializeComponent();
        }

        public SearchRow SearchRow { get; private set; }

        public bool InstallGlobally { get; private set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Install()
        {
            CloseDialog();
        }

        private void InstallGlobal()
        {
            InstallGlobally = true;
            Install();
        }

        public void CancelDialog()
        {
            SearchRow = null;
            base.CloseDialog();
        }
    }
}