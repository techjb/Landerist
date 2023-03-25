using landerist_orels.ES;
using System.Data;

namespace landerist_library.ES
{
    public class Listings
    {

        private const string TABLE_ES_LISTINGS = "[ES_LISTINGS]";

        private const string TABLE_ES_MEDIA = "[ES_MEDIA]";

        public Listings()
        {

        }

        public void Insert(Listing listing)
        {
            InsertListingData(listing);
            InsertListingMedia(listing);
        }

        private bool InsertListingData(Listing listing)
        {
            string query =
                "INSERT INTO " + TABLE_ES_LISTINGS + " " +
                "VALUES( " +
                "@Guid ,@ListingStatus ,@ListingDate ,@UnlistingDate ,@Operation ,@PropertyType ,@PropertySubType ,@PriceAmount ,@Description ,@DataSourceName ,@DataSourceGuid ,@DataSourceUpdate ,@DataSourceUrl ,@ContactName ,@ContactPhone ,@ContactEmail ,@ContactUrl ,@ContactOther ,@Address ,@Latitude ,@Longitude ,@LocationIsAccurate ,@CadastralReference ,@PropertySize ,@LandSize ,@ConstructionYear ,@ConstructionStatus ,@Floors ,@Floor ,@Bedrooms ,@Bathrooms ,@Parkings ,@Terrace ,@Garden ,@Garage ,@MotorbikeGarage ,@Pool ,@Lift ,@DisabledAccess ,@StorageRoom ,@Furnished ,@NonFurnished ,@Heating ,@AirConditioning ,@PetsAllowed ,@SecuritySystems" +
                ")";

            return new Database().Query(query, new Dictionary<string, object?> {
                {"Guid", listing.guid },
                {"ListingStatus", listing.listingStatus },
                {"ListingDate", listing.listingDate},
                {"UnlistingDate", listing.unlistingDate},
                {"Operation", listing.operation },
                {"PropertyType", listing.propertyType },
                {"PropertySubType", listing.propertySubtype},
                {"PriceAmount", listing.price.amount },
                {"Description", listing.description },
                {"DataSourceName", listing.dataSourceName },
                {"DataSourceGuid", listing.dataSourceGuid },
                {"DataSourceUpdate", listing.dataSourceUpdate},
                {"DataSourceUrl", listing.dataSourceUrl },
                {"ContactName", listing.contactName },
                {"ContactPhone", listing.contactPhone },
                {"ContactEmail", listing.contactEmail },
                {"ContactUrl", listing.contactUrl },
                {"ContactOther", listing.contactOther },
                {"Address", listing.address },
                {"Latitude", listing.latitude},
                {"Longitude", listing.longitude},
                {"LocationIsAccurate", listing.locationIsAccurate},
                {"CadastralReference", listing.cadastralReference },
                {"PropertySize", listing.propertySize},
                {"LandSize", listing.landSize},
                {"ConstructionYear", listing.constructionYear},
                {"ConstructionStatus", listing.constructionStatus},
                {"Floors", listing.floors},
                {"Floor", listing.floor },
                {"Bedrooms", listing.bedrooms },
                {"Bathrooms", listing.bathrooms },
                {"Parkings", listing.parkings },
                {"Terrace", listing.features!=null && listing.features.Contains(Feature.terrace) ?true: DBNull.Value },
                {"Garden", listing.features!=null && listing.features.Contains(Feature.garden) ?true: DBNull.Value },
                {"Garage", listing.features!=null && listing.features.Contains(Feature.garage)?true: DBNull.Value  },
                {"MotorbikeGarage", listing.features!=null && listing.features.Contains(Feature.motorbike_garage) ?true: DBNull.Value },
                {"Pool", listing.features!=null && listing.features.Contains(Feature.pool) ?true: DBNull.Value },
                {"Lift", listing.features!=null && listing.features.Contains(Feature.lift) ?true: DBNull.Value },
                {"DisabledAccess", listing.features!=null && listing.features.Contains(Feature.disabled_access) ?true: DBNull.Value },
                {"StorageRoom", listing.features != null && listing.features.Contains(Feature.storage_room) ? true : DBNull.Value },
                {"Furnished", listing.features != null && listing.features.Contains(Feature.furnished) ? true : DBNull.Value },
                {"NonFurnished", listing.features != null && listing.features.Contains(Feature.non_furnished) ? true : DBNull.Value },
                {"Heating", listing.features != null && listing.features.Contains(Feature.heating) ? true : DBNull.Value },
                {"AirConditioning", listing.features != null && listing.features.Contains(Feature.air_conditioning) ? true : DBNull.Value },
                {"PetsAllowed", listing.features != null && listing.features.Contains(Feature.pets_allowed) ? true : DBNull.Value },
                {"SecuritySystems", listing.features != null && listing.features.Contains(Feature.security_systems) ? true : DBNull.Value },
            });
        }

        private void InsertListingMedia(Listing listing)
        {
            foreach (var media in listing.media)
            {
                string query =
                    "INSERT INTO " + TABLE_ES_MEDIA + " " +
                    "VALUES(@ListingGuid ,@MediaType ,@Title ,@Url)";

                new Database().Query(query, new Dictionary<string, object?> {
                    {"ListingGuid", listing.guid },
                    {"MediaType", media.mediaType },
                    {"Title", media.title },
                    {"Url", media.url},
                });
            }
        }

        public List<Listing> GetAll()
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_LISTINGS;

            DataTable dataTable = new Database().QueryTable(query);

