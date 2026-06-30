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
            "Tu tarea es analizar el código HTML proporcionado por el usuario y extraer los datos del anuncio inmobiliario principal. " +
            "Una ficha de anuncio individual puede incluir contenido accesorio como inmuebles similares, carruseles, recomendaciones, breadcrumbs, bloques de agencia, formularios, banners, cookies o enlaces a otros inmuebles. " +
            "Si existe un anuncio principal claramente identificable y la mayor parte del contenido gira alrededor de una sola propiedad, extrae ese anuncio aunque aparezcan esos elementos secundarios. " +
            "No confundas una ficha de inmueble con un listado solo porque aparezcan varias tarjetas secundarias al final o en una barra lateral. " +
            "Devuelve JSON válido que siga estrictamente el esquema, con la propiedad anuncio conteniendo los datos extraídos. " +
            "Solo si por algún motivo el HTML no corresponde a un anuncio inmobiliario, devuelve la propiedad anuncio con valor null. " +
            "Respeta estrictamente los límites maxItems y maxLength del esquema. No copies HTML ni JSON embebido dentro de campos de texto. " +
            "Si faltan algunos datos del anuncio, usa null en los campos que no puedas inferir. " +
            "No incluyas explicaciones ni texto adicional ni markdown.";
    }
}
