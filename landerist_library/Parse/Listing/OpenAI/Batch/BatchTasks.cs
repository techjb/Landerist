namespace landerist_library.Parse.Listing.OpenAI.Batch
{
    public class BatchTasks
    {
        public static void Start()
        {
            BatchDownload.Start();
            BatchDownload.Clean();
            BatchUpload.Start();
        }
    }
}
