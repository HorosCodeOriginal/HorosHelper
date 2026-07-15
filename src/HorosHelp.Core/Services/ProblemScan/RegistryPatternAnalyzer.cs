namespace HorosHelp.Core.Services.ProblemScan;

public enum RegistryIssueKind
{
    MissingExecutable,
    InvalidAppPath,
    SuspiciousRunLocation,
}

public sealed record RegistryIssue(
    RegistryIssueKind Kind,
    string KeyPath,
    string ValueName,
    string ValueData,
    string Message);

public static class RegistryPatternAnalyzer
{
    private static readonly string[] SuspiciousRunSubKeys =
    [
        @"Software\Microsoft\Windows\CurrentVersion\RunOnce",
        @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer\Run",
        @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run",
    ];

    public static IReadOnlyList<RegistryIssue> AnalyzeRunValue(
        string keyPath,
        string valueName,
        string? valueData)
    {
        var issues = new List<RegistryIssue>();
        if (string.IsNullOrWhiteSpace(valueData))
            return issues;

        var executable = ExtractExecutablePath(valueData);
        if (!string.IsNullOrWhiteSpace(executable) && !File.Exists(executable))
        {
            issues.Add(new RegistryIssue(
                RegistryIssueKind.MissingExecutable,
                keyPath,
                valueName,
                valueData,
                $"Autostart verweist auf fehlende Datei: {executable}"));
        }

        foreach (var suspicious in SuspiciousRunSubKeys)
        {
            if (keyPath.Contains(suspicious, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new RegistryIssue(
                    RegistryIssueKind.SuspiciousRunLocation,
                    keyPath,
                    valueName,
                    valueData,
                    $"Verdächtiger Autostart-Ort: {keyPath}"));
                break;
            }
        }

        return issues;
    }

    public static IReadOnlyList<RegistryIssue> AnalyzeAppPath(
        string keyPath,
        string valueName,
        string? valueData)
    {
        if (string.IsNullOrWhiteSpace(valueData))
            return [];

        if (!valueData.Contains('\\', StringComparison.Ordinal) && !valueData.Contains('/', StringComparison.Ordinal))
        {
            return
            [
                new RegistryIssue(
                    RegistryIssueKind.InvalidAppPath,
                    keyPath,
                    valueName,
                    valueData,
                    $"Ungültiger App-Pfad (kein Verzeichnis): {valueName}"),
            ];
        }

        var path = valueData.Trim().TrimEnd('\\', '/');
        if (!Directory.Exists(path) && !File.Exists(path))
        {
            return
            [
                new RegistryIssue(
                    RegistryIssueKind.InvalidAppPath,
                    keyPath,
                    valueName,
                    valueData,
                    $"App-Pfad zeigt auf fehlendes Ziel: {path}"),
            ];
        }

        return [];
    }

    public static string? ExtractExecutablePath(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return null;

        command = command.Trim();
        if (command.StartsWith('"'))
        {
            var end = command.IndexOf('"', 1);
            return end > 1 ? command[1..end] : null;
        }

        var space = command.IndexOf(' ');
        return space > 0 ? command[..space] : command;
    }

    public static bool IsReversibleIssue(RegistryIssue issue) =>
        issue.Kind is RegistryIssueKind.MissingExecutable or RegistryIssueKind.InvalidAppPath;
}
