using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Parse.ListingParser;
using landerist_library.Parse.ListingParser.OpenAI.Batch;


namespace landerist_library.Tasks
{ 
    public class BatchDownload
    {
        public static void Start()
        {
            var batchIds = Batches.SelectNonDownloaded();
            Parallel.ForEach(batchIds,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1}, 
                batchId =>
            {
                Download(batchId);
            });
        }

        private static void Download(string batchId)
        {
            switch (Config.LLM_PROVIDER)
            {
                case LLMProviders.OpenAI: OpenAIBatchDownload.BatchDownload(batchId); break;
                //case LLMProviders.VertexAI: return VertexAIRequest.BatchIsComplete(batchId);

                //case LLMProviders.Anthropic: return AnthropicRequest.BatchIsComplete(batchId);
                default: break;
            }
        }
    }
}
