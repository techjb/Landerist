using Amazon;
using Amazon.Comprehend;
using Amazon.Comprehend.Model;

namespace landerist_library.Parse.Listing.MLModel.TrainingTests
{
    public class AWSComprehend : TrainingTests
    {

        private static readonly AmazonComprehendClient ComprehendClient = new(RegionEndpoint.EUWest1);

        public void Run()
        {
            StartTestsIsListing();
        }

        public override bool? PredictIsListing(string responseBodyText)
        {
            var classifyDocumentRequest = new ClassifyDocumentRequest()
            {
                Text = responseBodyText,
                EndpointArn = "arn:aws:comprehend:eu-west-1:568228643093:document-classifier-endpoint/IsListing"
            };

            try
            {
                var classifyResponse = ComprehendClient.ClassifyDocumentAsync(classifyDocumentRequest).Result;
                float? scoreTrue = GetScore(classifyResponse.Classes, "True");
                float? scoreFalse = GetScore(classifyResponse.Classes, "False");
                if (scoreTrue.HasValue && scoreFalse.HasValue)
                {
                    return scoreTrue > scoreFalse;
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
