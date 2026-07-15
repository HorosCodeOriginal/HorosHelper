using HorosHelp.Core.Models.Security;

namespace HorosHelp.Core.Services.Privacy;

public interface IPrivacyService
{
    PrivacySnapshot GetSnapshot();

    PrivacyWriteResult ApplySetting(string settingId, bool enabled);
}
