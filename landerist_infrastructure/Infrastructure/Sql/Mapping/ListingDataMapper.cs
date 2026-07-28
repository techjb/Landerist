using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Sql.Mapping;

public static class ListingDataMapper
{
    public static Listing Map(DataRow row) =>
        new()
        {
            guid = (string)row["guid"],
            listingStatus = Enum.Parse<ListingStatus>(row["listingStatus"].ToString()!),
            listingDate = GetDateTime(row, "listingDate"),
            unlistingDate = GetDateTime(row, "unlistingDate"),
            operation = Enum.Parse<Operation>(row["operation"].ToString()!),
            propertyType = Enum.Parse<PropertyType>(row["propertyType"].ToString()!),
            propertySubtype = GetEnum<PropertySubtype>(row, "propertySubtype"),
            price = row["priceAmount"] is DBNull
                ? null
                : new Price
                {
                    amount = Convert.ToDecimal(row["priceAmount"]),
                    currency = Enum.Parse<Currency>(row["priceCurrency"].ToString()!)
                },
            description = GetString(row, "description"),
            contactName = GetString(row, "contactName"),
            contactPhone = GetString(row, "contactPhone"),
            contactEmail = GetString(row, "contactEmail"),
            contactUrl = GetUri(row, "contactUrl"),
            contactOther = GetString(row, "contactOther"),
            address = GetString(row, "address"),
            lauId = GetString(row, "lauId"),
            lauName = GetString(row, "lauName"),
            latitude = GetDouble(row, "latitude"),
            longitude = GetDouble(row, "longitude"),
            locationIsAccurate = GetBoolean(row, "locationIsAccurate"),
            locationResolver = GetOptionalString(row, "locationResolver"),
            cadastralReference = GetString(row, "cadastralReference"),
            propertySize = GetDouble(row, "propertySize"),
            landSize = GetDouble(row, "landSize"),
            constructionYear = GetShort(row, "constructionYear"),
            constructionStatus = GetEnum<ConstructionStatus>(row, "constructionStatus"),
            energyEfficiencyRating = GetEnum<EnergyEfficiencyRating>(row, "energyEfficiencyRating"),
            floors = GetShort(row, "floors"),
            floor = GetString(row, "floor"),
            bedrooms = GetShort(row, "bedrooms"),
            bathrooms = GetShort(row, "bathrooms"),
            parkings = GetShort(row, "parkings"),
            terrace = GetBoolean(row, "terrace"),
            garden = GetBoolean(row, "garden"),
            garage = GetBoolean(row, "garage"),
            motorbikeGarage = GetBoolean(row, "motorbikeGarage"),
            pool = GetBoolean(row, "pool"),
            lift = GetBoolean(row, "lift"),
            disabledAccess = GetBoolean(row, "disabledAccess"),
            storageRoom = GetBoolean(row, "storageRoom"),
            furnished = GetBoolean(row, "furnished"),
            nonFurnished = GetBoolean(row, "nonFurnished"),
            heating = GetBoolean(row, "heating"),
            airConditioning = GetBoolean(row, "airConditioning"),
            petsAllowed = GetBoolean(row, "petsAllowed"),
            securitySystems = GetBoolean(row, "securitySystems")
        };

    private static T? GetEnum<T>(DataRow row, string columnName) where T : struct, Enum =>
        row[columnName] is DBNull
            ? null
            : Enum.Parse<T>(row[columnName].ToString()!);

    private static bool? GetBoolean(DataRow row, string columnName) =>
        row[columnName] is DBNull ? null : (bool)row[columnName];

    private static string? GetString(DataRow row, string columnName) =>
        row[columnName] is DBNull ? null : (string)row[columnName];

    private static string? GetOptionalString(DataRow row, string columnName) =>
        row.Table.Columns.Contains(columnName) ? GetString(row, columnName) : null;

    private static short? GetShort(DataRow row, string columnName) =>
        row[columnName] is DBNull ? null : (short)row[columnName];

    private static double? GetDouble(DataRow row, string columnName) =>
        row[columnName] is DBNull ? null : (double)row[columnName];

    private static DateTime? GetDateTime(DataRow row, string columnName) =>
        row[columnName] is DBNull ? null : (DateTime)row[columnName];

    private static Uri? GetUri(DataRow row, string columnName) =>
        row[columnName] is DBNull ? null : new Uri((string)row[columnName]);
}
