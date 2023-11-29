using OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ParseListingTool
    {
        private static readonly string FunctionName = "obtener_resultado";
        private static readonly string FunctionDescription = "Obtiene el resultados de la evaluación";
        private static readonly string ParameterName = "EsUnAnuncioDeOfertaInmobiliaria";
        private static readonly string ParameterDescription = "El texto sí es un anuncio de oferta inmobiliaria";

        public static readonly Function Tool = new(
                FunctionName,
                FunctionDescription,
                new JsonObject()
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        [ParameterName] = new JsonObject
                        {
                            ["type"] = "boolean",
                            ["description"] = ParameterDescription
                        },
                    },
                    ["required"] = new JsonArray { ParameterName }
                }
            );
    }
}
