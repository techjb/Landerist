using landerist_library.Configuration;
using landerist_library.Database;
using OpenAI;
using OpenAI.Batch;

namespace landerist_library.Parse.Listing.OpenAI.Batch
{
    public class BatchCleaner: BatchClient
    {
        public static void Start()
        {
            DeleteLocalBatches();
            DeleteLocalFiles();
        }

        private static void DeleteRemoteFiles()
        {
            //DeleteLocalBatches();
            //DeleteAllRemoteFiles();
        }

        private static void DeleteLocalBatches()
        {
            var batchIds = Batches.SelectDownloaded();
            Parallel.ForEach(batchIds, batchId =>
            {
                var batchResponse = GetBatch(batchId);
                if (batchResponse == null || !BatchIsCompleted(batchResponse))
                {
                    return;
                }
                //Delete(batchResponse);
                Batches.Delete(batchResponse.Id);
            });
        }

        //public static void DeleteAllRemoteFiles()
        //{
        //    var batchIds = Batches.SelectAll();
        //    if (batchIds.Count > 0)
        //    {
        //        return;
        //    }

        //    var files = ListFiles();
        //    if (files is null)
        //    {
        //        return;
        //    }
        //    Parallel.ForEach(files, filesId =>
        //    {
        //        DeleteFile(filesId);
        //    });
        //}

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
