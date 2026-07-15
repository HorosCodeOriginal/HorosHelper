using System.Security.Cryptography;

namespace HorosHelp.Core.Services.Backup;

public interface IFileHashService
{
    string ComputeSha256(string filePath);
    string ComputeSha256(Stream stream);
}

public sealed class FileHashService : IFileHashService
{
    public string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return ComputeSha256(stream);
    }

    public string ComputeSha256(Stream stream)
    {
        var hash = SHA256.HashData(stream);
        return Convert.ToHexString(hash);
    }
}
