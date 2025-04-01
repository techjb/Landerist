namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class VertexAIBatchCleaner
    {
        public static void RemoveFiles()
        {
            DateTime dateTime = DateTime.Now.AddDays(Configuration.Config.DAYS_TO_REMOVE_BATCH_FILES);
            try
            {
                BatchPredictions.Clean(dateTime);
                CloudStorage.DeleteFiles(dateTime);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("VertexAIBatchCleaner Clean", exception);
            }
        }
    }
}
