using GenerativeAI;

namespace landerist_library.Parse.Listing.Gemini
{
    [GenerativeAIFunctions]
    public interface INotListingService
    {
        //[Description("La imagen no corresponde a un único anuncio inmobiliario")]

        public string NoEsUnAnuncio();
    }
}
