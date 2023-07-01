using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Parse.Listing.MLModel.IsListingUrl
{
    public class IsListingUrl
    {
        public static void CreateCsv() 
        {
            Console.WriteLine("Reading Rris");
            var urls = Pages.GetAllUris();
            HashSet<string> list = new(StringComparer.OrdinalIgnoreCase);
            foreach (var url in urls)
            {
                Uri uri = new(url);
                list.Add(uri.PathAndQuery);
            }
            Console.WriteLine("Uris: " + list.Count);
            string file = Config.MLMODEL_TRAINING_DATA_DIRECTORY + "IsListingUrl.csv";
            Console.WriteLine("Creating " + file + " ..");
            File.Delete(file);
            Tools.Csv.Write(list, file);
        }
    }
}
