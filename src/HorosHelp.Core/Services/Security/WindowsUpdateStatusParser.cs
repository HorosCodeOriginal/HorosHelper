using System.Text.Json;

namespace HorosHelp.Core.Services.Security;

public static class WindowsUpdateStatusParser
{
    public static int ParsePendingSecurityUpdates(string? output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return 0;

        try
        {
            using var doc = JsonDocument.Parse(output.Trim());
            if (doc.RootElement.TryGetProperty("PendingSecurityUpdates", out var prop)
                && prop.TryGetInt32(out var count))
                return Math.Max(0, count);
        }
        catch
        {
            return 0;
        }

        return 0;
    }
}
