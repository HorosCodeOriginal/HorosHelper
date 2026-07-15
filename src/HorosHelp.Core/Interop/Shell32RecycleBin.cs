using System.Runtime.InteropServices;

namespace HorosHelp.Core.Interop;

public static class Shell32RecycleBin
{
    private const uint SHERB_NOCONFIRMATION = 0x00000001;
    private const uint SHERB_NOPROGRESSUI = 0x00000002;
    private const uint SHERB_NOSOUND = 0x00000004;

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(
        IntPtr hwnd,
        string? pszRootPath,
        uint dwFlags);

    public static bool TryEmptyRecycleBin(string? driveRoot = null)
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            var result = SHEmptyRecycleBin(
                IntPtr.Zero,
                driveRoot,
                SHERB_NOCONFIRMATION | SHERB_NOPROGRESSUI | SHERB_NOSOUND);

            return result == 0;
        }
        catch
        {
            return false;
        }
    }
}
