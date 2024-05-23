using Google.Apis.Auth.OAuth2;
using Google.Cloud.AIPlatform.V1;
using landerist_library.Websites;
using Grpc.Auth;
using System;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Parse.Listing.Gemini
{
    public class ParseListingGemini
    {
        public (PageType pageType, landerist_orels.ES.Listing? listing) Parse(Page page)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (page.Screenshot == null || page.Screenshot.Length == 0)
            {
                return result;
            }

            
            //string? text = Parse(page.Screenshot).Result;

            //Console.WriteLine(text);

            return result;
        }
        
        public static void Test()
        {
            var d = TextInput().Result;
        }

        private static async Task<string> TextInput(
            string projectId = "landerist",
            string location = "us-central1",
            string publisher = "google",
            string model = "gemini-1.5-pro-preview-0409")
        {

            var predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = $"{location}-aiplatform.googleapis.com",                
                CredentialsPath = "C:\\Users\\Chus\\Downloads\\landerist-b05e24797c62.json",

            }.Build();
            
            string prompt = @"What's a good name for a flower shop that specializes in selling bouquets of dried flowers?";

            var generateContentRequest = new GenerateContentRequest
            {
                Model = $"projects/{projectId}/locations/{location}/publishers/{publisher}/models/{model}",
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
                }
            };

            
            GenerateContentResponse response = await predictionServiceClient.GenerateContentAsync(generateContentRequest);

            string responseText = response.Candidates[0].Content.Parts[0].Text;
            Console.WriteLine(responseText);

            return responseText;
        }
    }
}
