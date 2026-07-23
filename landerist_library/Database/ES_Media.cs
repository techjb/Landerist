using landerist_library.Infrastructure.Sql;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Database
{
    public class ES_Media
    {
        public const string TABLE_ES_MEDIA = "[ES_MEDIA]";
        private static readonly MediaRepository Repository = new(global::landerist_library.Database.LegacyDatabase.Create());

        public static void Insert(Listing listing)
        {
            Repository.Insert(listing);
        }

        public static void Update(Listing listing)
        {
            Delete(listing);
            Insert(listing);
        }

        public static bool Delete(Listing listing)
        {
            return Delete(listing.guid);
        }

        public static bool Delete(string guid)
        {
            return Repository.Delete(guid);
        }

        public static bool Delete()
        {
            return Repository.Delete();
        }

        public static SortedSet<Media> GetMedia(Listing listing)
        {
            DataTable dataTable = Repository.GetMedia(listing);

            SortedSet<Media> medias = new(new MediaComparer());
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var media = GetMedia(dataRow);
                if (media != null)
                {
                    medias.Add(media);
                }
            }
            return medias;
        }

        internal static Media? GetMedia(DataRow dataRow)
        {
            MediaType? mediaType = dataRow["mediaType"] is DBNull ? null : (MediaType)Enum.Parse(typeof(MediaType), dataRow["mediaType"].ToString()!);
            var title = dataRow["title"] is DBNull ? null : (string)dataRow["title"];
            if (!Uri.TryCreate((string)dataRow["url"], UriKind.Absolute, out Uri? uri))
            {
                return null;
            }

            return new Media()
            {
                mediaType = mediaType,
                title = title,
                url = uri
            };
        }
    }
}
