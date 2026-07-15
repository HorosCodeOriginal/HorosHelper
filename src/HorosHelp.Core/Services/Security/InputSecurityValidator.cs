using System.Text.RegularExpressions;

namespace HorosHelp.Core.Services.Security;

/// <summary>
/// Validates user-supplied paths and command fragments before Process.Start or PowerShell execution.
/// </summary>
public static partial class InputSecurityValidator
{
    private static readonly char[] PathInvalidChars =
        Path.GetInvalidPathChars().Concat(['<', '>', '|', '*', '?']).Distinct().ToArray();

    private static readonly string[] ShellInjectionPatterns =
    [
        "&", "|", ";", "`", "$", "(", ")", "{", "}", "\r", "\n",
        "&&", "||", ">>", "<<", "%", "!",
    ];

    public static bool IsValidFilePath(string? path, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(path))
        {
            error = "Pfad darf nicht leer sein.";
            return false;
        }

        var trimmed = path.Trim();

        if (trimmed.Length > 260)
        {
            error = "Pfad ist zu lang.";
            return false;
        }

        if (trimmed.IndexOfAny(PathInvalidChars) >= 0)
        {
            error = "Pfad enthält ungültige Zeichen.";
            return false;
        }

        if (ContainsShellInjection(trimmed))
        {
            error = "Pfad enthält potenziell gefährliche Shell-Zeichen.";
            return false;
        }

        if (trimmed.StartsWith("\\\\", StringComparison.Ordinal) && trimmed.Count(c => c == '\\') < 3)
        {
            error = "UNC-Pfad ist unvollständig.";
            return false;
        }

        return true;
    }

    public static bool IsValidProcessFileName(string? fileName, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(fileName))
        {
            error = "Prozessname darf nicht leer sein.";
            return false;
        }

        var trimmed = fileName.Trim();

        if (trimmed.Contains(' ') || trimmed.Contains('"'))
        {
            error = "Prozessname enthält ungültige Zeichen.";
            return false;
        }

        if (ContainsShellInjection(trimmed))
        {
            error = "Prozessname enthält potenziell gefährliche Shell-Zeichen.";
            return false;
        }

        var allowed = AllowedProcessNameRegex();
        if (!allowed.IsMatch(trimmed))
        {
            error = "Prozessname hat ein ungültiges Format.";
            return false;
        }

        return true;
    }

    public static bool IsValidPowerShellLiteral(string? value, out string error)
    {
        error = "";

        if (value is null)
        {
            error = "Wert darf nicht null sein.";
            return false;
        }

        if (value.Contains('"', StringComparison.Ordinal) ||
            value.Contains('\'', StringComparison.Ordinal) ||
            value.Contains('`', StringComparison.Ordinal) ||
            value.Contains('\r', StringComparison.Ordinal) ||
            value.Contains('\n', StringComparison.Ordinal))
        {
            error = "Wert enthält ungültige Zeichen für PowerShell-Literale.";
            return false;
        }

        foreach (var pattern in ShellInjectionPatterns)
        {
            if (value.Contains(pattern, StringComparison.Ordinal))
            {
                error = "Wert enthält potenziell gefährliche Shell-Zeichen.";
                return false;
            }
        }

        return true;
    }

    public static bool IsValidTaskName(string? taskName, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(taskName))
        {
            error = "Aufgabenname darf nicht leer sein.";
            return false;
        }

        var trimmed = taskName.Trim();

        if (trimmed.Length > 200)
        {
            error = "Aufgabenname ist zu lang.";
            return false;
        }

        if (!TaskNameRegex().IsMatch(trimmed))
        {
            error = "Aufgabenname enthält ungültige Zeichen.";
            return false;
        }

        return true;
    }

    private static bool ContainsShellInjection(string value)
    {
        foreach (var pattern in ShellInjectionPatterns)
        {
            if (value.Contains(pattern, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    [GeneratedRegex(@"^[A-Za-z0-9._\\:/-]+$")]
    private static partial Regex AllowedProcessNameRegex();

    [GeneratedRegex(@"^[A-Za-z0-9._\-]+$")]
    private static partial Regex TaskNameRegex();
}
