using HorosHelp.Core.Models.Storage;

namespace HorosHelp.Core.Services.Storage;

public interface ISmartDiskService
{
    IReadOnlyList<DriveSmartInfo> GetDriveHealth();
}
