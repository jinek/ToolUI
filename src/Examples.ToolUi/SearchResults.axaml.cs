using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Consolonia.Themes.TurboVision.Templates.Controls.Dialog;
using DynamicData;
using Examples.ToolUi.Data;

namespace Examples.ToolUi
{
    public class SearchResults : DialogWindow
    {
        private DataGrid SearchDataGrid => this.GetElement<DataGrid>();
        public ObservableCollection<SearchRow> SearchRows { get; } = new();

        public SearchResults()
        {
            InitializeComponent();
            DataContext = this;
        }

        public SearchResults(SearchRow[] searchRows) : this()
        {
            SearchRows.AddRange(searchRows);
            SearchDataGrid.SelectedIndex = 0;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SearchAgain()
        {
            SearchMore = true;
            CloseDialog();
        }

        public void CancelDialog()
        {
            RowToInstall = null;
            base.CloseDialog();
        }

        public bool SearchMore { get; private set; }

        private void Install()
        {
            RowToInstall = (SearchRow)SearchDataGrid.SelectedItem;
            CloseDialog();
        }

        private void InstallGlobal()
        {
            InstallGlobally = true;
            Install();
        }

        public bool InstallGlobally { get; private set; }

        public SearchRow RowToInstall { get; private set; }
    }
}