using System.Diagnostics;
using HorosHelp.Core.Services.Security;

namespace HorosHelp.Core.Services.Windows;

public interface IPowerShellQuery
{
    string Execute(string script);
}

public sealed class PowerShellQuery : IPowerShellQuery
{
    public string Execute(string script)
    {
        if (!InputSecurityValidator.IsValidProcessFileName("powershell", out _))
            return "";

        var psi = new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{Escape(script)}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(psi);
        if (process is null)
            return "";

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit(TimeSpan.FromMinutes(2));

        return string.IsNullOrWhiteSpace(output) ? error : output;
    }

    private static string Escape(string script) =>
        script.Replace("\"", "\\\"", StringComparison.Ordinal);
}
