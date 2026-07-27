using System.Diagnostics;
using landerist_library.Application.Logging;

namespace landerist_library.Infrastructure.Browser;

public sealed class SystemChromeProcessController : IChromeProcessController
{
    private readonly IApplicationLogger _logger;

    public SystemChromeProcessController(IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }
    public void KillProcesses(string processName)
    {
        try
        {
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception exception)
                {
                    _logger.WriteError(
                        "SystemChromeProcessController KillProcess",
                        exception.ToString());
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch (Exception exception)
        {
            _logger.WriteError(
                "SystemChromeProcessController KillProcesses",
                        exception.ToString());
        }
    }

    public void ForceKillExecutable(string executableName)
    {
        try
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/F /IM \"{executableName}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit();
        }
        catch (Exception exception)
        {
            _logger.WriteError(
                "SystemChromeProcessController ForceKillExecutable",
                        exception.ToString());
        }
    }
}
