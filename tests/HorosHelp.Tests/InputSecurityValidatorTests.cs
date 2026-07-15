using HorosHelp.Core.Services.Security;

namespace HorosHelp.Tests;

public class InputSecurityValidatorTests
{
    [Theory]
    [InlineData(@"C:\Users\test\Documents", true)]
    [InlineData(@"D:\Backup\folder", true)]
    [InlineData("", false)]
    [InlineData(@"C:\test; rm -rf", false)]
    [InlineData(@"C:\test & del", false)]
    [InlineData(@"C:\test|cmd", false)]
    public void IsValidFilePath_ValidatesPaths(string path, bool expected)
    {
        var result = InputSecurityValidator.IsValidFilePath(path, out _);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("powershell", true)]
    [InlineData("schtasks", true)]
    [InlineData("cmd.exe", true)]
    [InlineData("evil & calc", false)]
    [InlineData("", false)]
    public void IsValidProcessFileName_ValidatesNames(string name, bool expected)
    {
        var result = InputSecurityValidator.IsValidProcessFileName(name, out _);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("HorosHelper Backup", true)]
    [InlineData("test; Remove-Item", false)]
    [InlineData("test`whoami", false)]
    [InlineData("test|Out-File", false)]
    public void IsValidPowerShellLiteral_BlocksInjection(string value, bool expected)
    {
        var result = InputSecurityValidator.IsValidPowerShellLiteral(value, out _);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("HorosHelper-Backup-documents", true)]
    [InlineData("invalid task name!", false)]
    public void IsValidTaskName_ValidatesTaskNames(string name, bool expected)
    {
        var result = InputSecurityValidator.IsValidTaskName(name, out _);
        Assert.Equal(expected, result);
    }
}
