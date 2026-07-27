namespace landerist_library.Pages;

public sealed record PageDownloadResult(
    string? Content,
    byte[]? Screenshot,
    short? HttpStatusCode,
    string? RedirectUrl,
    string? Etag,
    string? LastModified);