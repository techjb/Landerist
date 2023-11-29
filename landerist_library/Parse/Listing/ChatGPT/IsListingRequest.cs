using landerist_library.Logs;
using OpenAI;
using System.Text.Json;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class IsListingRequest : ChatGPTRequest
    {
        public static readonly string SystemMessage =
            "Un anuncio completo de oferta inmobiliaria debe contener la siguiente información:\r\n\r\n" +
            "1. Tipo de propiedad (por ejemplo, casa, apartamento, terreno, etc.).\r\n" +
            "2. Ubicación (puede ser la ciudad, barrio o dirección exacta).\r\n" +
            "3. Precio de venta o alquiler.\r\n" +
            "4. Descripción detallada de la propiedad (número de habitaciones, baños, tamaño en metros cuadrados, etc.).\r\n\r\n" +
            "Evalúa el texto introducido por el usuario y determina si contiene todos los datos completos de un anuncio de oferta inmobiliaria. " +
            "Asegúrate de identificar la presencia de cada uno de los puntos anteriores en el texto. " +
            "Si encuentras títulos de otros anuncios en el texto, ignóralos a menos que vengan acompañados de toda la información requerida.\r\n\r\n" +
            "Response sólo con \"si\" o \"no\" en formato Json"
            ;

        private static readonly Tool Tool = IsListingTool.Tool;

        public IsListingRequest() : base(SystemMessage, Tool)
        {

        }

        public bool? IsListing(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var response = GetResponse(text, false);
            if (response == null)
            {
                return null;
            }
            try
            {
                var usedTool = response.FirstChoice.Message.ToolCalls[0];
                string arguments = usedTool.Function.Arguments.ToString();
                var isListingResponse = JsonSerializer.Deserialize<IsListingResponse>(arguments);
                if (isListingResponse != null)
                {
                    return isListingResponse?.EsUnAnuncioDeOfertaInmobiliaria;
                }
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("IsListingRequest IsListing", exception);
            }
            return null;
        }

        public static bool IsLengthAllowed(string? text)
        {
            if (text == null)
            {
                return false;
            }
            return IsLengthAllowed(SystemMessage, text);
        }
    }
}
