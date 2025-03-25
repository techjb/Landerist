using Newtonsoft.Json;
using System.Text;

namespace landerist_library.Parse.Listing.MLModel.TrainingTests
{
    public class Danyalktk : TrainingTests
    {
        public void Run()
        {
            StartTestsIsListing();
        }

        public override bool? PredictIsListing(string responseBodyText)
        {
            try
            {
                var prediction = PredictIsListingAsync(responseBodyText).Result;
                return prediction.Equals("1");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }


        public static async Task<string> PredictIsListingAsync(string input)
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
