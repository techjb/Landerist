using AI.Dev.OpenAI.GPT;
using landerist_library.Websites;

namespace landerist_library.Parse.Listing
{
    public class ParseListingRequest
    {
        protected static readonly string SystemPrompt =
            "Tu tarea consiste en analizar el texto html proporcionado por el usuario, identificando si corresponde a una página web de un anuncio inmobiliario. " +
            "En caso de ser un anuncio inmobiliario deberás proceder a extraer los datos relevantes en formato json. " +
            "Asegúrate de tener una precisión exhaustiva en la identificación y extracción de los elementos clave. " +
            "Es imperativo que mantengas un enfoque riguroso durante este proceso para ofrecer la respuesta más precisa y de la más alta calidad posible.";

        protected static bool TooManyTokens(Page page, int maxContextWindow)
        {
            //https://github.com/dluc/openai-tools
            int systemTokens = GPT3Tokenizer.Encode(SystemPrompt).Count;
            string? text = UserTextInput.GetText(page);
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
