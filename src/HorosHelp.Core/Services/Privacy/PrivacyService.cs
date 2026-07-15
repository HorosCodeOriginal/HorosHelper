using HorosHelp.Core.Models.Security;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Windows;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Privacy;

public sealed class PrivacyService : IPrivacyService
{
    private const string TelemetryPolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";
    private const string TelemetryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection";
    private const string CortanaSearchKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search";
    private const string CortanaPolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\Windows Search";
    private const string ConsentStoreKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore";

    private readonly IRegistryAccessor _registry;
    private readonly IAdminElevationService _adminElevationService;
    private readonly ILogger<PrivacyService> _logger;

    public PrivacyService(
        IRegistryAccessor registry,
        IAdminElevationService adminElevationService,
        ILogger<PrivacyService> logger)
    {
        _registry = registry;
        _adminElevationService = adminElevationService;
        _logger = logger;
    }

    public PrivacySnapshot GetSnapshot()
    {
        try
        {
            var isAdmin = _adminElevationService.IsRunningAsAdmin;

            return new PrivacySnapshot
            {
                Settings = BuildSettings(isAdmin),
                AppPermissions = BuildAppPermissions(),
                IsMockData = false,
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Privacy snapshot failed; returning mock snapshot.");
            return BuildMockSnapshot();
        }
    }

    public PrivacyWriteResult ApplySetting(string settingId, bool enabled)
    {
        try
        {
            return settingId switch
            {
                PrivacySettingIds.Telemetry => ApplyTelemetry(enabled),
                PrivacySettingIds.Cortana => ApplyCortana(enabled),
                PrivacySettingIds.BingSearch => ApplyBingSearch(enabled),
                PrivacySettingIds.Camera => ApplyAppPermission("webcam", enabled),
                PrivacySettingIds.Microphone => ApplyAppPermission("microphone", enabled),
                PrivacySettingIds.Location => ApplyAppPermission("location", enabled),
                _ => new PrivacyWriteResult
                {
                    Success = false,
                    Message = "Unbekannte Privatsphäre-Einstellung.",
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to apply privacy setting {SettingId}", settingId);
            return new PrivacyWriteResult { Success = false, Message = "Änderung fehlgeschlagen." };
        }
    }

    private IReadOnlyList<PrivacySettingInfo> BuildSettings(bool isAdmin)
    {
        var telemetryEnabled = IsTelemetryEnabled();
        var cortanaEnabled = IsCortanaEnabled();
        var bingSearchEnabled = IsBingSearchEnabled();

        return
        [
            new PrivacySettingInfo
            {
                Id = PrivacySettingIds.Telemetry,
                Name = "Windows-Telemetrie",
                Description = "Diagnosedaten an Microsoft senden (AllowTelemetry).",
                IsEnabled = telemetryEnabled,
                CanWrite = true,
                RequiresAdmin = true,
            },
            new PrivacySettingInfo
            {
                Id = PrivacySettingIds.Cortana,
                Name = "Cortana / Suchassistent",
                Description = "Sprachassistent und personalisierte Suche.",
                IsEnabled = cortanaEnabled,
                CanWrite = true,
                RequiresAdmin = false,
            },
            new PrivacySettingInfo
            {
                Id = PrivacySettingIds.BingSearch,
                Name = "Bing-Suche in Windows",
                Description = "Online-Suchergebnisse in der Taskleistensuche.",
                IsEnabled = bingSearchEnabled,
                CanWrite = true,
                RequiresAdmin = false,
            },
        ];
    }

    private IReadOnlyList<AppPermissionInfo> BuildAppPermissions()
    {
        return
        [
            ReadAppPermission("webcam", "Kamera"),
            ReadAppPermission("microphone", "Mikrofon"),
            ReadAppPermission("location", "Standort"),
        ];
    }

    private AppPermissionInfo ReadAppPermission(string kind, string displayName)
    {
        var value = _registry.ReadString("HKCU", $"{ConsentStoreKey}\\{kind}", "Value");
        var allowed = string.Equals(value, "Allow", StringComparison.OrdinalIgnoreCase);

        return new AppPermissionInfo
        {
            Kind = kind,
            Name = displayName,
            IsAllowed = allowed,
            Label = allowed ? "Erlaubt" : "Blockiert",
            CanWrite = true,
        };
    }

    private bool IsTelemetryEnabled()
    {
        var policy = _registry.ReadDword("HKLM", TelemetryPolicyKey, "AllowTelemetry");
        if (policy.HasValue)
            return policy.Value > 0;

        var value = _registry.ReadDword("HKLM", TelemetryKey, "AllowTelemetry");
        return !value.HasValue || value.Value > 0;
    }

    private bool IsCortanaEnabled()
    {
        var policy = _registry.ReadDword("HKLM", CortanaPolicyKey, "AllowCortana");
        if (policy.HasValue)
            return policy.Value != 0;

        var consent = _registry.ReadDword("HKCU", CortanaSearchKey, "CortanaConsent");
        return !consent.HasValue || consent.Value != 0;
    }

    private bool IsBingSearchEnabled()
    {
        var value = _registry.ReadDword("HKCU", CortanaSearchKey, "BingSearchEnabled");
        return !value.HasValue || value.Value != 0;
    }

    private PrivacyWriteResult ApplyTelemetry(bool enabled)
    {
        if (!_adminElevationService.IsRunningAsAdmin)
        {
            return new PrivacyWriteResult
            {
                Success = false,
                RequiresAdmin = true,
                Message = "Telemetrie-Einstellungen erfordern Administratorrechte.",
            };
        }

        var value = enabled ? 1 : 0;
        var policyOk = _registry.WriteDword("HKLM", TelemetryPolicyKey, "AllowTelemetry", value);
        var dataOk = _registry.WriteDword("HKLM", TelemetryKey, "AllowTelemetry", value);

        if (!policyOk && !dataOk)
            return new PrivacyWriteResult { Success = false, Message = "Registry-Schreibzugriff fehlgeschlagen." };

        return new PrivacyWriteResult
        {
            Success = true,
            Message = enabled ? "Telemetrie aktiviert." : "Telemetrie deaktiviert.",
        };
    }

    private PrivacyWriteResult ApplyCortana(bool enabled)
    {
        var value = enabled ? 1 : 0;
        var ok = _registry.WriteDword("HKCU", CortanaSearchKey, "CortanaConsent", value);
        if (!ok)
            return new PrivacyWriteResult { Success = false, Message = "Cortana-Einstellung konnte nicht gespeichert werden." };

        return new PrivacyWriteResult
        {
            Success = true,
            Message = enabled ? "Cortana aktiviert." : "Cortana deaktiviert.",
        };
    }

    private PrivacyWriteResult ApplyBingSearch(bool enabled)
    {
        var value = enabled ? 1 : 0;
        var ok = _registry.WriteDword("HKCU", CortanaSearchKey, "BingSearchEnabled", value);
        if (!ok)
            return new PrivacyWriteResult { Success = false, Message = "Bing-Suche konnte nicht geändert werden." };

        return new PrivacyWriteResult
        {
            Success = true,
            Message = enabled ? "Bing-Suche aktiviert." : "Bing-Suche deaktiviert.",
        };
    }

    private PrivacyWriteResult ApplyAppPermission(string kind, bool allowed)
    {
        var value = allowed ? "Allow" : "Deny";
        var ok = _registry.WriteString("HKCU", $"{ConsentStoreKey}\\{kind}", "Value", value);
        if (!ok)
            return new PrivacyWriteResult { Success = false, Message = "App-Berechtigung konnte nicht gespeichert werden." };

        return new PrivacyWriteResult
        {
            Success = true,
            Message = allowed ? "Zugriff erlaubt." : "Zugriff blockiert.",
        };
    }

    private static PrivacySnapshot BuildMockSnapshot() =>
        new()
        {
            Settings =
            [
                new PrivacySettingInfo
                {
                    Id = PrivacySettingIds.Telemetry,
                    Name = "Windows-Telemetrie",
                    Description = "Diagnosedaten an Microsoft senden.",
                    IsEnabled = false,
                    CanWrite = false,
                    RequiresAdmin = true,
                },
                new PrivacySettingInfo
                {
                    Id = PrivacySettingIds.Cortana,
                    Name = "Cortana / Suchassistent",
                    Description = "Sprachassistent und personalisierte Suche.",
                    IsEnabled = false,
                    CanWrite = false,
                    RequiresAdmin = false,
                },
                new PrivacySettingInfo
                {
                    Id = PrivacySettingIds.BingSearch,
                    Name = "Bing-Suche in Windows",
                    Description = "Online-Suchergebnisse in der Taskleistensuche.",
                    IsEnabled = true,
                    CanWrite = false,
                    RequiresAdmin = false,
                },
            ],
            AppPermissions =
            [
                new AppPermissionInfo { Kind = "webcam", Name = "Kamera", IsAllowed = true, Label = "Erlaubt", CanWrite = false },
                new AppPermissionInfo { Kind = "microphone", Name = "Mikrofon", IsAllowed = true, Label = "Erlaubt", CanWrite = false },
                new AppPermissionInfo { Kind = "location", Name = "Standort", IsAllowed = false, Label = "Blockiert", CanWrite = false },
            ],
            IsMockData = true,
        };
}

public static class PrivacySettingIds
{
    public const string Telemetry = "telemetry";
    public const string Cortana = "cortana";
    public const string BingSearch = "bing-search";
    public const string Camera = "camera";
    public const string Microphone = "microphone";
    public const string Location = "location";
}
