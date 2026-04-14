

using landerist_library.Configuration;
using landerist_library.Pages;

namespace landerist_library.Downloaders.Multiple
{
    public class DownloadersPool
    {
        private static readonly List<SingleDownloader> Downloaders = [];

        private static readonly Lock Sync = new();

        public static bool Download(Page page, bool useProxy = false)
        {
            ArgumentNullException.ThrowIfNull(page);
            SingleDownloader? downloader = GetDownloader(useProxy);
            if (downloader is null)
            {
                Logs.Log.WriteError("MultipleDownloader Download", "Downloader not found");
                return false;
            }
            return downloader.Download(page);
        }

        private static SingleDownloader? GetDownloader(bool useProxy)
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

                if (Downloaders.Count >= Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER)
                {
                    Logs.Log.WriteInfo("MultipleDownloader GetDownloader",
                        $"Max downloaders reached: {Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER}");
                    return null;
                }

                int id = Downloaders.Count + 1;
                SingleDownloader newSingleDownloader = new(id, useProxy);
                if (newSingleDownloader.TryReserve(useProxy))
                {
                    Downloaders.Add(newSingleDownloader);
                    return newSingleDownloader;
                }

                Logs.Log.WriteError("MultipleDownloader GetDownloader", "Downloader not found");
                return null;
            }
        }

        public static void Clear()
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

        public static void Print()
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

                Logs.Log.WriteInfo("MultipleDownloaders",
                    $"Downloaders: {Downloaders.Count} WithProxy: {withProxy} MaxDownloads: {maxDownloads} MaxCrashCounter: {maxCrashCounter}");
            }
        }

        public static int GetDownloadersCounter()
        {
            lock (Sync)
            {
                return Downloaders.Count;
            }
        }

        public static int GetMaxCrashCounter()
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

        public static int GetMaxDownloads()
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
