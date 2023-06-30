namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ChatGPTIsListing: ChatGPTRequest
    {
        public static string SystemMessage =
            "Actúa como un clasificador de textos. Responde únicamente con las palabras \"sí\" o \"no\".";

        private static string UserMessageHeader = 
            "¿El siguiente texto contiene los datos de una oferta inmobiliaria? " +
            "Responde no cuando haya datos de varios anuncios o ninguno. \n" +
            "----";

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

        public static bool IsTextAllowed(string text)
        {
            string userMessage = GetUserMessage(text);
            return IsLengthAllowed(SystemMessage, userMessage);
        }
    }
}
