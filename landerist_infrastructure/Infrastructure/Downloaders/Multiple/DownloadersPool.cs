

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
        private int CreatingDownloaders;
        private int Generation;
        private int NextDownloaderId;

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
            SingleDownloader? downloader = await GetDownloaderAsync(
                useProxy,
                cancellationToken).ConfigureAwait(false);
            if (downloader is null)
            {
                Logger.WriteError("MultipleDownloader DownloadAsync", "Downloader not found");
                return false;
            }

            return await downloader
                .DownloadAsync(page, cancellationToken)
                .ConfigureAwait(false);
        }
        private async Task<SingleDownloader?> GetDownloaderAsync(
            bool useProxy,
            CancellationToken cancellationToken)
        {
            int generation;
            lock (Sync)
            {
                foreach (SingleDownloader downloader in
                    Downloaders.OrderBy(static _ => Random.Shared.Next()))
                {
                    if (downloader.TryReserve(useProxy))
                    {
                        return downloader;
                    }
                }

                if (Downloaders.Count + CreatingDownloaders >= MaxDownloaders)
                {
                    Logger.WriteInfo(
                        "MultipleDownloader GetDownloaderAsync",
                        $"Max downloaders reached: {MaxDownloaders}");
                    return null;
                }

                CreatingDownloaders++;
                generation = Generation;
            }

            SingleDownloader created;
            try
            {
                created = await SingleDownloader.CreateAsync(
                    useProxy,
                    SessionFactory,
                    cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                lock (Sync)
                {
                    CreatingDownloaders--;
                }
            }

            bool discard;
            lock (Sync)
            {
                discard = generation != Generation;
                if (!discard && created.TryReserve(useProxy))
                {
                    created.Id = ++NextDownloaderId;
                    Downloaders.Add(created);
                    return created;
                }
            }

            await created.CloseBrowserAsync().ConfigureAwait(false);
            return null;
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

                int id = ++NextDownloaderId;
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

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SingleDownloader[] toClear;
            lock (Sync)
            {
                Generation++;
                toClear = [.. Downloaders];
                Downloaders.Clear();
            }

            await Task.WhenAll(toClear.Select(
                static downloader => downloader.CloseBrowserAsync()))
                .WaitAsync(cancellationToken)
                .ConfigureAwait(false);
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
