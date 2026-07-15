using System.Globalization;
using HorosHelp.Core.Models.Security;
using HorosHelp.Core.Services.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HorosHelp.Core.Services.Security;

public sealed class WindowsUpdateService : IWindowsUpdateService
{
    private readonly IPowerShellQuery _powerShellQuery;
    private readonly ILogger<WindowsUpdateService> _logger;

    public WindowsUpdateService(IPowerShellQuery powerShellQuery, ILogger<WindowsUpdateService> logger)
    {
        _powerShellQuery = powerShellQuery;
        _logger = logger;
    }

    public WindowsUpdateStatus GetStatus()
    {
        if (!OperatingSystem.IsWindows())
            return BuildMockStatus();

        try
        {
            var pending = QueryPendingSecurityUpdates();
            var lastCheck = ReadLastCheckTime();
            var isCurrent = pending == 0;

            return new WindowsUpdateStatus
            {
                PendingSecurityUpdates = pending,
                LastCheckTime = lastCheck,
                IsCurrent = isCurrent,
                Label = isCurrent ? "Aktuell" : $"{pending} ausstehend",
                Description = BuildDescription(pending, lastCheck),
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Windows Update status query failed.");
            return BuildFallbackStatus();
        }
    }

    private int QueryPendingSecurityUpdates()
    {
        var script = """
            $session = New-Object -ComObject Microsoft.Update.Session
            $searcher = $session.CreateUpdateSearcher()
            $result = $searcher.Search("IsInstalled=0 and IsHidden=0 and Type='Software'")
            $count = 0
            foreach ($update in $result.Updates) {
              $isSecurity = $false
              foreach ($category in $update.Categories) {
                if ($category.Name -match 'Security|Kritisch|Critical|Important|Wichtig') { $isSecurity = $true; break }
              }
              if ($update.MsrcSeverity -in @('Critical','Important')) { $isSecurity = $true }
              if ($isSecurity) { $count++ }
            }
            [pscustomobject]@{ PendingSecurityUpdates = $count } | ConvertTo-Json -Compress
            """;

        var output = _powerShellQuery.Execute(script);
        return WindowsUpdateStatusParser.ParsePendingSecurityUpdates(output);
    }

    private static DateTimeOffset? ReadLastCheckTime()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\Results\Detect");

            var raw = key?.GetValue("LastSuccessTime")?.ToString();
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
                return new DateTimeOffset(dt);

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static string BuildDescription(int pending, DateTimeOffset? lastCheck)
    {
        var checkText = lastCheck.HasValue
            ? $"Letzte Prüfung: {lastCheck.Value.ToLocalTime():dd.MM.yyyy HH:mm} Uhr."
            : "Letzte Prüfung: unbekannt.";

        return pending == 0
            ? $"Keine ausstehenden Sicherheitsupdates. {checkText}"
            : $"{pending} Sicherheitsupdate(s) ausstehend. {checkText}";
    }

    private static WindowsUpdateStatus BuildFallbackStatus()
    {
        var lastCheck = ReadLastCheckTime();
        return new WindowsUpdateStatus
        {
            PendingSecurityUpdates = 0,
            LastCheckTime = lastCheck,
            IsCurrent = true,
            Label = "Unbekannt",
            Description = "Windows-Update-Status konnte nicht ermittelt werden.",
        };
    }

    private static WindowsUpdateStatus BuildMockStatus() =>
        new()
        {
            PendingSecurityUpdates = 0,
            LastCheckTime = DateTimeOffset.Now.AddHours(-3),
            IsCurrent = true,
            Label = "Aktuell",
            Description = "Keine ausstehenden Sicherheitsupdates (Mock).",
        };
}
