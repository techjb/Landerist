using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;


namespace landerist_library.Parse.Listing.VertexAI
{
    public class VertexAIBatch
    {

        private static void BarchPredicionJob()
        {
            CreateBatchPredictionJobRequest createBatchPredictionJobRequest = new()
            {
                BatchPredictionJob = new()
                {
                    Name = "",
                    DisplayName = "",
                    Model = "",
                },
                Parent = $"projects/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_PROJECTID}/locations/{PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_LOCATION}"
            };

            var jobServiceClient = GetJobServiceClient();
            var batchPredictionJob = jobServiceClient.CreateBatchPredictionJob(createBatchPredictionJobRequest);
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
