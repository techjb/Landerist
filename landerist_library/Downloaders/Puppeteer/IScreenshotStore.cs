using PuppeteerSharp;

namespace landerist_library.Downloaders.Puppeteer;

public interface IScreenshotStore
{
    void Save(string pageHash, ScreenshotType type, byte[] content);
}

public sealed class NullScreenshotStore : IScreenshotStore
{
    public void Save(string pageHash, ScreenshotType type, byte[] content)
    {
    }
}
