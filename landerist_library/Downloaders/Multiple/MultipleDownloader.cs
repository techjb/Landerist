﻿using landerist_library.Configuration;

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
                    if (singleDownloader.IsAvailable())
                    {
                        singleDownloader.SetUnavailable();
                        return singleDownloader;
                    }
                }
                SingleDownloader newSingleDownloader = new();
                if (newSingleDownloader.IsAvailable())
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
            Parallel.ForEach(List, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM_SCRAPER,
            },
            singleDownloader =>
            {
                singleDownloader.CloseBrowser();
            });
            List.Clear();
        }

        public void LogDownloadersCounter()
        {
            Logs.Log.WriteInfo("MultipleDownloader DownloadersCounter", List.Count.ToString());
        }
    }
}
