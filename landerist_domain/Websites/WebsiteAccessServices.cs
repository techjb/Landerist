namespace landerist_library.Websites;

public sealed record WebsiteAccessServices
{
    public IWebsiteRobotsPolicy Robots { get; }

    public IHttpClientTransportFactory HttpClients { get; }

    public WebsiteAccessServices(
        IWebsiteRobotsPolicy robots,
        IHttpClientTransportFactory httpClients)
    {
        ArgumentNullException.ThrowIfNull(robots);
        ArgumentNullException.ThrowIfNull(httpClients);
        Robots = robots;
        HttpClients = httpClients;
    }
}
