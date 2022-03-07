using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CliWrap;
using ToolUi.Runner.Dialog;
using ToolUi.Runner.Runtime;

namespace ToolUi.Runner.Forms
{
    public partial class ToolsDialogWindow
    {

        private void Output(string line)
        {
            _output(line);
        }

        private Task ExecuteDotnetAsync(string title, bool isCancellable, params string[] dotnetArguments)
        {
            return ExecuteDotnetAsync<object>(0, 0, title, isCancellable, dotnetArguments);
        }

        private async Task<T[]> ExecuteDotnetAsync<T>(int parametersNumber, int skipRows, string title,
            bool isCancellable,
            params string[] dotnetArguments)
        {
            var output = new List<string>();
            ProgressBar progressBar = ProgressBar;

            Output($@"{Environment.CurrentDirectory}> dotnet {string.Join(' ', dotnetArguments)}");
            
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

        private async void CatchOperationAbort(Func<Task> action)
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
                        OkVisible = false,
                        IsError = true
                    }.ShowDialog(this);
                    throw new OperationAbortException();
                });
            }
            catch (OperationAbortException)
            {
            }
        }
    }
}