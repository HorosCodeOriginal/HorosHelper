using HorosHelp.Core.Models.ProblemScan;
using HorosHelp.Core.Services.ProblemScan;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class RollbackStoreTests : IDisposable
{
    private readonly string _root;
    private readonly RollbackStore _store;

    public RollbackStoreTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "HorosHelper-RollbackTests", Guid.NewGuid().ToString("N"));
        _store = new RollbackStore(NullLogger<RollbackStore>.Instance, _root);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // ignore cleanup errors in tests
        }
    }

    [Fact]
    public void SaveManifest_AndGetRecentEntries_RoundTrip()
    {
        var dir = _store.CreateSnapshotDirectory(ProblemKind.TempFiles, "Test snapshot");
        var id = Path.GetFileName(dir);

        _store.SaveManifest(new RollbackManifest
        {
            Id = id,
            RepairKind = ProblemKind.TempFiles,
            Description = "Test snapshot",
            Timestamp = DateTime.UtcNow,
            Items =
            [
                new RollbackManifestItem
                {
                    Kind = RollbackEntryKind.FileList,
                    RelativePath = "files.txt",
                    Metadata = "1 Datei",
                },
            ],
        });

        var entries = _store.GetRecentEntries(5);

        Assert.Contains(entries, e => e.Id == id && e.Description == "Test snapshot");
    }

    [Fact]
    public void RestoreRegistryValue_WritesValueBack()
    {
        var exportPath = Path.Combine(_root, "restore-test.txt");
        File.WriteAllText(exportPath, "test-value");

        var restored = RollbackStore.RestoreRegistryValue(
            exportPath,
            $"HKCU|Software\\HorosHelperTest\\{Guid.NewGuid():N}|TestValue");

        Assert.True(restored);
    }
}
