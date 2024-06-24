using OpenAI;
using System.Text.Json.Nodes;

namespace landerist_library.Parse.Listing.OpenAI
{
    public class OpenAITools : ParseListingTool
    {
        public static List<Tool> GetTools()
        {
            var functionIsListing = GetToolIsListing();
            var functionIsNotListing = GetToolIsNotListing();

            return [functionIsListing, functionIsNotListing];
        }

        private static Tool GetToolIsListing()
        {
            var parameters = GetJsonObject();
            return new Function(FunctionNameIsListing, FunctionDescriptionIsListing, parameters);
        }

        public static JsonObject GetJsonObject()
        {
            var properties = new JsonObject();

            AddString(properties, nameof(fecha_de_publicacion));
            AddEnum(properties, nameof(tipo_de_operacion), TiposDeOperación);
            AddEnum(properties, nameof(tipo_de_inmueble), TiposDeInmueble);
            AddEnum(properties, nameof(subtipo_de_inmueble), SubtiposDeInmueble);
            AddNumber(properties, nameof(precio_del_anuncio));
            AddString(properties, nameof(descripcion_del_anuncio));
            AddString(properties, nameof(referencia_del_anuncio));
            AddString(properties, nameof(telefono_de_contacto));
            AddString(properties, nameof(email_de_contacto));
            AddString(properties, nameof(direccion_del_inmueble));
            AddString(properties, nameof(referencia_catastral));
            AddNumber(properties, nameof(tamanio_del_inmueble));
            AddNumber(properties, nameof(tamanio_de_la_parcela));
            AddNumber(properties, nameof(anio_de_construccion));
            AddEnum(properties, nameof(estado_de_la_construccion), EstadosDeLaConstrucción);
            AddNumber(properties, nameof(plantas_del_edificio));
            AddString(properties, nameof(plantas_del_inmueble));
            AddNumber(properties, nameof(numero_de_dormitorios));
            AddNumber(properties, nameof(numero_de_banios));
            AddNumber(properties, nameof(numero_de_parkings));
            AddBoolean(properties, nameof(tiene_terraza));
            AddBoolean(properties, nameof(tiene_jardin));
            AddBoolean(properties, nameof(tiene_garaje));
            AddBoolean(properties, nameof(tiene_parking_para_moto));
            AddBoolean(properties, nameof(tiene_piscina));
            AddBoolean(properties, nameof(tiene_ascensor));
            AddBoolean(properties, nameof(tiene_acceso_para_discapacitados));
            AddBoolean(properties, nameof(tiene_trastero));
            AddBoolean(properties, nameof(esta_amueblado));
            AddBoolean(properties, nameof(no_esta_amueblado));
            AddBoolean(properties, nameof(tiene_calefaccion));
            AddBoolean(properties, nameof(tiene_aire_acondicionado));
            AddBoolean(properties, nameof(permite_mascotas));
            AddBoolean(properties, nameof(tiene_sistemas_de_seguridad));

            return new JsonObject()
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = new JsonArray { }
            };
        }

        private static Tool GetToolIsNotListing()
        {
            var properties = new JsonObject();
            JsonArray NoEsUnAnuncio =
            [
                "NO_ES_UN_ANUNCIO"
            ];

            Add(properties, FunctionNameIsNotListing, "string", FunctionDescriptionIsNotListing, NoEsUnAnuncio);

            var parameters = new JsonObject()
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = new JsonArray { }
            };

            return new Function(FunctionNameIsNotListing, FunctionDescriptionIsNotListing, parameters);
        }

        private static void AddString(JsonObject jsonObject, string name)
        {
            Add(jsonObject, name, "string");
        }

        private static void AddEnum(JsonObject jsonObject, string name, JsonArray jsonArray)
        {
            Add(jsonObject, name, "string", jsonArray);
        }

        private static void AddNumber(JsonObject jsonObject, string name)
        {
            Add(jsonObject, name, "number");
        }

        private static void AddBoolean(JsonObject jsonObject, string name)
        {
            Add(jsonObject, name, "boolean");
        }

        private static void Add(JsonObject jsonObject, string name, string type, JsonArray? jsonArray = null)
        {
            var description = GetDescriptionAttribute(name);
            Add(jsonObject, name, type, description, jsonArray);
        }
        private static void Add(JsonObject jsonObject, string name, string type, string description, JsonArray? jsonArray = null)
        {
            var property = new JsonObject
            {
                ["type"] = type,
                ["description"] = description
            };
            if (jsonArray != null)
            {
                property.Add("enum", jsonArray);
            }
            jsonObject.Add(name, property);
        }
    }
}
