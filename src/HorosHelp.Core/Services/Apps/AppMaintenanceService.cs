using System.Diagnostics;
using System.Management;
using HorosHelp.Core.Models.Apps;
using HorosHelp.Core.Services.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace HorosHelp.Core.Services.Apps;

public sealed class AppMaintenanceService : IAppMaintenanceService
{
    private static readonly string[] UninstallKeyPaths =
    [
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall",
    ];

    private readonly ILogger<AppMaintenanceService> _logger;
    private readonly Dictionary<string, InstalledAppInfo> _appIndex = new(StringComparer.OrdinalIgnoreCase);

    public AppMaintenanceService(ILogger<AppMaintenanceService> logger)
    {
        _logger = logger;
    }

    public AppMaintenanceSnapshot GetSnapshot()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
            {
                _logger.LogWarning("App maintenance APIs are Windows-only; returning mock snapshot.");
                return BuildMockSnapshot();
            }

            var apps = ReadInstalledApps();
            var drivers = ReadDrivers();

            _appIndex.Clear();
            foreach (var app in apps)
                _appIndex[app.Id] = app;

            return new AppMaintenanceSnapshot
            {
                Apps = apps,
                Drivers = drivers,
                IsMockData = false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "App maintenance snapshot failed; returning mock snapshot.");
            return BuildMockSnapshot();
        }
    }

    public AppUninstallResult StartUninstall(string appId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(appId))
                return new AppUninstallResult { Success = false, Message = "Keine Anwendung ausgewählt." };

            if (!_appIndex.TryGetValue(appId, out var app))
            {
                var snapshot = GetSnapshot();
                app = snapshot.Apps.FirstOrDefault(a => a.Id.Equals(appId, StringComparison.OrdinalIgnoreCase));
            }

            if (app is null)
                return new AppUninstallResult { Success = false, Message = "Anwendung nicht gefunden." };

            if (string.IsNullOrWhiteSpace(app.UninstallString))
                return new AppUninstallResult { Success = false, Message = "Kein Deinstallationsbefehl verfügbar." };

            if (!InstalledAppParser.TryStartUninstallProcess(app.UninstallString, out var error))
                return new AppUninstallResult { Success = false, Message = error };

            return new AppUninstallResult
            {
                Success = true,
                Message = $"Deinstallation von „{app.Name}“ gestartet.",
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Uninstall failed for app {AppId}", appId);
            return new AppUninstallResult { Success = false, Message = "Deinstallation konnte nicht gestartet werden." };
        }
    }

    public DriverActionResult OpenDriverManager()
    {
        try
        {
            if (!InputSecurityValidator.IsValidProcessFileName("devmgmt.msc", out _))
            {
                return new DriverActionResult
                {
                    Success = false,
                    Message = "Geräte-Manager konnte nicht geöffnet werden.",
                };
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "devmgmt.msc",
                UseShellExecute = true,
            });

            return new DriverActionResult
            {
                Success = true,
                Message = "Geräte-Manager wird geöffnet.",
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to open device manager.");
            return new DriverActionResult
            {
                Success = false,
                Message = "Geräte-Manager konnte nicht geöffnet werden.",
            };
        }
    }

    public IReadOnlyList<OrphanedRegistryEntry> ScanOrphanedRegistryEntries()
    {
        if (!OperatingSystem.IsWindows())
            return [];

        var entries = new List<RegistryUninstallEntry>();
        CollectUninstallEntries(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", "HKLM", entries);
        CollectUninstallEntries(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", "HKLM", entries);
        CollectUninstallEntries(Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", "HKCU", entries);

        return OrphanedRegistryScanner.Scan(entries, Directory.Exists);
    }

    public RegistryCleanupResult RemoveOrphanedRegistryEntry(string registryPath)
    {
        if (string.IsNullOrWhiteSpace(registryPath))
            return new RegistryCleanupResult { Success = false, Message = "Kein Registry-Pfad angegeben." };

        if (!OperatingSystem.IsWindows())
            return new RegistryCleanupResult { Success = false, Message = "Nur unter Windows verfügbar." };

        try
        {
            var parts = registryPath.Split(':', 2);
            if (parts.Length != 2)
                return new RegistryCleanupResult { Success = false, Message = "Ungültiger Registry-Pfad." };

            var root = parts[0] switch
            {
                "HKLM" => Registry.LocalMachine,
                "HKCU" => Registry.CurrentUser,
                _ => null,
            };

            if (root is null)
                return new RegistryCleanupResult { Success = false, Message = "Nur HKLM/HKCU werden unterstützt." };

            using var key = root.OpenSubKey(parts[1], writable: true);
            if (key is null)
                return new RegistryCleanupResult { Success = false, Message = "Registry-Schlüssel nicht gefunden." };

            var parentPath = parts[1][..parts[1].LastIndexOf('\\')];
            var subKeyName = parts[1][(parts[1].LastIndexOf('\\') + 1)..];

            using var parent = root.OpenSubKey(parentPath, writable: true);
            if (parent is null)
                return new RegistryCleanupResult { Success = false, Message = "Übergeordneter Schlüssel nicht gefunden." };

            parent.DeleteSubKeyTree(subKeyName, throwOnMissingSubKey: false);

            return new RegistryCleanupResult
            {
                Success = true,
                Message = "Verwaister Registry-Eintrag wurde entfernt.",
            };
        }
        catch (UnauthorizedAccessException)
        {
            return new RegistryCleanupResult
            {
                Success = false,
                Message = "Administratorrechte erforderlich (UAC).",
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove orphaned registry entry {Path}", registryPath);
            return new RegistryCleanupResult { Success = false, Message = "Registry-Bereinigung fehlgeschlagen." };
        }
    }

    private static void CollectUninstallEntries(
        RegistryKey root,
        string path,
        string prefix,
        List<RegistryUninstallEntry> entries)
    {
        try
        {
            using var uninstallKey = root.OpenSubKey(path);
            if (uninstallKey is null)
                return;

            foreach (var subKeyName in uninstallKey.GetSubKeyNames())
            {
                try
                {
                    using var subKey = uninstallKey.OpenSubKey(subKeyName);
                    if (subKey is null)
                        continue;

                    var displayName = subKey.GetValue("DisplayName")?.ToString();
                    if (string.IsNullOrWhiteSpace(displayName))
                        continue;

                    var installLocation = subKey.GetValue("InstallLocation")?.ToString();

                    entries.Add(new RegistryUninstallEntry
                    {
                        RegistryPath = $"{prefix}:{path}\\{subKeyName}",
                        DisplayName = displayName.Trim(),
                        InstallLocation = installLocation,
                    });
                }
                catch
                {
                    // Skip unreadable keys.
                }
            }
        }
        catch
        {
            // Skip unreadable paths.
        }
    }

    private List<InstalledAppInfo> ReadInstalledApps()
    {
        var apps = new Dictionary<string, InstalledAppInfo>(StringComparer.OrdinalIgnoreCase);

        CollectAppsFromRoot(Registry.LocalMachine, "HKLM", apps);
        CollectAppsFromRoot(Registry.CurrentUser, "HKCU", apps);

        return apps.Values
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void CollectAppsFromRoot(RegistryKey root, string sourcePrefix, Dictionary<string, InstalledAppInfo> apps)
    {
        foreach (var path in UninstallKeyPaths)
        {
            try
            {
                using var uninstallKey = root.OpenSubKey(path);
                if (uninstallKey is null)
                    continue;

                foreach (var subKeyName in uninstallKey.GetSubKeyNames())
                {
                    try
                    {
                        using var subKey = uninstallKey.OpenSubKey(subKeyName);
                        if (subKey is null)
                            continue;

                        var values = InstalledAppParser.ReadRegistryValues(subKey);
                        if (!InstalledAppParser.TryParseRegistryEntry(
                                $"{sourcePrefix}:{path}",
                                subKeyName,
                                values,
                                out var app)
                            || app is null)
                            continue;

                        apps[app.Name] = app;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to read uninstall key {SubKey}", subKeyName);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to read uninstall path {Path}", path);
            }
        }
    }

    private List<DriverInfo> ReadDrivers()
    {
        var drivers = new List<DriverInfo>();

        try
        {
            using var searcher = new ManagementObjectSearcher(
                "SELECT DeviceName, DriverVersion, DriverProviderName, DriverDate, Status FROM Win32_PnPSignedDriver");

            var index = 0;
            foreach (var obj in searcher.Get().Cast<ManagementObject>())
            {
                var name = obj["DeviceName"]?.ToString();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var version = obj["DriverVersion"]?.ToString() ?? "—";
                var provider = obj["DriverProviderName"]?.ToString() ?? "—";
                var status = obj["Status"]?.ToString() ?? "OK";
                var installed = FormatDriverDate(obj["DriverDate"]);

                drivers.Add(new DriverInfo
                {
                    Id = $"driver-{index++}",
                    Name = name,
                    Version = version,
                    Provider = provider,
                    Installed = installed,
                    Status = status,
                    IsOutdated = IsDriverOutdated(installed),
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "WMI driver read failed.");
        }

        if (drivers.Count == 0)
            return BuildMockSnapshot().Drivers.ToList();

        return drivers
            .GroupBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToList();
    }

    private static bool IsDriverOutdated(string installed)
    {
        if (installed == "—")
            return false;

        if (DateTime.TryParseExact(installed, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var dt))
            return dt < DateTime.Now.AddMonths(-18);

        return false;
    }

    private static string FormatDriverDate(object? raw)
    {
        if (raw is null)
            return "—";

        if (raw is string s && s.Length >= 8
            && DateTime.TryParseExact(s[..8], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var dt))
            return dt.ToString("dd.MM.yyyy");

        return raw.ToString() ?? "—";
    }

    private static AppMaintenanceSnapshot BuildMockSnapshot() =>
        new()
        {
            Apps =
            [
                new()
                {
                    Id = "mock-office",
                    Name = "Microsoft Office 365",
                    Version = "16.0.17425.20176",
                    EstimatedSizeKb = 2_200_000,
                    InstallDate = "12.03.2024",
                    Publisher = "Microsoft Corporation",
                    UninstallString = "msiexec /I {000}",
                },
                new()
                {
                    Id = "mock-chrome",
                    Name = "Google Chrome",
                    Version = "124.0.6367.91",
                    EstimatedSizeKb = 890_000,
                    InstallDate = "08.04.2024",
                    Publisher = "Google LLC",
                    UninstallString = "\"chrome.exe\" --uninstall",
                },
            ],
            Drivers =
            [
                new()
                {
                    Id = "mock-nvidia",
                    Name = "NVIDIA GeForce Driver",
                    Version = "552.22",
                    Provider = "NVIDIA Corporation",
                    Installed = "10.04.2024",
                    Status = "OK",
                },
                new()
                {
                    Id = "mock-realtek",
                    Name = "Realtek Audio",
                    Version = "6.0.9457.1",
                    Provider = "Realtek Semiconductor",
                    Installed = "18.01.2024",
                    Status = "OK",
                    IsOutdated = true,
                },
            ],
            IsMockData = true,
        };
}
