using landerist_library.Statistics;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Database
{
    public class ES_Listings
    {

        public const string TABLE_ES_LISTINGS = "[ES_LISTINGS]";

        public static void InsertUpdate(Website website, Listing newListing)
        {
            Listing? oldListing = GetListing(newListing.guid, true, true);
            if (oldListing != null)
            {
                if (!oldListing.Equals(newListing))
                {
                    if (Update(oldListing, newListing))
                    {
                        StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ListingUpdate);
                    }
                }
            }
            else
            {
                if (Insert(website, newListing))
                {
                    StatisticsSnapshot.InsertDailyCounter(StatisticsKey.ListingInsert);
                }
            }
        }

        private static bool Insert(Website website, Listing listing)
        {
            if (Insert(listing))
            {
                website.IncreaseNumListings();
                ES_Media.Insert(listing);
                ES_Sources.Insert(listing);
                return true;
            }
            else
            {
                Logs.Log.WriteError("ES_LISTINGS", "Insert error");
                return false;
            }
        }

        private static bool Insert(Listing listing)
        {
            string query =
                "INSERT INTO " + TABLE_ES_LISTINGS + " " +
                "VALUES( " +
                "@guid, @listingStatus, @listingDate, @updated, @unlistingDate, @operation, @propertyType, " +
                "@propertySubtype, @priceAmount, @priceCurrency, @description, " +
                "@contactName, @contactPhone, @contactEmail, @contactUrl, @contactOther, @address, @lauId, @latitude, @longitude, " +
                "@locationIsAccurate, @cadastralReference, @propertySize, @landSize, @constructionYear, " +
                "@constructionStatus, @floors, @floor, @bedrooms, @bathrooms, @parkings, @terrace, @garden, " +
                "@garage, @motorbikeGarage, @pool, @lift, @disabledAccess, @storageRoom, @furnished, " +
                "@nonFurnished, @heating, @airConditioning, @petsAllowed, @securitySystems " +
                ")";

            var queryParameters = GetQueryParameters(listing);
            return new DataBase().Query(query, queryParameters);
        }

        private static Dictionary<string, object?> GetQueryParameters(Listing listing)
        {
            return new Dictionary<string, object?> {
                {"guid", listing.guid },
                {"listingStatus", listing.listingStatus.ToString() },
                {"listingDate", listing.listingDate},
                {"updated", DateTime.Now},
                {"unlistingDate", listing.unlistingDate},
                {"operation", listing.operation.ToString() },
                {"propertyType", listing.propertyType.ToString() },
                {"propertySubType", listing.propertySubtype?.ToString()},
                {"priceAmount", listing.price?.amount },
                {"priceCurrency", listing.price?.currency.ToString()},
                {"description", listing.description },
                {"contactName", listing.contactName },
                {"contactPhone", listing.contactPhone },
                {"contactEmail", listing.contactEmail },
                {"contactUrl", listing.contactUrl?.ToString() },
                {"contactOther", listing.contactOther },
                {"address", listing.address },
                {"lauId", listing.lauId},
                {"latitude", listing.latitude},
                {"longitude", listing.longitude},
                {"locationIsAccurate", listing.locationIsAccurate},
                {"cadastralReference", listing.cadastralReference },
                {"propertySize", listing.propertySize},
                {"landSize", listing.landSize},
                {"constructionYear", listing.constructionYear},
                {"constructionStatus", listing.constructionStatus?.ToString()},
                {"floors", listing.floors},
                {"floor", listing.floor },
                {"bedrooms", listing.bedrooms },
                {"bathrooms", listing.bathrooms },
                {"parkings", listing.parkings },
                {"terrace", listing.terrace },
                {"garden", listing.garden },
                {"garage", listing.garage },
                {"motorbikeGarage", listing.motorbikeGarage },
                {"pool", listing.pool },
                {"lift", listing.lift },
                {"disabledAccess", listing.disabledAccess },
                {"storageRoom", listing.storageRoom },
                {"furnished", listing.furnished },
                {"nonFurnished", listing.nonFurnished },
                {"heating", listing.heating },
                {"airConditioning", listing.airConditioning },
                {"petsAllowed", listing.petsAllowed },
                {"securitySystems", listing.securitySystems },
            };
        }

        private static bool Update(Listing oldListing, Listing newListing)
        {
            if (!Update(newListing))
            {
                Logs.Log.WriteError("ES_LISTINGS", "Update error");
                return false;
            }
            if (!ListingMediaAreEquals(oldListing, newListing))
            {
                ES_Media.Update(newListing);
            }
            if (!ListingSourcesAreEquals(oldListing, newListing))
            {
                ES_Sources.Update(newListing);
            }
            return true;
        }

        private static bool ListingMediaAreEquals(Listing oldListing, Listing newListing)
        {
            return
                oldListing.media == newListing.media ||
                (oldListing.media != null && newListing.media != null && oldListing.media.SetEquals(newListing.media));
        }

        private static bool ListingSourcesAreEquals(Listing oldListing, Listing newListing)
        {
            return
                oldListing.sources == newListing.sources ||
                (oldListing.sources != null && newListing.sources != null && oldListing.sources.SetEquals(newListing.sources));
        }

        public static bool Update(Listing listing)
        {
            string query =
                "UPDATE " + TABLE_ES_LISTINGS + " SET " +
                "[listingStatus] = @listingStatus, " +
                "[listingDate] = @listingDate, " +
                "[updated] = @updated, " +
                "[unlistingDate] = @unlistingDate, " +
                "[operation] = @operation, " +
                "[propertyType] = @propertyType, " +
                "[propertySubtype] = @propertySubtype, " +
                "[priceAmount] = @priceAmount, " +
                "[priceCurrency] = @priceCurrency, " +
                "[description] = @description, " +
                "[contactName] = @contactName, " +
                "[contactPhone] = @contactPhone, " +
                "[contactEmail] = @contactEmail, " +
                "[contactUrl] = @contactUrl, " +
                "[contactOther] = @contactOther, " +
                "[address] = @address, " +
                "[lauId] = @lauId, " +
                "[latitude] = @latitude, " +
                "[longitude] = @longitude, " +
                "[locationIsAccurate] = @locationIsAccurate, " +
                "[cadastralReference] = @cadastralReference, " +
                "[propertySize] = @propertySize, " +
                "[landSize] = @landSize, " +
                "[constructionYear] = @constructionYear, " +
                "[constructionStatus] = @constructionStatus, " +
                "[floors] = @floors, " +
                "[floor] = @floor, " +
                "[bedrooms] = @bedrooms, " +
                "[bathrooms] = @bathrooms, " +
                "[parkings] = @parkings, " +
                "[terrace] = @terrace, " +
                "[garden] = @garden, " +
                "[garage] = @garage, " +
                "[motorbikeGarage] = @motorbikeGarage, " +
                "[pool] = @pool, " +
                "[lift] = @lift, " +
                "[disabledAccess] = @disabledAccess, " +
                "[storageRoom] = @storageRoom, " +
                "[furnished] = @furnished, " +
                "[nonFurnished] = @nonFurnished, " +
                "[heating] = @heating, " +
                "[airConditioning] = @airConditioning, " +
                "[petsAllowed] = @petsAllowed, " +
                "[securitySystems] = @securitySystems " +
                "WHERE [guid] = @guid";

            var queryParameters = GetQueryParameters(listing);
            return new DataBase().Query(query, queryParameters);
        }

        public static SortedSet<Listing> GetAll(bool loadMedia, bool loadSources)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_LISTINGS;

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetAll(dataTable, loadMedia, loadSources);
        }

        public static SortedSet<Listing> GetPublished()
        {
            return GetListinStatus(ListingStatus.published);
        }

        public static SortedSet<Listing> GetUnPublished()
        {
            return GetListinStatus(ListingStatus.unpublished);
        }

        public static SortedSet<Listing> GetListinStatus(ListingStatus listingStatus)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_LISTINGS + " " +
                "WHERE [listingStatus] = @listingStatus";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"listingStatus", listingStatus.ToString() },
            });
            return GetAll(dataTable, true, true);
        }

        public static SortedSet<Listing> GetUnpublishedListings(DateTime unlistingDate)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_LISTINGS + " " +
                "WHERE [listingStatus] = @listingStatus AND " +
                "[unlistingDate] < @unlistingDate";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"listingStatus", ListingStatus.unpublished.ToString() },
                {"unlistingDate", unlistingDate }
            });

            return ParseListings(dataTable, false, true);
        }

        public static SortedSet<Listing> GetListings(bool loadMedia, bool loadSources, DateOnly dateFrom, DateOnly dateTo)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_LISTINGS + " " +
                "WHERE " +
                "   CAST([updated] AS DATE) >= CAST(@DateFrom AS DATE) AND " +
                "   CAST([updated] AS DATE) <= CAST(@DateTo AS DATE)";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                { "DateFrom", dateFrom },
                { "DateTo", dateTo },
            });

            return GetAll(dataTable, loadMedia, loadSources);
        }

        private static SortedSet<Listing> ParseListings(DataTable dataTable, bool loadMedia, bool loadSources)
        {
            SortedSet<Listing> listings = new(new ListingComparer());
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var listing = GetListing(dataRow, loadMedia, loadSources);
                listings.Add(listing);
            }
            return listings;
        }

        private static SortedSet<Listing> GetAll(DataTable dataTable, bool loadMedia, bool loadSources)
        {
            SortedSet<Listing> listings = new(new ListingComparer());
            var sync = new object();
            Parallel.ForEach(dataTable.AsEnumerable(), new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1
            }, dataRow =>
            {
                var listing = GetListing(dataRow, loadMedia, loadSources);
                lock (sync)
                {
                    listings.Add(listing);
                }
            });
            return listings;
        }

        public static Listing? GetListing(Page page, bool loadMedia, bool loadSources)
        {
            return GetListing(page.UriHash, loadMedia, loadSources);
        }

        private static Listing? GetListing(string guid, bool loadMedia, bool loadSources)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_LISTINGS + " " +
                "WHERE [Guid] = @Guid";

            DataTable dataTable = new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Guid", guid }
            });

            if (dataTable.Rows.Count.Equals(1))
            {
                return GetListing(dataTable.Rows[0], loadMedia, loadSources);
            }
            return null;
        }

        private static Listing GetListing(DataRow dataRow, bool loadMedia, bool loadSources)
        {
            var listing = GetListingData(dataRow);
            if (loadMedia)
            {
                var media = ES_Media.GetMedia(listing);
                listing.SetMedia(media);
            }
            if (loadSources)
            {
                var sources = ES_Sources.GetSources(listing);
                listing.SetSources(sources);
            }
            return listing;
        }

        private static Listing GetListingData(DataRow dataRow)
        {
            Listing listing = new()
            {
                guid = (string)dataRow["guid"],
                listingStatus = (ListingStatus)Enum.Parse(typeof(ListingStatus), dataRow["listingStatus"].ToString()!),
                listingDate = GetDateTime(dataRow, "listingDate"),
                unlistingDate = GetDateTime(dataRow, "unlistingDate"),
                operation = (Operation)Enum.Parse(typeof(Operation), dataRow["operation"].ToString()!),
                propertyType = (PropertyType)Enum.Parse(typeof(PropertyType), dataRow["propertyType"].ToString()!),
                propertySubtype = dataRow["propertySubtype"] is DBNull ? null : (PropertySubtype)Enum.Parse(typeof(PropertySubtype), dataRow["propertySubtype"].ToString()!),
                price = dataRow["priceAmount"] is DBNull ? null : new Price()
                {
                    amount = Convert.ToDecimal(dataRow["priceAmount"]),
                    currency = (Currency)Enum.Parse(typeof(Currency), dataRow["priceCurrency"].ToString()!)
                },
                description = GetString(dataRow, "description"),
                contactName = GetString(dataRow, "contactName"),
                contactPhone = GetString(dataRow, "contactPhone"),
                contactEmail = GetString(dataRow, "contactEmail"),
                contactUrl = GetUri(dataRow, "contactUrl"),
                contactOther = GetString(dataRow, "contactOther"),
                address = GetString(dataRow, "address"),
                lauId = GetString(dataRow, "lauId"),
                latitude = GetDouble(dataRow, "latitude"),
                longitude = GetDouble(dataRow, "longitude"),
                locationIsAccurate = GetBoolean(dataRow, "locationIsAccurate"),
                cadastralReference = GetString(dataRow, "cadastralReference"),
                propertySize = GetDouble(dataRow, "propertySize"),
                landSize = GetDouble(dataRow, "landSize"),
                constructionYear = GetShort(dataRow, "constructionYear"),
                constructionStatus = dataRow["constructionStatus"] is DBNull ? null : (ConstructionStatus)Enum.Parse(typeof(ConstructionStatus), dataRow["constructionStatus"].ToString()!),
                floors = GetShort(dataRow, "floors"),
                floor = GetString(dataRow, "floor"),
                bedrooms = GetShort(dataRow, "bedrooms"),
                bathrooms = GetShort(dataRow, "bathrooms"),
                parkings = GetShort(dataRow, "parkings"),
                terrace = GetBoolean(dataRow, "terrace"),
                garden = GetBoolean(dataRow, "garden"),
                garage = GetBoolean(dataRow, "garage"),
                motorbikeGarage = GetBoolean(dataRow, "motorbikeGarage"),
                pool = GetBoolean(dataRow, "pool"),
                lift = GetBoolean(dataRow, "lift"),
                disabledAccess = GetBoolean(dataRow, "disabledAccess"),
                storageRoom = GetBoolean(dataRow, "storageRoom"),
                furnished = GetBoolean(dataRow, "furnished"),
                nonFurnished = GetBoolean(dataRow, "nonFurnished"),
                heating = GetBoolean(dataRow, "heating"),
                airConditioning = GetBoolean(dataRow, "airConditioning"),
                petsAllowed = GetBoolean(dataRow, "petsAllowed"),
                securitySystems = GetBoolean(dataRow, "securitySystems"),
            };

            return listing;
        }

        private static bool? GetBoolean(DataRow dataRow, string columnName)
        {
            return dataRow[columnName] is DBNull ? null : (bool)dataRow[columnName];
        }

        private static string? GetString(DataRow dataRow, string columnName)
        {
            return dataRow[columnName] is DBNull ? null : (string)dataRow[columnName];
        }

        private static short? GetShort(DataRow dataRow, string columnName)
        {
            return dataRow[columnName] is DBNull ? null : (short)dataRow[columnName];
        }

        private static double? GetDouble(DataRow dataRow, string columnName)
        {
            return dataRow[columnName] is DBNull ? null : (double)dataRow[columnName];
        }

        private static DateTime? GetDateTime(DataRow dataRow, string columnName)
        {
            return dataRow[columnName] is DBNull ? null : (DateTime)dataRow[columnName];
        }

        private static Uri? GetUri(DataRow dataRow, string columnName)
        {
            return dataRow[columnName] is DBNull ? null : new Uri((string)dataRow[columnName]);
        }

        public static bool Delete(Listing listing)
        {
            return Delete(listing.guid);
        }

        public static bool Delete(string guid)
        {
            string query =
                "DELETE FROM " + TABLE_ES_LISTINGS + " " +
                "WHERE [guid] = @guid";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"guid", guid }
            });
        }

        public static bool Delete()
        {
            string query =
                "DELETE FROM " + TABLE_ES_LISTINGS;

            return new DataBase().Query(query);
        }
    }
}
