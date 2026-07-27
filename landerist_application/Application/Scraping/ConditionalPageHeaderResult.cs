namespace landerist_library.Application.Scraping;

public sealed record ConditionalPageHeaderResult
{
    public bool NotModified { get; init; }

    public short? HttpStatusCode { get; init; }

    public string? RedirectUrl { get; init; }

    public string? Etag { get; init; }

    public string? LastModified { get; init; }
}
