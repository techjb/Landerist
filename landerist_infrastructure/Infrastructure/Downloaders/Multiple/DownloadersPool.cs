

using landerist_library.Application.Logging;
using landerist_library.Pages;
using landerist_library.Infrastructure.Downloaders;

namespace landerist_library.Infrastructure.Downloaders.Multiple
{
    public class DownloadersPool
    {
        private readonly int MaxDownloaders;
        private readonly IDownloaderSessionFactory SessionFactory;
        private readonly IApplicationLogger Logger;

        public DownloadersPool(
            int maxDownloaders,
            IDownloaderSessionFactory sessionFactory,
            IApplicationLogger logger)
        {
            if (maxDownloaders <= 0) throw new ArgumentOutOfRangeException(nameof(maxDownloaders));
            ArgumentNullException.ThrowIfNull(sessionFactory);
            ArgumentNullException.ThrowIfNull(logger);
            MaxDownloaders = maxDownloaders;
            SessionFactory = sessionFactory;
            Logger = logger;
        }
        private readonly List<SingleDownloader> Downloaders = [];

        private readonly Lock Sync = new();

        public bool Download(Page page, bool useProxy = false)
        {
            ArgumentNullException.ThrowIfNull(page);
            SingleDownloader? downloader = GetDownloader(useProxy);
            if (downloader is null)
            {
                Logger.WriteError("MultipleDownloader Download", "Downloader not found");
                return false;
            }
            return downloader.Download(page);
        }

        public async Task<bool> DownloadAsync(
            Page page,
            bool useProxy = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);
            cancellationToken.ThrowIfCancellationRequested();
            SingleDownloader? downloader = GetDownloader(useProxy);
            if (downloader is null)
            {
                Logger.WriteError("MultipleDownloader DownloadAsync", "Downloader not found");
                return false;
            }

            return await downloader
                .DownloadAsync(page, cancellationToken)
                .ConfigureAwait(false);
        }
        private SingleDownloader? GetDownloader(bool useProxy)
        {
            lock (Sync)
            {
                foreach (var downloader in Downloaders.OrderBy(static _ => Random.Shared.Next()))
                {
                    if (downloader.TryReserve(useProxy))
                    {
                        return downloader;
                    }
                }

                if (Downloaders.Count >= MaxDownloaders)
                {
                    Logger.WriteInfo("MultipleDownloader GetDownloader",
                        $"Max downloaders reached: {MaxDownloaders}");
                    return null;
                }

                int id = Downloaders.Count + 1;
                SingleDownloader newSingleDownloader = new(useProxy, SessionFactory) { Id = id };
                if (newSingleDownloader.TryReserve(useProxy))
                {
                    Downloaders.Add(newSingleDownloader);
                    return newSingleDownloader;
                }

                Logger.WriteError("MultipleDownloader GetDownloader", "Downloader not found");
                return null;
            }
        }

        public void Clear()
        {
            SingleDownloader[] toClear;
            lock (Sync)
            {
                toClear = [.. Downloaders];
                Downloaders.Clear();
            }

            Parallel.ForEach(toClear, static singleDownloader =>
                singleDownloader.CloseBrowser());
        }

        public void Print()
        {
            lock (Sync)
            {
                if (Downloaders.Count == 0)
                {
                    return;
                }

                int maxCrashCounter = 0;
                int maxDownloads = 0;
                int withProxy = 0;

                foreach (SingleDownloader singleDownloader in Downloaders)
                {
                    var crashCounter = singleDownloader.CrashesCounter();
                    if (crashCounter > maxCrashCounter)
                    {
                        maxCrashCounter = crashCounter;
                    }
                    var counter = singleDownloader.ScrapedCounter();
                    if (counter > maxDownloads)
                    {
                        maxDownloads = counter;
                    }
                    if (singleDownloader.GetUseProxy())
                    {
                        withProxy++;
                    }
                }

                Logger.WriteInfo("MultipleDownloaders",
                    $"Downloaders: {Downloaders.Count} WithProxy: {withProxy} MaxDownloads: {maxDownloads} MaxCrashCounter: {maxCrashCounter}");
            }
        }

        public int GetDownloadersCounter()
        {
            lock (Sync)
            {
                return Downloaders.Count;
            }
        }

        public int GetMaxCrashCounter()
        {
            lock (Sync)
            {
                int maxCrashCounter = 0;
                foreach (SingleDownloader singleDownloader in Downloaders)
                {
                    var crashCounter = singleDownloader.CrashesCounter();
                    if (crashCounter > maxCrashCounter)
                    {
                        maxCrashCounter = crashCounter;
                    }
                }
                return maxCrashCounter;
            }
        }

        public int GetMaxDownloads()
        {
            lock (Sync)
            {
                int maxDownloads = 0;
                foreach (SingleDownloader singleDownloader in Downloaders)
                {
                    var counter = singleDownloader.ScrapedCounter();
                    if (counter > maxDownloads)
                    {
                        maxDownloads = counter;
                    }
                }
                return maxDownloads;
            }
        }
    }
}
