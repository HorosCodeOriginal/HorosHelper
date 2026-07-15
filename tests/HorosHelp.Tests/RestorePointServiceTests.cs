using HorosHelp.Core.Services.Admin;
using HorosHelp.Core.Services.Backup;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class RestorePointServiceTests
{
    [Fact]
    public void GetRestorePoints_ParsesPowerShellJson()
    {
        var ps = new MockPowerShellQuery
        {
            ScriptToReturn =
                """{"SequenceNumber":5,"CreationTime":"2026-07-15T10:00:00","RestorePointType":10,"Description":"Test"}""",
        };

        var service = new RestorePointService(ps, new MockAdminElevationService(), NullLogger<RestorePointService>.Instance);
        var points = service.GetRestorePoints();

        Assert.Single(points);
        Assert.Equal(5, points[0].SequenceNumber);
        Assert.Equal("Test", points[0].Description);
        Assert.Equal("Manuell", points[0].Type);
    }

    [Fact]
    public async Task CreateRestorePoint_RequiresAdmin()
    {
        var service = new RestorePointService(
            new MockPowerShellQuery(),
            new MockAdminElevationService { IsRunningAsAdmin = false },
            NullLogger<RestorePointService>.Instance);

        var result = await service.CreateRestorePointAsync();

        Assert.False(result.Success);
        Assert.Contains("Administratorrechte", result.Message);
    }

    [Fact]
    public async Task CreateRestorePoint_WithAdmin_SetsCreationMethod()
    {
        var ps = new MockPowerShellQuery { ScriptToReturn = "" };
        var service = new RestorePointService(
            ps,
            new MockAdminElevationService { IsRunningAsAdmin = true },
            NullLogger<RestorePointService>.Instance);

        await service.CreateRestorePointAsync();

        Assert.False(string.IsNullOrWhiteSpace(service.LastCreationMethod));
        Assert.True(
            service.LastCreationMethod.Contains("SRSetRestorePoint", StringComparison.Ordinal) ||
            service.LastCreationMethod.Contains("Checkpoint-Computer", StringComparison.Ordinal));
    }
}
