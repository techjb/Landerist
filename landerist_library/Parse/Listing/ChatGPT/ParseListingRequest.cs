using landerist_library.Websites;
using OpenAI;
using System.Text;
using System.Text.Json;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ParseListingRequest : ChatGPTRequest
    {
        public static readonly string SystemMessage =
            "Analiza detenidamente el texto proporcionado por el usuario. " +
            "Determina si se trata o no de un anuncio inmobiliario. " +
            "En caso afirmativo identifica y extrae de manera precisa los elementos clave, como por el precio del inmueble, " +
            "ubicación exacta, tamaño en metros cuadrados, tipo de inmueble, datos de contacto, etc. " +
            "Presenta los datos de manera clara, concisa y estructurada en formato json. " +
            "Mantente enfocado y da tu mejor respuesta.";

        private static readonly List<Tool> Tools = ParseListingTool.GetTools();

        public ParseListingRequest() : base(SystemMessage, Tools)
        {

        }

        public (bool, landerist_orels.ES.Listing?) Parse(Page page)
        {
            bool sucess = false;
            landerist_orels.ES.Listing? listing = null;
            var response = GetResponse(page.ResponseBodyText, false);
            if (response == null)
            {
                return (sucess, listing);
            }
            try
            {
                var usedTool = response.FirstChoice.Message.ToolCalls[0];
                string functionName = usedTool.Function.Name;
                switch (functionName)
                {
                    case ParseListingTool.FunctionNameIsNotListing:
                        {
                            page.UpdatePageType(PageType.PageType.NotListing);                            
                            sucess = true;
                        }
                        break;
                    case ParseListingTool.FunctionNameIsListing:
                        {
                            string arguments = usedTool.Function.Arguments.ToString();
                            var parseListingResponse = JsonSerializer.Deserialize<ParseListingResponse>(arguments);
                            if (parseListingResponse != null)
                            {
                                listing = parseListingResponse.ToListing(page);
                            }
                            sucess = true;
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Logs.Log.WriteLogErrors("ParseListingRequest Parse", exception);
            }
            return (sucess, listing);
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
