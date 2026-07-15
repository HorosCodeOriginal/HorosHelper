using HorosHelp.Core.Models.ProblemScan;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HorosHelp.Core.Services.ProblemScan;

public sealed class RegistryProblemCheck : IProblemCheck
{
    private readonly ILogger<RegistryProblemCheck> _logger;
    private readonly IRegistryReader _registryReader;

    public RegistryProblemCheck(
        ILogger<RegistryProblemCheck> logger,
        IRegistryReader? registryReader = null)
    {
        _logger = logger;
        _registryReader = registryReader ?? new WindowsRegistryReader();
    }

    public ProblemKind Kind => ProblemKind.Registry;

    public ProblemCheckResult Check()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
                return BuildResult([], usedMock: true);

            var issues = ScanRegistry();
            return BuildResult(issues, usedMock: false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Registry problem check failed.");
            return BuildResult([], usedMock: true);
        }
    }

    public static ProblemCheckResult BuildResult(IReadOnlyList<RegistryIssue> issues, bool usedMock)
    {
        if (issues.Count == 0)
        {
            return new ProblemCheckResult
            {
                Card = new ProblemCard
                {
                    Kind = ProblemKind.Registry,
                    Severity = ProblemSeverity.Good,
                    Title = "Registry optimiert",
                    Subtitle = usedMock
                        ? "Keine Probleme gefunden (eingeschränkter Zugriff)."
                        : "Keine Probleme gefunden.",
                    ProgressValue = 0,
                    IsRepairable = false,
                },
                Items = [],
                UsedMockData = usedMock,
            };
        }

        var maxSeverity = issues.Any(i => i.Kind == RegistryIssueKind.SuspiciousRunLocation)
            ? ProblemSeverity.Critical
            : ProblemSeverity.Warning;

        var repairable = issues.Any(RegistryPatternAnalyzer.IsReversibleIssue);

        return new ProblemCheckResult
        {
            Card = new ProblemCard
            {
                Kind = ProblemKind.Registry,
                Severity = maxSeverity,
                Title = "Registry-Probleme",
                Subtitle = $"{issues.Count} verdächtige oder defekte Einträge gefunden.",
                ProgressValue = maxSeverity == ProblemSeverity.Critical ? 1.0 : 0.72,
                IsRepairable = repairable,
            },
            Items = issues.Select(MapItem).ToList(),
            UsedMockData = usedMock,
        };
    }

    private List<RegistryIssue> ScanRegistry()
    {
        var issues = new List<RegistryIssue>();

        foreach (var (hive, subKey) in GetRunKeyPaths())
        {
            foreach (var (name, data) in _registryReader.ReadValues(hive, subKey))
            {
                issues.AddRange(RegistryPatternAnalyzer.AnalyzeRunValue($"{hive}\\{subKey}", name, data));
            }
        }

        foreach (var (hive, subKey) in GetAppPathsKeyPaths())
        {
            foreach (var (name, data) in _registryReader.ReadValues(hive, subKey))
            {
                issues.AddRange(RegistryPatternAnalyzer.AnalyzeAppPath($"{hive}\\{subKey}", name, data));
            }
        }

        return issues;
    }

    private static IEnumerable<(string Hive, string SubKey)> GetRunKeyPaths()
    {
        yield return ("HKCU", @"Software\Microsoft\Windows\CurrentVersion\Run");
        yield return ("HKLM", @"Software\Microsoft\Windows\CurrentVersion\Run");
        yield return ("HKCU", @"Software\Microsoft\Windows\CurrentVersion\RunOnce");
        yield return ("HKLM", @"Software\Microsoft\Windows\CurrentVersion\RunOnce");
    }

    private static IEnumerable<(string Hive, string SubKey)> GetAppPathsKeyPaths()
    {
        yield return ("HKLM", @"Software\Microsoft\Windows\CurrentVersion\App Paths");
    }

    private static ProblemItem MapItem(RegistryIssue issue) =>
        new()
        {
            Id = $"{issue.KeyPath}|{issue.ValueName}",
            Kind = ProblemKind.Registry,
            Severity = issue.Kind == RegistryIssueKind.SuspiciousRunLocation
                ? ProblemSeverity.Critical
                : ProblemSeverity.Warning,
            Title = issue.ValueName,
            Description = issue.Message,
            IsRepairable = RegistryPatternAnalyzer.IsReversibleIssue(issue),
            RepairHint = "Eintrag entfernen (mit Sicherung)",
        };
}

public interface IRegistryReader
{
    IEnumerable<(string Name, string? Data)> ReadValues(string hive, string subKey);
}

public sealed class WindowsRegistryReader : IRegistryReader
{
    public IEnumerable<(string Name, string? Data)> ReadValues(string hive, string subKey)
    {
        var root = hive.Equals("HKLM", StringComparison.OrdinalIgnoreCase)
            ? Registry.LocalMachine
            : Registry.CurrentUser;

        RegistryKey? key = null;
        try
        {
            key = root.OpenSubKey(subKey);
            if (key is null)
                yield break;

            foreach (var name in key.GetValueNames())
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                yield return (name, key.GetValue(name)?.ToString());
            }
        }
        finally
        {
            key?.Dispose();
        }
    }
}
