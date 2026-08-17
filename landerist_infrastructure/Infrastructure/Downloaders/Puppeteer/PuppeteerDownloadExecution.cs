namespace landerist_library.Infrastructure.Downloaders.Puppeteer;

internal readonly record struct PuppeteerDownloadExecutionResult<T>(
    bool TimedOut,
    T? Value);

internal static class PuppeteerDownloadExecution
{
    internal static async Task<PuppeteerDownloadExecutionResult<T>> WaitAsync<T>(
        Task<T> download,
        int timeoutMilliseconds,
        CancellationToken cancellationToken,
        Action<Task> onTimeout,
        Func<Task> closePageAsync)
    {
        Task timeout = Task.Delay(timeoutMilliseconds, cancellationToken);
        try
        {
            Task completed = await Task.WhenAny(download, timeout).ConfigureAwait(false);
            if (completed == download)
            {
                return new(false, await download.ConfigureAwait(false));
            }

            cancellationToken.ThrowIfCancellationRequested();
            onTimeout(download);
            await closePageAsync().ConfigureAwait(false);
            return new(true, default);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await closePageAsync().ConfigureAwait(false);
            throw;
        }
    }
}
