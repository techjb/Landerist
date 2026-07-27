using landerist_library.Infrastructure.Browser;

namespace landerist_unit_tests;

public sealed class ChromeMaintenanceServiceTests
{
    [Fact]
    public void KillChrome_WhenCleanupIsDisabled_DoesNothing()
    {
        RecordingProcesses processes = new();
        ChromeMaintenanceService service = CreateService(
            cleanupEnabled: false,
            taskKill: true,
            processes);

        service.KillChrome();

        Assert.Empty(processes.ProcessNames);
        Assert.Empty(processes.ExecutableNames);
    }

    [Fact]
    public void KillChrome_UsesConfiguredCleanupStrategies()
    {
        RecordingProcesses processes = new();
        ChromeMaintenanceService service = CreateService(
            cleanupEnabled: true,
            taskKill: true,
            processes);

        service.KillChrome();

        Assert.Equal(["chrome"], processes.ProcessNames);
        Assert.Equal(["chrome.exe"], processes.ExecutableNames);
    }

    [Fact]
    public void KillChrome_DoesNotUseTaskKillWhenFallbackIsDisabled()
    {
        RecordingProcesses processes = new();
        ChromeMaintenanceService service = CreateService(
            cleanupEnabled: true,
            taskKill: false,
            processes);

        service.KillChrome();

        Assert.Equal(["chrome"], processes.ProcessNames);
        Assert.Empty(processes.ExecutableNames);
    }

    [Fact]
    public void UpdateChrome_DelegatesToInstaller()
    {
        RecordingInstaller installer = new() { Result = true };
        ChromeMaintenanceService service = new(
            new ChromeMaintenanceOptions(false, false),
            new RecordingProcesses(),
            installer);

        bool result = service.UpdateChrome();

        Assert.True(result);
        Assert.Equal(1, installer.Calls);
    }

    private static ChromeMaintenanceService CreateService(
        bool cleanupEnabled,
        bool taskKill,
        RecordingProcesses processes) =>
        new(
            new ChromeMaintenanceOptions(cleanupEnabled, taskKill),
            processes,
            new RecordingInstaller());

    private sealed class RecordingProcesses : IChromeProcessController
    {
        public List<string> ProcessNames { get; } = [];
        public List<string> ExecutableNames { get; } = [];

        public void KillProcesses(string processName) =>
            ProcessNames.Add(processName);

        public void ForceKillExecutable(string executableName) =>
            ExecutableNames.Add(executableName);
    }

    private sealed class RecordingInstaller : IChromeBrowserInstaller
    {
        public bool Result { get; init; }
        public int Calls { get; private set; }

        public bool Update()
        {
            Calls++;
            return Result;
        }
    }
}
