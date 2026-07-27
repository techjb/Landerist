using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class WebsiteAccessServicesTests
{
    [Fact]
    public void Constructor_PreservesExplicitCollaborators()
    {
        StubWebsiteRobotsPolicy robots = new();
        StubHttpClientTransportFactory httpClients = new();

        WebsiteAccessServices services = new(robots, httpClients);

        Assert.Same(robots, services.Robots);
        Assert.Same(httpClients, services.HttpClients);
    }

    [Fact]
    public void Constructor_RejectsMissingCollaborators()
    {
        StubWebsiteRobotsPolicy robots = new();
        StubHttpClientTransportFactory httpClients = new();

        Assert.Throws<ArgumentNullException>(
            () => new WebsiteAccessServices(null!, httpClients));
        Assert.Throws<ArgumentNullException>(
            () => new WebsiteAccessServices(robots, null!));
    }

    private sealed class StubHttpClientTransportFactory
        : IHttpClientTransportFactory
    {
        public HttpClient Create(
            bool useProxy,
            TimeSpan timeout,
            bool allowAutoRedirect = true) =>
            throw new NotSupportedException();
    }
}
