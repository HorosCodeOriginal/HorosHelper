using System.Text.Json;
using HorosHelp.Core.Models.Backup;

namespace HorosHelp.Core.Services.Backup;

public interface IBackupManifestStore
{
    string GetManifestPath(string destinationFolder, string profileName);
    BackupManifest? Load(string destinationFolder, string profileName);
    Task SaveAsync(BackupManifest manifest, string destinationFolder, string profileName, CancellationToken cancellationToken = default);
}

public sealed class BackupManifestStore : IBackupManifestStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly IBackupFileSystem _fileSystem;

    public BackupManifestStore(IBackupFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string GetManifestPath(string destinationFolder, string profileName) =>
        Path.Combine(destinationFolder, profileName, "manifest.json");

    public BackupManifest? Load(string destinationFolder, string profileName)
    {
        var path = GetManifestPath(destinationFolder, profileName);
        if (!_fileSystem.FileExists(path))
            return null;

        try
        {
            var json = _fileSystem.ReadAllText(path);
            return JsonSerializer.Deserialize<BackupManifest>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task SaveAsync(
        BackupManifest manifest,
        string destinationFolder,
        string profileName,
        CancellationToken cancellationToken = default)
    {
        var path = GetManifestPath(destinationFolder, profileName);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            _fileSystem.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        await _fileSystem.WriteAllTextAsync(path, json, cancellationToken);
    }
}
