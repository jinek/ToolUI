using System;
using Avalonia.Markup.Xaml;
using ToolUi.Runner.Dialog;

namespace ToolUi.Runner.Forms
{
    public class SearchKeyword : DialogContentBase, IDialogContent<(string, bool)?>
    {
        private string _keyword;
        private bool _prerelease;

        public SearchKeyword()
        {
            Title = "Search";
            InitializeComponent();
            DataContext = this;
        }

        public bool Prerelease
        {
            get => _prerelease;
            set
            {
                _prerelease = value;
                RaiseResultChanged();
            }
        }

        public string Keyword
        {
            get => _keyword;
            set
            {
                _keyword = value;
                RaiseResultChanged();
            }
        }

        public event Action<(string, bool)?> ResultChanged;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void RaiseResultChanged()
        {
            if (string.IsNullOrEmpty(_keyword))
                ResultChanged(null);
            else
                ResultChanged((_keyword, _prerelease));
        }
    }
}