using System.Text.RegularExpressions;

namespace landerist_library.Database
{
    public class AddressCadastralReference
    {
        private const string TableAddressCadastralReference = "[ADDRESS_CADASTRAL_REFERENCE]";
        private const int AddressMaxLength = 200;
        private const int CadastralReferenceMaxLength = 50;

        public static bool Insert(string address, string? cadastralReference)
        {
            string? normalizedAddress = NormalizeAddress(address);
            string? normalizedCadastralReference = NormalizeCadastralReference(cadastralReference);

            if (normalizedAddress is null || normalizedCadastralReference is null)
            {
                return false;
            }

            string query =
                "INSERT INTO " + TableAddressCadastralReference + " " +
                "([DateInsert], [Address], [CadastralReference]) " +
                "VALUES (GETDATE(), @Address, @CadastralReference)";

            return new DataBase().Query(query, new Dictionary<string, object?>
            {
                { "Address", normalizedAddress },
                { "CadastralReference", normalizedCadastralReference }
            });
        }

        public static string? Select(string address)
        {
            string? normalizedAddress = NormalizeAddress(address);
            if (normalizedAddress is null)
            {
                return null;
            }

            string query =
                "SELECT [CadastralReference] " +
                "FROM " + TableAddressCadastralReference + " " +
                "WHERE [Address] = @Address";

            return new DataBase().QueryString(query, new Dictionary<string, object?>
            {
                { "Address", normalizedAddress }
            });
        }

        public static bool Clean()
        {
            string query =
                "DELETE FROM " + TableAddressCadastralReference + " " +
                "WHERE [DateInsert] < DATEADD(YEAR, -1, GETDATE())";

            return new DataBase().Query(query);
        }

        private static string? NormalizeAddress(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return null;
            }

            string normalizedAddress = Regex.Replace(address.Trim(), @"\s+", " ");
            return normalizedAddress.Length > AddressMaxLength ? null : normalizedAddress;
        }

        private static string? NormalizeCadastralReference(string? cadastralReference)
        {
            if (string.IsNullOrWhiteSpace(cadastralReference))
            {
                return null;
            }

            string normalizedCadastralReference = cadastralReference.Trim();
            return normalizedCadastralReference.Length > CadastralReferenceMaxLength
                ? null
                : normalizedCadastralReference;
        }
    }
}
