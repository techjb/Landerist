using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Tools;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Parse.Listing.MLModel
{
    public class TrainingData
    {
        private static readonly bool RemoveBreakLines = true;
        public static void Create()
        {
            CreateIsListing();
            CreateListings();
        }

        public static void CreateIsListing()
        {
            Console.WriteLine("Reading IsListing ..");
            DataTable dataTable = Pages.GetTrainingIsListing();
            string file = Config.MLMODEL_DIRECTORY + "IsListing.csv";
            CreateFile(dataTable, file);            
        }

        public static void CreateListings()
        {
            Console.WriteLine("Reading Listings ..");
            DataTable dataTable = ES_Listings.GetTrainingListings();
            string file = Config.MLMODEL_DIRECTORY + "Listings.csv";
            CreateFile(dataTable, file);
        }

        private static void CreateFile(DataTable dataTable, string file)
        {
            Console.WriteLine("Creating " + file + " ..");
            File.Delete(file);
            DataTableToCsv.Convert(dataTable, file, RemoveBreakLines);
        }
    }
}
