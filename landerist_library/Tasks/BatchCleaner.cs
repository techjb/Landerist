using landerist_library.Configuration;
using landerist_library.Database;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class BatchCleaner
    {
        public static void Start()
        {
            DeleteDwonloadedBatches();            
            DeleteLocalFiles();
        }

        private static void DeleteDwonloadedBatches()
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
