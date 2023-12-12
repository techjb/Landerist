using OpenAI;
using System.Text.Json.Nodes;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ParseListingTool : ParseListingResponse
    {
        public const string FunctionNameIsListing = "si_es_un_anuncio";
        public static readonly string FunctionDescriptionIsListing = "El texto introducido corresponde a un único anuncio inmobiliario";

        public const string FunctionNameIsNotListing = "no_es_un_anuncio";
        public static readonly string FunctionDescriptionIsNotListing = "El texto introducido no corresponde a un único anuncio inmobiliario";

        public static List<Tool> GetTools()
        {
            var functionIsListing = GetToolIsListing();
            var functionIsNotListing = GetToolIsNotListing();

            return [functionIsListing, functionIsNotListing];            
        }

        private static Tool GetToolIsListing()
        {
            var properties = new JsonObject();

            AddString(properties, nameof(FechaDePublicación), "fecha de publicación");
            AddEnum(properties, nameof(TipoDeOperación), "tipo de operación", TiposDeOperación);
            AddEnum(properties, nameof(TipoDeInmueble), "tipo de inmueble", TiposDeInmueble);
            AddEnum(properties, nameof(SubtipoDeInmueble), "subtipo de inmueble", SubtiposDeInmueble);
            AddNumber(properties, nameof(PrecioDelAnuncio), "precio del anuncio");
            AddString(properties, nameof(DescripciónDelAnuncio), "descripción del anuncio");
            AddString(properties, nameof(ReferenciaDelAnuncio), "referencia del anuncio");
            AddString(properties, nameof(TeléfonoDeContacto), "teléfono de contacto");
            AddString(properties, nameof(EmailDeContacto), "email de contacto");
            AddString(properties, nameof(DirecciónDelInmueble), "dirección del inmueble");
            AddString(properties, nameof(ReferenciaCatastral), "referencia catastral");
            AddNumber(properties, nameof(TamañoDelInmueble), "metros cuadrados del inmueble");
            AddNumber(properties, nameof(TamañoDeLaParcela), "metros cuadrados de la parcela");
            AddNumber(properties, nameof(AñoDeConstrucción), "año de construcción");
            AddEnum(properties, nameof(EstadoDeLaConstrucción), "estado de la construcción", EstadosDeLaConstrucción);
            AddNumber(properties, nameof(PlantasDelEdificio), "plantas del edificio");
            AddString(properties, nameof(PlantaDelInmueble), "planta del inmueble");
            AddNumber(properties, nameof(NúmeroDeDormitorios), "número de dormitorios");
            AddNumber(properties, nameof(NúmeroDeBaños), "número de baños");
            AddNumber(properties, nameof(NúmeroDeParkings), "número de parkings");
            AddBoolean(properties, nameof(TieneTerraza), "tiene terraza");
            AddBoolean(properties, nameof(TieneJardín), "tiene jardín");
            AddBoolean(properties, nameof(TieneGaraje), "tiene garaje");
            AddBoolean(properties, nameof(TieneParkingParaMoto), "tiene parking para moto");
            AddBoolean(properties, nameof(TienePiscina), "tiene piscina");
            AddBoolean(properties, nameof(TieneAscensor), "tiene ascensor");
            AddBoolean(properties, nameof(TieneAccesoParaDiscapacitados), "tiene acceso para discapacitados");
            AddBoolean(properties, nameof(TieneTrastero), "tiene trastero");
            AddBoolean(properties, nameof(EstáAmueblado), "está amueblado");
            AddBoolean(properties, nameof(NoEstáAmueblado), "no está amueblado");
            AddBoolean(properties, nameof(TienCalefácción), "tiene calefacción");
            AddBoolean(properties, nameof(TienAireAcondicionado), "tiene aire acondicionado");
            AddBoolean(properties, nameof(PermiteMascotas), "permite mascotas");
            AddBoolean(properties, nameof(TieneSistemasDeSeguridad), "tiene sistemas de seguridad");

            var parameters = new JsonObject()
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = new JsonArray { }
            };

            return new Function(FunctionNameIsListing, FunctionDescriptionIsListing, parameters);
        }

        private static Tool GetToolIsNotListing()
        {
            var properties = new JsonObject();
            JsonArray NoEsUnAnuncio =
            [
                "NO_ES_UN_ANUNCIO"
            ];

            AddEnum(properties, "NoEsUnAnuncio", "No es un anuncio", NoEsUnAnuncio);

            var parameters = new JsonObject()
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = new JsonArray { }
            };

            return new Function(FunctionNameIsNotListing, FunctionDescriptionIsNotListing, parameters);
        }

        private static void AddString(JsonObject jsonObject, string name, string description)
        {
            Add(jsonObject, name, "string", description);
        }

        private static void AddEnum(JsonObject jsonObject, string name, string description, JsonArray jsonArray)
        {
            Add(jsonObject, name, "string", description, jsonArray);
        }

        private static void AddNumber(JsonObject jsonObject, string name, string description)
        {
            Add(jsonObject, name, "number", description);
        }

        private static void AddBoolean(JsonObject jsonObject, string name, string description)
        {
            Add(jsonObject, name, "boolean", description);
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
