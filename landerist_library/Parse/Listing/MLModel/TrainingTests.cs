using landerist_library.Websites;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace landerist_library.Parse.Listing.MLModel
{
    public class TrainingTests
    {

        //public static void Run()
        //{
        //    string outputText, standardError;

        //    // Instantiate Machine Learning C# - Python class object            
        //    IMLSharpPython mlSharpPython = new MLSharpPython(filePythonExePath);
        //    // Test image
        //    string imagePathName = folderImagePath + "Image_Test_Name.png";
        //    // Define Python script file and input parameter name
        //    string fileNameParameter = $"{filePythonNamePath} {filePythonParameterName} {imagePathName}";
        //    // Execute the python script file 
        //    outputText = mlSharpPython.ExecutePythonScript(fileNameParameter, out standardError);
        //    if (string.IsNullOrEmpty(standardError))
        //    {
        //        switch (outputText.ToLower())
        //        {
        //            case "1":
        //                Console.WriteLine("Image category 1");
        //                break;
        //            case "0":
        //                Console.WriteLine("Image category 0");
        //                break;
        //            default:
        //                Console.WriteLine(outputText);
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine(standardError);
        //    }
        //    Console.ReadKey();
        //}


        public static void Run()
        {
            Thread.Sleep(3000);
            DataTable dataTable = Pages.GetTrainingIsListing(100);
            int total = dataTable.Rows.Count;
            int sucess = 0;
            int noSucess = 0;
            foreach (DataRow dataRow in dataTable.Rows)
            {
                string responseBodyText = (string)dataRow["ResponseBodyText"];
                bool isListing = (bool)dataRow["IsListing"];
                if (TestPredictionIsListing(responseBodyText, isListing))
                {
                    sucess++;
                }
                else
                {
                    noSucess++;
                }
                float percentageSucess = sucess * 100 / total;
                float percentageNoSucess = noSucess * 100 / total;

                Console.WriteLine(
                    "Total: " + total + " " +
                    "Suceess: " + sucess + " (" + Math.Round(percentageSucess, 2) + "%) " +
                    "No success: " + noSucess + " (" + Math.Round(percentageNoSucess, 2) + "%) ");
            }
        }

        public static bool TestPredictionIsListing(string responseBodyText, bool isListing)
        {
            var prediction = PredictIsListing(responseBodyText).Result;
            var isListingPredicted = prediction.Equals("1");
            return isListing == isListingPredicted;
        }


        public static async Task<string> PredictIsListing(string input)
        {
            using var client = new HttpClient();
            var theObject = new { input };
            var content = new StringContent(JsonConvert.SerializeObject(theObject), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://127.0.0.1:4449/predict", content);
            var prediction = await response.Content.ReadAsStringAsync();
            return prediction;
        }
    }
}
