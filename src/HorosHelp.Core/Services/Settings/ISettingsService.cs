using HorosHelp.Core.Models.Settings;

namespace HorosHelp.Core.Services.Settings;

public interface ISettingsService
{
    AppSettings Current { get; }

    AppSettings Load();

    void Save(AppSettings settings);
}
