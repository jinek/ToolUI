using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ToolUi.Runner.Dialog;
using ToolUi.Runner.Runtime;

namespace ToolUi.Runner.Forms
{
    public class RunDialog : DialogContentBase, IDialogContent<(string, string)>
    {
        private string _commandToRun;
        private string _text;

        public RunDialog(string toolName, string[] possibleCommands, string helpText) : this()
        {
            const int maxHelpLength = 750;
            if (helpText.Length > maxHelpLength)
            {
                helpText = helpText[..maxHelpLength];
            }
            PossibleCommands = possibleCommands;
            CommandToRun = possibleCommands[0];

            ParametersTextBlock.Text += toolName + " :";
            HelpTextBlock.Text = helpText;
            AttachedToVisualTree += (_, _) => { RaiseResult(); };

            DataContext = this;
        }

        public RunDialog()
        {
            Title = "Parameters";
            InitializeComponent();
        }

        private TextBlock ParametersTextBlock => this.GetElement<TextBlock>();
        private TextBlock HelpTextBlock => this.GetElement<TextBlock>();

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                RaiseResult();
            }
        }

        public string[] PossibleCommands { get; }

        public string CommandToRun
        {
            get => _commandToRun;
            set
            {
                _commandToRun = value;
                RaiseResult();
            }
        }

        public event Action<(string, string)> ResultChanged;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void RaiseResult()
        {
            ResultChanged?.Invoke((CommandToRun, _text));
        }
    }
}