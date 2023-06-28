using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;
using landerist_library.Websites;
using System.Data;

namespace landerist_library.Parse.Listing.MLModel.TrainingTests
{
    public class AWSComprehend
    {

        private static readonly AmazonComprehendClient ComprehendClient = new(RegionEndpoint.EUWest1);

        public static void Run()
        {
            DataTable dataTableIsListing = Pages.GetTrainingIsListing(30, true, true);
            DataTable dataTableIsNotListing = Pages.GetTrainingIsListing(30, false, true);

            DataTable dataTableAll = dataTableIsListing.Copy();
            dataTableAll.Merge(dataTableIsNotListing);
            dataTableAll.AcceptChanges();

            int total = dataTableAll.Rows.Count;
            int sucess = 0;
            int noSucess = 0;
            int errors = 0;

            var sync = new object();
            Parallel.ForEach(dataTableAll.AsEnumerable(), new ParallelOptions()
            {
                MaxDegreeOfParallelism = 1,
            }, dataRow =>
            {
                string responseBodyText = (string)dataRow["ResponseBodyText"];
                bool isListing = (bool)dataRow["IsListing"];
                bool? predictionIsOk = TestPredictionIsListing(responseBodyText, isListing);
                if (!predictionIsOk.HasValue)
                {
                    Interlocked.Increment(ref errors);
                    return;
                }
                if ((bool)predictionIsOk)
                {
                    Interlocked.Increment(ref sucess);
                }
                else
                {
                    Interlocked.Increment(ref noSucess);
                }
                float percentageSucess = sucess * 100 / total;
                float percentageNoSucess = noSucess * 100 / total;

                Console.WriteLine(
                    "Total: " + total + " " +
                    "Suceess: " + sucess + " (" + Math.Round(percentageSucess, 2) + "%) " +
                    "No success: " + noSucess + " (" + Math.Round(percentageNoSucess, 2) + "%) " +
                    "Errors: " + errors);
            });
        }

        public static bool? TestPredictionIsListing(string responseBodyText, bool isListing)
        {
            var classifyDocumentRequest = new ClassifyDocumentRequest()
            {
                Text = responseBodyText,
                EndpointArn = "arn:aws:comprehend:eu-west-1:568228643093:document-classifier-endpoint/IsListing"
            };

            try
            {
                DateTime dateTime = DateTime.Now;
                var classifyResponse = ComprehendClient.ClassifyDocumentAsync(classifyDocumentRequest).Result;
                Timers.Timer.SaveTimer("aws-comprehend", dateTime);

                float? scoreTrue = GetScore(classifyResponse.Classes, "True");
                float? scoreFalse = GetScore(classifyResponse.Classes, "False");
                if (scoreTrue.HasValue && scoreFalse.HasValue)
                {
                    bool predictedIsListing = scoreTrue > scoreFalse;
                    return predictedIsListing == isListing;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private static float? GetScore(List<DocumentClass> list, string name)
        {
            foreach (var documentClass in list)
            {
                if (documentClass.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return documentClass.Score;
                }
            }
            return null;
        }
    }
}
