using GenerativeAI;
using System.ComponentModel;

namespace landerist_library.Parse.Listing._Gemini
{
    [GenerativeAIFunctions]
    public interface IListingService
    {
        [Description("La imagen corresponde a un único anuncio inmobiliario, y no un listado.")]

        public string EsUnAnuncio();
    }
}
