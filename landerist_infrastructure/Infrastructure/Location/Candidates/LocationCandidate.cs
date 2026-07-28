namespace landerist_library.Infrastructure.Location.Candidates
{
    public sealed record LocationCandidate(double latitude, double longitude, bool isAccurate, string source);
}
