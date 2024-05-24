using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;
using landerist_library.Websites;
using PuppeteerSharp;
using System.Net;

namespace landerist_library.Parse.Listing.VertexAI
{
    public class ParseListingVertexAI
    {
        private static readonly string ModelName =
                            "gemini-1.5-flash-preview-0514";
                            //"gemini-1.5-pro-preview-0514";                            

        private static readonly string ProjectId = "landerist";

        private static readonly string Location = "us-central1";

        private static readonly string Publisher = "google";

        private static readonly string SystemPrompt = "";




        public (PageType pageType, landerist_orels.ES.Listing? listing) Parse(Websites.Page page)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (page.Screenshot == null || page.Screenshot.Length == 0)
            {
                return result;
            }

            return result;
        }

        public static void Test()
        {
            var d = TextInput().Result;
        }

        private static async Task<string> TextInput()
        {
            var predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = $"{Location}-aiplatform.googleapis.com",
                JsonCredentials = PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL
            }.Build();

            string prompt = @"What's a good name for a flower shop that specializes in selling bouquets of dried flowers?";

            var generateContentRequest = new GenerateContentRequest
            {
                Model = $"projects/{ProjectId}/locations/{Location}/publishers/{Publisher}/models/{ModelName}",
                Contents =
                {
                    new Content
                    {
                        Role = "USER",
                        Parts =
                        {
                            new Part { Text = prompt }
                        }
                    }
                },
                GenerationConfig = new GenerationConfig()
                {
                    Temperature = 0,
                },
                SystemInstruction = new Content
                {
                    Role = "USER",
                    Parts =
                        {
                            new Part { Text = SystemPrompt }
                        }
                }

            };

            GenerateContentResponse response = await predictionServiceClient.GenerateContentAsync(generateContentRequest);

            string responseText = response.Candidates[0].Content.Parts[0].Text;
            Console.WriteLine(responseText);

            return responseText;
        }
    }
}
