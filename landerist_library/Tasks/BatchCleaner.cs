using landerist_library.Configuration;
using landerist_library.Database;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class BatchCleaner
    {
        public static void Start()
        {
            DeleteBatches();
            DeleteLocalFiles();
        }

        private static void DeleteBatches()
        {
            var batches = Batches.SelectDownloaded();
            Parallel.ForEach(batches, Config.PARALLELOPTIONS1INLOCAL, Delete);
        }

        private static void Delete(Database.Batch batch)
        {
            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    OpenAIBatchCleaner.Delete(batch.Id);
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
