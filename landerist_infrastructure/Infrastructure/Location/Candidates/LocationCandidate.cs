namespace landerist_library.Parse.Location.Candidates
{
    public sealed record LocationCandidate(double latitude, double longitude, bool isAccurate, string source);
}
