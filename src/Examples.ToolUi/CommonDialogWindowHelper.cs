using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace Examples.ToolUi
{
    public static class CommonDialogWindowHelper
    {
        public static async Task<TResult> ShowDialog<TResult>(this IDialogContent<TResult> dialogContent,
            Control parent)
        {
            var dialogContentBase = (DialogContentBase)dialogContent;

            var taskCompletionSource = new TaskCompletionSource<TResult>();

            var result = default(TResult);

            var commonDialogWindow = new CommonDialogWindow((UserControl)dialogContent)
            {
                Title = dialogContentBase.Title,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (dialogContentBase.IsError)
                commonDialogWindow.Background = (IBrush)parent.FindResource("ThemeErrorBrush");


            var okButton = commonDialogWindow.Get<Button>("OkButton");
            bool okVisible = dialogContentBase.OkVisibile;
            if (!okVisible)
            {
                okButton.IsVisible = false;
                var cancelButton = commonDialogWindow.Get<Button>("CancelButton");
                cancelButton.IsDefault = true;
                cancelButton.SetValue(UIExtensions.FocusOnLoadProperty, true);
                cancelButton.Content = "Close";
            }

            commonDialogWindow.Closing += CommonDialogWindowOnClosing;
            dialogContent.ResultChanged += ResultChanged;
            okButton.Click += OkButtonClicked;

            try
            {
                await commonDialogWindow.ShowDialogAsync(parent);
            }
            catch (TaskCanceledException)
            {
            }

            return await taskCompletionSource.Task;

            void ResultChanged(TResult obj)
            {
                result = obj;
                commonDialogWindow.IsEnterEnabled = obj != null;
            }

            void OkButtonClicked(object? sender, RoutedEventArgs e)
            {
                taskCompletionSource.SetResult(result);
                commonDialogWindow.CloseDialog();
            }

            void CommonDialogWindowOnClosing()
            {
                commonDialogWindow.Closing -= CommonDialogWindowOnClosing;
                dialogContent.ResultChanged -= ResultChanged;
                okButton.Click -= OkButtonClicked;

                if (okVisible)
                {
                    if (!taskCompletionSource.Task.IsCompleted)
                        taskCompletionSource.SetException(new OperationAbortException());
                }
                else
                {
                    taskCompletionSource.SetResult(default);
                }
            }
        }
    }
}