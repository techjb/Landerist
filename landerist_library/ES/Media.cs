﻿using landerist_orels.ES;
using System.Data;

namespace landerist_library.ES
{
    public class Media
    {
        public static string TABLE_ES_MEDIA = "[ES_MEDIA]";

        public static void Insert(Listing listing)
        {
            foreach (var media in listing.media)
            {
                string query =
                    "INSERT INTO " + TABLE_ES_MEDIA + " " +
                    "VALUES(@ListingGuid ,@MediaType ,@Title ,@Url)";

                new Database().Query(query, new Dictionary<string, object?> {
                    {"listingGuid", listing.guid },
                    {"mediaType", media.mediaType },
                    {"title", media.title },
                    {"url", media.url},
                });
            }
        }

        public static SortedSet<landerist_orels.ES.Media> GetMedia(Listing listing)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_MEDIA + " " +
                "WHERE [listingGuid] = @listingGuid";

            DataTable dataTable = new Database().QueryTable(query, new Dictionary<string, object?>()
            {
                { "listingGuid", listing.guid }
            });

            SortedSet<landerist_orels.ES.Media> medias = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var media = GetMedia(dataRow);
                medias.Add(media);
            }
            return medias;
        }

        private static landerist_orels.ES.Media GetMedia(DataRow dataRow)
        {
            return new landerist_orels.ES.Media()
            {
                mediaType = dataRow["mediaType"] is DBNull ? null : (MediaType)dataRow["mediaType"],
                title = dataRow["title"] is DBNull ? null : (string)dataRow["title"],
                url = new Uri((string)dataRow["url"])
            };
        }
    }
}