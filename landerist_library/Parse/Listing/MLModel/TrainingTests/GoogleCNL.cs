using Google.Api.Gax.Grpc;
using Google.Cloud.Language.V1;
using Grpc.Core;
using landerist_library.Configuration;


namespace landerist_library.Parse.Listing.MLModel.TrainingTests
{
    public class GoogleCNL : TrainingTests
    {
        readonly LanguageServiceClient languageServiceClient;

        public GoogleCNL()
        {
            LanguageServiceSettings settings = new()
            {
                CallSettings = CallSettings.FromHeader("X-Goog-Api-Key", Config.GOOGLE_NATURAL_LANGUAGE_API_KEY)
            };

            languageServiceClient = new LanguageServiceClientBuilder
            {
                ChannelCredentials = ChannelCredentials.SecureSsl,
                Settings = settings
            }.Build();
        }

        public void Run()
        {
            StartTestsIsListing();
        }

        // List of categories: https://cloud.google.com/natural-language/docs/categories?hl=es-419
        public override bool? PredictIsListing(string responseBodyText)
        {
            var document = new Document
            {
                Content = responseBodyText,
                Type = Document.Types.Type.PlainText,
                //Language = "es",
            };

            ClassificationModelOptions classificationModelOptions = new()
            {
                V2Model = new ClassificationModelOptions.Types.V2Model()
            };

            ClassifyTextRequest classifyTextRequest = new()
            {
                Document = document,
                ClassificationModelOptions = classificationModelOptions
            };

            try
            {
                var response = languageServiceClient.ClassifyText(classifyTextRequest);
                foreach (var category in response.Categories)
                {
                    string name = category.Name;
                    float confidence = category.Confidence;

                    if (category.Name.Contains("/Real Estate Listings"))
                    {
                        if(category.Confidence >= 8)
                        {
                            return true;
                        }                        
                    }
                }
                
                return false;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
            return null;
        }
    }
}
