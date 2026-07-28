using System.Text.RegularExpressions;

namespace landerist_library.Database;

public sealed class AddressCadastralReference
{
    private const string TableName = "[ADDRESS_CADASTRAL_REFERENCE]";
    private const int AddressMaxLength = 200;
    private const int CadastralReferenceMaxLength = 50;
    private readonly IDatabase _database;

    public AddressCadastralReference(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    public bool Insert(string address, string? cadastralReference)
    {
        string? normalizedAddress = NormalizeAddress(address);
        string? normalizedReference = NormalizeCadastralReference(cadastralReference);
        if (normalizedAddress is null || normalizedReference is null)
        {
            return false;
        }
        const string query =
            "INSERT INTO " + TableName + " " +
            "([DateInsert], [Address], [CadastralReference]) " +
            "VALUES (GETDATE(), @Address, @CadastralReference)";
        return _database.Query(query, new Dictionary<string, object?>
        {
            { "Address", normalizedAddress },
            { "CadastralReference", normalizedReference }
        });
    }

    public string? Select(string address)
    {
        string? normalizedAddress = NormalizeAddress(address);
        if (normalizedAddress is null)
        {
            return null;
        }
        const string query =
            "SELECT [CadastralReference] FROM " + TableName + " " +
            "WHERE [Address] = @Address";
        return _database.QueryString(query, new Dictionary<string, object?>
        {
            { "Address", normalizedAddress }
        });
    }

    public bool Clean() => _database.Query(
        "DELETE FROM " + TableName + " " +
        "WHERE [DateInsert] < DATEADD(YEAR, -1, GETDATE())");

    private static string? NormalizeAddress(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }
        string normalized = Regex.Replace(address.Trim(), @"\s+", " ");
        return normalized.Length > AddressMaxLength ? null : normalized;
    }

    private static string? NormalizeCadastralReference(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        string normalized = value.Trim();
        return normalized.Length > CadastralReferenceMaxLength ? null : normalized;
    }
}