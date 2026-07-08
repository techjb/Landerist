using landerist_library.Database;
using landerist_library.Pages;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql
{
    public class ListingRepository
    {
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
            return new DataBase().Query(query, queryParameters, out exception);
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
            return new DataBase().Query(query, queryParameters);
        }

        public int Count(string host)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [host] = @Host";

            return new DataBase().QueryInt(query, new Dictionary<string, object?> { { "Host", host } });
        }

        public int Count(string host, ListingStatus listingStatus)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [host] = @Host AND [listingStatus] = @ListingStatus";

            return new DataBase().QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() }
            });
        }

        public int CountSinceListingDate(string host, DateTime listingDateFrom)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [host] = @Host " +
                "AND [listingDate] >= @ListingDateFrom";

            return new DataBase().QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingDateFrom", listingDateFrom }
            });
        }

        public int CountWithAddress(string host, ListingStatus listingStatus)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [host] = @Host " +
                "AND [listingStatus] = @ListingStatus " +
                "AND NULLIF(LTRIM(RTRIM([address])), '') IS NOT NULL";

            return new DataBase().QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() }
            });
        }

        public int CountWithCoordinates(string host, ListingStatus listingStatus)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [host] = @Host " +
                "AND [listingStatus] = @ListingStatus " +
                "AND [latitude] IS NOT NULL " +
                "AND [longitude] IS NOT NULL";

            return new DataBase().QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() }
            });
        }

        public int CountWithImages(string host, ListingStatus listingStatus)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " AS L " +
                "WHERE L.[host] = @Host " +
                "AND L.[listingStatus] = @ListingStatus " +
                "AND EXISTS (" +
                "   SELECT 1 " +
                "   FROM " + ES_Media.TABLE_ES_MEDIA + " AS M " +
                "   WHERE M.[listingGuid] = L.[guid] " +
                "   AND M.[mediaType] = @MediaType" +
                ")";

            return new DataBase().QueryInt(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus.ToString() },
                { "MediaType", MediaType.image.ToString() }
            });
        }

        public int Count(ListingStatus listingStatus, Operation operation, PropertyType propertyType)
        {
            string query =
                "SELECT COUNT(*) " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " AS L " +
                "WHERE L.[listingStatus] = @ListingStatus AND " +
                "L.[operation] = @Operation AND " +
                "L.[propertyType] = @PropertyType";

            return new DataBase().QueryInt(query, new Dictionary<string, object?>
            {
                { "ListingStatus", listingStatus.ToString() },
                { "Operation", operation.ToString() },
                { "PropertyType", propertyType.ToString() }
            });
        }

        public DataTable GetAll()
        {
            string query = "SELECT * FROM " + ES_Listings.TABLE_ES_LISTINGS;
            return new DataBase().QueryTable(query);
        }

        public DataTable GetListings(ListingStatus listingStatus)
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [listingStatus] = @listingStatus";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"listingStatus", listingStatus.ToString() },
            });
        }

        public DataTable GetListings(ListingStatus listingStatus, Operation operation, PropertyType propertyType)
        {
            string query =
                "SELECT L.* " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " AS L " +
                "WHERE L.[listingStatus] = @listingStatus AND " +
                "L.[operation] = @operation AND " +
                "L.[propertyType] = @propertyType";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"listingStatus", listingStatus.ToString() },
                {"operation", operation.ToString() },
                {"propertyType", propertyType.ToString() },
            });
        }

        public DataTable GetListings(string host, ListingStatus? listingStatus = null)
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Host] = @Host " +
                (listingStatus is null ? string.Empty : "AND [ListingStatus] = @ListingStatus");

            return new DataBase().QueryTable(query, new Dictionary<string, object?>
            {
                { "Host", host },
                { "ListingStatus", listingStatus?.ToString() }
            });
        }

        public DataTable GetListingWithCatastralReference()
        {
            return new DataBase().QueryTable("SELECT * FROM " + ES_Listings.TABLE_ES_LISTINGS + " WHERE [cadastralReference] IS NOT NULL");
        }

        public DataTable GetListingsWithoutLauName()
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [lauName] IS NULL AND " +
                "[latitude] IS NOT NULL AND " +
                "[longitude] IS NOT NULL";

            return new DataBase().QueryTable(query);
        }

        public DataTable GetListingWithCatastralReferenceAndNoAddress()
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [cadastralReference] IS NOT NULL " +
                "AND [address] IS NULL";

            return new DataBase().QueryTable(query);
        }

        public DataTable GetListingsWithoutCatastralReferenceAndLocationIsAccurate()
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [cadastralReference] IS NULL " +
                "AND [locationIsAccurate] = 1";

            return new DataBase().QueryTable(query);
        }

        public DataTable GetListingsLocationIsAccurateNoCadastralReference()
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [locationIsAccurate] = 1 AND " +
                " [cadastralReference] IS NULL";

            return new DataBase().QueryTable(query);
        }

        public DataTable GetUnpublishedListings(DateTime unlistingDate)
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [listingStatus] = @listingStatus AND " +
                "[unlistingDate] < @unlistingDate";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"listingStatus", ListingStatus.unpublished.ToString() },
                {"unlistingDate", unlistingDate }
            });
        }

        public DataTable GetListings(DateOnly dateFrom, DateOnly dateTo)
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE " +
                "   CAST([updated] AS DATE) >= CAST(@DateFrom AS DATE) AND " +
                "   CAST([updated] AS DATE) <= CAST(@DateTo AS DATE)";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                { "DateFrom", dateFrom },
                { "DateTo", dateTo },
            });
        }

        public DataTable GetListings(ListingStatus listingStatus, DateOnly dateFrom, DateOnly dateTo)
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE " +
                "   [listingStatus] = @ListingStatus AND " +
                "   CAST([updated] AS DATE) >= CAST(@DateFrom AS DATE) AND " +
                "   CAST([updated] AS DATE) <= CAST(@DateTo AS DATE)";

            return new DataBase().QueryTable(query, new Dictionary<string, object?>()
            {
                { "ListingStatus", listingStatus.ToString() },
                { "DateFrom", dateFrom },
                { "DateTo", dateTo },
            });
        }

        public DataTable GetListing(string guid)
        {
            string query =
                "SELECT * " +
                "FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [Guid] = @Guid";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Guid", guid }
            });
        }

        public bool Delete(string guid)
        {
            string query =
                "DELETE FROM " + ES_Listings.TABLE_ES_LISTINGS + " " +
                "WHERE [guid] = @guid";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"guid", guid }
            });
        }

        public bool Delete()
        {
            return new DataBase().Query("DELETE FROM " + ES_Listings.TABLE_ES_LISTINGS);
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

            return new DataBase().Query(query, new Dictionary<string, object?> {
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
