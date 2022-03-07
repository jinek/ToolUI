using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ToolUi.Runner.Dialog;
using ToolUi.Runner.Runtime;

namespace ToolUi.Runner.Forms
{
    public class OkCancel : DialogContentBase, IDialogContent<object>
    {
        private TextBlock TextBlock => this.GetElement<TextBlock>();

        public OkCancel(string question) : this()
        {
            const int maxSymbols = 1000;
            if (question.Length > maxSymbols)
                question = question[..maxSymbols];

            TextBlock.Text = question;
            AttachedToVisualTree += (_, _) =>
            {
                ResultChanged(new object());
            };
        }

        public OkCancel()
        {
            InitializeComponent();
        }

        public event Action<object> ResultChanged;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}