using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Application.Statistics;
using landerist_library.Database;
using Newtonsoft.Json;

namespace landerist_library.Infrastructure.Location.Providers.Goolzoom;

public sealed class GoolzoomCadastralReferenceProvider(
    AddressCadastralReference cache,
    IGlobalStatisticsRepository statistics,
    IGoolzoomClient goolzoom,
    IAddressCandidateSelector addresses,
    IApplicationLogger logger) : ICadastralReferenceProvider
{
    public string? GetCadastralReference(
        double? latitude,
        double? longitude,
        string address)
    {
        if (!latitude.HasValue
            || !longitude.HasValue
            || string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        string? cached = cache.Select(address);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            return cached;
        }

        return Resolve(latitude.Value, longitude.Value, address);
    }

    private string? Resolve(
        double latitude,
        double longitude,
        string searchAddress)
    {
        try
        {
            for (int radius = 50; radius <= 100; radius += 50)
            {
                statistics.InsertDailyCounter(
                    "AddressToCadastralReferenceRequest",
                    1);
                string? content = goolzoom.GetAddresses(
                    latitude,
                    longitude,
                    radius);
                string? reference = FindReference(
                    searchAddress,
                    content);
                if (string.IsNullOrWhiteSpace(reference))
                {
                    continue;
                }

                cache.Insert(searchAddress, reference);
                return reference;
            }
        }
        catch (Exception exception)
        {
            logger.WriteError(
                nameof(GoolzoomCadastralReferenceProvider),
                exception.ToString());
        }

        return null;
    }

    private string? FindReference(
        string searchAddress,
        string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        AddressList? result = JsonConvert.DeserializeObject<AddressList>(
            content);
        if (result?.Addresses is not { Count: > 0 })
        {
            return null;
        }

        List<string> candidates =
        [
            .. result.Addresses
                .Where(item => !string.IsNullOrWhiteSpace(item.Address))
                .Select(item => item.Address),
        ];
        string? selected = addresses.Select(searchAddress, candidates);
        if (string.IsNullOrWhiteSpace(selected)
            || selected.Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return result.Addresses
            .FirstOrDefault(item =>
                string.Equals(
                    item.Address,
                    selected,
                    StringComparison.OrdinalIgnoreCase))
            ?.LocalId;
    }

    private sealed class AddressList
    {
        [JsonProperty("addresses")]
        public List<AddressItem> Addresses { get; init; } = [];
    }

    private sealed class AddressItem
    {
        [JsonProperty("localid")]
        public string LocalId { get; init; } = string.Empty;

        [JsonProperty("address")]
        public string Address { get; init; } = string.Empty;
    }
}
