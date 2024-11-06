using Anthropic.SDK.Common;
using Anthropic.SDK.Messaging;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Anthropic_SDK_Common = Anthropic.SDK.Common;


namespace landerist_library.Parse.Listing.Anthropic
{
    public class AnthropicTools : ParseListingTool
    {
        public List<Anthropic_SDK_Common.Tool> GetTools()
        {
            var functionIsListing = GetFunctionIsListing();
            var functionIsNotListing = GetFunctionIsNotListing();

            return [functionIsListing, functionIsNotListing];
        }

        public static Function GetFunctionIsNotListing()
        {
            var parameters = new JsonObject()
            {
                ["type"] = "object",
                ["properties"] = new JsonObject(),
                ["required"] = new JsonArray { }
            };

            return new Function(FunctionNameIsNotListing, FunctionDescriptionIsNotListing, parameters);
        }


        public Function GetFunctionIsListing()
        {
            var inputschema = GetInputSchema();

            JsonSerializerOptions? jsonSerializerOptions = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };
            string jsonString = JsonSerializer.Serialize(inputschema, jsonSerializerOptions);
            return new Function(FunctionNameIsListing, FunctionDescriptionIsListing, JsonNode.Parse(jsonString));
        }

        public InputSchema GetInputSchema()
        {
            var properties = new Dictionary<string, Property>();

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
            AddString(properties, nameof(planta_del_inmueble));
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

            return new InputSchema()
            {
                Type = "object",
                Properties = properties,
                Required = []
            };
        }


        private static void AddString(Dictionary<string, Property> properties, string name)
        {
            Add(properties, name, "string");
        }

        private static void AddEnum(Dictionary<string, Property> properties, string name, JsonArray jsonArray)
        {
            Add(properties, name, "string", jsonArray);
        }

        private static void AddNumber(Dictionary<string, Property> properties, string name)
        {
            Add(properties, name, "number");
        }

        private static void AddBoolean(Dictionary<string, Property> properties, string name)
        {
            Add(properties, name, "boolean");
        }

        private static void Add(Dictionary<string, Property> properties, string name, string type, JsonArray? jsonArray = null)
        {
            var description = GetDescriptionAttribute(name);
            Add(properties, name, type, description, jsonArray);
        }
        private static void Add(Dictionary<string, Property> properties, string name, string type, string description, JsonArray? jsonArray = null)
        {
            var property = new Property
            {
                Type = type,
                Description = description
            };
            if (jsonArray != null)
            {
                string[] stringArray = jsonArray.Select(jsonElement => jsonElement!.ToString()).ToArray();

                property.Enum = stringArray;
            }
            properties.Add(name, property);
        }
    }
}
