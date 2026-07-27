using landerist_library.Downloaders.Puppeteer;
using PuppeteerSharp;

namespace landerist_unit_tests;

public sealed class PuppeteerScreenshotPolicyTests
{
    [Fact]
    public void Validate_AcceptsExplicitPolicy()
    {
        PuppeteerScreenshotPolicy policy = new(
            ScreenshotType.Jpeg,
            MaxPixelsPerSide: 4096,
            InitialJpegQuality: 85);

        Assert.Same(policy, policy.Validate());
    }

    [Theory]
    [InlineData(0, 90)]
    [InlineData(100, -1)]
    [InlineData(100, 101)]
    public void Validate_RejectsInvalidLimits(
        int maxPixelsPerSide,
        int jpegQuality)
    {
        PuppeteerScreenshotPolicy policy = new(
            ScreenshotType.Jpeg,
            maxPixelsPerSide,
            jpegQuality);

        Assert.Throws<ArgumentOutOfRangeException>(() => policy.Validate());
    }
}
