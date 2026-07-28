namespace landerist_library.Application.Parsing;

public interface IAddressCandidateSelector
{
    string? Select(
        string searchAddress,
        IReadOnlyList<string> candidates);
}
