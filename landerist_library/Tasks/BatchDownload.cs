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
            var batches = Batches.SelectNonDownloaded();
            Parallel.ForEach(batches, Config.PARALLELOPTIONS1INLOCAL, Download);
        }

        private static void Download(Batch batch)
        {
            switch (batch.LLMProvider)
            {
                case LLMProvider.OpenAI: OpenAIBatchDownload.BatchDownload(batch.Id); break;
                //case LLMProviders.VertexAI: return VertexAIRequest.BatchIsComplete(batchId);

                //case LLMProviders.Anthropic: return AnthropicRequest.BatchIsComplete(batchId);
                default: break;
            }
        }
    }
}
