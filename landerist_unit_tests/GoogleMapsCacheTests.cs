using landerist_library.Infrastructure.Location.Providers.GoogleMaps;
using landerist_library.Application.Logging;
using System.Net;

namespace landerist_unit_tests;

public sealed class GoogleMapsCacheTests
{
    [Fact]
    public void CachedNegativeDoesNotCallGoogle()
    {
        var database = new RecordingDatabase { QueryExistsResult = true };
        var handler = new Handler("ZERO_RESULTS");
        var api = new GoogleMapsApi(database, "test", httpClient: new HttpClient(handler));
        Assert.Equal(GoogleMapsLatLngLookupStatus.NotFound, api.GetLatLngLookup("Madrid").Status);
        Assert.Equal(0, handler.Calls);
        Assert.Contains(database.Calls, call => call.Query.Contains("ExpiresAt > GETUTCDATE()"));
    }

    [Fact]
    public void NormalizesWhitespaceAndUnicodeAndPersistsNegative()
    {
        var database = new RecordingDatabase { QueryResult = true };
        var handler = new Handler("ZERO_RESULTS");
        var api = new GoogleMapsApi(database, "test", httpClient: new HttpClient(handler), negativeCacheLifetimeDays: 3);
        Assert.Equal(GoogleMapsLatLngLookupStatus.NotFound, api.GetLatLngLookup("  Calle\t Pen\u0303a  12, Madrid ").Status);
        Assert.Equal("Calle Pe\u00f1a 12, Madrid", database.LastParameters!["Address"]);
        Assert.Equal(3, database.LastParameters["Days"]);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public void GoogleErrorsAreNotCached()
    {
        var database = new RecordingDatabase();
        var api = new GoogleMapsApi(database, "test", httpClient: new HttpClient(new Handler("REQUEST_DENIED")));
        Assert.Equal(GoogleMapsLatLngLookupStatus.Error, api.GetLatLngLookup("Madrid").Status);
        Assert.DoesNotContain(database.Calls, call => call.Query.Contains("INSERT INTO"));
    }

    [Fact]
    public void FailedInsertionIsLoggedWithoutLosingGoogleResult()
    {
        var logger = new Logger();
        var database = new RecordingDatabase { QueryResult = false };
        var api = new GoogleMapsApi(database, "test", logger, new HttpClient(new Handler("ZERO_RESULTS")));
        Assert.Equal(GoogleMapsLatLngLookupStatus.NotFound, api.GetLatLngLookup("Madrid").Status);
        Assert.Contains(logger.Errors, message => message.Contains("insertion failed"));
    }

    [Fact]
    public void OversizedAddressDoesNotSpendGoogleRequest()
    {
        var handler = new Handler("ZERO_RESULTS");
        var api = new GoogleMapsApi(new RecordingDatabase(), "test", httpClient: new HttpClient(handler));
        Assert.Equal(GoogleMapsLatLngLookupStatus.Error, api.GetLatLngLookup(new string('a', 401)).Status);
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public void PositiveCacheTakesPrecedenceOverNegativeAndGoogle()
    {
        var database = new RecordingDatabase { QueryExistsResult = true };
        database.TableResult.Columns.Add("Lat", typeof(double));
        database.TableResult.Columns.Add("Lng", typeof(double));
        database.TableResult.Columns.Add("IsAccurate", typeof(bool));
        database.TableResult.Rows.Add(40.4, -3.7, true);
        var handler = new Handler("ZERO_RESULTS");
        var api = new GoogleMapsApi(database, "test", httpClient: new HttpClient(handler));
        var result = api.GetLatLngLookup("Madrid");
        Assert.Equal(GoogleMapsLatLngLookupStatus.Found, result.Status);
        Assert.Equal(40.4, result.Coordinates!.Value.Latitude);
        Assert.Equal(0, handler.Calls);
    }

    private sealed class Handler(string status) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"status\":\"" + status + "\",\"results\":[]}")
            });
        }
    }

    private sealed class Logger : IApplicationLogger
    {
        public List<string> Errors { get; } = [];
        public void WriteError(string source, string message) => Errors.Add(message);
        public void WriteInfo(string source, string message) { }
    }
}
