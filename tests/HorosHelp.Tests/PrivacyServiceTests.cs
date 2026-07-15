using HorosHelp.Core.Models.Security;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Privacy;
using HorosHelp.Core.Services.Windows;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public sealed class MockRegistryAccessor : IRegistryAccessor
{
    private readonly Dictionary<string, int?> _dwords = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string?> _strings = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string Hive, string SubKey, string ValueName, int Value)> _writtenDwords = [];
    private readonly List<(string Hive, string SubKey, string ValueName, string Value)> _writtenStrings = [];

    public void SetDword(string hive, string subKey, string valueName, int? value) =>
        _dwords[Key(hive, subKey, valueName)] = value;

    public void SetString(string hive, string subKey, string valueName, string? value) =>
        _strings[Key(hive, subKey, valueName)] = value;

    public int? ReadDword(string hive, string subKey, string valueName) =>
        _dwords.TryGetValue(Key(hive, subKey, valueName), out var value) ? value : null;

    public string? ReadString(string hive, string subKey, string valueName) =>
        _strings.TryGetValue(Key(hive, subKey, valueName), out var value) ? value : null;

    public bool WriteDword(string hive, string subKey, string valueName, int value)
    {
        _dwords[Key(hive, subKey, valueName)] = value;
        _writtenDwords.Add((hive, subKey, valueName, value));
        return true;
    }

    public bool WriteString(string hive, string subKey, string valueName, string value)
    {
        _strings[Key(hive, subKey, valueName)] = value;
        _writtenStrings.Add((hive, subKey, valueName, value));
        return true;
    }

    public IReadOnlyList<(string Hive, string SubKey, string ValueName, int Value)> GetWrittenDwords() => _writtenDwords;

    public IReadOnlyList<(string Hive, string SubKey, string ValueName, string Value)> GetWrittenStrings() => _writtenStrings;

    private static string Key(string hive, string subKey, string valueName) =>
        $"{hive}|{subKey}|{valueName}";
}

public sealed class MockAdminElevationService : IAdminElevationService
{
    public bool IsRunningAsAdmin { get; set; }

    public event EventHandler? AdminStatusChanged;

    public void RefreshAdminStatus()
    {
    }

    public bool RequestElevation(string? arguments = null) => false;
}

public class PrivacyServiceTests
{
    [Fact]
    public void GetSnapshot_ReadsTelemetryAndPermissions_FromMockRegistry()
    {
        var registry = new MockRegistryAccessor();
        registry.SetDword("HKLM", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0);
        registry.SetDword("HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0);
        registry.SetDword("HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 1);
        registry.SetString("HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\webcam", "Value", "Deny");
        registry.SetString("HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\microphone", "Value", "Allow");
        registry.SetString("HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny");

        var service = new PrivacyService(registry, new MockAdminElevationService(), NullLogger<PrivacyService>.Instance);
        var snapshot = service.GetSnapshot();

        Assert.False(snapshot.Settings.First(s => s.Id == PrivacySettingIds.Telemetry).IsEnabled);
        Assert.False(snapshot.Settings.First(s => s.Id == PrivacySettingIds.Cortana).IsEnabled);
        Assert.True(snapshot.Settings.First(s => s.Id == PrivacySettingIds.BingSearch).IsEnabled);
        Assert.False(snapshot.AppPermissions.First(p => p.Kind == "webcam").IsAllowed);
        Assert.True(snapshot.AppPermissions.First(p => p.Kind == "microphone").IsAllowed);
    }

    [Fact]
    public void ApplySetting_Telemetry_RequiresAdmin()
    {
        var registry = new MockRegistryAccessor();
        var service = new PrivacyService(registry, new MockAdminElevationService { IsRunningAsAdmin = false }, NullLogger<PrivacyService>.Instance);

        var result = service.ApplySetting(PrivacySettingIds.Telemetry, true);

        Assert.False(result.Success);
        Assert.True(result.RequiresAdmin);
        Assert.Empty(registry.GetWrittenDwords());
    }

    [Fact]
    public void ApplySetting_Telemetry_WritesRegistry_WhenAdmin()
    {
        var registry = new MockRegistryAccessor();
        var service = new PrivacyService(registry, new MockAdminElevationService { IsRunningAsAdmin = true }, NullLogger<PrivacyService>.Instance);

        var result = service.ApplySetting(PrivacySettingIds.Telemetry, false);

        Assert.True(result.Success);
        Assert.Contains(registry.GetWrittenDwords(), w =>
            w.Hive == "HKLM"
            && w.SubKey.Contains("DataCollection", StringComparison.Ordinal)
            && w.ValueName == "AllowTelemetry"
            && w.Value == 0);
    }

    [Fact]
    public void ApplySetting_Camera_WritesUserConsent_WithoutAdmin()
    {
        var registry = new MockRegistryAccessor();
        var service = new PrivacyService(registry, new MockAdminElevationService(), NullLogger<PrivacyService>.Instance);

        var result = service.ApplySetting(PrivacySettingIds.Camera, true);

        Assert.True(result.Success);
        Assert.Contains(registry.GetWrittenStrings(), w =>
            w.SubKey.Contains("webcam", StringComparison.Ordinal)
            && w.Value == "Allow");
    }

    [Fact]
    public void ApplySetting_Cortana_WritesHkcuValue()
    {
        var registry = new MockRegistryAccessor();
        var service = new PrivacyService(registry, new MockAdminElevationService(), NullLogger<PrivacyService>.Instance);

        var result = service.ApplySetting(PrivacySettingIds.Cortana, false);

        Assert.True(result.Success);
        Assert.Equal(0, registry.ReadDword("HKCU", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent"));
    }
}
