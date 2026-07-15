using System.Text.Json;
using HorosHelp.Core.Models.Settings;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Settings;

public sealed class SettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsFilePath;
    private AppSettings _current = new();

    public SettingsService(ILogger<SettingsService> logger)
        : this(logger, GetDefaultSettingsPath())
    {
    }

    public SettingsService(ILogger<SettingsService> logger, string settingsFilePath)
    {
        _logger = logger;
        _settingsFilePath = settingsFilePath;
        _current = Load();
    }

    public AppSettings Current => _current;

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                _current = new AppSettings();
                _logger.LogInformation("No settings file at {Path}; using defaults.", _settingsFilePath);
                return _current;
            }

            var json = File.ReadAllText(_settingsFilePath);
            _current = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            _logger.LogInformation("Settings loaded from {Path}", _settingsFilePath);
            return _current;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load settings from {Path}; using defaults.", _settingsFilePath);
            _current = new AppSettings();
            return _current;
        }
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsFilePath, json);
            _current = settings;
            _logger.LogInformation("Settings saved to {Path}", _settingsFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings to {Path}", _settingsFilePath);
            throw;
        }
    }

    public static string GetDefaultSettingsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "HorosHelper", "settings.json");
    }

    public static string GetDefaultLogsDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appData, "HorosHelper", "logs");
    }
}
