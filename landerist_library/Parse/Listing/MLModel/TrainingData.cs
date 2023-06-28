using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Tools;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Parse.Listing.MLModel
{
    public class TrainingData
    {

        public static void TestData()
        {
            DataTable dataTable = Pages.GetTrainingIsListingNotNull();
            int charCounter = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                string responseBodyText = (string)row["ResponseBodyText"];
                bool isListing = (bool)row["IsListing"];

                charCounter += responseBodyText.Length + isListing.ToString().Length;
            }
            Console.WriteLine("Total chars: " + charCounter);
        }


        public static void Create()
        {
            CreateIsListing();
            CreateListings();
        }

        public static void CreateIsListing(int rows)
        {
            Console.WriteLine("Reading IsListing ..");
            DataTable dataTableIsListing = Pages.GetTrainingIsListing(rows, true, false);
            DataTable dataTableIsNotListing = Pages.GetTrainingIsListing(rows, false, false);

            DataTable dataTableAll = dataTableIsListing.Copy();
            dataTableAll.Merge(dataTableIsNotListing);

            string file = Config.MLMODEL_TRAINING_DATA_DIRECTORY + "IsListing" + rows + ".csv";
            CreateFile(dataTableAll, file);
        }

        public static void CreateIsListing()
        {
            Console.WriteLine("Reading IsListing ..");
            DataTable dataTable = Pages.GetTrainingIsListingNotNull();
            string file = Config.MLMODEL_TRAINING_DATA_DIRECTORY + "IsListing.csv";
            CreateFile(dataTable, file);
        }

        public static void CreateListings()
        {
            Console.WriteLine("Reading Listings ..");
            DataTable dataTable = ES_Listings.GetTrainingListings();
            string file = Config.MLMODEL_TRAINING_DATA_DIRECTORY + "Listings.csv";
            CreateFile(dataTable, file);
        }

        private static void CreateFile(DataTable dataTable, string file)
        {
            Console.WriteLine("Creating " + file + " ..");
            File.Delete(file);
            DataTableToCsv.Convert(dataTable, file, false);
        }
    }
}
