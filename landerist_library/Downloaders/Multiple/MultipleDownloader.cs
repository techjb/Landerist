namespace landerist_library.Downloaders.Multiple
{
    public class MultipleDownloader
    {
        private static readonly HashSet<SingleDownloader> Downloaders = [];

        private static readonly object Sync = new();

        public static SingleDownloader? GetDownloader(bool useProxy)
        {
            lock (Sync)
            {
                var availables = Downloaders.Where(o => o.IsAvailable(useProxy)).ToList();
                if (availables.Count != 0)
                {
                    return availables[new Random().Next(availables.Count)];
                }

                int id = Downloaders.Count + 1;
                SingleDownloader newSingleDownloader = new(id, useProxy);
                if (newSingleDownloader.IsAvailable(useProxy))
                {
                    newSingleDownloader.SetUnavailable();
                    Downloaders.Add(newSingleDownloader);
                    return newSingleDownloader;
                }
                Logs.Log.WriteError("MultipleDownloader GetDownloader", "Downloader not found");
                return null;
            }
        }

        public static void Clear()
        {
            //Print();
            Parallel.ForEach(Downloaders, new ParallelOptions()
            {

            },
            singleDownloader =>
            {
                singleDownloader.CloseBrowser();
            });
            Downloaders.Clear();
        }

        public static void Print()
        {
            if (Downloaders.Count.Equals(0))
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

        public static int GetDownloadersCounter() => Downloaders.Count;

        public static int GetMaxCrashCounter()
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

        public static int GetMaxDownloads()
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
