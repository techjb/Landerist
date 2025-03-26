using landerist_library.Database;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class OpenAIBatchCleaner : OpenAIBatchClient
    {
        public static void Delete(string batchId)
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
    }
}
