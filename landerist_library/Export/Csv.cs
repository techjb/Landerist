namespace landerist_library.Export
{
    public class Csv
    {
        private const string FILE_NAME_LISTINGS = "ES_LISTINGS.csv";

        private const string FILE_NAME_MEDIA = "ES_MEDIA.csv";

        public Csv() 
        { 

        }

        public bool Export()
        {
            return ExportListings() && ExportMedia();
        }

        private bool ExportListings()
        {
            string fileName = Config.EXPORT_DIRECTORY + FILE_NAME_LISTINGS;
            return Export(ES.Listings.TABLE_ES_LISTINGS, fileName);
        }

        private bool ExportMedia()
        {
            string fileName = Config.EXPORT_DIRECTORY + FILE_NAME_MEDIA;
            return Export(ES.Media.TABLE_ES_MEDIA, fileName);
        }

        private bool Export(string tableName, string fileName)
        {
            File.Delete(fileName);
            tableName = "[" + Config.DATABASE_NAME + "].[dbo]." + tableName;

            string query =
               "EXEC xp_cmdshell " +
               "'bcp \"SELECT * FROM " + tableName + ";\" queryout \"" + fileName + "\" -T -c -t,';  ";

            return new Database().Query(query);
        }
    }
}
