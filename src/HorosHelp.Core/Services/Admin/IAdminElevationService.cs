namespace HorosHelp.Core.Services.Admin;

public interface IAdminElevationService
{
    bool IsRunningAsAdmin { get; }

    event EventHandler? AdminStatusChanged;

    void RefreshAdminStatus();

    /// <summary>
    /// Restarts the application with UAC elevation (runas). Returns false if elevation was cancelled or failed.
    /// </summary>
    bool RequestElevation(string? arguments = null);
}
