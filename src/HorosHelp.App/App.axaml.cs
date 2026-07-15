using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HorosHelp.Core.Services.Logging;
using HorosHelp.UI.Services;
using HorosHelp.UI.ViewModels.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HorosHelp.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; internal set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Dispatcher.UIThread.UnhandledException += OnUiThreadUnhandledException;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var shellViewModel = Services.GetRequiredService<ShellViewModel>();
            var mainWindow = new MainWindow();
            mainWindow.BindShell(shellViewModel);
            desktop.MainWindow = mainWindow;
            desktop.ShutdownRequested += (_, _) =>
            {
                var logger = Services.GetService<ILogger<App>>();
                logger?.LogInformation("HorosHelper shutting down");
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnUiThreadUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var logger = Services.GetService<ILogger<App>>();
        logger?.LogError(e.Exception, "Unhandled UI thread exception");

        try
        {
            var crashService = Services.GetService<ICrashReportService>();
            var crashPath = crashService?.WriteCrashReport(e.Exception, "UIThread");

            var notifier = Services.GetService<IExceptionNotificationService>();
            var crashHint = string.IsNullOrWhiteSpace(crashPath)
                ? ""
                : $"\n\nCrash-Report: {crashPath}";

            notifier?.ShowError(
                "UI-Fehler",
                "Ein Fehler ist in der Benutzeroberfläche aufgetreten. "
                + "Details wurden ins Protokoll geschrieben."
                + crashHint);
        }
        catch (Exception notifyEx)
        {
            logger?.LogError(notifyEx, "Failed to show UI exception notification");
        }

        e.Handled = true;
    }
}
