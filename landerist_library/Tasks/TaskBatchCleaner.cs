using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Parse.ListingParser.VertexAI.Batch;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class TaskBatchCleaner
    {
        public static void Start()
        {
            DeleteDownloadedBatches();            
            DeleteLocalFiles();

            VertexAIBatchCleaner.RemoveFiles();
            OpenAIBatchCleaner.RemoveFiles();
        }

        private static void DeleteDownloadedBatches()
        {
            var batches = Batches.SelectDownloaded();
            foreach(var batch in batches)
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
