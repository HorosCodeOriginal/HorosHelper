namespace HorosHelp.Core.Models.Storage;

public sealed class DriveStorageInfo
{
    public required string Letter { get; init; }
    public required string Label { get; init; }
    public required string DriveType { get; init; }
    public required string FileSystem { get; init; }
    public long TotalBytes { get; init; }
    public long UsedBytes { get; init; }
    public long FreeBytes { get; init; }
    public double PercentUsed { get; init; }
    public bool IsReady { get; init; }
    public SmartHealthStatus SmartStatus { get; init; } = SmartHealthStatus.Unknown;
    public string SmartStatusLabel { get; init; } = "Unbekannt";
}

public sealed class StorageCategoryInfo
{
    public required string Name { get; init; }
    public long SizeBytes { get; init; }
    public double PercentOfUsed { get; init; }
}

public sealed class CleanupCandidateInfo
{
    public required string Name { get; init; }
    public required string CategoryId { get; init; }
    public long SizeBytes { get; init; }
    public double SharePercent { get; init; }
}

public sealed class StorageSnapshot
{
    public IReadOnlyList<DriveStorageInfo> Drives { get; init; } = [];
    public IReadOnlyList<StorageCategoryInfo> Categories { get; init; } = [];
    public IReadOnlyList<CleanupCandidateInfo> CleanupCandidates { get; init; } = [];
    public long TotalReclaimableBytes { get; init; }
    public double CleanupChartPercent { get; init; }
    public bool IsMockData { get; init; }
}

public sealed class StorageCleanupResult
{
    public long BytesFreed { get; init; }
    public int FilesDeleted { get; init; }
    public int Errors { get; init; }
    public IReadOnlyList<string> Messages { get; init; } = [];
}
