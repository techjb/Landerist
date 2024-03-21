using System.Security.Cryptography;
using System.Text;

namespace landerist_library.Database
{
    public class ValidInvalidImages
    {
        private const string VALID_IMAGES = "[VALID_IMAGES]";

        private const string INVALID_IMAGES = "[INVALID_IMAGES]";

        public static bool IsValid(Uri uri)
        {
            return IsValidInvalid(uri, true);
        }

        public static bool IsInvalid(Uri uri)
        {
            return IsValidInvalid(uri, false);
        }

        private static bool IsValidInvalid(Uri uri, bool isValid)
        {
            string tableName = isValid ? VALID_IMAGES : INVALID_IMAGES;
            string uriHash = CalculateHash(uri);
            string query =
                "SELECT 1 " +
                "FROM " + tableName + " " +
                "WHERE UriHash = @UriHash";               

            return new DataBase().QueryExists(query, new Dictionary<string, object?> {
                {"UriHash", uriHash }
            });
        }

        public static bool InsertValid(Uri uri)
        {
            return Insert(uri, true);
        }

        public static bool InsertInvalid(Uri uri)
        {
            return Insert(uri, false);
        }

        private static bool Insert(Uri uri, bool isValid)
        {
            string tableName = isValid ? VALID_IMAGES : INVALID_IMAGES;
            string uriHash = CalculateHash(uri);
            string query =
                "INSERT INTO " + tableName + " " +
                "VALUES (GETDATE(), @UriHash)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", uriHash }
            });
        }

        private static string CalculateHash(Uri uri)
        {
            string text = uri.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            byte[] hash = SHA256.HashData(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
