using System.Net;
using landerist_library.Parse.Location.Providers.Goolzoom;
using landerist_library.Websites;

namespace landerist_unit_tests;

public sealed class GoolzoomApiTests
{
    [Fact]
    public void Constructor_AppliesExplicitApiKeyAndTimeout()
    {
        RecordingTransportFactory transport = new(
            new SequenceHandler(HttpStatusCode.OK, """{"lat":40.4,"lng":-3.7}"""));

        _ = new GoolzoomApi(
            transport,
            new GoolzoomOptions(
                "  secret  ",
                TimeSpan.FromSeconds(4),
                MaxRetryAttempts: 2));

        Assert.Equal(TimeSpan.FromSeconds(4), transport.Timeout);
        Assert.Equal(
            "secret",
            Assert.Single(
                transport.Client!.DefaultRequestHeaders.GetValues("x-api-key")));
    }

    [Fact]
    public void GetLatLng_RetriesTransientResponseUsingInjectedDelay()
    {
        SequenceHandler handler = new(
            HttpStatusCode.InternalServerError,
            "{}",
            HttpStatusCode.OK,
            """{"lat":40.4,"lng":-3.7}""");
        RecordingTransportFactory transport = new(handler);
        List<TimeSpan> delays = [];
        GoolzoomApi api = new(
            transport,
            new GoolzoomOptions(null, TimeSpan.FromSeconds(3), 2),
            (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });

        GoolzoomLatLngResult? result =
            api.GetLatLng("9441515XM7094A0001FT");

        Assert.Equal(2, handler.Requests);
        Assert.Equal([TimeSpan.FromSeconds(1)], delays);
        Assert.Equal(40.4, result?.Latitude);
        Assert.Equal(-3.7, result?.Longitude);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Options_RejectInvalidValues(
        int timeoutSeconds,
        int retryAttempts)
    {
        GoolzoomOptions options = new(
            null,
            TimeSpan.FromSeconds(timeoutSeconds),
            retryAttempts);

        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
    }

    private sealed class RecordingTransportFactory(HttpMessageHandler handler)
        : IHttpClientTransportFactory
    {
        public HttpClient? Client { get; private set; }
        public TimeSpan Timeout { get; private set; }

        public HttpClient Create(
            bool useProxy,
            TimeSpan timeout,
            bool allowAutoRedirect = true)
        {
            Timeout = timeout;
            Client = new HttpClient(handler) { Timeout = timeout };
            return Client;
        }
    }

    private sealed class SequenceHandler(params object[] responses)
        : HttpMessageHandler
    {
        private int _index;

        public int Requests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests++;
            HttpStatusCode status = (HttpStatusCode)responses[_index++];
            string content = (string)responses[_index++];
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(content)
            });
        }
    }
}
