using landerist_library.Application.Parsing;
using landerist_library.Parse.CadastralReference;

namespace landerist_library.Infrastructure.Parsing;

public sealed class LegacyAddressCandidateSelector : IAddressCandidateSelector
{
    public string? Select(
        string searchAddress,
        IReadOnlyList<string> candidates)
    {
        AddressAIFinder finder = new(searchAddress, [.. candidates]);
        var result = finder.GetAddress().GetAwaiter().GetResult();
        return result.success ? result.address : null;
    }
}
