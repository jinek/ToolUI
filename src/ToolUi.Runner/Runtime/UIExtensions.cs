using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace ToolUi.Runner.Runtime
{
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