using AI.Dev.OpenAI.GPT;
using landerist_library.Configuration;
using landerist_library.Pages;
using landerist_library.Parse.ListingParser.LocalAI;
using landerist_library.Parse.ListingParser.OpenAI;
using landerist_library.Parse.ListingParser.VertexAI;

namespace landerist_library.Parse.ListingParser
{
    public class SytemPrompt
    {
        public static readonly string Text =
            "Tu tarea es analizar el código HTML proporcionado por el usuario y determinar si corresponde a una página web de un único anuncio inmobiliario. " +
            "Una ficha de anuncio individual puede incluir contenido accesorio como inmuebles similares, carruseles, recomendaciones, breadcrumbs, bloques de agencia, formularios, banners, cookies o enlaces a otros inmuebles. " +
            "Si existe un anuncio principal claramente identificable y la mayor parte del contenido gira alrededor de una sola propiedad, debes considerarlo un anuncio individual aunque aparezcan esos elementos secundarios. " +
            "Solo debes responder que no es un anuncio individual cuando el contenido principal de la página sea claramente un listado de resultados, una página de categoría, una home, una página corporativa o una colección de varias propiedades con peso similar. " +
            "No confundas una ficha de inmueble con un listado solo porque aparezcan varias tarjetas secundarias al final o en una barra lateral. " +
            "En caso de identificar que el HTML corresponde a un único anuncio inmobiliario, extrae los datos relevantes y devuélvelos en formato JSON válido que siga estrictamente el esquema. " +
            "Si faltan algunos datos, mantén la clasificación como anuncio individual y usa null en los campos que no puedas inferir. " +
            "No incluyas explicaciones ni texto adicional ni markdown.";
    }
}