            List<Listing> listings = new();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var listing = GetListing(dataRow);
                LoadListingMedia(listing);
                listings.Add(listing);
            }
            return listings;
        }

        public Listing GetListing(DataRow dataRow)
        {
            var listing = new Listing
            {
                guid = (string)dataRow["Guid"],
                listingStatus = (ListingStatus)dataRow["ListingStatus"],
                listingDate = dataRow["ListingDate"] is DBNull ? null : (DateTime)dataRow["ListingDate"],
                unlistingDate = dataRow["UnlistingDate"] is DBNull ? null : (DateTime)dataRow["UnlistingDate"],
                operation = (Operation)dataRow["Operation"],
                propertyType = (PropertyType)dataRow["PropertyType"],
                propertySubtype = dataRow["PropertySubtype"] is DBNull ? null : (PropertySubtype)dataRow["PropertySubtype"],
                price = dataRow["PriceAmount"] is DBNull ? null : new Price() { amount = Convert.ToDecimal(dataRow["PriceAmount"]) },
                description = dataRow["Description"] is DBNull ? null : (string)dataRow["Description"],
                dataSourceName = dataRow["DataSourceName"] is DBNull ? null : (string)dataRow["DataSourceName"],
                dataSourceGuid = dataRow["DataSourceGuid"] is DBNull ? null : (string)dataRow["DataSourceGuid"],
                dataSourceUpdate = dataRow["DataSourceUpdate"] is DBNull ? null : (DateTime)dataRow["DataSourceUpdate"],
                dataSourceUrl = dataRow["DataSourceUrl"] is DBNull ? null : new Uri((string)dataRow["DataSourceUrl"]),
                contactName = dataRow["ContactName"] is DBNull ? null : (string)dataRow["ContactName"],
                contactPhone = dataRow["ContactPhone"] is DBNull ? null : (string)dataRow["ContactPhone"],
                contactEmail = dataRow["ContactEmail"] is DBNull ? null : (string)dataRow["ContactEmail"],
                contactUrl = dataRow["ContactEmail"] is DBNull ? null : new Uri((string)dataRow["ContactEmail"]),
                contactOther = dataRow["ContactOther"] is DBNull ? null : (string)dataRow["ContactOther"],
                address = dataRow["Address"] is DBNull ? null : (string)dataRow["Address"],
                latitude = dataRow["Latitude"] is DBNull ? null : (double)dataRow["Latitude"],
                longitude = dataRow["Longitude"] is DBNull ? null : (double)dataRow["Longitude"],
                locationIsAccurate = dataRow["LocationIsAccurate"] is DBNull ? null : (bool)dataRow["LocationIsAccurate"],
                cadastralReference = dataRow["CadastralReference"] is DBNull ? null : (string)dataRow["CadastralReference"],
                propertySize = dataRow["PropertySize"] is DBNull ? null : (double)dataRow["PropertySize"],
                landSize = dataRow["LandSize"] is DBNull ? null : (double)dataRow["LandSize"],
                constructionYear = dataRow["ConstructionYear"] is DBNull ? null : (int)dataRow["ConstructionYear"],
                constructionStatus = dataRow["ConstructionStatus"] is DBNull ? null : (ConstructionStatus)dataRow["ConstructionStatus"],
                floors = dataRow["Floors"] is DBNull ? null : (int)dataRow["Floors"],
                floor = dataRow["Floor"] is DBNull ? null : (string)dataRow["Floor"],
                bedrooms = dataRow["Bedrooms"] is DBNull ? null : (int)dataRow["Bedrooms"],
                bathrooms = dataRow["Bathrooms"] is DBNull ? null : (int)dataRow["Bathrooms"],
                parkings = dataRow["Parkings"] is DBNull ? null : (int)dataRow["Parkings"],
            };
            
            AddListingFeature(listing, dataRow, "Terrace", Feature.terrace);
            AddListingFeature(listing, dataRow, "Garden", Feature.garden);
            AddListingFeature(listing, dataRow, "Garage", Feature.garage);
            AddListingFeature(listing, dataRow, "MotorbikeGarage", Feature.motorbike_garage);
            AddListingFeature(listing, dataRow, "Pool", Feature.pool);
            AddListingFeature(listing, dataRow, "Lift", Feature.lift);
            AddListingFeature(listing, dataRow, "DisabledAccess", Feature.disabled_access);
            AddListingFeature(listing, dataRow, "StorageRoom", Feature.storage_room);
            AddListingFeature(listing, dataRow, "Furnished", Feature.furnished);
            AddListingFeature(listing, dataRow, "NonFurnished", Feature.non_furnished);
            AddListingFeature(listing, dataRow, "Heating", Feature.heating);
            AddListingFeature(listing, dataRow, "AirConditioning", Feature.air_conditioning);
            AddListingFeature(listing, dataRow, "PetsAllowed", Feature.pets_allowed);
            AddListingFeature(listing, dataRow, "SecuritySystems", Feature.security_systems);

            return listing;
        }

        private void AddListingFeature(Listing listing, DataRow dataRow, string rowName, Feature feature)
        {
            if(dataRow[rowName] is DBNull)
            {
                return;
            }

            var value = (bool?)dataRow[rowName];
            listing.AddFeature(value, feature);            
        }

        private void LoadListingMedia(Listing listing)
        {
            string query =
                "SELECT * " +
                "FROM " + TABLE_ES_MEDIA + " " +
                "WHERE [ListingGuid] = @ListingGuid";

            DataTable dataTable = new Database().QueryTable(query, new Dictionary<string, object?>()
            {
                { "ListingGuid", listing.guid }
            });
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var media = GetMedia(dataRow);
                listing.AddMedia(media);
            }
        }

        private Media GetMedia(DataRow dataRow)
        {
            return new Media
            {
                mediaType = dataRow["MediaType"] is DBNull ? null : (MediaType)dataRow["MediaType"],
                title = dataRow["Title"] is DBNull ? null : (string)dataRow["Title"],
                url = new Uri((string)dataRow["Url"])
            };
        }
    }
}
