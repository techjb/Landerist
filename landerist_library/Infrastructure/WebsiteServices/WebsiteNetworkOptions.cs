namespace landerist_library.Infrastructure.WebsiteServices;

public sealed record WebsiteNetworkOptions(
    string ProxyHost,
    int ProxyPort,
    bool RandomizeStickyPorts,
    int StickyPortMin,
    int StickyPortMax,
    string ProxyUsername,
    string ProxyPassword);
