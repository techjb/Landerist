using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class VertexAIBatchDownload
    {
        public static void BatchDownload(string batchId)
        {
            try
            {
                var batchPredictionJob = BatchPredictions.GetBatchPredictionJob(batchId);
                if (batchPredictionJob is null || !batchPredictionJob.State.Equals(JobState.Succeeded))
                {
                    return;
                }
                DownloadFile(batchPredictionJob);
            }
            catch (Exception e)
            {
                Logs.Log.WriteError("VertexAIBatchDownload BatchDownload", e);
            }
        }

        private static void DownloadFile(BatchPredictionJob batchPredictionJob)
        {
            string outputFilePath = Config.BATCH_DIRECTORY + batchPredictionJob.OutputConfig.GcsDestination.OutputUriPrefix.Split('/')[^1];
            string objectName = batchPredictionJob.OutputInfo.GcsOutputDirectory.
                Replace("gs://" + PrivateConfig.GOOGLE_CLOUD_BUCKET_NAME + "/", "") + "/predictions.jsonl";

            File.Delete(outputFilePath);
            if (!CloudStorage.DownloadFile(objectName, outputFilePath))
            {
                return;
            }

            // read download file.
            //Batches.UpdateToDownloaded(batchId);
        }
    }
}
