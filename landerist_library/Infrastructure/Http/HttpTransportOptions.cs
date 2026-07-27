namespace landerist_library.Infrastructure.Http;

public sealed record HttpTransportOptions(
    string ProxyHost,
    int ProxyPort,
    bool RandomizeStickyPorts,
    int StickyPortMin,
    int StickyPortMax,
    string ProxyUsername,
    string ProxyPassword);
