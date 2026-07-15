using HorosHelp.Core.Models.Backup;
using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Backup;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public sealed class MockBackupFileSystem : IBackupFileSystem
{
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);

    public void AddDirectory(string path) => _directories.Add(Normalize(path));

    public void AddFile(string path, byte[] content)
    {
        var normalized = Normalize(path);
        _files[normalized] = content;
        var dir = Path.GetDirectoryName(normalized);
        if (!string.IsNullOrWhiteSpace(dir))
            _directories.Add(dir);
    }

    public void UpdateFile(string path, byte[] content) => AddFile(path, content);

    public bool DirectoryExists(string path) => _directories.Contains(Normalize(path));

    public bool FileExists(string path) => _files.ContainsKey(Normalize(path));

    public IEnumerable<string> EnumerateFiles(string directory, string pattern, SearchOption searchOption)
    {
        var prefix = Normalize(directory);
        return _files.Keys
            .Where(f => f.StartsWith(prefix + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
                        || f.StartsWith(prefix + '/', StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void CreateDirectory(string path) => _directories.Add(Normalize(path));

    public void CopyFile(string source, string destination, bool overwrite)
    {
        var normalizedSource = Normalize(source);
        if (!_files.TryGetValue(normalizedSource, out var content))
            throw new FileNotFoundException(source);

        _files[Normalize(destination)] = content;
    }

    public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        _files[Normalize(path)] = System.Text.Encoding.UTF8.GetBytes(content);
        return Task.CompletedTask;
    }

    public string ReadAllText(string path) =>
        System.Text.Encoding.UTF8.GetString(_files[Normalize(path)]);

    public long GetFileLength(string path) => _files[Normalize(path)].Length;

    public DateTime GetLastWriteTimeUtc(string path) => DateTime.UtcNow;

    public Stream OpenRead(string path) => new MemoryStream(_files[Normalize(path)]);

    public Stream OpenWrite(string path) => new MemoryStream();

    public void DeleteFile(string path) => _files.Remove(Normalize(path));

    private static string Normalize(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
}

public sealed class MockFileHashService : IFileHashService
{
    private readonly Dictionary<string, string> _hashes = new(StringComparer.OrdinalIgnoreCase);

    public void SetHash(string path, string hash) => _hashes[Normalize(path)] = hash;

    public string ComputeSha256(string filePath) =>
        _hashes.TryGetValue(Normalize(filePath), out var hash) ? hash : "DEFAULT_HASH";

    public string ComputeSha256(Stream stream) => "STREAM_HASH";

    private static string Normalize(string path) =>
        path.Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
}

public sealed class MockBackupEncryptionService : IBackupEncryptionService
{
    public bool IsEncryptionAvailable => true;

    public Task EncryptFileAsync(string sourcePath, string encryptedPath, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DecryptFileAsync(string encryptedPath, string targetPath, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public byte[] ProtectKey(byte[] key) => key;

    public byte[] UnprotectKey(byte[] protectedKey) => protectedKey;
}

public class BackupServiceTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly MockBackupFileSystem _fileSystem;
    private readonly MockFileHashService _hashService;
    private readonly BackupService _service;

    public BackupServiceTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "HorosHelper-BackupTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);

        _fileSystem = new MockBackupFileSystem();
        var sourceDir = Path.Combine(_tempRoot, "source");
        var destDir = Path.Combine(_tempRoot, "dest");
        _fileSystem.AddDirectory(sourceDir);
        _fileSystem.AddDirectory(destDir);
        _fileSystem.AddFile(Path.Combine(sourceDir, "doc.txt"), "hello"u8.ToArray());
        _hashService = new MockFileHashService();
        _hashService.SetHash(Path.Combine(sourceDir, "doc.txt"), "HASH1");

        var profilesPath = Path.Combine(_tempRoot, "backup-profiles.json");
        var profileJson = """
            [{
              "Id": "test",
              "Name": "TestProfil",
              "SourceFolders": ["__SOURCE__"],
              "DestinationFolder": "__DEST__",
              "EncryptBackups": false,
              "Schedule": { "IsEnabled": false, "Frequency": "Manuell" }
            }]
            """.Replace("__SOURCE__", sourceDir.Replace("\\", "\\\\"))
               .Replace("__DEST__", destDir.Replace("\\", "\\\\"));

        _fileSystem.AddFile(profilesPath, System.Text.Encoding.UTF8.GetBytes(profileJson));

        _service = new BackupService(
            NullLogger<BackupService>.Instance,
            new MockAdminElevationService(),
            new MockRestorePointService(),
            _fileSystem,
            _hashService,
            new BackupManifestStore(_fileSystem),
            new MockBackupEncryptionService(),
            new MockBackupSchedulerService(),
            configDirectoryOverride: _tempRoot);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempRoot, recursive: true); } catch { /* best effort */ }
    }

    [Fact]
    public async Task RunProfileBackup_CopiesFiles_OnFirstRun()
    {
        var result = await _service.RunProfileBackupAsync("test");

        Assert.True(result.Success);
        Assert.Contains("1 Dateien kopiert", result.Message);
    }

    [Fact]
    public async Task RunProfileBackup_SkipsUnchangedFiles_OnSecondRun()
    {
        await _service.RunProfileBackupAsync("test");
        var result = await _service.RunProfileBackupAsync("test");

        Assert.True(result.Success);
        Assert.Contains("unverändert", result.Message);
    }

    [Fact]
    public async Task RunProfileBackup_CopiesOnlyChangedFiles()
    {
        await _service.RunProfileBackupAsync("test");
        _hashService.SetHash(Path.Combine(_tempRoot, "source", "doc.txt"), "HASH2");

        var result = await _service.RunProfileBackupAsync("test");

        Assert.True(result.Success);
        Assert.Contains("1 Dateien kopiert", result.Message);
        Assert.Contains("0 unverändert", result.Message);
    }

    [Fact]
    public async Task RunProfileBackup_RejectsShellInjectionInSource()
    {
        var injectionPath = Path.Combine(_tempRoot, "bad;cmd");
        var badJson = $$"""
            [{
              "Id": "bad",
              "Name": "Bad",
              "SourceFolders": ["{{injectionPath.Replace("\\", "\\\\")}}"],
              "DestinationFolder": "{{Path.Combine(_tempRoot, "dest").Replace("\\", "\\\\")}}",
              "EncryptBackups": false
            }]
            """;
        await _fileSystem.WriteAllTextAsync(Path.Combine(_tempRoot, "backup-profiles.json"), badJson);

        var result = await _service.RunProfileBackupAsync("bad");
        Assert.False(result.Success);
    }
}

internal sealed class MockRestorePointService : IRestorePointService
{
    public string LastCreationMethod { get; private set; } = "Mock";

    public IReadOnlyList<RestorePointInfo> GetRestorePoints() => [];

    public Task<BackupOperationResult> CreateRestorePointAsync(CancellationToken cancellationToken = default)
    {
        LastCreationMethod = "Mock";
        return Task.FromResult(new BackupOperationResult { Success = true, Message = "OK" });
    }
}

internal sealed class MockBackupSchedulerService : IBackupSchedulerService
{
    public bool RegisterProfileSchedule(BackupProfileConfig profile, string executablePath) => true;
    public bool UnregisterProfileSchedule(string profileId) => true;
    public bool IsScheduleRegistered(string profileId) => false;
    public IReadOnlyList<string> GetRegisteredProfileIds() => [];
    public string FormatScheduleLabel(BackupScheduleConfig? schedule) => schedule is { IsEnabled: true } ? "Geplant" : "Manuell";
}
