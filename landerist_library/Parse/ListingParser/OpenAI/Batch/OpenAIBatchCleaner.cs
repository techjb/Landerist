using landerist_library.Database;
using OpenCvSharp.XImgProc;

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

        public static void DeleteAllRemoteFiles()
        {
            var batches = Batches.SelectAll(LLMProvider.OpenAI);
            if (batches.Count > 0)
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
