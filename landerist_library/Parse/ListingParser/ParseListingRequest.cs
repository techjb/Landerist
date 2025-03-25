using AI.Dev.OpenAI.GPT;
using landerist_library.Websites;

namespace landerist_library.Parse.ListingParser
{
    public class ParseListingRequest
    {
        protected static readonly string SystemPrompt =
            "Tu tarea consiste en analizar la url y el código html proporcionado por el usuario, identificando si corresponde a una página web de un único anuncio inmobiliario. " +
            "Si corresponde a un página que es el resultado de una búsqueda, donde aparecen uno o varios anuncios listados como resultado de una búsqueda, entonces no es un anuncio. " +
            "En caso de ser una página con los datos de un anuncio inmobiliario, deberás proceder a extraer los datos relevantes del código html en formato json. " +
            "Asegúrate de tener una precisión exhaustiva en la identificación y extracción de los elementos clave. Mantén un enfoque riguroso durante el proceso para ofrecer la respuesta más precisa y de la más alta calidad posible.";

        protected static bool TooManyTokens(Page page, int maxContextWindow)
        {
            //https://github.com/dluc/openai-tools
            int systemTokens = GPT3Tokenizer.Encode(SystemPrompt).Count;
            string? text = UserInputText.GetText(page);
            if (text == null)
            {
                return false;
            }
            int userTokens = GPT3Tokenizer.Encode(text).Count;
            int totalTokens = systemTokens + userTokens;
            return totalTokens > maxContextWindow;
        }

        public static string GetSystemPrompt()
        {
            return SystemPrompt;
        }
    }
}
