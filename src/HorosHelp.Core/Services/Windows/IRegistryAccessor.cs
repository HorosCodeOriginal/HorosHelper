namespace HorosHelp.Core.Services.Windows;

public interface IRegistryAccessor
{
    int? ReadDword(string hive, string subKey, string valueName);

    string? ReadString(string hive, string subKey, string valueName);

    bool WriteDword(string hive, string subKey, string valueName, int value);

    bool WriteString(string hive, string subKey, string valueName, string value);
}

public sealed class WindowsRegistryAccessor : IRegistryAccessor
{
    public int? ReadDword(string hive, string subKey, string valueName)
    {
        try
        {
            using var key = OpenKey(hive, subKey, writable: false);
            var value = key?.GetValue(valueName);
            return value switch
            {
                int i => i,
                byte[] bytes when bytes.Length >= 4 => BitConverter.ToInt32(bytes, 0),
                _ => null,
            };
        }
        catch
        {
            return null;
        }
    }

    public string? ReadString(string hive, string subKey, string valueName)
    {
        try
        {
            using var key = OpenKey(hive, subKey, writable: false);
            return key?.GetValue(valueName)?.ToString();
        }
        catch
        {
            return null;
        }
    }

    public bool WriteDword(string hive, string subKey, string valueName, int value)
    {
        try
        {
            using var key = OpenKey(hive, subKey, writable: true, create: true);
            if (key is null)
                return false;

            key.SetValue(valueName, value, Microsoft.Win32.RegistryValueKind.DWord);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool WriteString(string hive, string subKey, string valueName, string value)
    {
        try
        {
            using var key = OpenKey(hive, subKey, writable: true, create: true);
            if (key is null)
                return false;

            key.SetValue(valueName, value, Microsoft.Win32.RegistryValueKind.String);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static Microsoft.Win32.RegistryKey? OpenKey(
        string hive,
        string subKey,
        bool writable,
        bool create = false)
    {
        var root = hive.Equals("HKLM", StringComparison.OrdinalIgnoreCase)
            ? Microsoft.Win32.Registry.LocalMachine
            : Microsoft.Win32.Registry.CurrentUser;

        return create
            ? root.CreateSubKey(subKey, writable)
            : root.OpenSubKey(subKey, writable);
    }
}
