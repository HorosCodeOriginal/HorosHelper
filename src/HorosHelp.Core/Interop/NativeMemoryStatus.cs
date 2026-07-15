using System.Runtime.InteropServices;

namespace HorosHelp.Core.Interop;

internal static class NativeMemoryStatus
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);

    internal static bool TryGetMemoryLoad(out uint memoryLoadPercent, out ulong totalBytes, out ulong availableBytes)
    {
        memoryLoadPercent = 0;
        totalBytes = 0;
        availableBytes = 0;

        var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status))
            return false;

        memoryLoadPercent = status.MemoryLoad;
        totalBytes = status.TotalPhys;
        availableBytes = status.AvailPhys;
        return true;
    }
}
