﻿using landerist_library.Websites;
using OpenAI;
using OpenAI.Chat;
using System.Text;
using System.Text.Json;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ParseListingRequest : ChatGPTRequest
    {
        public static readonly string SystemMessage =
            "Analiza detenidamente el texto proporcionado por el usuario. " +
            "Determina si se trata o no de un anuncio inmobiliario. " +
            "En caso de que sea un anuncio inmobiliario, extrae de manera precisa los elementos clave en formato json. " +
            "Mantente enfocado y da tu mejor respuesta.";

        private static readonly List<Tool> Tools = ParseListingTool.GetTools();

        public ParseListingRequest() : base(SystemMessage, Tools)
        {

        }

        public (PageType pageType, landerist_orels.ES.Listing? listing) Parse(Page page)
        {
            var chatResponse = GetResponse(page.ResponseBodyText, false);
            if (chatResponse == null)
            {
                return (PageType.MayBeListing, null);
            }
            return Parse(page, chatResponse);
        }

        private static (PageType, landerist_orels.ES.Listing?) Parse(Page page, ChatResponse chatResponse)
        {
            PageType pageType = PageType.MayBeListing;
            landerist_orels.ES.Listing? listing = null;

            try
            {
                var tool = chatResponse.FirstChoice.Message.ToolCalls[0];
                string functionName = tool.Function.Name;
                switch (functionName)
                {
                    case ParseListingTool.FunctionNameIsNotListing:
                        {
                            pageType = PageType.NotListing;
                        }
                        break;
                    case ParseListingTool.FunctionNameIsListing:
                        {
                            string arguments = tool.Function.Arguments.ToString();
                            var parseListingResponse = JsonSerializer.Deserialize<ParseListingResponse>(arguments);
                            if (parseListingResponse != null)
                            {
                                listing = parseListingResponse.ToListing(page);
                                if (listing != null)
                                {
                                    pageType = PageType.Listing;
                                }
                                else
                                {

                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("ParseListingRequest Parse", exception);
            }
            return (pageType, listing);
        }

        // Problem with encoding should be fixed in the future:
        // https://community.openai.com/t/gpt-4-1106-preview-messes-up-function-call-parameters-encoding/478500?page=2
        // Has to do with JSON Mode (response format)
        static string EncodeToUTF8(string texto)
        {
            byte[] bytes = Encoding.GetEncoding("Windows-1252").GetBytes(texto);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}