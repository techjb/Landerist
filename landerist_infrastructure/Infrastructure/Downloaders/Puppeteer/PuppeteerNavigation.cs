using PuppeteerSharp;
using System.Diagnostics;

namespace landerist_library.Infrastructure.Downloaders.Puppeteer;

internal static class PuppeteerNavigation
{
    internal static async Task<IResponse?> NavigateAsync(
        IPage browserPage,
        string url,
        string? waitSelector,
        int timeout,
        Action<string> setExecutionStep,
        Func<Task> closePageAsync)
    {
        Task<IResponse?> navigationTask = string.IsNullOrWhiteSpace(waitSelector)
            ? NavigateUntilNetworkIdleAsync(browserPage, url, timeout)
            : NavigateUntilSelectorAsync(
                browserPage,
                url,
                waitSelector.Trim(),
                timeout,
                setExecutionStep);

        Task completedTask = await Task.WhenAny(navigationTask, Task.Delay(timeout))
            .ConfigureAwait(false);
        if (completedTask == navigationTask)
        {
            return await navigationTask.ConfigureAwait(false);
        }

        setExecutionStep("Navigation timeout");
        await closePageAsync().ConfigureAwait(false);
        throw new NavigationException($"Navigation timed out after {timeout} ms.");
    }

    private static Task<IResponse?> NavigateUntilNetworkIdleAsync(
        IPage browserPage,
        string url,
        int timeout)
    {
        NavigationOptions navigationOptions = new()
        {
            WaitUntil = [WaitUntilNavigation.Networkidle2],
            Timeout = timeout
        };

        return browserPage.GoToAsync(url, navigationOptions);
    }

    private static async Task<IResponse?> NavigateUntilSelectorAsync(
        IPage browserPage,
        string url,
        string selector,
        int timeout,
        Action<string> setExecutionStep)
    {
        var stopwatch = Stopwatch.StartNew();
        NavigationOptions navigationOptions = new()
        {
            WaitUntil = [WaitUntilNavigation.DOMContentLoaded],
            Timeout = timeout
        };

        IResponse? response = await browserPage.GoToAsync(url, navigationOptions)
            .ConfigureAwait(false);
        int remainingTimeout = timeout - (int)Math.Min(stopwatch.ElapsedMilliseconds, timeout);
        if (remainingTimeout <= 0)
        {
            throw new NavigationException($"Navigation timed out after {timeout} ms.");
        }

        setExecutionStep("Waiting navigation selector");
        await browserPage.WaitForSelectorAsync(selector, new WaitForSelectorOptions
        {
            Visible = true,
            Timeout = remainingTimeout
        }).ConfigureAwait(false);

        return response;
    }
}
