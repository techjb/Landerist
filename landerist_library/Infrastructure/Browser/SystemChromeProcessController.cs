using System.Diagnostics;

namespace landerist_library.Infrastructure.Browser;

public sealed class SystemChromeProcessController : IChromeProcessController
{
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
                    Logs.Log.WriteError(
                        "SystemChromeProcessController KillProcess",
                        exception);
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch (Exception exception)
        {
            Logs.Log.WriteError(
                "SystemChromeProcessController KillProcesses",
                exception);
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
            Logs.Log.WriteError(
                "SystemChromeProcessController ForceKillExecutable",
                exception);
        }
    }
}
