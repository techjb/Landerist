using landerist_library.Logs;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace landerist_library.Parse.Listing.ChatGPT
{

    public class ChatGPTIsListing : ChatGPTRequest
    {
        public static readonly string SystemMessage =
            "Un anuncio completo de oferta inmobiliaria debe contener la siguiente información:\r\n\r\n" +
            "1. Tipo de propiedad (por ejemplo, casa, apartamento, terreno, etc.).\r\n" +
            "2. Ubicación (puede ser la ciudad, barrio o dirección exacta).\r\n" +
            "3. Precio de venta o alquiler.\r\n" +
            "4. Descripción detallada de la propiedad (número de habitaciones, baños, tamaño en metros cuadrados, etc.).\r\n\r\n" +
            "Evalúa el siguiente texto y determina si contiene todos los datos completos de un anuncio de oferta inmobiliaria. " +
            "Asegúrate de identificar la presencia de cada uno de los puntos anteriores en el texto. " +
            "Si encuentras títulos de otros anuncios en el texto, ignóralos a menos que vengan acompañados de toda la información requerida.\r\n\r\n" +
            "Response sólo con \"sí\" o \"no\""
            ;

        private static readonly string FunctionCallValidarTexto = "obtener_resultados";
        private static readonly string FunctionCallValidarTextoDescription = "Obtiene el resultados de la evaluación";
        private static readonly string ParameterEsAnuncioDeOfertaInmobiliaria = "EsUnAnuncioDeOfertaInmobiliaria";
        private static readonly string ParameterEsAnuncioDeOfertaInmobiliariaDescription = "El texto sí es un anuncio de oferta inmobiliaria";

        private static readonly List<Tool> Tools = new()
        {
            new Function(
                FunctionCallValidarTexto,
                FunctionCallValidarTextoDescription,
                new JsonObject()
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        [ParameterEsAnuncioDeOfertaInmobiliaria] = new JsonObject
                        {
                            ["type"] = "boolean",
                            ["description"] = ParameterEsAnuncioDeOfertaInmobiliariaDescription
                        },
                    },
                    ["required"] = new JsonArray { ParameterEsAnuncioDeOfertaInmobiliaria }
                }
            )
        };

        public ChatGPTIsListing() : base(SystemMessage, Tools, FunctionCallValidarTexto)
        {

        }

        public bool? IsListing(string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            if (!IsTextAllowed(text))
            {
                return null;
            }

            var response = GetResponse(text);
            if (response == null)
            {
                return null;
            }
            try
            {
                var usedTool = response.FirstChoice.Message.ToolCalls[0];
                bool isListing = JsonSerializer.Deserialize<bool>(usedTool.Function.Arguments.ToString());
                return isListing;
                //var functionResult = FunctionCallValidarTexto(functionArgs);

                //string message = response.FirstChoice.Message;
                //return message.ToLower().Equals("sí");

                //var arguments = response.FirstChoice.Message.Function.Arguments.ToString();
                //JObject json = JObject.Parse(arguments);
                //return (bool?)json[ParameterEsAnuncioDeOfertaInmobiliaria];
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("ChatGPT IsListing", exception);
            }
            return null;
        }

        public static bool IsTextAllowed(string? text)
        {
            if (text == null)
            {
                return false;
            }
            return IsLengthAllowed(SystemMessage, text);
        }
    }
}
