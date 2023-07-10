namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ChatGPTIsListing: ChatGPTRequest
    {
        public static string SystemMessage =
            "Un anuncio completo de oferta inmobiliaria debe contener la siguiente información:\r\n\r\n" +
            "1. Tipo de propiedad (por ejemplo, casa, apartamento, terreno, etc.).\r\n" +
            "2. Ubicación (puede ser la ciudad, barrio o dirección exacta).\r\n" +
            "3. Precio de venta o alquiler.\r\n" +
            "4. Descripción detallada de la propiedad (número de habitaciones, baños, tamaño en metros cuadrados, etc.).\r\n\r\n" +            
            "Evalúa el siguiente texto y determina si contiene todos los datos completos de un anuncio de oferta inmobiliaria. " +
            "Asegúrate de identificar la presencia de cada uno de los puntos anteriores en el texto. " +
            "Si encuentras títulos de otros anuncios en el texto, ignóralos a menos que vengan acompañados de toda la información requerida.\r\n\r\n" +
            "Responde únicamente con las palabras \"Sí\" o \"No\".";

        private static string UserMessageHeader = 
            "";

        public ChatGPTIsListing() : base(SystemMessage)
        {

        }

        private static string GetUserMessage(string text)
        {
            return UserMessageHeader + text;
        }

        public bool? IsListing(string text)
        {
            text = GetUserMessage(text);
            var response = GetResponse(text);
            if (!string.IsNullOrEmpty(response))
            {
                return response.ToLower().StartsWith("sí");
            }
            return null;
        }

        public static bool IsTextAllowed(string? text)
        {
            if(text == null)
            {
                return false;
            }
            string userMessage = GetUserMessage(text);
            return IsLengthAllowed(SystemMessage, userMessage);
        }
    }
}
