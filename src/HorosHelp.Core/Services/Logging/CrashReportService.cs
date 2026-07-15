using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Logging;

public interface ICrashReportService
{
    string CrashesDirectory { get; }

    string WriteCrashReport(Exception exception, string source);
}

public sealed class CrashReportService : ICrashReportService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly ILogger<CrashReportService> _logger;
    private readonly string _crashesDirectory;

    public CrashReportService(ILogger<CrashReportService> logger)
        : this(logger, Path.Combine(SettingsService.GetDefaultLogsDirectory(), "crashes"))
    {
    }

    public CrashReportService(ILogger<CrashReportService> logger, string crashesDirectory)
    {
        _logger = logger;
        _crashesDirectory = crashesDirectory;
        Directory.CreateDirectory(_crashesDirectory);
    }

    public string CrashesDirectory => _crashesDirectory;

    public string WriteCrashReport(Exception exception, string source)
    {
        ArgumentNullException.ThrowIfNull(exception);

        Directory.CreateDirectory(_crashesDirectory);

        var timestamp = DateTimeOffset.Now;
        var fileName = $"crash-{timestamp:yyyyMMdd-HHmmss-fff}.json";
        var path = Path.Combine(_crashesDirectory, fileName);

        var report = new
        {
            timestamp = timestamp.ToString("O"),
            source,
            version = GetAppVersion(),
            os = RuntimeInformation.OSDescription,
            architecture = RuntimeInformation.OSArchitecture.ToString(),
            message = exception.Message,
            exceptionType = exception.GetType().FullName,
            stackTrace = exception.ToString(),
        };

        try
        {
            var json = JsonSerializer.Serialize(report, JsonOptions);
            File.WriteAllText(path, json);
            _logger.LogError(exception, "Crash report written to {Path}", path);
            return path;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write crash report.");
            return "";
        }
    }

    private static string GetAppVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        return assembly.GetName().Version?.ToString() ?? "unknown";
    }
}
