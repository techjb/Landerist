using landerist_library.Configuration;
using landerist_library.Database;
using OpenCvSharp;

namespace landerist_library.Export
{
    public class Csv
    {
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
            return Export(ES_Listings.TABLE_ES_LISTINGS, filePath);
        }

        private static string GetFilePath(string fileName)
        {
            return Config.EXPORT_DIRECTORY + fileName;
        }

        private static bool ExportMedia()
        {
            string filePath = GetFilePath(FILE_NAME_MEDIA);
            return Export(ES_Media.TABLE_ES_MEDIA, filePath);
        }

        private static bool Export(string tableName, string fileName)
        {
            File.Delete(fileName);
            tableName = "[" + Config.DATABASE_NAME + "].[dbo]." + tableName;

            string query =
               "EXEC xp_cmdshell " +
               "'bcp \"SELECT * FROM " + tableName + ";\" queryout \"" + fileName + "\" -T -c -t,';  ";

            return new DataBase().Query(query);
        }

        public static void ExportHostsMainUri()
        {
            var dataTable = Websites.Websites.GetDataTableHostMainUri();
            string fileName = Config.EXPORT_DIRECTORY + "HostMainUri.csv";            
            Tools.Csv.Write(dataTable, fileName, true);
        }
    }
}
