using landerist_library.Configuration;
using landerist_library.Database;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class BatchCleaner : BatchClient
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
                var batchResponse = GetBatch(batchId);
                if (batchResponse == null || !BatchIsCompleted(batchResponse))
                {
                    return;
                }
                if (DeleteFile(batchResponse.InputFileId) && DeleteFile(batchResponse.OutputFileId) && DeleteFile(batchResponse.ErrorFileId))
                {
                    Batches.Delete(batchResponse.Id);
                }
            });
        }

        public static void DeleteAllRemoteFiles()
        {
            var batchIds = Batches.SelectAll();
            if (batchIds.Count > 0)
            {
                return;
            }

            var files = ListFiles();
            if (files is null)
            {
                return;
            }
            Parallel.ForEach(files, filesId =>
            {
                DeleteFile(filesId);
            });
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


        //private static void Delete(BatchResponse batchResponse)
        //{
        //    DeleteFile(batchResponse.OutputFileId);
        //    DeleteFile(batchResponse.InputFileId);
        //    DeleteFile(batchResponse.ErrorFileId);
        //    Batches.Delete(batchResponse.Id);
        //}

    }
}
