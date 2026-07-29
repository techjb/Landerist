using landerist_library.Export;
using landerist_library.Infrastructure.Sql;
using landerist_library.Websites;
using landerist_library.Configuration;
using landerist_library.Database;

namespace landerist_library.Infrastructure.Administration
{
    public class CsvExportService
    {
        private static Func<IDatabase>? DatabaseFactory;

        public static void Configure(Func<IDatabase> databaseFactory)
        {
            ArgumentNullException.ThrowIfNull(databaseFactory);
            DatabaseFactory = databaseFactory;
        }
        private const string FILE_NAME_LISTINGS = "ES_LISTINGS.csv";

        private const string FILE_NAME_MEDIA = "ES_MEDIA.csv";

        private const string ZIP_FILE = "listings.csv.zip";

        public static bool Export(bool makeZip)
        {
            var success = ExportListings() && ExportMedia();
            if (makeZip)
            {
                return MakeZip();
            }
            return success;
        }

        private static bool MakeZip()
        {
            var csvListings = GetFilePath(FILE_NAME_LISTINGS);
            var csvMedia = GetFilePath(FILE_NAME_MEDIA);
            var zipFile = GetFilePath(ZIP_FILE);
            var files = new string[] { csvListings, csvMedia };
            return Zip.Compress(files, zipFile);
        }

        private static bool ExportListings()
        {
            string filePath = GetFilePath(FILE_NAME_LISTINGS);
            return Export(SqlTableNames.Listings, filePath);
        }

        private static string GetFilePath(string fileName)
        {
            return Config.EXPORT_DIRECTORY + fileName;
        }

        private static bool ExportMedia()
        {
            string filePath = GetFilePath(FILE_NAME_MEDIA);
            return Export(SqlTableNames.Media, filePath);
        }

        private static bool Export(string tableName, string fileName)
        {
            File.Delete(fileName);
            tableName = "[" + Config.DATABASE_NAME + "].[dbo]." + tableName;

            string query =
               "EXEC xp_cmdshell " +
               "'bcp \"SELECT * FROM " + tableName + ";\" queryout \"" + fileName + "\" -T -c -t,';  ";

            return (DatabaseFactory
                ?? throw new InvalidOperationException("CsvExportService is not configured."))()
                .Query(query);
        }

        public static void ExportHostsMainUri(IEnumerable<Website> websites)
        {
            var dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Host", typeof(string));
            dataTable.Columns.Add("MainUri", typeof(string));
            foreach (var website in websites)
            {
                dataTable.Rows.Add(website.Host, website.MainUri.ToString());
            }
            string fileName = Config.EXPORT_DIRECTORY + "HostMainUri.csv";
            Tools.Csv.Write(dataTable, fileName, true);
        }
    }
}
