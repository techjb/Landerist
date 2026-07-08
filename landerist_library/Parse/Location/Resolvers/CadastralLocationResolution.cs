using landerist_library.Parse.Location.Candidates;

namespace landerist_library.Parse.Location.Resolvers
{
    internal sealed record CadastralLocationResolution(
        LocationCandidate Candidate,
        string? ResolvedAddress);
}
