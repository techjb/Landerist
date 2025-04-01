namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class OpenAIBatchCleaner : OpenAIBatchClient
    {
        public static bool Delete(string batchId)
        {
            var batchResponse = GetBatch(batchId);
            if (batchResponse == null || !BatchIsCompleted(batchResponse))
            {
                return false;
            }
            if (DeleteFile(batchResponse.InputFileId) && DeleteFile(batchResponse.OutputFileId) && DeleteFile(batchResponse.ErrorFileId))
            {
                return true;
            }
            return false;
        }

        public static void RemoveFiles()
        {
            var files = ListFiles();
            if (files is null || files.Count.Equals(0))
            {
                return;
            }
            DateTime dateTime = DateTime.Now.AddDays(Configuration.Config.DAYS_TO_REMOVE_BATCH_FILES);
            Parallel.ForEach(files, Configuration.Config.PARALLELOPTIONS1INLOCAL, file =>
            {
                if (file.CreatedAt < dateTime)
                {
                    DeleteFile(file);
                }
            });
        }
    }
}
