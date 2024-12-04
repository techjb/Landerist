namespace landerist_library.Downloaders.Multiple
{
    public class MultipleDownloader
    {
        private static readonly HashSet<SingleDownloader> Downloaders = [];

        private static readonly object Sync = new();

        public static SingleDownloader? GetDownloader()
        {
            lock (Sync)
            {
                //RemoveBrowserWithErrors();
                var availables = Downloaders.Where(o => o.IsAvailable()).ToList();
                if (availables.Count != 0)
                {
                    return availables[new Random().Next(availables.Count)];
                }

                int id = Downloaders.Count + 1;
                SingleDownloader newSingleDownloader = new(id);
                if (newSingleDownloader.IsAvailable())
                {
                    newSingleDownloader.SetUnavailable();
                    Downloaders.Add(newSingleDownloader);
                    return newSingleDownloader;
                }

                Logs.Log.WriteError("MultipleDownloader GetDownloader", "Downloader not found");
                return null;
            }
        }

        //private static void RemoveBrowserWithErrors()
        //{
        //    var downloaders = Downloaders.Where(singleDownloader => singleDownloader.BrowserHasErrors()).ToList();
        //    if (downloaders.Count == 0)
        //    {
        //        return;
        //    }

        //    Logs.Log.Console("RemoveBrowserWithErrors: " + downloaders.Count + "/" + Downloaders.Count);
        //    foreach (var singleDownloader in downloaders)
        //    {
        //        singleDownloader.CloseBrowser();
        //    }
        //    //Downloaders.RemoveWhere(singleDownloader => singleDownloader.BrowserHasErrors());
        //}

        public static void Clear()
        {
            Parallel.ForEach(Downloaders, new ParallelOptions()
            {

            },
            singleDownloader =>
            {
                singleDownloader.CloseBrowser();
            });
            Downloaders.Clear();
        }

        //public static void LogDownloadersCounter()
        //{
        //    Logs.Log.WriteInfo("MultipleDownloader DownloadersCounter", Downloaders.Count.ToString());
        //}

        public static void Print()
        {
            if (Downloaders.Count.Equals(0))
            {
                return;
            }

            int maxCrashCounter = 0;
            int maxDownloads = 0;

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
            }

            Logs.Log.WriteInfo("MultipleDownloaders",
                $"Downloaders: {Downloaders.Count} MaxDownloads: {maxDownloads} MaxCrashCounter: {maxCrashCounter}");
        }
    }
}
