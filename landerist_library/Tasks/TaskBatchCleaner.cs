using System.IO;
using landerist_library.Configuration;
using landerist_library.Infrastructure.Sql;
using landerist_library.Database;
using landerist_library.Parse.ListingParser.OpenAI.Batch;
using landerist_library.Parse.ListingParser.VertexAI.Batch;

namespace landerist_library.Tasks
{
    public sealed class TaskBatchCleaner
    {
        private readonly BatchRepository _batches;

        public TaskBatchCleaner(BatchRepository batches)
        {
            ArgumentNullException.ThrowIfNull(batches);
            _batches = batches;
        }

        public void Start()
        {
            DeleteDownloadedBatches();
            DeleteLocalFiles();

            VertexAIBatchCleaner.RemoveFiles();
            //OpenAIBatchCleaner.RemoveFiles();
        }

        private void DeleteDownloadedBatches()
        {
            var batches = _batches.Select(downloaded: true);
            foreach (var batch in batches)
            {
                _batches.Delete(batch.Id);
            }
        }

        private static void DeleteLocalFiles()
        {
            if (!Directory.Exists(Config.BATCH_DIRECTORY))
            {
                return;
            }

            var files = Directory.GetFiles(Config.BATCH_DIRECTORY);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
    }
}
