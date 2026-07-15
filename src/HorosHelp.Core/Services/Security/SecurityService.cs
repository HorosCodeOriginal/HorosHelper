using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using HorosHelp.Core.Models.Security;
using HorosHelp.Core.Services.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HorosHelp.Core.Services.Security;

public sealed partial class SecurityService : ISecurityService
{
    private const string RealTimeProtectionKeyPath =
        @"Software\Microsoft\Windows Defender\Real-Time Protection";

    private readonly ILogger<SecurityService> _logger;
    private readonly IAdminElevationService _adminElevationService;
    private readonly IWindowsUpdateService _windowsUpdateService;

    public SecurityService(
        ILogger<SecurityService> logger,
        IAdminElevationService adminElevationService,
        IWindowsUpdateService windowsUpdateService)
    {
        _logger = logger;
        _adminElevationService = adminElevationService;
        _windowsUpdateService = windowsUpdateService;
    }

    public SecuritySnapshot GetSnapshot()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                _logger.LogWarning("Security APIs are Windows-only; returning mock snapshot.");
                return BuildMockSnapshot();
            }

            var isAdmin = _adminElevationService.IsRunningAsAdmin;
            var firewall = ReadFirewallStatus();
            var defender = ReadDefenderStatus();
            var windowsUpdate = _windowsUpdateService.GetStatus();
            var updatesCurrent = windowsUpdate.IsCurrent;
            var uacLevel = ReadUacLevel();
            var liveProtection = BuildLiveProtection(defender);
            var recentScan = SecurityScoreCalculator.HasRecentScan(defender.LastQuickScanTime);

            var score = SecurityScoreCalculator.Calculate(new SecurityScoreInput
            {
                FirewallEnabled = firewall.IsEnabled,
                DefenderActive = defender.IsActive,
                RealTimeProtectionEnabled = defender.RealTimeProtectionEnabled,
                SecurityUpdatesCurrent = updatesCurrent,
                RecentScan = recentScan,
            });

            return new SecuritySnapshot
            {
                SecurityScore = score,
                Firewall = firewall,
                Defender = defender,
                SecurityUpdatesCurrent = updatesCurrent,
                WindowsUpdate = windowsUpdate,
                UacLevel = uacLevel,
                LiveProtection = liveProtection,
                RealTimeProtectionToggleWritable = isAdmin,
                IsRunningAsAdmin = isAdmin,
                IsMockData = false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Security snapshot failed; returning mock snapshot.");
            return BuildMockSnapshot();
        }
    }

    public SecurityToggleResult SetRealTimeProtectionEnabled(bool enabled)
    {
        try
        {
            if (!OperatingSystem.IsWindows())
                return new SecurityToggleResult { Success = false, Message = "Nur unter Windows verfügbar." };

            if (!_adminElevationService.IsRunningAsAdmin)
            {
                return new SecurityToggleResult
                {
                    Success = false,
                    Message = "Echtzeitschutz kann ohne Administratorrechte nicht geändert werden.",
                };
            }

            using var key = Registry.LocalMachine.CreateSubKey(
                @"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection",
                writable: true);

            if (key is null)
                return new SecurityToggleResult { Success = false, Message = "Registry-Zugriff fehlgeschlagen." };

            key.SetValue("DisableRealtimeMonitoring", enabled ? 0 : 1, RegistryValueKind.DWord);

            return new SecurityToggleResult
            {
                Success = true,
                Message = enabled ? "Echtzeitschutz aktiviert." : "Echtzeitschutz deaktiviert.",
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to toggle real-time protection to {Enabled}", enabled);
            return new SecurityToggleResult { Success = false, Message = "Änderung fehlgeschlagen." };
        }
    }

    private static FirewallStatus ReadFirewallStatus()
    {
        try
        {
            var output = RunProcessSync("netsh", "advfirewall show allprofiles state");
            if (!string.IsNullOrWhiteSpace(output))
            {
                var onCount = FirewallOnRegex().Matches(output).Count;
                var offCount = FirewallOffRegex().Matches(output).Count;

                if (onCount > 0 && offCount == 0)
                    return new FirewallStatus { IsEnabled = true, Label = "Aktiv" };

                if (onCount > 0)
                    return new FirewallStatus { IsEnabled = true, Label = "Teilweise aktiv" };

                if (offCount > 0)
                    return new FirewallStatus { IsEnabled = false, Label = "Deaktiviert" };
            }
        }
        catch
        {
            // fall through to registry
        }

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile");

            var enabled = key?.GetValue("EnableFirewall");
            var isOn = enabled is int i && i != 0;
            return new FirewallStatus
            {
                IsEnabled = isOn,
                Label = isOn ? "Aktiv" : "Deaktiviert",
            };
        }
        catch
        {
            return new FirewallStatus { IsEnabled = true, Label = "Unbekannt" };
        }
    }

    private DefenderStatus ReadDefenderStatus()
    {
        var defender = TryReadDefenderViaWmi();
        if (defender is not null)
            return defender;

        return TryReadDefenderViaPowerShell() ?? new DefenderStatus
        {
            IsActive = true,
            RealTimeProtectionEnabled = ReadRealTimeProtectionFromRegistry(),
            ProductName = "Windows Defender",
        };
    }

    private DefenderStatus? TryReadDefenderViaWmi()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\SecurityCenter2",
                "SELECT displayName, productState FROM AntiVirusProduct");

            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                var name = obj["displayName"]?.ToString() ?? "Windows Defender";
                if (!name.Contains("Defender", StringComparison.OrdinalIgnoreCase)
                    && !name.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                    continue;

                var state = Convert.ToUInt32(obj["productState"] ?? 0u);
                var rtpEnabled = (state & 0x1000) != 0 || ReadRealTimeProtectionFromRegistry();

                return new DefenderStatus
                {
                    IsActive = state != 0,
                    RealTimeProtectionEnabled = rtpEnabled,
                    ProductName = name,
                    LastQuickScanTime = null,
                    ThreatsBlocked = 0,
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "WMI SecurityCenter2 read failed.");
        }

        return null;
    }

    private DefenderStatus? TryReadDefenderViaPowerShell()
    {
        try
        {
            var output = RunProcessSync(
                "powershell",
                "-NoProfile -Command \"Get-MpComputerStatus | Select-Object AMServiceEnabled,RealTimeProtectionEnabled,QuickScanEndTime | ConvertTo-Json -Compress\"");

            if (string.IsNullOrWhiteSpace(output))
                return null;

            var serviceEnabled = output.Contains("\"AMServiceEnabled\":true", StringComparison.OrdinalIgnoreCase);
            var rtp = output.Contains("\"RealTimeProtectionEnabled\":true", StringComparison.OrdinalIgnoreCase)
                      || ReadRealTimeProtectionFromRegistry();

            DateTimeOffset? scanTime = null;
            var scanMatch = QuickScanRegex().Match(output);
            if (scanMatch.Success
                && DateTimeOffset.TryParse(scanMatch.Groups[1].Value, out var parsed))
                scanTime = parsed;

            return new DefenderStatus
            {
                IsActive = serviceEnabled,
                RealTimeProtectionEnabled = rtp,
                LastQuickScanTime = scanTime,
                ThreatsBlocked = 0,
                ProductName = "Windows Defender",
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "PowerShell Defender status read failed.");
            return null;
        }
    }

    private static bool ReadRealTimeProtectionFromRegistry()
    {
        try
        {
            using var hkcu = Registry.CurrentUser.OpenSubKey(RealTimeProtectionKeyPath);
            var hkcuDisabled = hkcu?.GetValue("DisableRealtimeMonitoring");
            if (hkcuDisabled is int hkcuVal)
                return hkcuVal == 0;

            using var hklm = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection");

            var disabled = hklm?.GetValue("DisableRealtimeMonitoring");
            return disabled is not int val || val == 0;
        }
        catch
        {
            return true;
        }
    }

    private static UacLevelInfo ReadUacLevel()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");

            var enableLua = key?.GetValue("EnableLUA");
            var consent = key?.GetValue("ConsentPromptBehaviorAdmin");

            var luaEnabled = enableLua is not int luaVal || luaVal != 0;
            var consentBehavior = consent is int consentVal ? consentVal : 5;

            return new UacLevelInfo
            {
                IsEnabled = luaEnabled,
                ConsentPromptBehavior = consentBehavior,
                Label = GetUacLabel(luaEnabled, consentBehavior),
                Description = GetUacDescription(luaEnabled, consentBehavior),
            };
        }
        catch
        {
            return new UacLevelInfo
            {
                IsEnabled = true,
                ConsentPromptBehavior = 5,
                Label = "Standard",
                Description = "UAC-Status konnte nicht gelesen werden.",
            };
        }
    }

    private static string GetUacLabel(bool enabled, int consentBehavior)
    {
        if (!enabled)
            return "Deaktiviert";

        return consentBehavior switch
        {
            0 => "Nie benachrichtigen",
            1 => "Immer auffordern (Anmeldedaten)",
            2 => "Immer benachrichtigen",
            3 => "Standard mit sicherem Desktop",
            4 => "Standard ohne sicheren Desktop",
            5 => "Standard",
            _ => "Benutzerdefiniert",
        };
    }

    private static string GetUacDescription(bool enabled, int consentBehavior)
    {
        if (!enabled)
            return "Benutzerkontensteuerung ist deaktiviert — Sicherheitsrisiko.";

        return consentBehavior switch
        {
            0 => "Programme können ohne Aufforderung mit Administratorrechten starten.",
            1 or 2 => "Sie werden bei jeder Administratoraktion benachrichtigt.",
            _ => "Windows-Standard für Administratorgenehmigungen.",
        };
    }

    private static IReadOnlyList<LiveProtectionFeatureInfo> BuildLiveProtection(DefenderStatus defender)
    {
        var active = defender.RealTimeProtectionEnabled && defender.IsActive;

        return
        [
            new() { Name = "Dateischutz", IsActive = active },
            new() { Name = "Verhaltensüberwachung", IsActive = active },
            new() { Name = "Webschutz", IsActive = active },
            new() { Name = "E-Mail-Schutz", IsActive = active },
            new() { Name = "Exploit-Schutz", IsActive = active },
        ];
    }

    private static string RunProcessSync(string fileName, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
            return "";

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        return string.IsNullOrWhiteSpace(output) ? error : output;
    }

    private static SecuritySnapshot BuildMockSnapshot()
    {
        var defender = new DefenderStatus
        {
            IsActive = true,
            RealTimeProtectionEnabled = true,
            LastQuickScanTime = DateTimeOffset.Now.Date.AddHours(8).AddMinutes(47),
            ThreatsBlocked = 0,
            ProductName = "Windows Defender",
        };

        return new SecuritySnapshot
        {
            SecurityScore = 92,
            Firewall = new FirewallStatus { IsEnabled = true, Label = "Aktiv" },
            Defender = defender,
            SecurityUpdatesCurrent = true,
            WindowsUpdate = new WindowsUpdateStatus
            {
                PendingSecurityUpdates = 0,
                IsCurrent = true,
                Label = "Aktuell",
                Description = "Keine ausstehenden Sicherheitsupdates (Mock).",
            },
            UacLevel = new UacLevelInfo
            {
                IsEnabled = true,
                ConsentPromptBehavior = 5,
                Label = "Standard",
                Description = "Windows-Standard für Administratorgenehmigungen.",
            },
            LiveProtection = BuildLiveProtection(defender),
            RealTimeProtectionToggleWritable = false,
            IsRunningAsAdmin = false,
            IsMockData = true,
        };
    }

    [GeneratedRegex(@"ON\b", RegexOptions.IgnoreCase)]
    private static partial Regex FirewallOnRegex();

    [GeneratedRegex(@"OFF\b", RegexOptions.IgnoreCase)]
    private static partial Regex FirewallOffRegex();

    [GeneratedRegex(@"""QuickScanEndTime""\s*:\s*""([^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex QuickScanRegex();
}
