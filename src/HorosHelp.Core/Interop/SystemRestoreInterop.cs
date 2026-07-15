using System.Runtime.InteropServices;
using System.Text;

namespace HorosHelp.Core.Interop;

/// <summary>
/// P/Invoke bindings for Windows System Restore API (srclient.dll).
/// Primary approach for restore point creation; falls back to Checkpoint-Computer PowerShell if unavailable.
/// </summary>
internal static class SystemRestoreInterop
{
    public const int BeginSystemChange = 100;
    public const int EndSystemChange = 101;
    public const int ModifySettings = 12;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct RestorePointInfo
    {
        public int EventType;
        public int RestorePointType;
        public int SequenceNumber;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Description;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StateMgrStatus
    {
        public int NStatus;
        public int LlSequenceNumber;
    }

    [DllImport("srclient.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern uint SRSetRestorePointW(ref RestorePointInfo restorePointInfo, out StateMgrStatus status);

    public static bool TryCreateRestorePoint(string description, out string error)
    {
        error = "";

        if (!OperatingSystem.IsWindows())
        {
            error = "Nur unter Windows verfügbar.";
            return false;
        }

        var info = new RestorePointInfo
        {
            EventType = BeginSystemChange,
            RestorePointType = ModifySettings,
            SequenceNumber = 0,
            Description = TruncateDescription(description),
        };

        var result = SRSetRestorePointW(ref info, out var status);
        if (result != 0)
        {
            error = $"SRSetRestorePoint fehlgeschlagen (Status {status.NStatus}).";
            return false;
        }

        info.EventType = EndSystemChange;
        info.SequenceNumber = status.LlSequenceNumber;
        result = SRSetRestorePointW(ref info, out status);
        if (result != 0)
        {
            error = $"SRSetRestorePoint (End) fehlgeschlagen (Status {status.NStatus}).";
            return false;
        }

        return true;
    }

    private static string TruncateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "HorosHelper";

        var trimmed = description.Trim();
        return trimmed.Length <= 64 ? trimmed : trimmed[..64];
    }
}
