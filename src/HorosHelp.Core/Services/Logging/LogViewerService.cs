using HorosHelp.Core.Services.Settings;

namespace HorosHelp.Core.Services.Logging;

public sealed class LogFileInfo
{
    public string FileName { get; init; } = "";
    public string FullPath { get; init; } = "";
    public long SizeBytes { get; init; }
    public DateTimeOffset LastModified { get; init; }
    public string SizeLabel { get; init; } = "";
}

public interface ILogViewerService
{
    string LogsDirectory { get; }

    IReadOnlyList<LogFileInfo> GetRecentLogFiles(int maxCount = 20);

    string ReadTail(string fileName, int lineCount = 200);
}

public sealed class LogViewerService : ILogViewerService
{
    private readonly string _logsDirectory;

    public LogViewerService()
        : this(SettingsService.GetDefaultLogsDirectory())
    {
    }

    public LogViewerService(string logsDirectory) =>
        _logsDirectory = logsDirectory;

    public string LogsDirectory => _logsDirectory;

    public IReadOnlyList<LogFileInfo> GetRecentLogFiles(int maxCount = 20)
    {
        if (!Directory.Exists(_logsDirectory))
            return [];

        return Directory.EnumerateFiles(_logsDirectory, "*.log", SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .Take(maxCount)
            .Select(f => new LogFileInfo
            {
                FileName = f.Name,
                FullPath = f.FullName,
                SizeBytes = f.Length,
                LastModified = f.LastWriteTimeUtc,
                SizeLabel = FormatSize(f.Length),
            })
            .ToList();
    }

    public string ReadTail(string fileName, int lineCount = 200)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "";

        var safeName = Path.GetFileName(fileName);
        var path = Path.Combine(_logsDirectory, safeName);

        if (!File.Exists(path))
            return "Protokolldatei nicht gefunden.";

        try
        {
            var lines = File.ReadLines(path).ToList();
            if (lines.Count <= lineCount)
                return string.Join(Environment.NewLine, lines);

            return string.Join(Environment.NewLine, lines.Skip(lines.Count - lineCount));
        }
        catch (Exception ex)
        {
            return $"Protokoll konnte nicht gelesen werden: {ex.Message}";
        }
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        var kb = bytes / 1024.0;
        if (kb < 1024)
            return $"{kb:0.#} KB";

        return $"{kb / 1024.0:0.#} MB";
    }
}
