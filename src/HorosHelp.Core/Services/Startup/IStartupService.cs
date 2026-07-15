using HorosHelp.Core.Models.Startup;

namespace HorosHelp.Core.Services.Startup;

public interface IStartupService
{
    StartupSnapshot GetSnapshot();

    StartupToggleResult SetEntryEnabled(string entryId, bool enabled);
}
