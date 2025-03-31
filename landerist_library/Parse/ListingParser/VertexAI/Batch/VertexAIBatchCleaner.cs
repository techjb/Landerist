namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class VertexAIBatchCleaner
    {
        private static readonly int DaysToCleanFiles = Configuration.Config.IsConfigurationLocal() ? 1 : -7;
        public static void Clean()
        {
            DateTime dateTime = DateTime.Now.AddDays(DaysToCleanFiles);
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
