namespace HorosHelp.Core.Services.Security;

/// <summary>DPAPI-backed secret storage for API keys and similar credentials.</summary>
public interface ISecureSecretStore
{
    bool TryGetSecret(string key, out string? value);

    void SetSecret(string key, string? value);
}
