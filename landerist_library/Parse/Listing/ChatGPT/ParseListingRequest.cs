using landerist_library.Websites;
using OpenAI;
using System.Text;
using System.Text.Json;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ParseListingRequest : ChatGPTRequest
    {
        public static readonly string SystemMessage =
            "Analiza detenidamente el anuncio inmobiliario proporcionado por el usuario. " +
            "Identifica y extrae de manera precisa los elementos clave, como por el precio del inmueble, " +
            "ubicación exacta, tamaño en metros cuadrados, tipo de inmueble, una descripción, etc. " +
            "Extrae además las características especiales como el estado de la propiedad, año de construcción, " +
            "datos de contacto, etc. Presenta los datos de manera clara, concisa y estructurada en formato json. " +
            "Mantente enfocado y da tu mejor respuesta.";

        private static readonly Tool Tool = ParseListingTool.GetTool();

        public ParseListingRequest() : base(SystemMessage, Tool)
        {
            
        }

        public landerist_orels.ES.Listing? Parse(Page page)
        {
            var response = GetResponse(page.ResponseBodyText, false);
            if (response == null)
            {
                return null;
            }
            try
            {
                var usedTool = response.FirstChoice.Message.ToolCalls[0];                
                string arguments = usedTool.Function.Arguments.ToString();
                string argumentsEncoded = EncodeToUTF8(arguments);
                var parseListingResponse = JsonSerializer.Deserialize<ParseListingResponse>(argumentsEncoded);
                if (parseListingResponse != null)
                {
                    return parseListingResponse.ToListing(page);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Logs.Log.WriteLogErrors("ParseListingRequest Parse", exception);
            }
            return null;
        }

        // Problem with encoding should be fixed in the future:
        // https://community.openai.com/t/gpt-4-1106-preview-messes-up-function-call-parameters-encoding/478500?page=2
        static string EncodeToUTF8(string texto)
        {
            byte[] bytes = Encoding.GetEncoding("Windows-1252").GetBytes(texto);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
