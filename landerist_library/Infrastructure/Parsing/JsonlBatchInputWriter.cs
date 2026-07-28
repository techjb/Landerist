using landerist_library.Application.Logging;
using landerist_library.Application.Parsing;
using landerist_library.Pages;

namespace landerist_library.Infrastructure.Parsing;

public sealed class JsonlBatchInputWriter : IBatchInputWriter
{
    private enum WriteResult
    {
        Written,
        Full,
        Invalid,
        Error
    }

    private readonly BatchInputWriterOptions _options;
    private readonly IListingBatchUploadProvider _provider;
    private readonly IListingInputPreparer _listingInput;
    private readonly TimeProvider _timeProvider;
    private readonly IApplicationLogger _logger;

    public JsonlBatchInputWriter(
        BatchInputWriterOptions options,
        IListingBatchUploadProvider provider,
        IListingInputPreparer listingInput,
        TimeProvider timeProvider,
        IApplicationLogger logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(listingInput);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(logger);
        if (string.IsNullOrWhiteSpace(options.Directory))
            throw new ArgumentException("A batch directory is required.", nameof(options));
        if (options.MaxFileSizeInBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(options));
        if (options.MinPagesPerBatch <= 0)
            throw new ArgumentOutOfRangeException(nameof(options));
        if (provider.Provider != options.Provider)
            throw new ArgumentException(
                "The writer provider must match its options.",
                nameof(provider));

        _options = options;
        _provider = provider;
        _listingInput = listingInput;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public BatchInputWriteResult Write(IReadOnlyList<Page> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);
        HashSet<string> written = [];
        HashSet<string> invalid = [];
        if (pages.Count == 0)
            return new(null, written, invalid);

        Directory.CreateDirectory(_options.Directory);
        string filePath = Path.Combine(
            _options.Directory,
            $"batch_{_options.Provider.ToString().ToLowerInvariant()}_" +
            $"{_timeProvider.GetLocalNow():yyyyMMddHHmmss}_input.json");
        File.Delete(filePath);

        int errors = 0;
        int skipped = 0;
        using (StreamWriter writer = new(filePath, append: false)
        {
            AutoFlush = true
        })
        {
            for (int index = 0; index < pages.Count; index++)
            {
                WriteResult result = WritePage(pages[index], writer);
                if (result == WriteResult.Written)
                {
                    written.Add(pages[index].UriHash);
                    continue;
                }

                if (result == WriteResult.Full)
                {
                    skipped = pages.Count - index;
                    break;
                }

                errors++;
                if (result == WriteResult.Invalid)
                    invalid.Add(pages[index].UriHash);
            }
        }

        _logger.WriteInfo(
            nameof(JsonlBatchInputWriter),
            $"Write {written.Count}/{pages.Count} skipped: {skipped} errors: {errors}");

        if (written.Count >= _options.MinPagesPerBatch)
            return new(filePath, written, invalid);

        File.Delete(filePath);
        return new(null, written, invalid);
    }

    private WriteResult WritePage(Page page, StreamWriter writer)
    {
        try
        {
            page.SetResponseBodyFromZipped();
            _listingInput.Prepare(page);
            string? userInput = page.ListingParserInput;
            page.RemoveResponseBody();
            if (string.IsNullOrEmpty(userInput))
                return WriteResult.Invalid;

            string? json = _provider.Serialize(page, userInput);
            if (string.IsNullOrEmpty(json))
                return WriteResult.Invalid;

            writer.Flush();
            int sizeToAdd =
                writer.Encoding.GetByteCount(json + Environment.NewLine);
            if (writer.BaseStream.Length + sizeToAdd >
                _options.MaxFileSizeInBytes)
            {
                return WriteResult.Full;
            }

            writer.WriteLine(json);
            return WriteResult.Written;
        }
        catch (Exception exception)
        {
            _logger.WriteError(
                $"{nameof(JsonlBatchInputWriter)} WritePage",
                exception.ToString());
            return WriteResult.Error;
        }
    }
}
