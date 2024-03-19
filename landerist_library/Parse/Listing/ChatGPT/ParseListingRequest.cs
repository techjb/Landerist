using landerist_library.Websites;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ParseListingRequest : ChatGPTRequest
    {

        //public static readonly string SystemMessage =
        //   "Un anuncio completo de oferta inmobiliaria debe contener la siguiente información:\r\n\r\n" +
        //   "1. Tipo de propiedad (por ejemplo, casa, apartamento, terreno, etc.).\r\n" +
        //   "2. Ubicación (puede ser la ciudad, barrio o dirección exacta).\r\n" +
        //   "3. Precio de venta o alquiler.\r\n" +
        //   "4. Descripción detallada de la propiedad (número de habitaciones, baños, tamaño en metros cuadrados, etc.).\r\n\r\n" +
        //   "Evalúa el texto introducido por el usuario y determina si contiene todos los datos completos de un anuncio de oferta inmobiliaria. " +
        //   "Asegúrate de identificar la presencia de cada uno de los puntos anteriores en el texto. " +
        //   "Si encuentras títulos de otros anuncios en el texto, ignóralos a menos que vengan acompañados de toda la información requerida.\r\n\r\n" +
        //   "Response sólo con \"si\" o \"no\" en formato Json"
        //   ;

        public static readonly string SystemMessage =
            "Tu tarea consiste en procesar el texto proporcionado por el usuario, identificando si corresponde a un anuncio inmobiliario. " +
            "De ser así, deberás analizar meticulosamente el contenido para determinar que efectivamente se trata de un único anuncio y proceder a extraer los datos relevantes.  " +
            "Estos deberán ser presentados en un formato estructurado JSON, asegurando una precisión exhaustiva en la identificación y extracción de los elementos clave. " +
            "Es imperativo que mantengas un enfoque riguroso durante este proceso para ofrecer la respuesta más precisa y de la más alta calidad posible.";

        private static readonly List<Tool> Tools = ParseListingTool.GetTools();

        public ParseListingRequest() : base(SystemMessage, Tools)
        {

        }

        public static bool TooManyTokens(Page page)
        {
            if (page.ResponseBodyText == null)
            {
                return false;
            }
            return TooManyTokens(SystemMessage, page.ResponseBodyText);
        }

        public (PageType pageType, landerist_orels.ES.Listing? listing) Parse(Page page)
        {
            var chatResponse = GetResponse(page.ResponseBodyText);
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (chatResponse == null)
            {
                return result;
            }

            var tool = GetTool(chatResponse);
            if (tool == null)
            {
                return result;
            }
            switch (tool.Function.Name)
            {
                case ParseListingTool.FunctionNameIsNotListing:
                    {
                        result.pageType = PageType.NotListingByParser;
                    }
                    break;
                case ParseListingTool.FunctionNameIsListing:
                    {
                        result = ParseListing(page, tool);
                    }
                    break;
            }

            return result;
        }

        private static Tool? GetTool(ChatResponse chatResponse)
        {
            try
            {
                return chatResponse.FirstChoice.Message.ToolCalls[0];                
            }
            catch 
            {
            }
            return null;
        }

        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseListing(Page page, Tool tool)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            try
            {
                string arguments = tool.Function.Arguments.ToString();
                var parseListingResponse = JsonSerializer.Deserialize<ParseListingResponse>(arguments);
                if (parseListingResponse != null)
                {
                    result.pageType = PageType.ListingButNotParsed;
                    result.listing = parseListingResponse.ToListing(page);
                    if (result.listing != null)
                    {
                        result.pageType = PageType.Listing;
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("ParseListingRequest ParseListing", page.Uri, exception);
            }
            return result;
        }
    }
}
