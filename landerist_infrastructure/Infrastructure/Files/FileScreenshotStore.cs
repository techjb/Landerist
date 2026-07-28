using landerist_library.Infrastructure.Downloaders.Puppeteer;
using PuppeteerSharp;

namespace landerist_library.Infrastructure.Files;

public sealed class FileScreenshotStore(string directory) : IScreenshotStore
{
    private readonly string _directory =
        string.IsNullOrWhiteSpace(directory)
            ? throw new ArgumentException(
                "Screenshot directory is required.",
                nameof(directory))
            : Path.GetFullPath(directory);

    public void Save(string pageHash, ScreenshotType type, byte[] content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pageHash);
        ArgumentNullException.ThrowIfNull(content);
        Directory.CreateDirectory(_directory);
        string extension = type.ToString().ToLowerInvariant();
        File.WriteAllBytes(
            Path.Combine(_directory, pageHash + "." + extension),
            content);
    }
}
