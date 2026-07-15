namespace HorosHelp.Core.Models.ProblemScan;

public enum RollbackEntryKind
{
    RegistryValue,
    FileList,
    ServiceState,
}

public sealed class RollbackEntry
{
    public required string Id { get; init; }
    public required string RepairId { get; init; }
    public ProblemKind RepairKind { get; init; }
    public RollbackEntryKind EntryKind { get; init; }
    public required string Description { get; init; }
    public DateTime Timestamp { get; init; }
    public required string SnapshotPath { get; init; }
}

public sealed class RollbackManifest
{
    public required string Id { get; init; }
    public ProblemKind RepairKind { get; init; }
    public required string Description { get; init; }
    public DateTime Timestamp { get; init; }
    public IReadOnlyList<RollbackManifestItem> Items { get; init; } = [];
}

public sealed class RollbackManifestItem
{
    public RollbackEntryKind Kind { get; init; }
    public required string RelativePath { get; init; }
    public string? Metadata { get; init; }
}
