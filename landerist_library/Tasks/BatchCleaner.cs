using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Parse.ListingParser.VertexAI.Batch;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class BatchCleaner
    {
        public static void Start()
        {
            DeleteBatches();
            VertexAIBatchCleaner.Clean();
            DeleteLocalFiles();
        }

        private static void DeleteBatches()
        {
            var batches = Batches.SelectDownloaded();
            Parallel.ForEach(batches, Config.PARALLELOPTIONS1INLOCAL, Delete);
        }

        private static void Delete(Database.Batch batch)
        {
            bool sucess = false;
            switch (Config.LLM_PROVIDER)
            {
                case LLMProvider.OpenAI:
                    sucess = OpenAIBatchCleaner.Delete(batch.Id);
                    break;
                case LLMProvider.VertexAI:
                    sucess = true;
                    break;
                default:
                    break;
            }
            if (sucess)
            {
                Batches.Delete(batch.Id);
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
