using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Consolonia.Themes.TurboVision.Templates.Controls.Dialog;
using ToolUi.Runner.Runtime;

namespace ToolUi.Runner.Dialog
{
    public class CommonDialogWindow : DialogWindow
    {
        public static readonly DirectProperty<CommonDialogWindow, bool> IsEnterEnabledProperty =
            AvaloniaProperty.RegisterDirect<CommonDialogWindow, bool>(nameof(IsEnterEnabled),
                window => window.IsEnterEnabled);

        private bool _isEnterEnabled;

        public CommonDialogWindow(INamed dialogContent) : this()
        {
            ContentPresenter.Content = dialogContent;
            DataContext = this;
        }

        public CommonDialogWindow()
        {
            InitializeComponent();
        }

        private ContentPresenter ContentPresenter => this.GetElement<ContentPresenter>();
        internal Button OkButton => this.GetElement<Button>();

        public bool IsEnterEnabled
        {
            get => _isEnterEnabled;
            set => SetAndRaise(IsEnterEnabledProperty, ref _isEnterEnabled, value);
        }

        public override void CloseDialog()
        {
            Closing?.Invoke();
            base.CloseDialog();
        }

        public event Action Closing;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}