namespace landerist_library.Parse.Listing
{
    public class ParseListingRequest
    {
        protected static readonly string SystemPrompt =
            "Tu tarea consiste en analizar el texto html proporcionado por el usuario, identificando si corresponde a una página web de un anuncio inmobiliario. " +
            "En caso de ser un anuncio inmobiliario deberás proceder a extraer los datos relevantes. " +
            "Asegúrate de tener una precisión exhaustiva en la identificación y extracción de los elementos clave. " +
            "Es imperativo que mantengas un enfoque riguroso durante este proceso para ofrecer la respuesta más precisa y de la más alta calidad posible.";

    }
}
