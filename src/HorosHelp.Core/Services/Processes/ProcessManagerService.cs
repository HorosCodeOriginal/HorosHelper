using System.Diagnostics;
using HorosHelp.Core.Models.Processes;
using HorosHelp.Core.Models.Startup;
using Microsoft.Extensions.Logging;

namespace HorosHelp.Core.Services.Processes;

public interface IProcessManagerService
{
    IReadOnlyList<ManagedProcessInfo> GetBackgroundProcesses();

    ProcessTerminateResult TryTerminateProcess(int processId);
}

public sealed class ProcessManagerService : IProcessManagerService
{
    private readonly ILogger<ProcessManagerService> _logger;
    private readonly Dictionary<int, (TimeSpan CpuTime, DateTime Timestamp)> _cpuSnapshots = new();
    private DateTime _lastCpuSnapshotUtc = DateTime.MinValue;

    public ProcessManagerService(ILogger<ProcessManagerService> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<ManagedProcessInfo> GetBackgroundProcesses()
    {
        try
        {
            if (!OperatingSystem.IsWindows())
                return BuildMockProcesses();

            var now = DateTime.UtcNow;
            var elapsedSeconds = _lastCpuSnapshotUtc == DateTime.MinValue
                ? 0
                : Math.Max(0.5, (now - _lastCpuSnapshotUtc).TotalSeconds);

            var processes = Process.GetProcesses()
                .Select(p => MapProcess(p, elapsedSeconds))
                .Where(p => p is not null)
                .Cast<ManagedProcessInfo>()
                .OrderByDescending(p => p.WorkingSetBytes)
                .ThenByDescending(p => p.CpuPercent)
                .Take(12)
                .ToList();

            _lastCpuSnapshotUtc = now;
            return processes;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Process listing failed; returning mock data.");
            return BuildMockProcesses();
        }
    }

    public ProcessTerminateResult TryTerminateProcess(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            var safety = ProcessClassifier.Classify(process.ProcessName);

            if (!ProcessClassifier.CanTerminate(safety))
            {
                return new ProcessTerminateResult
                {
                    Success = false,
                    Message = $"Systemprozess „{process.ProcessName}“ kann nicht beendet werden.",
                };
            }

            process.Kill(entireProcessTree: true);
            process.WaitForExit(3000);

            _logger.LogInformation("Terminated process {Name} (PID {Pid})", process.ProcessName, processId);
            return new ProcessTerminateResult
            {
                Success = true,
                Message = $"Prozess „{process.ProcessName}“ wurde beendet.",
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to terminate process {Pid}", processId);
            return new ProcessTerminateResult
            {
                Success = false,
                Message = "Prozess konnte nicht beendet werden.",
            };
        }
    }

    private ManagedProcessInfo? MapProcess(Process process, double elapsedSeconds)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(process.ProcessName)
                || process.Id == 0
                || process.WorkingSet64 < 5 * 1024 * 1024)
            {
                return null;
            }

            var safety = ProcessClassifier.Classify(process.ProcessName);
            return new ManagedProcessInfo
            {
                Name = $"{process.ProcessName}.exe",
                ProcessId = process.Id,
                CpuPercent = CalculateCpuPercent(process, elapsedSeconds),
                WorkingSetBytes = process.WorkingSet64,
                SafetyLevel = safety,
                SafetyLabel = ProcessClassifier.GetSafetyLabel(safety),
            };
        }
        catch
        {
            return null;
        }
        finally
        {
            process.Dispose();
        }
    }

    private double CalculateCpuPercent(Process process, double elapsedSeconds)
    {
        try
        {
            var cpuTime = process.TotalProcessorTime;
            if (!_cpuSnapshots.TryGetValue(process.Id, out var previous))
            {
                _cpuSnapshots[process.Id] = (cpuTime, DateTime.UtcNow);
                return 0;
            }

            var delta = (cpuTime - previous.CpuTime).TotalMilliseconds;
            var cpuPercent = delta / (elapsedSeconds * 1000 * Environment.ProcessorCount) * 100;
            _cpuSnapshots[process.Id] = (cpuTime, DateTime.UtcNow);

            return Math.Clamp(Math.Round(cpuPercent, 0, MidpointRounding.AwayFromZero), 0, 100);
        }
        catch
        {
            return 0;
        }
    }

    private static IReadOnlyList<ManagedProcessInfo> BuildMockProcesses() =>
    [
        new()
        {
            Name = "Chrome.exe",
            ProcessId = 1,
            CpuPercent = 12,
            WorkingSetBytes = 512L * 1024 * 1024,
            SafetyLevel = ProcessSafetyLevel.Safe,
            SafetyLabel = "Sicher",
        },
        new()
        {
            Name = "Teams.exe",
            ProcessId = 2,
            CpuPercent = 8,
            WorkingSetBytes = 342L * 1024 * 1024,
            SafetyLevel = ProcessSafetyLevel.Safe,
            SafetyLabel = "Sicher",
        },
        new()
        {
            Name = "svchost.exe",
            ProcessId = 3,
            CpuPercent = 2,
            WorkingSetBytes = 128L * 1024 * 1024,
            SafetyLevel = ProcessSafetyLevel.System,
            SafetyLabel = "System",
        },
    ];
}
