using System.Security.Cryptography;

namespace HorosHelp.Core.Services.Backup;

public interface IBackupEncryptionService
{
    bool IsEncryptionAvailable { get; }
    Task EncryptFileAsync(string sourcePath, string encryptedPath, CancellationToken cancellationToken = default);
    Task DecryptFileAsync(string encryptedPath, string targetPath, CancellationToken cancellationToken = default);
    byte[] ProtectKey(byte[] key);
    byte[] UnprotectKey(byte[] protectedKey);
}

/// <summary>
/// AES-256-CBC backup encryption with per-file IV. Master key protected via Windows DPAPI (CurrentUser scope).
/// </summary>
public sealed class BackupEncryptionService : IBackupEncryptionService
{
    private const int AesKeySizeBytes = 32;
    private const int IvSizeBytes = 16;
    private const string EncryptedExtension = ".horos.enc";

    public bool IsEncryptionAvailable => OperatingSystem.IsWindows();

    public static string GetEncryptedFileName(string relativePath) =>
        relativePath + EncryptedExtension;

    public async Task EncryptFileAsync(string sourcePath, string encryptedPath, CancellationToken cancellationToken = default)
    {
        var key = await GetOrCreateMasterKeyAsync(cancellationToken);
        var directory = Path.GetDirectoryName(encryptedPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var input = File.OpenRead(sourcePath);
        await using var output = File.Create(encryptedPath);

        var iv = RandomNumberGenerator.GetBytes(IvSizeBytes);
        await output.WriteAsync(iv, cancellationToken);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        await using var crypto = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await input.CopyToAsync(crypto, cancellationToken);
        await crypto.FlushFinalBlockAsync(cancellationToken);
    }

    public async Task DecryptFileAsync(string encryptedPath, string targetPath, CancellationToken cancellationToken = default)
    {
        var key = await GetOrCreateMasterKeyAsync(cancellationToken);
        var directory = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var input = File.OpenRead(encryptedPath);

        var iv = new byte[IvSizeBytes];
        var read = await input.ReadAsync(iv, cancellationToken);
        if (read != IvSizeBytes)
            throw new CryptographicException("Verschlüsselte Datei ist beschädigt (IV fehlt).");

        await using var output = File.Create(targetPath);

        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;
        aes.IV = iv;

        await using var crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        await crypto.CopyToAsync(output, cancellationToken);
    }

    public byte[] ProtectKey(byte[] key)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("DPAPI ist nur unter Windows verfügbar.");

        return ProtectedData.Protect(key, optionalEntropy: null, DataProtectionScope.CurrentUser);
    }

    public byte[] UnprotectKey(byte[] protectedKey)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("DPAPI ist nur unter Windows verfügbar.");

        return ProtectedData.Unprotect(protectedKey, optionalEntropy: null, DataProtectionScope.CurrentUser);
    }

    private static string MasterKeyPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HorosHelper",
            "backup-master-key.dpapi");

    private static async Task<byte[]> GetOrCreateMasterKeyAsync(CancellationToken cancellationToken)
    {
        var keyPath = MasterKeyPath;
        var directory = Path.GetDirectoryName(keyPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        if (File.Exists(keyPath))
        {
            var protectedKey = await File.ReadAllBytesAsync(keyPath, cancellationToken);
            return ProtectedData.Unprotect(protectedKey, optionalEntropy: null, DataProtectionScope.CurrentUser);
        }

        var key = RandomNumberGenerator.GetBytes(AesKeySizeBytes);
        var protectedBytes = ProtectedData.Protect(key, optionalEntropy: null, DataProtectionScope.CurrentUser);
        await File.WriteAllBytesAsync(keyPath, protectedBytes, cancellationToken);
        return key;
    }
}
