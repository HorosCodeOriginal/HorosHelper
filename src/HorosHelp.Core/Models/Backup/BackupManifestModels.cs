namespace HorosHelp.Core.Models.Backup;

public sealed class BackupScheduleConfig
{
    public bool IsEnabled { get; init; }
    public string Frequency { get; init; } = "Manuell";
    public string TimeOfDay { get; init; } = "02:00";
    public string? DayOfWeek { get; init; }
}

public sealed class BackupManifestEntry
{
    public string RelativePath { get; init; } = "";
    public string Sha256 { get; init; } = "";
    public long SizeBytes { get; init; }
    public DateTimeOffset LastModifiedUtc { get; init; }
    public bool IsEncrypted { get; init; }
}

public sealed class BackupManifest
{
    public string ProfileId { get; init; } = "";
    public string ProfileName { get; init; } = "";
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset LastRunUtc { get; init; }
    public bool EncryptionEnabled { get; init; }
    public IReadOnlyList<BackupManifestEntry> Files { get; init; } = [];
}
