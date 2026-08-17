using landerist_library.Infrastructure.Downloaders.Puppeteer;

namespace landerist_unit_tests;

public sealed class PuppeteerDownloadExecutionTests
{
    [Fact]
    public async Task WaitAsync_ReturnsCompletedDownloadWithoutClosingPage()
    {
        int closeCalls = 0;
        int timeoutCalls = 0;

        PuppeteerDownloadExecutionResult<string> result =
            await PuppeteerDownloadExecution.WaitAsync(
                Task.FromResult("content"),
                100,
                CancellationToken.None,
                _ => timeoutCalls++,
                () =>
                {
                    closeCalls++;
                    return Task.CompletedTask;
                });

        Assert.False(result.TimedOut);
        Assert.Equal("content", result.Value);
        Assert.Equal(0, timeoutCalls);
        Assert.Equal(0, closeCalls);
    }

    [Fact]
    public async Task WaitAsync_MarksTimeoutAndClosesPage()
    {
        int closeCalls = 0;
        int timeoutCalls = 0;
        var unfinishedDownload = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        PuppeteerDownloadExecutionResult<string> result =
            await PuppeteerDownloadExecution.WaitAsync(
                unfinishedDownload.Task,
                1,
                CancellationToken.None,
                _ => timeoutCalls++,
                () =>
                {
                    closeCalls++;
                    return Task.CompletedTask;
                });

        Assert.True(result.TimedOut);
        Assert.Null(result.Value);
        Assert.Equal(1, timeoutCalls);
        Assert.Equal(1, closeCalls);
    }

    [Fact]
    public async Task WaitAsync_WhenCancelled_ClosesPageAndPropagatesCancellation()
    {
        int closeCalls = 0;
        int timeoutCalls = 0;
        var unfinishedDownload = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            PuppeteerDownloadExecution.WaitAsync(
                unfinishedDownload.Task,
                100,
                cancellation.Token,
                _ => timeoutCalls++,
                () =>
                {
                    closeCalls++;
                    return Task.CompletedTask;
                }));

        Assert.Equal(0, timeoutCalls);
        Assert.Equal(1, closeCalls);
    }
}
