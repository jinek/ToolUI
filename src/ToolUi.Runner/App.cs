using System;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml.Styling;
using Consolonia.Core.Infrastructure;
using Consolonia.Themes.TurboVision.Themes.TurboVisionDark;
using ToolUi.Runner.Runtime;

namespace ToolUi.Runner
{
    internal class App : ConsoloniaApplication
    {
        public App()
        {
            var baseUri = new Uri($"avares://{typeof(App).Namespace}");
            Styles.Add(new TurboVisionDarkTheme(baseUri));

            Styles.Add(new StyleInclude(baseUri)
                { Source = new Uri(@"Styles/SpecialStyles.axaml", UriKind.Relative) });
        }

        public override void OnFrameworkInitializationCompleted()
        {
            ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime).MainWindow = new MainWindow();
            base.OnFrameworkInitializationCompleted();
        }
    }
}