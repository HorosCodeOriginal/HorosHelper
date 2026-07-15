using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using HorosHelp.Core.DependencyInjection;
using HorosHelp.Core.Services.Backup;
using HorosHelp.Core.Services.Logging;
using HorosHelp.Core.Services.Settings;
using HorosHelp.UI.DependencyInjection;
using HorosHelp.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HorosHelp.App;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var logsDirectory = SettingsService.GetDefaultLogsDirectory();
        Directory.CreateDirectory(logsDirectory);
        Directory.CreateDirectory(Path.Combine(logsDirectory, "crashes"));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(logsDirectory, "horoshelper-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14)
            .CreateLogger();

        try
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddSerilog(dispose: true);
                builder.SetMinimumLevel(LogLevel.Information);
            });
            services.AddHorosHelpCore();
            services.AddHorosHelpUi();
            App.Services = services.BuildServiceProvider();

            if (TryRunHeadlessBackup(args, App.Services))
                return;

            var globalHandler = App.Services.GetRequiredService<IGlobalExceptionHandler>();
            globalHandler.Register();
            globalHandler.UnhandledExceptionOccurred += OnUnhandledExceptionOccurred;

            var logger = App.Services.GetRequiredService<ILoggerFactory>().CreateLogger("HorosHelp.App");
            logger.LogInformation("HorosHelper starting");

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "HorosHelper terminated unexpectedly");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();

    private static bool TryRunHeadlessBackup(string[] args, IServiceProvider services)
    {
        if (args.Length < 2 || !string.Equals(args[0], "--backup-run", StringComparison.OrdinalIgnoreCase))
            return false;

        var profileId = args[1];
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("HorosHelp.App");
        logger.LogInformation("Headless backup run for profile {ProfileId}", profileId);

        var backupService = services.GetRequiredService<IBackupService>();
        var result = backupService.RunProfileBackupAsync(profileId).GetAwaiter().GetResult();

        if (result.Success)
            logger.LogInformation("Headless backup succeeded: {Message}", result.Message);
        else
            logger.LogWarning("Headless backup failed: {Message}", result.Message);

        Environment.Exit(result.Success ? 0 : 1);
        return true;
    }

    private static void OnUnhandledExceptionOccurred(object? sender, UnhandledExceptionNotification e)
    {
        Log.Fatal(e.Exception, "Unhandled exception from {Source}", e.Source);

        try
        {
            var notifier = App.Services?.GetService<IExceptionNotificationService>();
            var crashHint = string.IsNullOrWhiteSpace(e.CrashReportPath)
                ? ""
                : $"\n\nCrash-Report: {e.CrashReportPath}";

            notifier?.ShowError(
                "Unerwarteter Fehler",
                "HorosHelper ist auf ein unerwartetes Problem gestoßen. "
                + "Details wurden ins Protokoll geschrieben."
                + crashHint);
        }
        catch
        {
            // Last resort only — logging already happened.
        }
    }
}
