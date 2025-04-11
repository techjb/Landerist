using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;
using landerist_library.Logs;


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

        public static void Clean(DateTime dateTime)
        {
            var listBatchPredictionJobsRequest = new ListBatchPredictionJobsRequest
            {
                Parent = GetParent(),
            };
            var listBatchPredictionJobsResponse = GetJobServiceClient().ListBatchPredictionJobs(listBatchPredictionJobsRequest);
            var total = listBatchPredictionJobsResponse.Count();
            int deleted = 0;
            var jobServiceClient = GetJobServiceClient();
            bool error = false;
            foreach (var batchPredictionJob in listBatchPredictionJobsResponse)
            {
                if (batchPredictionJob.State.Equals(JobState.Succeeded) && batchPredictionJob.EndTime.ToDateTime() < dateTime)
                {
                    if (!error && DeleteBatchPredictionJob(jobServiceClient, batchPredictionJob.Name))
                    {
                        deleted++;
                    }
                    else
                    {
                        error = true;
                    }
                }
            }
            Log.WriteInfo("BatchPredictions Clean", "Jobs: " + total + " Deleted: " + deleted);
        }

        public static bool DeleteBatchPredictionJob(JobServiceClient jobServiceClient, string name)
        {
            var deleteBatchPredictionJobRequest = new DeleteBatchPredictionJobRequest
            {
                Name = name,
            };

            try
            {
                var operation = jobServiceClient.DeleteBatchPredictionJob(deleteBatchPredictionJobRequest);
                operation.PollUntilCompleted();
                return true;
            }
            catch
            {
                return false;
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
