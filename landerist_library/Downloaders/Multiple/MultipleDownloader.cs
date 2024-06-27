using landerist_library.Configuration;
using landerist_library.Downloaders.Puppeteer;

namespace landerist_library.Downloaders.Multiple
{
    public class MultipleDownloader
    {
        private readonly List<SingleDownloader> List = [];

        private readonly object Sync = new();

        public SingleDownloader? GetDownloader()
        {
            lock (Sync)
            {
                foreach (SingleDownloader singleDownloader in List)
                {
                    if (singleDownloader.IsAvailable)
                    {
                        singleDownloader.SetUnavailable();
                        return singleDownloader;
                    }
                }
                SingleDownloader newSingleDownloader = new();
                if (newSingleDownloader.ContainsBrowser())
                {
                    List.Add(newSingleDownloader);
                    newSingleDownloader.SetUnavailable();
                    return newSingleDownloader;
                }
                return null;
            }
        }

        public void Clear()
        {
            var maxDegreeOfParallelism = Config.IsConfigurationProduction() ? Environment.ProcessorCount - 1 : 1;
            Parallel.ForEach(List, new ParallelOptions()
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
            }, singleDownloader =>
            {
                singleDownloader.CloseBrowser();
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
