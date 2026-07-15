using HorosHelp.Core.Models.Copilot;

namespace HorosHelp.Core.Services.Copilot;

public interface ICopilotService
{
    CopilotSystemContext BuildContext();

    CopilotResponse GenerateResponse(string userMessage, CopilotSystemContext? context = null);
}
