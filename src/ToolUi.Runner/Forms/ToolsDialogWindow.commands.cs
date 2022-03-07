using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DynamicData;
using ToolUi.Runner.Data;
using ToolUi.Runner.Dialog;
using ToolUi.Runner.Runtime;

namespace ToolUi.Runner.Forms
{
    public partial class ToolsDialogWindow
    {
        private ObservableCollection<ToolRow> ToolsList { get; } = new();
        private ToolRow SelectedTool => (ToolRow)ToolsTable.SelectedItem;

        private void CreateManifest()
        {
            CatchOperationAbort(async () =>
            {
                var output =
                    await ExecuteDotnetAsync<RawStringRow>(1, 0, "Manifest initialization...", true,
                        "new tool-manifest");

                await new OkCancel(string.Join('\n', output.Select(s => s.str)))
                {
                    OkVisible = false,
                    Title = "Manifest created"
                }.ShowDialog(this);
            });
        }

        private async Task LoadData()
        {
            // await ExecuteDotnetAsync("Initializing...", "--info");

            var localTools = await ExecuteDotnetAsync<ToolRow>(4, 2, "Reading local tools", false, "tool list");

            var globalTools =
                await ExecuteDotnetAsync<ToolRow>(3, 2, "Reading global tools", false, "tool list --global");

            ToolsList.Clear();
            ToolsList.AddRange(localTools);
            ToolsList.AddRange(globalTools);
            ToolsTable.SelectedIndex = 0;
            FocusTheGrid();
        }

        private async void Refresh()
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
                    OkVisible = false
                }.ShowDialog(this);
            });
        }

        private void RunTool()
        {
            CatchOperationAbort(async () =>
            {
                (string id, string _, string toolCommand, string manifest) = SelectedTool;

                // Only one command https://github.com/dotnet/sdk/issues/10014
                bool isGlobal = manifest == ToolRow.GlobalManifestKey;
                string command = isGlobal ? $"{RemoveDotnetEscape} {toolCommand}" : $"tool run {toolCommand}";

                string helpString;

                try
                {
                    var helpStrings = await ExecuteDotnetAsync<RawStringRow>(1, 0, $"Getting help for {id}", true,
                        (command + (isGlobal ? " --help" : " -- --help")).Split());
                    helpString = string.Join("\n", helpStrings.Select(str => str.str));
                }
                catch (OperationErrorException operationErrorException)
                {
                    helpString =
                        $"This tool does not provide any help in response to --help parameter. The message was: {Environment.NewLine} {operationErrorException.Message}";
                }

                string[] possibleCommands = toolCommand.Split();

                (string selectedCommand /*is always one*/, string parameters) =
                    await new RunDialog(id, possibleCommands, helpString)
                        .ShowDialog(this);


                if (!string.IsNullOrEmpty(parameters))
                {
                    if (!isGlobal)
                        command += " --";
                    command += $" {parameters}";
                }

                var runStrings = await ExecuteDotnetAsync<RawStringRow>(1, 0, $"Running {id}", true, command.Split());
                if (!runStrings.Any()) runStrings = new[] { new RawStringRow("Completed.") };

                await new OkCancel(string.Join("\n", runStrings.Select(str => str.str)))
                {
                    Title = "Run Output",
                    OkVisible = false
                }.ShowDialog(this);
            });
        }

        private void Update()
        {
            CatchOperationAbort(async () =>
            {
                ToolRow selectedTool = SelectedTool;
                string dotnetArguments = "tool update ";
                if (selectedTool.Manifest == ToolRow.GlobalManifestKey)
                    dotnetArguments += "--global ";
                dotnetArguments += selectedTool.Id;
                await ExecuteDotnetAsync($"Updating {selectedTool.Id}", true, dotnetArguments);

                await new OkCancel($"{selectedTool.Id} has been updated")
                    {
                        Title = "Tool updated",
                        OkVisible = false
                    }
                    .ShowDialog(this);
            });
        }

        private void Restore()
        {
            CatchOperationAbort(async () =>
            {
                await ExecuteDotnetAsync("Restoring", true, "tool restore");
                await new OkCancel("Tools have been restored.")
                    {
                        Title = "Restored",
                        OkVisible = false
                    }
                    .ShowDialog(this);
            });
        }

        private void Cancel()
        {
            _currentCommandCts?.Cancel();
        }

        public void Uninstall()
        {
            (string id, string _, string _, string manifest) = (ToolRow)ToolsTable.SelectedItem;
            CatchOperationAbort(async () =>
            {
                await new OkCancel(
                        @$"Are you sure you want to uninstall {id} from {manifest}?")
                    {
                        Title = "Uninstall"
                    }
                    .ShowDialog(this);
                string command = $"tool uninstall {id}";
                if (manifest == ToolRow.GlobalManifestKey)
                    command += " --global";
                var installationStrings =
                    await ExecuteDotnetAsync<RawStringRow>(1, 0, $"Uninstalling {id}", true, command);
                await new OkCancel(string.Join("\n", installationStrings.Select(str => str.str)))
                {
                    Title = "Tool Uninstalled",
                    OkVisible = false
                }.ShowDialog(this);

                ToolsTable.SelectedIndex--;
                await RefreshInternal(true);
            });
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
                string version = searchResultsDialog.RowToInstall.Version;
                
                bool installGlobally = searchResultsDialog.InstallGlobally;

                await new OkCancel(
                        @$"Are you sure you want to install {packageId} {(installGlobally ? "globally" : "locally")}?")
                    {
                        Title = "Installation"
                    }
                    .ShowDialog(this);
                string command = $"tool install {packageId}";
                command += installGlobally ? " --global" : " --local";
                command += $" --version {version}";

                var installationStrings =
                    await ExecuteDotnetAsync<RawStringRow>(1, 0, $"Installing {packageId}", true, command);

                await new OkCancel(string.Join("\n", installationStrings.Select(str => str.str)))
                {
                    Title = "Tool Installed",
                    OkVisible = false
                }.ShowDialog(this);

                await RefreshInternal();

                ToolRow itemToSelect = ToolsList.SingleOrDefault(tool =>
                    tool.Id == packageId && (tool.Manifest == ToolRow.GlobalManifestKey) ^ !installGlobally);

                if (itemToSelect != null) // can be null if uninstalled outside
                    ToolsTable.SelectedItem = itemToSelect;
            });
        }


        public void Quit()
        {
            CatchOperationAbort(async () =>
            {
                await new OkCancel("Are you sure you want to quit?")
                {
                    Title = "Exit",
                    OkVisible = true
                }.ShowDialog(this);
                _allowExit = true;
                CloseDialog();
            });
        }
    }
}