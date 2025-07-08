using System.Data;

namespace landerist_library.Database
{
    public class AddressCadastralReference
    {
        private const string ADDRESS_CADASTRAL_REFERENCE = "[ADDRESS_CADASTRAL_REFERENCE]";

        public bool Insert(string address, string? cadastralReference)
        {
            string query =
                "INSERT INTO " + ADDRESS_CADASTRAL_REFERENCE + " " +
                "VALUES (GETDATE(), @Address, @CadastralReference)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"Address", address },
                {"CadastralReference", cadastralReference }
            });
        }

        public DataTable SelectTop1(string address)
        {
            string query =
                "SELECT TOP 1 * " +
                "FROM " + ADDRESS_CADASTRAL_REFERENCE + " " +
                "WHERE Address = @Address";

            return new DataBase().QueryTable(query, new Dictionary<string, object?> {
                {"Address", address }
            });
        }

        public static bool Clean()
        {
            string query =
                "DELETE FROM " + ADDRESS_CADASTRAL_REFERENCE + " " +
                "WHERE [DateInsert] < DATEADD(YEAR, -2, GETDATE())";

            return new DataBase().Query(query);
        }
    }
}
