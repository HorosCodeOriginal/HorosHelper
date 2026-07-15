using System.Text.Json;
using HorosHelp.Core.Services.Logging;
using HorosHelp.Core.Services.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace HorosHelp.Tests;

public class LogViewerServiceTests
{
    [Fact]
    public void ReadTail_ReturnsLastLines_FromTempLogDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "horoshelper-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var logPath = Path.Combine(tempDir, "test.log");
            File.WriteAllLines(logPath, Enumerable.Range(1, 10).Select(i => $"line-{i}"));

            var service = new LogViewerService(tempDir);
            var tail = service.ReadTail("test.log", 3);

            var lines = tail.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(3, lines.Length);
            Assert.Equal("line-8", lines[0]);
            Assert.Equal("line-9", lines[1]);
            Assert.Equal("line-10", lines[2]);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void GetRecentLogFiles_OrdersByLastModifiedDescending()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "horoshelper-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var older = Path.Combine(tempDir, "older.log");
            var newer = Path.Combine(tempDir, "newer.log");
            File.WriteAllText(older, "old");
            File.WriteAllText(newer, "new");
            File.SetLastWriteTimeUtc(older, DateTime.UtcNow.AddHours(-2));
            File.SetLastWriteTimeUtc(newer, DateTime.UtcNow);

            var service = new LogViewerService(tempDir);
            var files = service.GetRecentLogFiles();

            Assert.Equal(2, files.Count);
            Assert.Equal("newer.log", files[0].FileName);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void CrashReportService_WritesJsonReport()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "horoshelper-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = new CrashReportService(NullLogger<CrashReportService>.Instance, tempDir);
            var path = service.WriteCrashReport(new InvalidOperationException("boom"), "Test");

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.True(File.Exists(path));

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal("boom", doc.RootElement.GetProperty("message").GetString());
            Assert.Equal("Test", doc.RootElement.GetProperty("source").GetString());
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
