using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using HorosHelp.Core.DependencyInjection;
using HorosHelp.Core.Services.Settings;
using HorosHelp.UI.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HorosHelp.App;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        ConfigureGlobalExceptionHandlers();

        var logsDirectory = SettingsService.GetDefaultLogsDirectory();
        Directory.CreateDirectory(logsDirectory);

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

    private static void ConfigureGlobalExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                Log.Fatal(ex, "Unhandled AppDomain exception");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };
    }
}
