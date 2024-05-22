using GenerativeAI.Classes;
using GenerativeAI.Models;
using GenerativeAI.Services;
using GenerativeAI.Types;
using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Parse.Listing.Gemini
{
    public class ParseListingGemini
    {
        private readonly string Prompt = "Dime si la siguiente imagen corresponde a un anuncio inmobiliario o no";

        private const string ModelName = "gemini-1.5-flash-latest";
        public (PageType pageType, landerist_orels.ES.Listing? listing) Parse(Page page)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (page.Screenshot == null || page.Screenshot.Length == 0)
            {
                return result;
            }

            string? text = Parse(page.Screenshot).Result;

            Console.WriteLine(text);

            return result;
        }

        private async Task<string?> Parse(byte[] screenshot)
        {
            var modelParams = new ModelParams
            {
                Model = ModelName,
                GenerationConfig = new GenerationConfig
                {
                    Temperature = 0,                    
                }                
            };            

            var model = new GenerativeModel(PrivateConfig.GEMINI_API_KEY, modelParams);

            var textPart = new Part()
            {
                Text = Prompt
            };            

            var imagePart = new Part()
            {
                InlineData = new GenerativeContentBlob()
                {
                    MimeType = "image/png",
                    Data = Convert.ToBase64String(screenshot)
                }
            };

            var parts = new[] { textPart, imagePart };

            try
            {
                var result = await model.GenerateContentAsync(parts);
                return result.Text();
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static void Test()
        {
            var service = new ModelInfoService(PrivateConfig.GEMINI_API_KEY);
            var models = Task.Run(async () => await service.GetModelsAsync()).Result;
            foreach (var model in models)
            {
                Console.WriteLine(model.Name);
            }
        }
    }
}
