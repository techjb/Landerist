﻿using GenerativeAI;

namespace landerist_library.Parse.ListingParser.Gemini
{
    [GenerativeAIFunctions]
    public interface INotListingService
    {
        //[Description("La imagen no corresponde a un único anuncio inmobiliario")]

        public string NoEsUnAnuncio();
    }
}
