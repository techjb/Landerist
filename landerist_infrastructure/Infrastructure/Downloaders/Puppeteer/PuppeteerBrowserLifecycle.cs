using landerist_library.Application.Logging;
using PuppeteerSharp;

namespace landerist_library.Infrastructure.Downloaders.Puppeteer;

internal static class PuppeteerBrowserLifecycle
{
    internal static async Task<IBrowser?> LaunchAsync(
        LaunchOptions launchOptions,
        IApplicationLogger logger,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Task<IBrowser> launch = PuppeteerSharp.Puppeteer.LaunchAsync(launchOptions);
        try
        {
            return await launch.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _ = CloseCancelledLaunchAsync(launch);
            throw;
        }
        catch (Exception exception)
        {
            logger.WriteError("PuppeteerDownloader LaunchAsync", exception.ToString());
            return null;
        }
    }

    internal static async Task CloseBrowserAsync(IBrowser browser, IApplicationLogger logger)
    {
        try
        {
            await browser.CloseAsync().ConfigureAwait(false);
            await browser.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.WriteError("PuppeteerDownloader CloseBrowserAsync", exception.ToString());
        }
    }

    internal static async Task ClosePageAsync(IPage browserPage, IApplicationLogger logger)
    {
        try
        {
            await browserPage.CloseAsync().ConfigureAwait(false);
            await browserPage.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.WriteError("PuppeteerDownloader ClosePageAsync", exception.ToString());
        }
    }

    private static async Task CloseCancelledLaunchAsync(Task<IBrowser> launch)
    {
        try
        {
            IBrowser browser = await launch.ConfigureAwait(false);
            await browser.CloseAsync().ConfigureAwait(false);
            await browser.DisposeAsync().ConfigureAwait(false);
        }
        catch
        {
        }
    }
}
