using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;
using landerist_library.Database;


namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class BatchPredictions
    {
        public static string? CreateBatch(string name)
        {
            string outputName = name.Replace("input/", "output/").Replace("_input", "_output");
            var createBatchPredictionJobRequest = new CreateBatchPredictionJobRequest
            {
                BatchPredictionJob = new BatchPredictionJob()
                {
                    Name = name,
                    DisplayName = name,
                    Model = "publishers/google/models/" + VertexAIRequest.ModelName,
                    InputConfig = new BatchPredictionJob.Types.InputConfig()
                    {
                        InstancesFormat = "jsonl",
                        GcsSource = new GcsSource()
                        {
                            Uris = { "gs://" + PrivateConfig.GOOGLE_CLOUD_BUCKET_NAME + "/" + name }
                        }
                    },
                    OutputConfig = new BatchPredictionJob.Types.OutputConfig()
                    {
                        PredictionsFormat = "jsonl",
                        GcsDestination = new GcsDestination()
                        {
                            OutputUriPrefix = "gs://" + PrivateConfig.GOOGLE_CLOUD_BUCKET_NAME + "/" + outputName,
                        },
                    },
                },
                Parent = GetParent()
            };

            try
            {
                var jobServiceClient = GetJobServiceClient();
                var batchPredictionJob = jobServiceClient.CreateBatchPredictionJob(createBatchPredictionJobRequest);
                return batchPredictionJob.Name;
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("BatchPredictions CreateBatch", exception);
                return null;
            }
        }

        //public static void BatchDownload(string batchId)
        //{
        //    var batchPredicionJob = GetBatchPredictionJob(batchId);
        //    if (batchPredicionJob is null || !batchPredicionJob.State.Equals(JobState.Succeeded))
        //    {
        //        return;
        //    }
        //    string outputFilePath = Config.BATCH_DIRECTORY + batchPredicionJob.OutputConfig.GcsDestination.OutputUriPrefix.Split('/')[^1];
        //    string objectName = batchPredicionJob.OutputInfo.GcsOutputDirectory.
        //        Replace("gs://" + PrivateConfig.GOOGLE_CLOUD_BUCKET_NAME + "/", "") + "/predictions.jsonl";

        //    File.Delete(outputFilePath);
        //    if (!CloudStorage.DownloadFile(objectName, outputFilePath))
        //    {
        //        return;                
        //    }

        //    // read file.
        //    //Batches.UpdateToDownloaded(batchId);
        //}

        public static BatchPredictionJob? GetBatchPredictionJob(string name)
        {
            var jobServiceClient = GetJobServiceClient();
            var getBatchPredictionJobRequest = new GetBatchPredictionJobRequest
            {
                Name = name
            };
            try
            {
                return jobServiceClient.GetBatchPredictionJob(getBatchPredictionJobRequest);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("BatchPredictions GetBatchPredictionJob", exception);
                return null;
            }
        }

        public static void ListAllPredictionJobs()
        {
            var jobServiceClient = GetJobServiceClient();
            var listBatchPredictionJobsRequest = new ListBatchPredictionJobsRequest
            {
                Parent = GetParent(),
            };
            var listBatchPredictionJobsResponse = jobServiceClient.ListBatchPredictionJobs(listBatchPredictionJobsRequest);
            foreach (var batchPredictionJob in listBatchPredictionJobsResponse)
            {
                Console.WriteLine(batchPredictionJob.Name);
            }
        }

        private static string GetParent()
        {
            return $"projects/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PROJECTID}/locations/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}";
        }
        private static JobServiceClient GetJobServiceClient()
        {
            return new JobServiceClientBuilder
            {
                Endpoint = $"{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}-aiplatform.googleapis.com",
                JsonCredentials = PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL,
            }.Build();
        }
    }
}
