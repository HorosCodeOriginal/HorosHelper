using System.Diagnostics;
using System.Security.Principal;

namespace HorosHelp.Core.Services.Admin;

public sealed class AdminElevationService : IAdminElevationService
{
    private bool _isRunningAsAdmin;

    public AdminElevationService()
    {
        RefreshAdminStatus();
    }

    public bool IsRunningAsAdmin => _isRunningAsAdmin;

    public event EventHandler? AdminStatusChanged;

    public void RefreshAdminStatus()
    {
        var isAdmin = ComputeIsRunningAsAdmin();
        if (isAdmin == _isRunningAsAdmin)
            return;

        _isRunningAsAdmin = isAdmin;
        AdminStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool RequestElevation(string? arguments = null)
    {
        if (!OperatingSystem.IsWindows())
            return false;

        if (IsRunningAsAdmin)
            return true;

        try
        {
            var exePath = Environment.ProcessPath
                          ?? Process.GetCurrentProcess().MainModule?.FileName;

            if (string.IsNullOrWhiteSpace(exePath))
                return false;

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = true,
                Verb = "runas",
            };

            Process.Start(psi);
            return true;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
    }

    internal static bool ComputeIsRunningAsAdmin()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}
