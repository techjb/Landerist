using landerist_library.Configuration;
using landerist_library.Infrastructure.Sql;
using landerist_library.Parse.ListingParser;

namespace landerist_library.Database
{
    public class Batches
    {
        private static readonly BatchRepository Repository = new();

        public static bool Insert(string id, HashSet<string> pagesUriHashes)
        {
            return Repository.Insert(id, pagesUriHashes, Config.LLM_PROVIDER);
        }

        public static bool Delete(string id)
        {
            return Repository.Delete(id);
        }

        public static List<Batch> SelectNonDownloaded()
        {
            return Repository.Select(downloaded: false);
        }

        public static List<Batch> SelectDownloaded()
        {
            return Repository.Select(downloaded: true);
        }

        public static Batch? Select(string id)
        {
            return Repository.Select(id);
        }

        public static List<string> SelectAll(LLMProvider lLMProvider)
        {
            return Repository.SelectAll(lLMProvider);
        }

        public static bool UpdateToDownloaded(Batch batch)
        {
            return Repository.Update(batch.Id, downloaded: true);
        }

        public static bool UpdateToDownloaded(string id)
        {
            return Repository.Update(id, downloaded: true);
        }
    }
}
