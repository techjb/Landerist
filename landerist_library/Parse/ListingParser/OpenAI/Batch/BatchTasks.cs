using landerist_library.Logs;

namespace landerist_library.Parse.ListingParser.OpenAI.Batch
{
    public class BatchTasks
    {
        public static void Start()
        {
            BatchDownload.Start();
            BatchCleaner.Start();            
            BatchUpload.Start();
        }
    }
}
