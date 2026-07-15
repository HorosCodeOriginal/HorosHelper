namespace HorosHelp.Core.Models.Backup;

public sealed class RestorePointInfo
{
    public int SequenceNumber { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Type { get; init; } = "";
    public string Description { get; init; } = "";
}

public sealed class BackupProfileConfig
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public IReadOnlyList<string> SourceFolders { get; init; } = [];
    public string DestinationFolder { get; init; } = "";
    public DateTimeOffset? LastBackupUtc { get; init; }
    public long? LastBackupSizeBytes { get; init; }
    public string? LastBackupStatus { get; init; }
    public BackupScheduleConfig? Schedule { get; init; }
    public bool EncryptBackups { get; init; } = true;
}

public sealed class BackupSnapshot
{
    public IReadOnlyList<RestorePointInfo> RestorePoints { get; init; } = [];
    public IReadOnlyList<BackupProfileConfig> Profiles { get; init; } = [];
    public bool IsRunningAsAdmin { get; init; }
    public bool IsMockData { get; init; }
}

public sealed class BackupOperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = "";
    public long BytesCopied { get; init; }
}
