namespace landerist_library.Database
{
    public class RedirectUrl
    {
        private const string REDIRECT_URL = "[REDIRECT_URL]";

        public bool Insert(string originalUrl, string redirectUrl)
        {
            string query =
                "INSERT INTO " + REDIRECT_URL+ " VALUES(GETDATE(), @originalUrl, @redirectUrl)";

            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"originalUrl", originalUrl },
                {"redirectUrl", redirectUrl },
            });
        }
    }
}
