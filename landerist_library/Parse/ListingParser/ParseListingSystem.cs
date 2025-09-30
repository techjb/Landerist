using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.VertexAI;
using landerist_library.Websites;

namespace landerist_library.Parse.ListingParser
{
    public class ParseListingSystem
    {
        //protected static readonly string SystemPrompt =
        //    "Tu tarea consiste en analizar el código html proporcionado por el usuario, identificando si corresponde a una página web de un único anuncio inmobiliario. " +
        //    "Si corresponde a un página que es el resultado de una búsqueda, donde aparecen uno o varios anuncios listados como resultado de una búsqueda, entonces no es un anuncio. " +
        //    "En caso de ser una página con los datos de un anuncio inmobiliario, deberás proceder a extraer los datos relevantes del código html en formato json. " +
        //    "Asegúrate de tener una precisión exhaustiva en la identificación y extracción de los elementos clave. Mantén un enfoque riguroso durante el proceso para ofrecer la respuesta más precisa y de la más alta calidad posible.";


        public static readonly string SystemPrompt =
            "Tu tarea es analizar el código HTML proporcionado por el usuario y determinar si corresponde a una página web de un único anuncio inmobiliario. Si la página representa un listado de resultados de búsqueda (es decir, contiene múltiples anuncios inmobiliarios o resúmenes de anuncios), no se considera un anuncio individual y no debes extraer datos. En caso de identificar que el HTML corresponde a un único anuncio inmobiliario, extrae los datos relevantes del código HTML y devuélvelos en formato JSON.";

        //public static readonly string SystemPrompt =
        //    "Actúas como un sistema automatizado de extracción de datos (scraper) especializado en el sector inmobiliario. Tu objetivo es analizar el código HTML proporcionado y rellenar el JSON Schema adjunto con la información extraída. " +
        //    "1. Valida el tipo de página: Primero, determina si el HTML corresponde a un único anuncio inmobiliario o no. En caso de ser una lista de resultados entonces no es un anuncio " +
        //    "2. Si es un anuncio único: Procede a extraer la información del HTML. Tu respuesta final DEBE ser un único objeto JSON que valide estricta y exclusivamente contra el schema proporcionado." +
        //    "" +
        //    "Reglas de Extracción y Normalización: " +
        //    "- Adherencia al Schema: No inventes campos que no estén en el schema. Rellena únicamente los campos definidos. " +
        //    "- Normalización Numérica: Para campos definidos como numéricos (integer o number) en el schema (ej: precio, superficie), extrae solo el valor numérico. Elimina símbolos de moneda (€, $, etc.), puntos de millares y otros caracteres no numéricos. " +
        //    "- Limpieza de Texto: Para campos de texto (string), elimina cualquier etiqueta HTML residual, saltos de línea excesivos y espacios en blanco al principio o al final. " +
        //    "- Campos Obligatorios: Haz el máximo esfuerzo por encontrar todos los campos requeridos por el schema. Si un campo requerido es absolutamente imposible de encontrar, prioriza devolver una estructura válida (por ejemplo, con un valor por defecto como 0 o \"\" si el schema lo permite) antes que fallar.";




        private const int DEFAULT_MAX_TOKENS = 128000;

        public static bool TooManyTokens(Page page)
        {
            //var maxContextWindow = DEFAULT_MAX_TOKENS;
            //switch (Config.LLM_PROVIDER)
            //{
            //    case LLMProvider.OpenAI: maxContextWindow = OpenAIRequest.MAX_CONTEXT_WINDOW; break;
            //    case LLMProvider.VertexAI: maxContextWindow = VertexAIRequest.MAX_CONTEXT_WINDOW; break;
            //    case LLMProvider.LocalAI: maxContextWindow = LocalAIRequest.MAX_CONTEXT_WINDOW; break;
            //}
            var maxContextWindow = LocalAIRequest.MAX_CONTEXT_WINDOW;
            return TooManyTokens(page, maxContextWindow);
        }

        public static bool TooManyTokens(Page page, int maxContextWindow)
        {
            //https://github.com/dluc/openai-tools
            int systemTokens = GPT3Tokenizer.Encode(SystemPrompt).Count;
            string? text = ParseListingUserInput.GetText(page);
            if (text == null)
            {
                return false;
            }
            page.TokenCount = GPT3Tokenizer.Encode(text).Count;
            if (page.TokenCount is null)
            {
                return false;
            }
            int totalTokens = systemTokens + (int)page.TokenCount;
            return totalTokens > maxContextWindow;
        }

        public static string GetSystemPrompt()
        {
            return SystemPrompt;
        }
    }
}
