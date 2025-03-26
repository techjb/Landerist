using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class VertexAIBatch
    {

        public static void StartTest()
        {
            var createBatchPredictionJobRequest = new CreateBatchPredictionJobRequest
            {
                BatchPredictionJob = new BatchPredictionJob()
                {
                    Name = "new-run",
                    DisplayName = "New Run",
                    Model = "publishers/google/models/" + VertexAIRequest.ModelName,
                    InputConfig = new BatchPredictionJob.Types.InputConfig()
                    {
                        InstancesFormat = "jsonl",
                        GcsSource = new GcsSource()
                        {
                            Uris = { "gs://landerist-ia/test.json" }
                        }
                    },
                    OutputConfig = new BatchPredictionJob.Types.OutputConfig()
                    {
                        PredictionsFormat = "jsonl",
                        GcsDestination = new GcsDestination()
                        {
                            OutputUriPrefix = "gs://landerist-ia/output/",
                        },

                    },
                },
                
                Parent = GetParent()
            };

            var jobServiceClient = GetJobServiceClient();
            var batchPredictionJob = jobServiceClient.CreateBatchPredictionJob(createBatchPredictionJobRequest);                        
            Console.WriteLine(batchPredictionJob.BatchPredictionJobName.BatchPredictionJobId);

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
                Console.WriteLine(batchPredictionJob.DisplayName + " " + batchPredictionJob.State + " " + batchPredictionJob.BatchPredictionJobName.BatchPredictionJobId);
            }
        }

        private static void BarchPredicionJob()
        {
            var createBatchPredictionJobRequest = new CreateBatchPredictionJobRequest()
            {
                BatchPredictionJob =
                {
                    Name = "new-run",
                    DisplayName = "NewRun",
                    Model = "publishers/google/models/gemini-1.5-flash-001",
                    InputConfig =
                    {
                        InstancesFormat = "bigquery",
                        BigquerySource = {
                            InputUri = "bq://myprivateproject.datasetname.GeminiBatchTable"
                        }
                    },
                    OutputConfig =
                    {
                        PredictionsFormat = "bigquery",
                        BigqueryDestination = {
                            OutputUri = "bq://myprivateproject.datasetname.GeminiBatchTable"
                        }
                    }

                },
                Parent = GetParent()
            };

            var jobServiceClient = GetJobServiceClient();
            var batchPredictionJob = jobServiceClient.CreateBatchPredictionJob(createBatchPredictionJobRequest);
            var state = batchPredictionJob.State;
            Console.WriteLine(state.ToString());


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

        public static GenerateContentResponse? GetGenerateContentResponse(string text)
        {
            try
            {
                //return JsonConvert.DeserializeObject<GenerateContentResponse?>(text); // not working
                var data = (JObject?)JsonConvert.DeserializeObject(text);
                if (data == null)
                {
                    return null;
                }
                var candidates = data["candidates"];
                return null;
            }
            catch //(Exception e)
            {
                return null;
            }
        }
    }
}
