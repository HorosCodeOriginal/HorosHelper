using HorosHelp.Core.Models.Storage;
using HorosHelp.Core.Services.Storage;

namespace HorosHelp.Tests;

public class SmartDiskServiceTests
{
    [Theory]
    [InlineData("OK", false, SmartHealthStatus.Ok)]
    [InlineData("Degraded", false, SmartHealthStatus.Warning)]
    [InlineData("Error", false, SmartHealthStatus.Warning)]
    [InlineData("Pred Fail", false, SmartHealthStatus.Warning)]
    [InlineData(null, true, SmartHealthStatus.Warning)]
    [InlineData("", null, SmartHealthStatus.Unknown)]
    public void MapWmiStatus_MapsExpectedHealth(string? status, bool? predictFailure, SmartHealthStatus expected)
    {
        var mapped = SmartDiskMapper.MapWmiStatus(status, predictFailure);
        Assert.Equal(expected, mapped);
    }

    [Theory]
    [InlineData(SmartHealthStatus.Ok, "OK")]
    [InlineData(SmartHealthStatus.Warning, "Warnung")]
    [InlineData(SmartHealthStatus.Unknown, "Unbekannt")]
    public void MapStatusLabel_ReturnsGermanLabels(SmartHealthStatus status, string expected)
    {
        Assert.Equal(expected, SmartDiskMapper.MapStatusLabel(status));
    }

    [Fact]
    public void GetDriveHealth_ReturnsAtLeastOneEntry_OnWindowsOrMock()
    {
        var service = new SmartDiskService(
            Microsoft.Extensions.Logging.Abstractions.NullLogger<SmartDiskService>.Instance);

        var health = service.GetDriveHealth();

        Assert.NotEmpty(health);
        Assert.All(health, h => Assert.False(string.IsNullOrWhiteSpace(h.StatusLabel)));
    }
}
