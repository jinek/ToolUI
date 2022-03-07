using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Consolonia.Themes.TurboVision.Templates.Controls.Dialog;
using ToolUi.Runner.Runtime;

namespace ToolUi.Runner.Forms
{
    public partial class ToolsDialogWindow : DialogWindow
    {
        private readonly Action<string> _output;
        private bool _allowExit;
        private CancellationTokenSource _currentCommandCts;

        public ToolsDialogWindow(Action<string> output) : this()
        {
            _output = output;
            Output("> dotnet tool run ui");
            Output("Welcome to Avalonia!");
            Output("Welcome to Consolonia!");
            Output("This is promotional tool. Please, check HELP for more information.");
            LoadData().HandleErrors();
        }

        public ToolsDialogWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private Panel ProgressPanel => this.GetElement<Panel>();
        private TextBlock ProgressTextBlock => this.GetElement<TextBlock>();
        private Border InteractionBorder => this.GetElement<Border>();
        private Button MenuButton => this.GetElement<Button>();
        private DataGrid ToolsTable => this.GetElement<DataGrid>();
        private ProgressBar ProgressBar => this.GetElement<ProgressBar>();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ShowMenu()
        {
            IInputElement? focusedElement = FocusManager.Instance.Current;

            // workaround for https://github.com/AvaloniaUI/Avalonia/issues/7695
            Button menuButton = MenuButton;

            menuButton.Flyout.Closed += MenuClosed;
            menuButton.Flyout.ShowAt(menuButton, false);

            void MenuClosed(object? sender, EventArgs e)
            {
                menuButton.Flyout.Closed -= MenuClosed;
                focusedElement?.Focus();
            }
        }

        public override void CloseDialog()
        {
            Cancel();
            if (_allowExit)
                base.CloseDialog();
        }

        private void FocusTheGrid()
        {
            if (!ToolsTable.IsFocused && !ToolsTable.IsKeyboardFocusWithin)
                ToolsTable.Focus();
        }

        //todo: no DispatcherUnhandledException, how to catch exceptions?
    }
}