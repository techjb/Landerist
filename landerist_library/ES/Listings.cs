using landerist_library.Database;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.ES
{
    public class Listings
    {

        public static string TABLE_ES_LISTINGS = "[ES_LISTINGS]";

        public Listings()
        {

        }

        public void Insert(Listing listing)
        {
            InsertData(listing);
            Media.Insert(listing);
        }

        private bool InsertData(Listing listing)
        {
            string query =
                "INSERT INTO " + TABLE_ES_LISTINGS + " " +
                "VALUES( " +
                "@guid, @listingStatus, @listingDate, @unlistingDate, @operation, @propertyType, @propertySubtype, @priceAmount, @priceCurrency, @description, @dataSourceName, @dataSourceGuid, @dataSourceUpdate, @dataSourceUrl, @contactName, @contactPhone, @contactEmail, @contactUrl, @contactOther, @address, @latitude, @longitude, @locationIsAccurate, @cadastralReference, @propertySize, @landSize, @constructionYear, @constructionStatus, @floors, @floor, @bedrooms, @bathrooms, @parkings, @terrace, @garden, @garage, @motorbikeGarage, @pool, @lift, @disabledAccess, @storageRoom, @furnished, @nonFurnished, @heating, @airConditioning, @petsAllowed, @securitySystems " +
                ")";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"guid", listing.guid },
                {"listingStatus", listing.listingStatus.ToString() },
                {"listingDate", listing.listingDate},
                {"unlistingDate", listing.unlistingDate},
                {"operation", listing.operation.ToString() },
                {"propertyType", listing.propertyType.ToString() },
                {"propertySubType", listing.propertySubtype?.ToString()},
                {"priceAmount", listing.price?.amount },
                {"priceCurrency", listing.price?.currency},
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
                {"terrace", listing.features!=null && listing.features.Contains(Feature.terrace) ?true: DBNull.Value },
                {"garden", listing.features!=null && listing.features.Contains(Feature.garden) ?true: DBNull.Value },
                {"garage", listing.features!=null && listing.features.Contains(Feature.garage)?true: DBNull.Value  },
                {"motorbikeGarage", listing.features!=null && listing.features.Contains(Feature.motorbike_garage) ?true: DBNull.Value },
                {"pool", listing.features!=null && listing.features.Contains(Feature.pool) ?true: DBNull.Value },
                {"lift", listing.features!=null && listing.features.Contains(Feature.lift) ?true: DBNull.Value },
                {"disabledAccess", listing.features!=null && listing.features.Contains(Feature.disabled_access) ?true: DBNull.Value },
                {"storageRoom", listing.features != null && listing.features.Contains(Feature.storage_room) ? true : DBNull.Value },
                {"furnished", listing.features != null && listing.features.Contains(Feature.furnished) ? true : DBNull.Value },
                {"nonFurnished", listing.features != null && listing.features.Contains(Feature.non_furnished) ? true : DBNull.Value },
                {"heating", listing.features != null && listing.features.Contains(Feature.heating) ? true : DBNull.Value },
                {"airConditioning", listing.features != null && listing.features.Contains(Feature.air_conditioning) ? true : DBNull.Value },
                {"petsAllowed", listing.features != null && listing.features.Contains(Feature.pets_allowed) ? true : DBNull.Value },
                {"securitySystems", listing.features != null && listing.features.Contains(Feature.security_systems) ? true : DBNull.Value },
            });
        }


        public SortedSet<Listing> GetAll(bool loadMedia)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_LISTINGS;

            DataTable dataTable = new DataBase().QueryTable(query);

            SortedSet<Listing> listings = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var listing = GetListing(dataRow);
                if (loadMedia)
                {
                    var media = Media.GetMedia(listing);
                    listing.SetMedia(media);
                }
                listings.Add(listing);
            }
            return listings;
        }

        public Listing GetListing(DataRow dataRow)
        {
            var listing = new Listing
            {
                guid = (string)dataRow["guid"],
                listingStatus = (ListingStatus)dataRow["listingStatus"],
                listingDate = dataRow["listingDate"] is DBNull ? null : (DateTime)dataRow["listingDate"],
                unlistingDate = dataRow["unlistingDate"] is DBNull ? null : (DateTime)dataRow["unlistingDate"],
                operation = (Operation)dataRow["operation"],
                propertyType = (PropertyType)dataRow["propertyType"],
                propertySubtype = dataRow["propertySubtype"] is DBNull ? null : (PropertySubtype)dataRow["propertySubtype"],
                price = dataRow["priceAmount"] is DBNull ? null : new Price()
                {
                    amount = Convert.ToDecimal(dataRow["priceAmount"]),
                    currency = (Currency)dataRow["priceCurrency"]
                },
                description = dataRow["description"] is DBNull ? null : (string)dataRow["description"],
                dataSourceName = dataRow["dataSourceName"] is DBNull ? null : (string)dataRow["dataSourceName"],
                dataSourceGuid = dataRow["dataSourceGuid"] is DBNull ? null : (string)dataRow["dataSourceGuid"],
                dataSourceUpdate = dataRow["dataSourceUpdate"] is DBNull ? null : (DateTime)dataRow["dataSourceUpdate"],
                dataSourceUrl = dataRow["dataSourceUrl"] is DBNull ? null : new Uri((string)dataRow["dataSourceUrl"]),
                contactName = dataRow["contactName"] is DBNull ? null : (string)dataRow["contactName"],
                contactPhone = dataRow["contactPhone"] is DBNull ? null : (string)dataRow["contactPhone"],
                contactEmail = dataRow["contactEmail"] is DBNull ? null : (string)dataRow["contactEmail"],
                contactUrl = dataRow["contactEmail"] is DBNull ? null : new Uri((string)dataRow["contactEmail"]),
                contactOther = dataRow["contactOther"] is DBNull ? null : (string)dataRow["contactOther"],
                address = dataRow["address"] is DBNull ? null : (string)dataRow["address"],
                latitude = dataRow["latitude"] is DBNull ? null : (double)dataRow["latitude"],
                longitude = dataRow["longitude"] is DBNull ? null : (double)dataRow["longitude"],
                locationIsAccurate = dataRow["locationIsAccurate"] is DBNull ? null : (bool)dataRow["locationIsAccurate"],
                cadastralReference = dataRow["cadastralReference"] is DBNull ? null : (string)dataRow["cadastralReference"],
                propertySize = dataRow["propertySize"] is DBNull ? null : (double)dataRow["propertySize"],
                landSize = dataRow["landSize"] is DBNull ? null : (double)dataRow["landSize"],
                constructionYear = dataRow["constructionYear"] is DBNull ? null : (int)dataRow["constructionYear"],
                constructionStatus = dataRow["constructionStatus"] is DBNull ? null : (ConstructionStatus)dataRow["constructionStatus"],
                floors = dataRow["floors"] is DBNull ? null : (int)dataRow["floors"],
                floor = dataRow["floor"] is DBNull ? null : (string)dataRow["floor"],
                bedrooms = dataRow["bedrooms"] is DBNull ? null : (int)dataRow["bedrooms"],
                bathrooms = dataRow["bathrooms"] is DBNull ? null : (int)dataRow["bathrooms"],
                parkings = dataRow["parkings"] is DBNull ? null : (int)dataRow["parkings"],
            };

            AddFeature(listing, dataRow, "terrace", Feature.terrace);
            AddFeature(listing, dataRow, "garden", Feature.garden);
            AddFeature(listing, dataRow, "garage", Feature.garage);
            AddFeature(listing, dataRow, "motorbikeGarage", Feature.motorbike_garage);
            AddFeature(listing, dataRow, "pool", Feature.pool);
            AddFeature(listing, dataRow, "lift", Feature.lift);
            AddFeature(listing, dataRow, "disabledAccess", Feature.disabled_access);
            AddFeature(listing, dataRow, "storageRoom", Feature.storage_room);
            AddFeature(listing, dataRow, "furnished", Feature.furnished);
            AddFeature(listing, dataRow, "nonFurnished", Feature.non_furnished);
            AddFeature(listing, dataRow, "heating", Feature.heating);
            AddFeature(listing, dataRow, "airConditioning", Feature.air_conditioning);
            AddFeature(listing, dataRow, "petsAllowed", Feature.pets_allowed);
            AddFeature(listing, dataRow, "securitySystems", Feature.security_systems);

            return listing;
        }

        private void AddFeature(Listing listing, DataRow dataRow, string rowName, Feature feature)
        {
            if (dataRow[rowName] is DBNull)
            {
                return;
            }

            var value = (bool?)dataRow[rowName];
            listing.AddFeature(value, feature);
        }
    }
}
