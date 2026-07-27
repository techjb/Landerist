namespace landerist_library.Pages;

public sealed record PageRules(
    int MaxPageTypeCounter,
    int MinListingParserInputLength,
    int MaxListingParserInputLength,
    int MaxScreenshotSize)
{
    public static PageRules Default { get; } = new(
        MaxPageTypeCounter: 1000,
        MinListingParserInputLength: 50,
        MaxListingParserInputLength: 100000,
        MaxScreenshotSize: 5 * 1024 * 1024);
}
