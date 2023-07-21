using Amazon.Auth.AccessControlPolicy;
using Newtonsoft.Json;
using OpenAI.Chat;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

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
            "Si encuentras títulos de otros anuncios en el texto, ignóralos a menos que vengan acompañados de toda la información requerida.";


        private static readonly string FunctionCallValidarTexto = "validar_texto";
        private static readonly string ParameterEsAnuncioDeOfertaInmobiliaria = "EsAnuncioDeOfertaInmobiliaria";

        private static readonly List<Function> Functions = new()
        {
            new Function(
                FunctionCallValidarTexto,
                "Valida el texto introducido por el usuario",
                new JsonObject()
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        [ParameterEsAnuncioDeOfertaInmobiliaria] = new JsonObject
                        {
                            ["type"] = "boolean",
                            ["description"] = "El texto es un anuncio de oferta inmobiliaria"
                        },                        
                    },
                    ["required"] = new JsonArray { ParameterEsAnuncioDeOfertaInmobiliaria }
                }
            )
        };



        public ChatGPTIsListing() : base(SystemMessage, Functions, FunctionCallValidarTexto)
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
            if (response!=null)
            {
                var arguments = response.FirstChoice.Message.Function.Arguments.ToString();
                var responseObject = JsonConvert.DeserializeObject(response.FirstChoice.Message.Function.Arguments.ToString());
                object d = responseObject.GetType().GetProperty(ParameterEsAnuncioDeOfertaInmobiliaria).GetValue(responseObject, null);
                //return response.ToLower().StartsWith("sí");
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
