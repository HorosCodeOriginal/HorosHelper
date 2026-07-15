using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using HorosHelp.Core.Models.Apps;
using Microsoft.Win32;

namespace HorosHelp.Core.Services.Apps;

public static partial class InstalledAppParser
{
    public static bool TryParseRegistryEntry(
        string sourceKey,
        string subKeyName,
        IReadOnlyDictionary<string, object?> values,
        out InstalledAppInfo? app)
    {
        app = null;

        var displayName = GetString(values, "DisplayName");
        if (string.IsNullOrWhiteSpace(displayName))
            return false;

        if (IsSystemComponent(values))
            return false;

        var version = GetString(values, "DisplayVersion");
        var publisher = GetString(values, "Publisher");
        var uninstall = GetString(values, "UninstallString");
        var installDate = FormatInstallDate(GetString(values, "InstallDate"));
        var sizeKb = ParseEstimatedSizeKb(values);

        app = new InstalledAppInfo
        {
            Id = $"{sourceKey}\\{subKeyName}",
            Name = displayName.Trim(),
            Version = string.IsNullOrWhiteSpace(version) ? "—" : version.Trim(),
            Publisher = string.IsNullOrWhiteSpace(publisher) ? "—" : publisher.Trim(),
            InstallDate = installDate,
            EstimatedSizeKb = sizeKb,
            UninstallString = string.IsNullOrWhiteSpace(uninstall) ? null : uninstall.Trim(),
        };

        return true;
    }

    public static string FormatSize(long estimatedSizeKb)
    {
        if (estimatedSizeKb <= 0)
            return "—";

        var bytes = estimatedSizeKb * 1024d;
        if (bytes >= 1_073_741_824)
            return $"{bytes / 1_073_741_824:F1} GB".Replace('.', ',');

        if (bytes >= 1_048_576)
            return $"{bytes / 1_048_576:F0} MB";

        return $"{bytes / 1024:F0} KB";
    }

    public static string GetInitial(string name) =>
        string.IsNullOrWhiteSpace(name) ? "?" : name.Trim()[0].ToString().ToUpperInvariant();

    private static bool IsSystemComponent(IReadOnlyDictionary<string, object?> values)
    {
        if (GetDword(values, "SystemComponent") == 1)
            return true;

        if (GetDword(values, "ParentKeyName") is not null)
            return true;

        var name = GetString(values, "DisplayName");
        return string.IsNullOrWhiteSpace(GetString(values, "UninstallString"))
               && string.IsNullOrWhiteSpace(name);
    }

    private static long ParseEstimatedSizeKb(IReadOnlyDictionary<string, object?> values)
    {
        var raw = GetDword(values, "EstimatedSize");
        return raw ?? 0;
    }

    private static string FormatInstallDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return "—";

        if (raw.Length == 8
            && DateTime.TryParseExact(raw, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt.ToString("dd.MM.yyyy");

        return raw;
    }

    private static string GetString(IReadOnlyDictionary<string, object?> values, string key) =>
        values.TryGetValue(key, out var value) ? value?.ToString() ?? "" : "";

    private static int? GetDword(IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var value) || value is null)
            return null;

        return value switch
        {
            int i => i,
            long l => (int)l,
            string s when int.TryParse(s, out var parsed) => parsed,
            _ => null,
        };
    }

    public static IReadOnlyDictionary<string, object?> ReadRegistryValues(RegistryKey key)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in key.GetValueNames())
        {
            if (string.IsNullOrWhiteSpace(name))
                continue;

            values[name] = key.GetValue(name);
        }

        return values;
    }

    [GeneratedRegex(@"^msiexec(\.exe)?\s+/i\s*\{[0-9A-Fa-f-]+\}", RegexOptions.IgnoreCase)]
    public static partial Regex MsiUninstallRegex();

    public static bool TryStartUninstallProcess(string uninstallString, out string error)
    {
        error = "";

        var trimmed = uninstallString.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            error = "Leerer Deinstallationsbefehl.";
            return false;
        }

        string fileName;
        string arguments;

        if (trimmed.StartsWith("msiexec", StringComparison.OrdinalIgnoreCase))
        {
            fileName = "msiexec.exe";
            var idx = trimmed.IndexOf(' ');
            arguments = idx > 0 ? trimmed[(idx + 1)..].Replace("/I", "/X", StringComparison.OrdinalIgnoreCase) : "/X";
        }
        else if (trimmed.StartsWith('"'))
        {
            var end = trimmed.IndexOf('"', 1);
            if (end <= 1)
            {
                error = "Ungültiger Deinstallationsbefehl.";
                return false;
            }

            fileName = trimmed[1..end];
            arguments = trimmed[(end + 1)..].Trim();
        }
        else
        {
            var space = trimmed.IndexOf(' ');
            if (space > 0)
            {
                fileName = trimmed[..space];
                arguments = trimmed[(space + 1)..];
            }
            else
            {
                fileName = trimmed;
                arguments = "";
            }
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = true,
        });

        return true;
    }
}
