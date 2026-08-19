namespace landerist_library.Infrastructure.Logging;

public static class HealthchecksUriBuilder
{
    public static Uri GetHeartbeatUri(Uri baseUri, bool healthy)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        if (healthy)
        {
            return baseUri;
        }

        UriBuilder builder = new(baseUri)
        {
            Path = baseUri.AbsolutePath.TrimEnd('/') + "/fail"
        };
        return builder.Uri;
    }
}
