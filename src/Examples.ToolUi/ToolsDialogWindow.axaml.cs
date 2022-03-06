using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CliWrap;
using Consolonia.Themes.TurboVision.Templates.Controls.Dialog;
using DynamicData;
using Examples.ToolUi.Data;

namespace Examples.ToolUi
{
    public class ToolsDialogWindow : DialogWindow
    {
        private readonly Action<string> _output;
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


        private ObservableCollection<DotnetTool> ToolsList { get; } = new();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CreateManifest()
        {
            CatchOperationAbort(async () =>
            {
                var output =
                    await ExecuteDotnetAsync<RawString>(1, 0, "Manifest initialization...", true, "new tool-manifest");

                await new OkCancel(string.Join('\n', output.Select(s => s.str)))
                    {
                        OkVisibile = false,
                        Title = "Manifest created"
                    }
                    .ShowDialog(this);
            });
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
            if(_allowExit)
                base.CloseDialog();
        }

        private async Task LoadData()
        {
            // await ExecuteDotnetAsync("Initializing...", "--info");

            var localTools = await ExecuteDotnetAsync<DotnetTool>(4, 2, "Reading local tools", false, "tool list");

            var globalTools =
                await ExecuteDotnetAsync<DotnetTool>(3, 2, "Reading global tools", false, "tool list --global");

            ToolsList.Clear();
            ToolsList.AddRange(localTools);
            ToolsList.AddRange(globalTools);
            ToolsTable.SelectedIndex = 0;
            FocusGrid();
        }

        private void FocusGrid()
        {
            if (!ToolsTable.IsFocused && !ToolsTable.IsKeyboardFocusWithin)
                ToolsTable.Focus();
        }

        private Task ExecuteDotnetAsync(string title, bool isCancellable, params string[] dotnetArguments)
        {
            return ExecuteDotnetAsync<object>(0, 0, title, isCancellable, dotnetArguments);
        }

        public async void Refresh()
        {
            await RefreshInternal(true);
        }

        private async Task RefreshInternal(bool keepPosition = false)
        {
            object selectedItem = ToolsTable.SelectedItem;
            await LoadData();

            if (keepPosition)
                ToolsTable.SelectedItem = selectedItem;
        }

        private void ShowHelp()
        {
            CatchOperationAbort(async () =>
            {
                await new HelpText
                {
                    OkVisibile = false
                }.ShowDialog(this);
            });
        }

