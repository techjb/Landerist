using landerist_library.Parse.ListingParser.OpenAI.Batch;

namespace landerist_library.Tasks
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
