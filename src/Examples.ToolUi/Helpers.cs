using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Threading;

namespace Examples.ToolUi
{
    public static class Helpers
    {
        public static T GetElement<T>(this ILogical visual, [CallerMemberName] string elementName = null)
            where T : class, ILogical
        {
            return visual.Get<T>(elementName);
        }

        public static void HandleErrors(this Task task)
        {
            task.ContinueWith(
                task1 =>
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                        throw new InvalidOperationException("Exception happened in unobserved Task", task1.Exception));
                }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }

    public class UIExtensions : AvaloniaObject
    {
        static UIExtensions()
        {
            FocusOnLoadProperty.Changed.Subscribe(args =>
            {
                if (args.NewValue.GetValueOrDefault() is false)
                    throw new NotImplementedException();

                var control = (Control)args.Sender;
                control.AttachedToVisualTree += (sender, _) =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        var control2 = (Control)sender;
                        control2.Focus();
                    });
                };
            });
        }

        public static readonly AttachedProperty<bool> FocusOnLoadProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>("FocusOnLoad", typeof(UIExtensions));

        public static void SetFocusOnLoad(Control control, bool value)
        {
            control.SetValue(FocusOnLoadProperty, value);
        }

        public static bool GetFocusOnLoad(Control control)
        {
            return control.GetValue(FocusOnLoadProperty);
        }
    }
}