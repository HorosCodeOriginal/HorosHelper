using HorosHelp.Core.Models.Storage;

namespace HorosHelp.Core.Services.Storage;

public interface IStorageService
{
    StorageSnapshot GetSnapshot();

    Task<StorageCleanupResult> RunSafeCleanupAsync(CancellationToken cancellationToken = default);
}
