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
            Listing? oldListing = GetListing(newListing.guid);
            if (oldListing != null)
            {
                if (!oldListing.Equals(newListing))
                {
                    Update(oldListing, newListing);
                }
            }
            else
            {
                Insert(website, newListing);
            }
        }

        private static void Insert(Website website, Listing listing)
        {
            if (InsertData(listing))
            {
                website.IncreaseNumListings();
                ES_Media.Insert(listing);
            }
        }

        private static bool InsertData(Listing listing)
        {
            string query =
                "INSERT INTO " + TABLE_ES_LISTINGS + " " +
                "VALUES( " +
                "@guid, @listingStatus, @listingDate, @unlistingDate, @operation, @propertyType, " +
                "@propertySubtype, @priceAmount, @priceCurrency, @description, @dataSourceName, " +
                "@dataSourceGuid, @dataSourceUpdate, @dataSourceUrl, @contactName, @contactPhone, " +
                "@contactEmail, @contactUrl, @contactOther, @address, @lauId, @latitude, @longitude, " +
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
                {"unlistingDate", listing.unlistingDate},
                {"operation", listing.operation.ToString() },
                {"propertyType", listing.propertyType.ToString() },
                {"propertySubType", listing.propertySubtype?.ToString()},
                {"priceAmount", listing.price?.amount },
                {"priceCurrency", listing.price?.currency.ToString()},
                {"description", listing.description },
                {"dataSourceName", listing.dataSourceName },
                {"dataSourceGuid", listing.dataSourceGuid },
                {"dataSourceUpdate", listing.dataSourceUpdate},
                {"dataSourceUrl", listing.dataSourceUrl?.ToString() },
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

        private static void Update(Listing oldListing, Listing newListing)
        {
            Update(newListing);
            if (!ListingMediaAreEquals(oldListing, newListing))
            {
                ES_Media.Update(newListing);
            }
        }

        private static bool ListingMediaAreEquals(Listing oldListing, Listing newListing)
        {
            return
                oldListing.media == newListing.media ||
                (oldListing.media != null && newListing.media != null && oldListing.media.SetEquals(newListing.media));
        }

        public static bool Update(Listing listing)
        {
            string query =
                "UPDATE " + TABLE_ES_LISTINGS + " SET " +
                "[listingStatus] = @listingStatus, " +
                "[listingDate] = @listingDate, " +
                "[unlistingDate] = @unlistingDate, " +
                "[operation] = @operation, " +
                "[propertyType] = @propertyType, " +
                "[propertySubtype] = @propertySubtype, " +
                "[priceAmount] = @priceAmount, " +
                "[priceCurrency] = @priceCurrency, " +
                "[description] = @description, " +
                "[dataSourceName] = @dataSourceName, " +
                "[dataSourceGuid] = @dataSourceGuid, " +
                "[dataSourceUpdate] = @dataSourceUpdate, " +
                "[dataSourceUrl] = @dataSourceUrl, " +
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

        public static SortedSet<Listing> GetListingsWithParalelism(bool loadMedia)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_LISTINGS;

            DataTable dataTable = new DataBase().QueryTable(query);
            return GetListingsWithParalelism(dataTable, loadMedia);
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

            return GetListings(dataTable, false);
        }

        public static SortedSet<Listing> GetListingsWithParalelism(bool loadMedia, DateTime dataSourceUpdate)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_LISTINGS + " " +
                "WHERE CONVERT(date, [dataSourceUpdate]) = CONVERT(date, @DataSourceUpdate)";

            DataTable dataTable = new DataBase().QueryTable(query, "DataSourceUpdate", dataSourceUpdate);
            return GetListingsWithParalelism(dataTable, loadMedia);
        }

        private static SortedSet<Listing> GetListings(DataTable dataTable, bool loadMedia)
        {
            SortedSet<Listing> listings = new(new ListingComparer());
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var listing = GetListing(dataRow, loadMedia);
                listings.Add(listing);
            }
            return listings;
        }

        private static SortedSet<Listing> GetListingsWithParalelism(DataTable dataTable, bool loadMedia)
        {
            SortedSet<Listing> listings = new(new ListingComparer());
            var sync = new object();
            Parallel.ForEach(dataTable.AsEnumerable(), new ParallelOptions()
            {
                //MaxDegreeOfParallelism = Configuration.Config.MAX_DEGREE_OF_PARALLELISM
            }, dataRow =>
            {
                var listing = GetListing(dataRow, loadMedia);
                lock (sync)
                {
                    listings.Add(listing);
                }
            });
            return listings;
        }

        public static Listing? GetListing(Page page, bool loadMedia = true)
        {
            return GetListing(page.UriHash, loadMedia);
        }

        private static Listing? GetListing(string guid, bool loadMedia = true)
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
                return GetListing(dataTable.Rows[0], loadMedia);
            }
            return null;
        }

        private static Listing GetListing(DataRow dataRow, bool loadMedia)
        {
            var listing = GetListingData(dataRow);
            if (loadMedia)
            {
                var media = ES_Media.GetMedia(listing);
                listing.SetMedia(media);
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
                dataSourceName = GetString(dataRow, "dataSourceName"),
                dataSourceGuid = GetString(dataRow, "dataSourceGuid"),
                dataSourceUpdate = GetDateTime(dataRow, "dataSourceUpdate"),
                dataSourceUrl = GetUri(dataRow, "dataSourceUrl"),
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


        public static DataTable GetTrainingListings()
        {
            string query =
                "SELECT " +
                Pages.PAGES + ".[ResponseBodyText], " +
                "[operation], " +
                "[propertyType], " +
                "[propertySubtype], " +
                "[priceAmount], " +
                "[description], " +
                "[dataSourceGuid], " +
                "[contactPhone], " +
                "[contactEmail], " +
                "[address], " +
                "[cadastralReference], " +
                "[propertySize], " +
                "[landSize], " +
                "[constructionYear], " +
                "[constructionStatus], " +
                "[floors], " +
                "[floor], " +
                "[bedrooms], " +
                "[bathrooms], " +
                "[parkings], " +
                "[terrace], " +
                "[garden], " +
                "[garage], " +
                "[motorbikeGarage], " +
                "[pool], " +
                "[lift], " +
                "[disabledAccess], " +
                "[storageRoom], " +
                "[furnished], " +
                "[nonFurnished], " +
                "[heating], " +
                "[airConditioning], " +
                "[petsAllowed], " +
                "[securitySystems] " +
                "FROM " + TABLE_ES_LISTINGS + " " +
                "INNER JOIN PAGES ON " + TABLE_ES_LISTINGS + ".[guid] = " + Pages.PAGES + ".[UriHash] ";

            return new DataBase().QueryTable(query);
        }
    }
}
