using System;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Consolonia.Themes.TurboVision.Templates.Controls.Dialog;

namespace Examples.ToolUi
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

    public interface IDialogContent<out TResult>
    {
        public event Action<TResult> ResultChanged;
    }

    [Serializable]
    public class OperationAbortException : ApplicationException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        internal OperationAbortException()
        {
        }

        protected OperationAbortException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}