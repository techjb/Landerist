using landerist_library.Infrastructure.Logging;

namespace landerist_unit_tests;

public sealed class LanderistHealthWorkerTests
{
    [Fact]
    public void GetHeartbeatUri_WhenHealthy_UsesBasePingUrl()
    {
        Uri baseUri = new("https://hc-ping.com/check-id");

        Uri result = HealthchecksUriBuilder.GetHeartbeatUri(baseUri, healthy: true);

        Assert.Equal(baseUri, result);
    }

    [Fact]
    public void GetHeartbeatUri_WhenUnhealthy_AppendsFailBeforeQuery()
    {
        Uri baseUri = new("https://hc-ping.com/check-id?key=value");

        Uri result = HealthchecksUriBuilder.GetHeartbeatUri(baseUri, healthy: false);

        Assert.Equal("https://hc-ping.com/check-id/fail?key=value", result.AbsoluteUri);
    }
}
