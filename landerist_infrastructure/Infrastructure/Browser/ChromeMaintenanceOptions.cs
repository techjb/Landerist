namespace landerist_library.Infrastructure.Browser;

public sealed record ChromeMaintenanceOptions(
    bool ProcessCleanupEnabled,
    bool UseTaskKillFallback);
