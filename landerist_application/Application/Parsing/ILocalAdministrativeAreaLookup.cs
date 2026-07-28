using landerist_library.Websites;

namespace landerist_library.Application.Parsing;

public sealed record LocalAdministrativeArea(string Id, string Name);

public interface ILocalAdministrativeAreaLookup
{
    LocalAdministrativeArea? Find(
        CountryCode countryCode,
        double latitude,
        double longitude);
}
