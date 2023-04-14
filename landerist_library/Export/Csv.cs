using landerist_library.Configuration;
using landerist_library.Database;

namespace landerist_library.Export
{
    public class Csv
    {
        private const string FILE_NAME_LISTINGS = "ES_LISTINGS.csv";

        private const string FILE_NAME_MEDIA = "ES_MEDIA.csv";

        private const string ZIP_FILE = "listings.csv.zip";

        public Csv()
        {

        }

        public bool Export(bool makeZip)
        {
            var success = ExportListings() && ExportMedia();
            if (makeZip)
            {
                return MakeZip();
            }
            return success;
        }

        private bool MakeZip()
        {
            var csvListings = GetFilePath(FILE_NAME_LISTINGS);
            var csvMedia = GetFilePath(FILE_NAME_MEDIA);
            var zipFile = GetFilePath(ZIP_FILE);
            var files = new string[] { csvListings, csvMedia };            
            return new Zip().Export(files, zipFile);
        }

        private bool ExportListings()
        {
            string filePath = GetFilePath(FILE_NAME_LISTINGS);
            return Export(ES_Listings.TABLE_ES_LISTINGS, filePath);
        }

        private string GetFilePath(string fileName)
        {
            return Config.EXPORT_DIRECTORY + fileName;
        }

        private bool ExportMedia()
        {
            string filePath = GetFilePath(FILE_NAME_MEDIA);
            return Export(ES_Media.TABLE_ES_MEDIA, filePath);
        }

        private bool Export(string tableName, string fileName)
        {
            File.Delete(fileName);
            tableName = "[" + Config.DATABASE_NAME + "].[dbo]." + tableName;

            string query =
               "EXEC xp_cmdshell " +
               "'bcp \"SELECT * FROM " + tableName + ";\" queryout \"" + fileName + "\" -T -c -t,';  ";

            return new DataBase().Query(query);
        }
    }
}
