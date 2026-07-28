using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Infrastructure.Parsing;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser;

namespace landerist_unit_tests;

public sealed class JsonlBatchInputWriterTests
{
    [Fact]
    public void Write_CreatesJsonlAndReportsWrittenPages()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            Page first = CreatePage("1", "first input");
            Page second = CreatePage("2", "second input");
            JsonlBatchInputWriter writer = CreateWriter(
                directory,
                maxFileSize: 1000,
                minPages: 1);

            BatchInputWriteResult result = writer.Write([first, second]);

            Assert.NotNull(result.FilePath);
            Assert.True(File.Exists(result.FilePath));
            Assert.Equal(2, File.ReadAllLines(result.FilePath).Length);
            Assert.Contains(first.UriHash, result.WrittenPageHashes);
            Assert.Contains(second.UriHash, result.WrittenPageHashes);
            Assert.Empty(result.InvalidPageHashes);
        }
        finally
        {
            DeleteTemporaryDirectory(directory);
        }
    }

    [Fact]
    public void Write_WhenFileLimitCannotFitMinimum_DeletesPartialFile()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            Page page = CreatePage("1", "input larger than five bytes");
            JsonlBatchInputWriter writer = CreateWriter(
                directory,
                maxFileSize: 5,
                minPages: 1);

            BatchInputWriteResult result = writer.Write([page]);

            Assert.Null(result.FilePath);
            Assert.Empty(result.WrittenPageHashes);
            Assert.Empty(Directory.GetFiles(directory));
        }
        finally
        {
            DeleteTemporaryDirectory(directory);
        }
    }

    private static JsonlBatchInputWriter CreateWriter(
        string directory,
        long maxFileSize,
        int minPages) =>
        new(
            new BatchInputWriterOptions(
                LLMProvider.OpenAI,
                directory,
                maxFileSize,
                minPages),
            new StubProvider(),
            new StubListingInputPreparer(),
            new FixedTimeProvider(
                new DateTimeOffset(2026, 7, 27, 10, 0, 0, TimeSpan.Zero)),
            new NullApplicationLogger());

    private static Page CreatePage(string id, string input) =>
        new($"https://example.com/listing/{id}")
        {
            ListingParserInput = input
        };

    private static string CreateTemporaryDirectory()
    {
        string directory = Path.Combine(
            Path.GetTempPath(),
            $"landerist-batch-writer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static void DeleteTemporaryDirectory(string directory)
    {
        foreach (string file in Directory.GetFiles(directory))
            File.Delete(file);
        Directory.Delete(directory);
    }

    private sealed class NullApplicationLogger : IApplicationLogger
    {
        public void WriteError(string source, string message)
        {
        }

        public void WriteInfo(string source, string message)
        {
        }
    }
    private sealed class StubListingInputPreparer : IListingInputPreparer
    {
        public void Prepare(Page page) { }
        public bool MatchesUnavailableRule(Page page) => false;
    }

    private sealed class StubProvider : IListingBatchUploadProvider
    {
        public LLMProvider Provider => LLMProvider.OpenAI;
        public string? Serialize(Page page, string userInput) =>
            $"{{\"input\":\"{userInput}\"}}";
        public string? UploadFile(string filePath) => filePath;
        public string? CreateBatch(string fileId) => fileId;
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
