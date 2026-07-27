using landerist_library.Infrastructure.Http;

namespace landerist_unit_tests;

public sealed class HttpClientTransportFactoryTests
{
    [Fact]
    public void Create_AppliesExplicitTimeout()
    {
        HttpClientTransportFactory factory = CreateFactory();

        using HttpClient client = factory.Create(
            useProxy: false,
            TimeSpan.FromSeconds(7),
            allowAutoRedirect: false);

        Assert.Equal(TimeSpan.FromSeconds(7), client.Timeout);
    }

    [Fact]
    public void GetProxyPort_UsesInjectedRandomSourceWithinConfiguredRange()
    {
        int receivedMinimum = 0;
        int receivedMaximum = 0;
        HttpClientTransportFactory factory = new(
            new HttpTransportOptions(
                "proxy.example",
                8000,
                RandomizeStickyPorts: true,
                StickyPortMin: 8100,
                StickyPortMax: 8199,
                "user",
                "password"),
            (minimum, maximum) =>
            {
                receivedMinimum = minimum;
                receivedMaximum = maximum;
                return 8150;
            });

        int port = factory.GetProxyPort();

        Assert.Equal(8150, port);
        Assert.Equal(8100, receivedMinimum);
        Assert.Equal(8200, receivedMaximum);
    }

    [Fact]
    public void GetProxyPort_UsesFixedPortWhenRandomizationIsDisabled()
    {
        HttpClientTransportFactory factory = CreateFactory();

        Assert.Equal(8000, factory.GetProxyPort());
    }

    [Fact]
    public void Create_RejectsNonPositiveTimeout()
    {
        HttpClientTransportFactory factory = CreateFactory();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => factory.Create(false, TimeSpan.Zero));
    }

    private static HttpClientTransportFactory CreateFactory() =>
        new(new HttpTransportOptions(
            "proxy.example",
            8000,
            RandomizeStickyPorts: false,
            StickyPortMin: 8100,
            StickyPortMax: 8199,
            "user",
            "password"));
}
