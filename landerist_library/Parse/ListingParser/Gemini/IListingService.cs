using GenerativeAI;

namespace landerist_library.Parse.ListingParser.Gemini
{
    [GenerativeAIFunctions]
    public interface IListingService
    {
        //[Description("La imagen corresponde a un único anuncio inmobiliario, y no un listado.")]

        public string EsUnAnuncio();
    }
}
