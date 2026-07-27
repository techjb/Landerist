using PuppeteerSharp;

namespace landerist_library.Downloaders.Puppeteer;

public sealed record PuppeteerScreenshotPolicy(
    ScreenshotType Type,
    int MaxPixelsPerSide,
    int InitialJpegQuality = 90)
{
    public PuppeteerScreenshotPolicy Validate()
    {
        if (MaxPixelsPerSide <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxPixelsPerSide));
        }

        if (InitialJpegQuality is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(InitialJpegQuality));
        }

        return this;
    }
}
