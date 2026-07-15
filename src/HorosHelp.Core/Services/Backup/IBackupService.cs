using HorosHelp.Core.Models.Backup;

namespace HorosHelp.Core.Services.Backup;

public interface IBackupService
{
    BackupSnapshot GetSnapshot();

    Task<BackupOperationResult> CreateRestorePointAsync(CancellationToken cancellationToken = default);

    Task<BackupOperationResult> RunProfileBackupAsync(
        string profileId,
        CancellationToken cancellationToken = default);

    bool RegisterProfileSchedule(string profileId, string executablePath);

    bool UnregisterProfileSchedule(string profileId);

    string GetRestorePointCreationMethod();
}
