using HorosHelp.Core.Models.Security;

namespace HorosHelp.Core.Services.Security;

public interface ISecurityService
{
    SecuritySnapshot GetSnapshot();

    SecurityToggleResult SetRealTimeProtectionEnabled(bool enabled);
}
