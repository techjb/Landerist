namespace landerist_library.Infrastructure.Browser;

public sealed class ChromeMaintenanceService
{
    private readonly ChromeMaintenanceOptions _options;
    private readonly IChromeProcessController _processes;
    private readonly IChromeBrowserInstaller _installer;

    public ChromeMaintenanceService(
        ChromeMaintenanceOptions options,
        IChromeProcessController processes,
        IChromeBrowserInstaller installer)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(processes);
        ArgumentNullException.ThrowIfNull(installer);
        _options = options;
        _processes = processes;
        _installer = installer;
    }

    public void KillChrome()
    {
        if (!_options.ProcessCleanupEnabled)
        {
            return;
        }

        _processes.KillProcesses("chrome");
        if (_options.UseTaskKillFallback)
        {
            _processes.ForceKillExecutable("chrome.exe");
        }
    }

    public bool UpdateChrome() => _installer.Update();
}
