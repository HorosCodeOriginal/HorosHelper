using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Logging;

public interface IGlobalExceptionHandler
{
    void Register();

    event EventHandler<UnhandledExceptionNotification>? UnhandledExceptionOccurred;
}

public sealed class UnhandledExceptionNotification
{
    public required Exception Exception { get; init; }
    public required string Source { get; init; }
    public string? CrashReportPath { get; init; }
}

public sealed class GlobalExceptionHandler : IGlobalExceptionHandler
{
    private readonly ICrashReportService _crashReportService;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private bool _registered;

    public GlobalExceptionHandler(
        ICrashReportService crashReportService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _crashReportService = crashReportService;
        _logger = logger;
    }

    public event EventHandler<UnhandledExceptionNotification>? UnhandledExceptionOccurred;

    public void Register()
    {
        if (_registered)
            return;

        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        _registered = true;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception ex)
            return;

        HandleException(ex, "AppDomain");
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        HandleException(e.Exception, "TaskScheduler");
        e.SetObserved();
    }

    private void HandleException(Exception exception, string source)
    {
        _logger.LogCritical(exception, "Unhandled exception from {Source}", source);
        var crashPath = _crashReportService.WriteCrashReport(exception, source);

        UnhandledExceptionOccurred?.Invoke(this, new UnhandledExceptionNotification
        {
            Exception = exception,
            Source = source,
            CrashReportPath = string.IsNullOrEmpty(crashPath) ? null : crashPath,
        });
    }
}
