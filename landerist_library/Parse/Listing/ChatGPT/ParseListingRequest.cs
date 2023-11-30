﻿using landerist_library.Websites;
using OpenAI;
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
            var response = GetResponse(page.ResponseBodyText, true);
            if (response == null)
            {
                return null;
            }
            try
            {
                var usedTool = response.FirstChoice.Message.ToolCalls[0];
                string arguments = usedTool.Function.Arguments.ToString();
                var parseListingResponse = JsonSerializer.Deserialize<ParseListingResponse>(arguments);
                if (parseListingResponse != null)
                {
                    return parseListingResponse.ToListing(page);
                }

            }
            catch (Exception exception)
            {
                Logs.Log.WriteLogErrors("ParseListingRequest Parse", exception);
            }
            return null;
        }
    }
}
