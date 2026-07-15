using HorosHelp.Core.Models.Settings;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class SettingsServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _settingsPath;

    public SettingsServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "HorosHelper.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _settingsPath = Path.Combine(_tempDir, "settings.json");
    }

    [Fact]
    public void Load_ReturnsDefaults_WhenFileMissing()
    {
        var service = new SettingsService(NullLogger<SettingsService>.Instance, _settingsPath);
        var settings = service.Load();

        Assert.True(settings.OpenOnStartup);
        Assert.Equal("Dunkel", settings.Theme);
        Assert.Equal(2, settings.ScanIntervalSeconds);
    }

    [Fact]
    public void SaveAndLoad_PersistsSettings()
    {
        var service = new SettingsService(NullLogger<SettingsService>.Instance, _settingsPath);

        service.Save(new AppSettings
        {
            OpenOnStartup = false,
            StartMinimized = true,
            Theme = "Hell",
            Language = "English",
            ScanIntervalSeconds = 5,
            NotificationsEnabled = false,
        });

        var reloaded = new SettingsService(NullLogger<SettingsService>.Instance, _settingsPath).Load();

        Assert.False(reloaded.OpenOnStartup);
        Assert.True(reloaded.StartMinimized);
        Assert.Equal("Hell", reloaded.Theme);
        Assert.Equal("English", reloaded.Language);
        Assert.Equal(5, reloaded.ScanIntervalSeconds);
        Assert.False(reloaded.NotificationsEnabled);
    }

    [Fact]
    public void SaveAndLoad_PersistsHealthThresholdsAndFavorites()
    {
        var service = new SettingsService(NullLogger<SettingsService>.Instance, _settingsPath);

        service.Save(new AppSettings
        {
            HealthThresholds = new HealthThresholdSettings
            {
                CpuWarn = 75,
                RamWarn = 70,
                DiskWarn = 82,
            },
            FavoriteArticleIds = ["wlan-settings", "dns-settings"],
        });

        var reloaded = new SettingsService(NullLogger<SettingsService>.Instance, _settingsPath).Load();

        Assert.Equal(75, reloaded.HealthThresholds.CpuWarn);
        Assert.Equal(70, reloaded.HealthThresholds.RamWarn);
        Assert.Equal(82, reloaded.HealthThresholds.DiskWarn);
        Assert.Equal(2, reloaded.FavoriteArticleIds.Count);
        Assert.Contains("wlan-settings", reloaded.FavoriteArticleIds);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // best effort cleanup
        }
    }
}
