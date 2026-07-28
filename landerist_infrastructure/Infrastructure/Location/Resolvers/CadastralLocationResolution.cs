using landerist_library.Infrastructure.Location.Candidates;

namespace landerist_library.Infrastructure.Location.Resolvers
{
    public sealed record CadastralLocationResolution(
        LocationCandidate Candidate,
        string? ResolvedAddress);
}
