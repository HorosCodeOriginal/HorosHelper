using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Security;

/// <summary>Stores secrets as DPAPI-protected blobs under %AppData%\HorosHelper\secrets\.</summary>
public sealed class DpapiSecretStore : ISecureSecretStore
{
    private readonly ILogger<DpapiSecretStore> _logger;
    private readonly string _secretsDirectory;

    public DpapiSecretStore(ILogger<DpapiSecretStore> logger)
        : this(logger, GetDefaultSecretsDirectory())
    {
    }

    public DpapiSecretStore(ILogger<DpapiSecretStore> logger, string secretsDirectory)
    {
        _logger = logger;
        _secretsDirectory = secretsDirectory;
    }

    public bool TryGetSecret(string key, out string? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(key))
            return false;

        var path = GetSecretPath(key);
        if (!File.Exists(path))
            return false;

        try
        {
            var protectedBytes = File.ReadAllBytes(path);
            if (!OperatingSystem.IsWindows())
            {
                _logger.LogWarning("DPAPI unavailable; cannot read secret {Key}.", key);
                return false;
            }

            var plain = ProtectedData.Unprotect(protectedBytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
            value = System.Text.Encoding.UTF8.GetString(plain);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read secret {Key}.", key);
            return false;
        }
    }

    public void SetSecret(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Secret key is required.", nameof(key));

        Directory.CreateDirectory(_secretsDirectory);
        var path = GetSecretPath(key);

        if (string.IsNullOrEmpty(value))
        {
            if (File.Exists(path))
                File.Delete(path);
            return;
        }

        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("DPAPI ist nur unter Windows verfügbar.");

        var plain = System.Text.Encoding.UTF8.GetBytes(value);
        var protectedBytes = ProtectedData.Protect(plain, optionalEntropy: null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(path, protectedBytes);
        _logger.LogInformation("Secret {Key} stored via DPAPI.", key);
    }

    public static string GetDefaultSecretsDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "HorosHelper", "secrets");
    }

    public static string CopilotApiKeySecretName => "copilot-api-key";

    private string GetSecretPath(string key)
    {
        var safeName = string.Concat(key.Select(c =>
            char.IsAsciiLetterOrDigit(c) || c is '-' or '_' ? c : '_'));
        return Path.Combine(_secretsDirectory, safeName + ".dpapi");
    }
}
