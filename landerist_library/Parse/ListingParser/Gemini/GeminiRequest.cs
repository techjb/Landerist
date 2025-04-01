using GenerativeAI.Models;
using GenerativeAI.Services;
using GenerativeAI.Types;
using landerist_library.Configuration;
using landerist_library.Websites;

namespace landerist_library.Parse.ListingParser.Gemini
{
    public class GeminiRequest : ParseListingSystem
    {
        private static readonly string Prompt =
            "Dime si la siguiente imagen corresponde a un único anuncio inmobiliario o no";

        //public static readonly string Prompt =
        //  "Procesa la imagen proporcionada identificando si corresponde a un único anuncio inmobiliario. " +
        //  "De ser así, deberás analizar meticulosamente el contenido para determinar que efectivamente se trata de un único anuncio y proceder a extraer los datos relevantes.  " +
        //  "Estos deberán ser presentados en un formato estructurado JSON, asegurando una precisión exhaustiva en la identificación y extracción de los elementos clave. " +
        //  "Es imperativo que mantengas un enfoque riguroso durante este proceso para ofrecer la respuesta más precisa y de la más alta calidad posible.";

        public static readonly int MAX_CONTEXT_WINDOW = 128000;

        public static bool TooManyTokens(Page page)
        {
            return TooManyTokens(page, MAX_CONTEXT_WINDOW);
        }

        private readonly ModelParams ModelParams = new()
        {
            //Model = "gemini-1.5-pro-latest",
            Model = "gemini-1.5-flash-latest",
            GenerationConfig = new GenerationConfig
            {
                Temperature = 0,
            },
            SafetySettings =
            [
                new SafetySetting()
                {
                    Category = HarmCategory.HARM_CATEGORY_HARASSMENT,
                    Threshold = HarmBlockThreshold.BLOCK_ONLY_HIGH
                },
                new SafetySetting()
                {
                    Category = HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT,
                    Threshold = HarmBlockThreshold.BLOCK_ONLY_HIGH
                },
                new SafetySetting()
                {
                    Category = HarmCategory.HARM_CATEGORY_HATE_SPEECH,
                    Threshold = HarmBlockThreshold.BLOCK_ONLY_HIGH
                },
                new SafetySetting()
                {
                    Category = HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT,
                    Threshold = HarmBlockThreshold.BLOCK_ONLY_HIGH
                },
                //new SafetySetting() // throws error
                //{
                //    Category = HarmCategory.HARM_CATEGORY_UNSPECIFIED,
                //    Threshold = HarmBlockThreshold.BLOCK_ONLY_HIGH
                //},
            ]
        };

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
            try
            {
                var parts = GetParts(screenshot);
                var model = GetModel();
                var result = await model.GenerateContentAsync(parts);
                var text = result.Text();
                var function = result.GetFunction();
                var candidates = result.Candidates;
                return text;
            }
            catch //(Exception ex)
            {

            }
            return null;
        }

        private static Part[] GetParts(byte[] screenshot)
        {
            //var listingService = new ListingService();

            var textPart = new Part
            {
                Text = Prompt,
                //FunctionResponse = listingService.
            };



            var imagePart = new Part()
            {
                InlineData = new GenerativeContentBlob()
                {
                    MimeType = "image/png",
                    Data = Convert.ToBase64String(screenshot)
                }
            };

            return [textPart, imagePart];
        }

        private GenerativeModel GetModel()
        {
            var listingService = new ListingService();
            var notListingService = new NotListingService();
            var model = new GenerativeModel(PrivateConfig.GEMINI_API_KEY, ModelParams);

            model.AddGlobalFunctions(listingService.AsGoogleFunctions(), listingService.AsGoogleCalls());
            model.AddGlobalFunctions(notListingService.AsGoogleFunctions(), notListingService.AsGoogleCalls());

            return model;
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