        private async Task<T[]> ExecuteDotnetAsync<T>(int parametersNumber, int skipRows, string title,
            bool isCancellable,
            params string[] dotnetArguments)
        {
            var output = new List<string>();
            ProgressBar progressBar = ProgressBar;

            try
            {
                if (_currentCommandCts != null)
                    throw new InvalidOperationException("Another command is already in process");

                ToolsTable.IsEnabled = false; // workaround: disabling the window does not disable children
                ProgressTextBlock.Text = title;
                if (isCancellable)
                {
                    _currentCommandCts = new CancellationTokenSource();
                    ProgressTextBlock.Text += " [ Cancel (Esc)]";
                }

                ProgressPanel.IsVisible = true;
                progressBar.Value = 0;
                InteractionBorder.IsEnabled = false;

                var errorStringBuilder = new StringBuilder();

                CommandResult cliResult;
                try
                {
                    cliResult = await Cli.Wrap("dotnet")
                        .WithArguments(dotnetArguments, false)
                        .WithValidation(CommandResultValidation.None)
                        .WithStandardOutputPipe(PipeTarget.ToDelegate(HandleLine))
                        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(errorStringBuilder))
                        .ExecuteAsync(_currentCommandCts?.Token ?? default);
                }
                catch (TaskCanceledException)
                {
                    throw new OperationAbortException();
                }

                int exitCode = cliResult.ExitCode;

                if (exitCode != 0)
                    throw new OperationErrorException(
                        errorStringBuilder + Environment.NewLine + string.Join('\n', output), exitCode);

                await Dispatcher.UIThread.InvokeAsync(() => { progressBar.Value = 100; });
                await Task.Delay(500);

                if (parametersNumber == 0) return null;

                return output.Skip(skipRows)
                    .Select(str =>
                    {
                        object[] constructorParameters = str.Split("  ",
                                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                            .Take(parametersNumber)
                            .Cast<object>()
                            .ToArray();

                        int dummyNumberToAdd = parametersNumber - constructorParameters.Length;
                        if (dummyNumberToAdd > 0)
                            constructorParameters = constructorParameters
                                .Concat(Enumerable.Repeat((string)null, dummyNumberToAdd)).ToArray();

                        return (T)Activator.CreateInstance(typeof(T), constructorParameters);
                    }).ToArray(); //todo: change to delayed execution
            }
            finally
            {
                _currentCommandCts = null;
                ProgressPanel.IsVisible = false;
                InteractionBorder.IsEnabled = true;
                ToolsTable.IsEnabled = true;
            }

            Output($@"{Environment.CurrentDirectory}> dotnet {string.Join(' ', dotnetArguments)}");

            void HandleLine(string line)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    progressBar.Value += (100 - progressBar.Value) / 30; //todo: fake progress works faster
                    Output(line);
                });
                output.Add(line);
            }
        }

        private void Output(string line)
        {
            _output(line);
        }

        private void RunTool()
        {
            //if (e.Key == Key.Enter)
            {
                //  e.Handled = true;

                CatchOperationAbort(async () =>
                {
                    (string id, string _, string toolCommand, string manifest) = SelectedTool;
                    

                    // Only one command https://github.com/dotnet/sdk/issues/10014
                    string command = $"tool run {toolCommand}";

                    string helpString;

                    try
                    {
                        var helpStrings = await ExecuteDotnetAsync<RawString>(1, 0, $"Getting help for {id}", true,
                            command + " -- --help");
                        helpString = string.Join("\n", helpStrings.Select(str => str.str));
                    }
                    catch (OperationErrorException operationErrorException)
                    {
                        helpString =
                            $"This tool does not provide any help in response to --help parameter. The message was: {Environment.NewLine} {operationErrorException.Message}";
                    }

                    string[] possibleCommands = toolCommand.Split();

                    (string selectedCommand/*is always one*/, string parameters) = await new RunDialog(id, possibleCommands, helpString)
                        .ShowDialog(this);


                    if (!string.IsNullOrEmpty(parameters))
                        command += $" -- {parameters}";

                    var runStrings = await ExecuteDotnetAsync<RawString>(1, 0, $"Running {id}", true, command);
                    if (!runStrings.Any()) runStrings = new[] { new RawString("Completed.") };

                    await new OkCancel(string.Join("\n", runStrings.Select(str => str.str)))
                    {
                        Title = "Run Output",
                        OkVisibile = false
                    }.ShowDialog(this);
                });
            }
        }

        private DotnetTool SelectedTool => (DotnetTool)ToolsTable.SelectedItem;

        private void Update()
        {
            CatchOperationAbort(async () =>
            {
                DotnetTool selectedTool = SelectedTool;
                string dotnetArguments = "tool update ";
                if (selectedTool.Manifest == DotnetTool.ManifestKeyword)
                    dotnetArguments += "--global ";
                dotnetArguments += selectedTool.Id;
                await ExecuteDotnetAsync($"Updating {selectedTool.Id}", true, dotnetArguments);

                await new OkCancel($"{selectedTool.Id} has been updated")
                    {
                        Title = "Tool updated",
                        OkVisibile = false
                    }
                    .ShowDialog(this);
            });
        }

        private void Restore()
        {
            CatchOperationAbort(async () =>
            {
                await ExecuteDotnetAsync($"Restoring", true, "tool restore");
                await new OkCancel("Tools have been restored.")
                    {
                        Title = "Restored",
                        OkVisibile = false
                    }
                    .ShowDialog(this);
            });
        }

        private void Cancel()
        {
            _currentCommandCts?.Cancel();
        }

        private void Search()
        {
            CatchOperationAbort(async () =>
            {
                SearchResults searchResultsDialog;
                do
                {
                    (string keyword, bool prerelease) = ((string, bool))await new SearchKeyword().ShowDialog(this);
                    string parameters = @$"tool search ""{keyword}""";
                    if (prerelease) parameters += " --prerelease";
                    var searchRows =
                        await ExecuteDotnetAsync<SearchRow>(5, 2, $"Searching \"{keyword}\"", true, parameters);
                    searchResultsDialog = new SearchResults(searchRows);
                    await searchResultsDialog.ShowDialogAsync(this);
                } while (searchResultsDialog.SearchMore);

                if (searchResultsDialog.RowToInstall == null) // cancel button clicked
                    return;
                
                string packageId = searchResultsDialog.RowToInstall.Id;
                bool installGlobally = searchResultsDialog.InstallGlobally;

                await new OkCancel(
                        @$"Are you sure you want to install {packageId} {(installGlobally ? "globally" : "locally")}?")
                    {
                        Title = "Installation"
                    }
                    .ShowDialog(this);
                string command = $"tool install {packageId}";
                command += installGlobally ? " --global" : " --local";

                var installationStrings =
                    await ExecuteDotnetAsync<RawString>(1, 0, $"Installing {packageId}", true, command);

                await new OkCancel(string.Join("\n", installationStrings.Select(str => str.str)))
                {
                    Title = "Tool Installed",
                    OkVisibile = false
                }.ShowDialog(this);

                await RefreshInternal();

                DotnetTool itemToSelect = ToolsList.SingleOrDefault(tool =>
                    tool.Id == packageId && (tool.Manifest == DotnetTool.ManifestKeyword) ^ !installGlobally);

                if (itemToSelect != null) // can be null if uninstalled outside
                    ToolsTable.SelectedItem = itemToSelect;
            });
        }

        public void Uninstall()
        {
            (string id, string _, string _, string manifest) = (DotnetTool)ToolsTable.SelectedItem;
            CatchOperationAbort(async () =>
            {
                await new OkCancel(
                        @$"Are you sure you want to uninstall {id} from {manifest}?")
                    {
                        Title = "Uninstall"
                    }
                    .ShowDialog(this);
                string command = $"tool uninstall {id}";
                if (manifest == DotnetTool.ManifestKeyword)
                    command += " --global";
                var installationStrings =
                    await ExecuteDotnetAsync<RawString>(1, 0, $"Uninstalling {id}", true, command);
                await new OkCancel(string.Join("\n", installationStrings.Select(str => str.str)))
                {
                    Title = "Tool Uninstalled",
                    OkVisibile = false
                }.ShowDialog(this);

                ToolsTable.SelectedIndex--;
                await RefreshInternal(true);
            });
        }

        //todo: no DispatcherUnhandledException, how to catch exceptions?
        public async void CatchOperationAbort(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (OperationErrorException operationErrorException)
            {
                CatchOperationAbort(async () =>
                {
                    await new OkCancel(operationErrorException.Message)
                    {
                        Title = $"dotnet tool error (code={operationErrorException.ExitCode})",
                        OkVisibile = false,
                        IsError = true
                    }.ShowDialog(this);
                    throw new OperationAbortException();
                });
            }
            catch (OperationAbortException)
            {
            }
        }

        private bool _allowExit; 

        public void Quit()
        {
            CatchOperationAbort(async () =>
            {
                await new OkCancel("Are you sure you want to quit?")
                {
                    Title = "Exit",
                    OkVisibile = true
                }.ShowDialog(this);
                _allowExit = true;
                CloseDialog();
            });
        }
    }
}