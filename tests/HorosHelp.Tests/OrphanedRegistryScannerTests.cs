using HorosHelp.Core.Services.Apps;

namespace HorosHelp.Tests;

public class OrphanedRegistryScannerTests
{
    [Fact]
    public void Scan_FindsMissingInstallLocations()
    {
        var entries = new[]
        {
            new RegistryUninstallEntry
            {
                RegistryPath = "HKLM:SOFTWARE\\...\\app1",
                DisplayName = "Gone App",
                InstallLocation = @"C:\Program Files\GoneApp",
            },
            new RegistryUninstallEntry
            {
                RegistryPath = "HKLM:SOFTWARE\\...\\app2",
                DisplayName = "Present App",
                InstallLocation = @"C:\Program Files\PresentApp",
            },
            new RegistryUninstallEntry
            {
                RegistryPath = "HKLM:SOFTWARE\\...\\app3",
                DisplayName = "No Location",
                InstallLocation = null,
            },
        };

        var orphans = OrphanedRegistryScanner.Scan(entries, path =>
            path.Contains("PresentApp", StringComparison.OrdinalIgnoreCase));

        Assert.Single(orphans);
        Assert.Equal("Gone App", orphans[0].DisplayName);
        Assert.Equal("Installationsordner existiert nicht mehr", orphans[0].Reason);
    }
}
