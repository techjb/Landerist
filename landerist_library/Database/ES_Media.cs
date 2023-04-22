using landerist_orels.ES;
using System.Data;

namespace landerist_library.Database
{
    public class ES_Media
    {
        public const string TABLE_ES_MEDIA = "[ES_MEDIA]";

        public static void Insert(Listing listing)
        {
            if (listing.media == null)
            {
                return;
            }

            foreach (var media in listing.media)
            {
                string query =
                    "INSERT INTO " + TABLE_ES_MEDIA + " " +
                    "VALUES(@ListingGuid ,@MediaType ,@Title ,@Url)";

                new DataBase().Query(query, new Dictionary<string, object?> {
                    {"listingGuid", listing.guid },
                    {"mediaType", media.mediaType.ToString() },
                    {"title", media.title },
                    {"url", media.url?.ToString()},
                });
            }
        }

        public static void Update(Listing listing)
        {
            Delete(listing);
            Insert(listing);
        }

        private static bool Delete(Listing listing)
        {
            string query =
                "DELETE " +
                "FROM " + TABLE_ES_MEDIA + " " +
                "WHERE [listingGuid] = @listingGuid";

            return new DataBase().Query(query, new Dictionary<string, object?>()
            {
                { "listingGuid", listing.guid }
            });
        }

        public static SortedSet<Media> GetMedia(Listing listing)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_MEDIA + " " +
                "WHERE [listingGuid] = @listingGuid";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                { "listingGuid", listing.guid }
            });

            SortedSet<Media> medias = new(new MediaComparer());
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var media = GetMedia(dataRow);
                medias.Add(media);
            }
            return medias;
        }

        private static Media GetMedia(DataRow dataRow)
        {
            return new Media()
            {
                mediaType = dataRow["mediaType"] is DBNull ? null : (MediaType)Enum.Parse(typeof(MediaType), dataRow["mediaType"].ToString()!),
                title = dataRow["title"] is DBNull ? null : (string)dataRow["title"],
                url = new Uri((string)dataRow["url"])
            };
        }
    }
}
