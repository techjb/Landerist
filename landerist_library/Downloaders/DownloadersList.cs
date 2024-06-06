using landerist_library.Configuration;

namespace landerist_library.Downloaders
{
    public class DownloadersList
    {
        private readonly List<Downloader> List = [];

        private readonly object Sync = new();

        public Downloader GetDownloader()
        {
            lock(Sync)
            {
                foreach (Downloader downloader in List)
                {
                    if (downloader.IsAvailable)
                    {
                        downloader.SetUnavailable();
                        return downloader;
                    }
                }
                Downloader newDownloader = new();
                List.Add(newDownloader);
                newDownloader.SetUnavailable();
                return newDownloader;
            }
        }

        public void Clear()
        {
            var maxDegreeOfParallelism = Config.IsConfigurationProduction() ? Environment.ProcessorCount - 1 : 1;
            Parallel.ForEach(List, new ParallelOptions()
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
            }, downloader =>
            {
                downloader.CloseBrowser();
            });
            List.Clear();
            PuppeteerDownloader.KillChrome();
        }

        public void LogDownloadersCounter()
        {
            Logs.Log.WriteLogInfo("DownloadersList DownloadersCounter", List.Count.ToString());
        }
    }
}
