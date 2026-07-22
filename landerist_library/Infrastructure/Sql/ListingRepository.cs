using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class ListingRepository
    {
        private readonly IDatabase? _database;

        public ListingRepository()
        {
        }

        public ListingRepository(IDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);
            _database = database;
        }

        private IDatabase Database => _database ?? new DataBase();

        public bool Insert(Listing listing, string host, ListingUnpublishDecision? unpublishDecision, out Exception? exception)
        {
            string query =
                "INSERT INTO " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "([guid], [listingStatus], [listingDate], [updated], [unlistingDate], [unlistingReason], [unlistingPageType], " +
                "[unlistingHttpStatusCode], [unlistingEvidenceCount], [unlistingRequiredEvidenceCount], [operation], [propertyType], " +
                "[propertySubtype], [priceAmount], [priceCurrency], [description], " +
                "[contactName], [contactPhone], [contactEmail], [contactUrl], [contactOther], [address], [lauId], [lauName], [latitude], [longitude], " +
                "[locationIsAccurate], [locationResolver], [cadastralReference], [propertySize], [landSize], [constructionYear], " +
                "[constructionStatus], [energyEfficiencyRating], [floors], [floor], [bedrooms], [bathrooms], [parkings], [terrace], [garden], " +
                "[garage], [motorbikeGarage], [pool], [lift], [disabledAccess], [storageRoom], [furnished], " +
                "[nonFurnished], [heating], [airConditioning], [petsAllowed], [securitySystems], [host]) " +
                "VALUES( " +
                "@guid, @listingStatus, @listingDate, @updated, @unlistingDate, @unlistingReason, @unlistingPageType, " +
                "@unlistingHttpStatusCode, @unlistingEvidenceCount, @unlistingRequiredEvidenceCount, @operation, @propertyType, " +
                "@propertySubtype, @priceAmount, @priceCurrency, @description, " +
                "@contactName, @contactPhone, @contactEmail, @contactUrl, @contactOther, @address, @lauId, @lauName, @latitude, @longitude, " +
                "@locationIsAccurate, @locationResolver, @cadastralReference, @propertySize, @landSize, @constructionYear, " +
                "@constructionStatus, @energyEfficiencyRating, @floors, @floor, @bedrooms, @bathrooms, @parkings, @terrace, @garden, " +
                "@garage, @motorbikeGarage, @pool, @lift, @disabledAccess, @storageRoom, @furnished, " +
                "@nonFurnished, @heating, @airConditioning, @petsAllowed, @securitySystems, @host " +
                ")";

            var queryParameters = GetQueryParameters(listing, unpublishDecision);
            queryParameters.Add("host", host);
            return Database.Query(query, queryParameters, out exception);
        }

        public bool Update(Listing listing, ListingUnpublishDecision? unpublishDecision = null)
        {
            string query =
                "UPDATE " + ES_Listings.TABLE_ES_LISTINGS + " SET " +
                "[listingStatus] = @listingStatus, " +
                "[updated] = @updated, " +
                "[unlistingDate] = @unlistingDate, " +
                "[unlistingReason] = CASE WHEN @listingStatus = 'published' THEN NULL ELSE COALESCE(@unlistingReason, [unlistingReason]) END, " +
                "[unlistingPageType] = CASE WHEN @listingStatus = 'published' THEN NULL ELSE COALESCE(@unlistingPageType, [unlistingPageType]) END, " +
                "[unlistingHttpStatusCode] = CASE WHEN @listingStatus = 'published' THEN NULL ELSE COALESCE(@unlistingHttpStatusCode, [unlistingHttpStatusCode]) END, " +
                "[unlistingEvidenceCount] = CASE WHEN @listingStatus = 'published' THEN NULL ELSE COALESCE(@unlistingEvidenceCount, [unlistingEvidenceCount]) END, " +
                "[unlistingRequiredEvidenceCount] = CASE WHEN @listingStatus = 'published' THEN NULL ELSE COALESCE(@unlistingRequiredEvidenceCount, [unlistingRequiredEvidenceCount]) END, " +
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
                "[lauName] = @lauName, " +
                "[latitude] = @latitude, " +
                "[longitude] = @longitude, " +
                "[locationIsAccurate] = @locationIsAccurate, " +
                "[locationResolver] = @locationResolver, " +
                "[cadastralReference] = @cadastralReference, " +
                "[propertySize] = @propertySize, " +
                "[landSize] = @landSize, " +
                "[constructionYear] = @constructionYear, " +
                "[constructionStatus] = @constructionStatus, " +
                "[energyEfficiencyRating] = @energyEfficiencyRating, " +
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

            var queryParameters = GetQueryParameters(listing, unpublishDecision);
            return Database.Query(query, queryParameters);
        }


        public bool Delete(string guid)
        {
            string query =
                "DELETE FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [guid] = @guid";

            return Database.Query(query, new Dictionary<string, object?> {
                {"guid", guid }
            });
        }

        public bool Delete()
        {
            return Database.Query("DELETE FROM " + ES_Listings.TABLE_ES_LISTINGS);
        }

        public bool UpdateAddress(string guid, double? latitude, double? longitude, bool? locationIsAccurate, string? locationResolver = null)
        {
            string query =
                "UPDATE " + ES_Listings.TABLE_ES_LISTINGS + " SET " +
                "[latitude] = @latitude, " +
                "[longitude] = @longitude, " +
                "[locationIsAccurate] = @locationIsAccurate, " +
                "[locationResolver] = @locationResolver " +
                "WHERE [guid] = @guid";

            return Database.Query(query, new Dictionary<string, object?> {
                {"guid", guid },
                {"latitude", latitude },
                {"longitude", longitude },
                {"locationIsAccurate", locationIsAccurate },
                {"locationResolver", locationResolver }
            });
        }

        private static Dictionary<string, object?> GetQueryParameters(Listing listing, ListingUnpublishDecision? unpublishDecision = null)
        {
            return new Dictionary<string, object?> {
                {"guid", listing.guid },
                {"listingStatus", listing.listingStatus.ToString() },
                {"listingDate", listing.listingDate},
                {"updated", DateTime.Now},
                {"unlistingDate", listing.unlistingDate},
                {"unlistingReason", GetUnlistingReason(listing, unpublishDecision)},
                {"unlistingPageType", GetUnlistingPageType(listing, unpublishDecision)},
                {"unlistingHttpStatusCode", GetUnlistingHttpStatusCode(listing, unpublishDecision)},
                {"unlistingEvidenceCount", GetUnlistingEvidenceCount(listing, unpublishDecision)},
                {"unlistingRequiredEvidenceCount", GetUnlistingRequiredEvidenceCount(listing, unpublishDecision)},
                {"operation", listing.operation.ToString() },
                {"propertyType", listing.propertyType.ToString() },
                {"propertySubtype", listing.propertySubtype?.ToString()},
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
                {"lauName", listing.lauName},
                {"latitude", listing.latitude},
                {"longitude", listing.longitude},
                {"locationIsAccurate", listing.locationIsAccurate},
                {"locationResolver", listing.locationResolver },
                {"cadastralReference", listing.cadastralReference },
                {"propertySize", listing.propertySize},
                {"landSize", listing.landSize},
                {"constructionYear", listing.constructionYear},
                {"constructionStatus", listing.constructionStatus?.ToString()},
                {"energyEfficiencyRating", listing.energyEfficiencyRating?.ToString()},
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

        private static string? GetUnlistingReason(Listing listing, ListingUnpublishDecision? unpublishDecision)
        {
            return listing.listingStatus == ListingStatus.unpublished
                ? unpublishDecision?.Reason.ToString()
                : null;
        }

        private static string? GetUnlistingPageType(Listing listing, ListingUnpublishDecision? unpublishDecision)
        {
            return listing.listingStatus == ListingStatus.unpublished
                ? unpublishDecision?.PageType?.ToString()
                : null;
        }

        private static short? GetUnlistingHttpStatusCode(Listing listing, ListingUnpublishDecision? unpublishDecision)
        {
            return listing.listingStatus == ListingStatus.unpublished
                ? unpublishDecision?.HttpStatusCode
                : null;
        }

        private static short? GetUnlistingEvidenceCount(Listing listing, ListingUnpublishDecision? unpublishDecision)
        {
            return listing.listingStatus == ListingStatus.unpublished && unpublishDecision is not null
                ? (short)unpublishDecision.ActualEvidenceCount
                : null;
        }

        private static short? GetUnlistingRequiredEvidenceCount(Listing listing, ListingUnpublishDecision? unpublishDecision)
        {
            return listing.listingStatus == ListingStatus.unpublished && unpublishDecision?.RequiredEvidenceCount is not null
                ? (short)unpublishDecision.RequiredEvidenceCount.Value
                : null;
        }
    }
}
