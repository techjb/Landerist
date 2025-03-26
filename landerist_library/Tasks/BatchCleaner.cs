using landerist_library.Configuration;
using landerist_library.Database;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class BatchCleaner
    {
        public static void Start()
        {
            DeleteDownloadedBatches();
            DeleteLocalFiles();
        }

        private static void DeleteDownloadedBatches()
        {
            var batchIds = Batches.SelectDownloaded();
            Parallel.ForEach(batchIds, batchId =>
            {
                Delete(batchId);
            });
        }      

        private static void Delete(string batchId)
        {
            switch(Config.LLM_PROVIDER)
            {
                case LLMProviders.OpenAI:
                    OpenAIBatchCleaner.Delete(batchId);
                    break;
                default:
                    break;
            }
        }

        private static void DeleteLocalFiles()
        {
            if (Directory.Exists(Config.BATCH_DIRECTORY))
            {
                var files = Directory.GetFiles(Config.BATCH_DIRECTORY);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
