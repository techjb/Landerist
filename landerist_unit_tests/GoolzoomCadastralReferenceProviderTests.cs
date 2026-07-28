using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Database;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Infrastructure.Statistics;

namespace landerist_unit_tests;

public sealed class GoolzoomCadastralReferenceProviderTests
{
    [Fact]
    public void GetCadastralReference_WhenCached_DoesNotCallExternalServices()
    {
        RecordingDatabase database = new()
        {
            QueryStringResult = "1234567AB1234C",
        };
        RecordingGoolzoom goolzoom = new();
        GoolzoomCadastralReferenceProvider provider = new(
            new AddressCadastralReference(database),
            new GlobalStatisticsRepository(database),
            goolzoom,
            new RejectingSelector(),
            new NullLogger());

        string? result = provider.GetCadastralReference(
            40.4,
            -3.7,
            "Calle Mayor 1");

        Assert.Equal("1234567AB1234C", result);
        Assert.Equal(0, goolzoom.AddressRequests);
        Assert.Contains(
            "[ADDRESS_CADASTRAL_REFERENCE]",
            database.LastQuery);
    }

    private sealed class RecordingGoolzoom : IGoolzoomClient
    {
        public int AddressRequests { get; private set; }

        public GoolzoomLatLngResult? GetLatLng(string cadastralReference) =>
            null;

        public string? GetAddress(string cadastralReference) => null;

        public string? GetAddresses(
            double latitude,
            double longitude,
            int radius)
        {
            AddressRequests++;
            return null;
        }
    }

    private sealed class RejectingSelector : IAddressCandidateSelector
    {
        public string? Select(
            string searchAddress,
            IReadOnlyList<string> candidates) =>
            throw new InvalidOperationException(
                "The selector must not run for a cache hit.");
    }

    private sealed class NullLogger : IApplicationLogger
    {
        public void WriteError(string source, string message)
        {
        }

        public void WriteInfo(string source, string message)
        {
        }
    }
}
