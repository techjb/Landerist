namespace landerist_library.Infrastructure.Browser;

public interface IChromeProcessController
{
    void KillProcesses(string processName);

    void ForceKillExecutable(string executableName);
}
