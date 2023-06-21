using landerist_library.Configuration;
using landerist_library.Database;
using landerist_library.Tools;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Parse.Listing.MLModel
{
    public class TrainingData
    {
        public static void Create()
        {
            CreateIsListing();
            CreateListings();
        }

        private static void CreateIsListing()
        {
            Console.WriteLine("Creating IsListing ..");
            DataTable dataTable = Pages.GetTrainingIsListing();
            string file = Config.MLMODEL_DIRECTORY + "IsListing.csv";
            File.Delete(file);
            DataTableToCsv.Convert(dataTable, file);
        }

        private static void CreateListings()
        {
            Console.WriteLine("Creating Listings ..");
            DataTable dataTable = ES_Listings.GetTrainingListings();
            string file = Config.MLMODEL_DIRECTORY + "Listings.csv";
            File.Delete(file);
            DataTableToCsv.Convert(dataTable, file);
        }
    }
}
