using OpenAI;
using System.Text.Json.Nodes;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ParseListingTool : ParseListingResponse
    {
        protected const string OPERACION_VENTA = "venta";

        protected const string OPERACION_ALQUILER = "alquiler";

        protected const string TIPO_DE_INMUEBLE_VIVIENDA = "vivienda";

        protected const string TIPO_DE_INMUEBLE_DORMITORIO = "dormitorio";

        protected const string TIPO_DE_INMUEBLE_LOCAL_COMERCIAL = "local_comercial";

        protected const string TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL = "nave_industrial";

        protected const string TIPO_DE_INMUEBLE_GARAJE = "garaje";

        protected const string TIPO_DE_INMUEBLE_TRASTERO = "trastero";

        protected const string TIPO_DE_INMUEBLE_OFICINA = "oficina";

        protected const string TIPO_DE_INMUEBLE_PARCELA = "terreno_o_parcela";

        protected const string TIPO_DE_INMUEBLE_EDIFICIO = "edificio";

        protected const string SUBTIPO_DE_INMUEBLE_PISO = "piso";

        protected const string SUBTIPO_DE_INMUEBLE_APARTAMENTO = "apartamento";

        protected const string SUBTIPO_DE_INMUEBLE_ÁTICO = "ático";

        protected const string SUBTIPO_DE_INMUEBLE_BUNGALOW = "bungalow";

        protected const string SUBTIPO_DE_INMUEBLE_DUPLEX = "dúplex";

        protected const string SUBTIPO_DE_INMUEBLE_CHALET_INDEPENDIENTE = "chalet_independiente";

        protected const string SUBTIPO_DE_INMUEBLE_CHALET_PAREADO = "chalet_pareado";

        protected const string SUBTIPO_DE_INMUEBLE_CHALET_ADOSADO = "chalet_adosado";

        protected const string SUBTIPO_DE_INMUEBLE_PARCELA_URBANA = "parcela_urbana";

        protected const string SUBTIPO_DE_INMUEBLE_PARCELA_URBANIZABLE = "parcela_urbanizable";

        protected const string SUBTIPO_DE_INMUEBLE_PARCELA_NO_URBANIZABLE = "parcela_no_urbanizable";

        protected const string ESTADO_DE_LA_CONSTRUCCIÓN_OBRA_NUEVA = "obra_nueva";

        protected const string ESTADO_DE_LA_CONSTRUCCIÓN_BUENO = "buen_estado";

        protected const string ESTADO_DE_LA_CONSTRUCCIÓN_A_REFORMAR = "a_reformar";

        protected const string ESTADO_DE_LA_CONSTRUCCIÓN_EN_RUINAS = "en_ruinas";

        private static readonly string FunctionName = "data_extractor";
        private static readonly string FunctionDescription = "Extract the data from the advertisement.";

        private static readonly JsonArray TiposDeOperación = new()
        {
            OPERACION_VENTA,
            OPERACION_ALQUILER
        };

        private static readonly JsonArray TiposDeInmueble = new()
        {
            TIPO_DE_INMUEBLE_VIVIENDA,
            TIPO_DE_INMUEBLE_DORMITORIO,
            TIPO_DE_INMUEBLE_LOCAL_COMERCIAL,
            TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL,
            TIPO_DE_INMUEBLE_GARAJE,
            TIPO_DE_INMUEBLE_TRASTERO,
            TIPO_DE_INMUEBLE_OFICINA,
            TIPO_DE_INMUEBLE_PARCELA,
            TIPO_DE_INMUEBLE_EDIFICIO
        };

        private static readonly JsonArray SubtiposDeInmueble = new()
        {
            SUBTIPO_DE_INMUEBLE_PISO,
            SUBTIPO_DE_INMUEBLE_APARTAMENTO,
            SUBTIPO_DE_INMUEBLE_ÁTICO,
            SUBTIPO_DE_INMUEBLE_BUNGALOW,
            SUBTIPO_DE_INMUEBLE_DUPLEX,
            SUBTIPO_DE_INMUEBLE_CHALET_INDEPENDIENTE,
            SUBTIPO_DE_INMUEBLE_CHALET_PAREADO,
            SUBTIPO_DE_INMUEBLE_CHALET_ADOSADO,
            SUBTIPO_DE_INMUEBLE_PARCELA_URBANA,
            SUBTIPO_DE_INMUEBLE_PARCELA_URBANIZABLE,
            SUBTIPO_DE_INMUEBLE_PARCELA_NO_URBANIZABLE
        };

        private static readonly JsonArray EstadosDeLaConstrucción = new()
        {
            ESTADO_DE_LA_CONSTRUCCIÓN_OBRA_NUEVA,
            ESTADO_DE_LA_CONSTRUCCIÓN_BUENO,
            ESTADO_DE_LA_CONSTRUCCIÓN_A_REFORMAR,
            ESTADO_DE_LA_CONSTRUCCIÓN_EN_RUINAS
        };

        public static Function GetTool()
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
            AddNumber(properties, nameof(AñoDeConstrucción), "año de construcción");
            AddEnum(properties, nameof(EstadoDeLaConstrucción), "estado de la construcción", EstadosDeLaConstrucción);
            AddNumber(properties, nameof(PlantasDelEdificio), "plantas del edificio");
            AddNumber(properties, nameof(PlantaDelInmueble), "planta del inmueble");
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

            return new Function(FunctionName, FunctionDescription, parameters);
        }

        private static void AddString(JsonObject jsonObject, string name, string description)
        {
            var property = new JsonObject
            {
                ["type"] = "string",
                ["description"] = description
            };
            jsonObject.Add(name, property);
        }

        private static void AddEnum(JsonObject jsonObject, string name, string description, JsonArray jsonArray)
        {
            var property = new JsonObject
            {
                ["type"] = "string",
                ["description"] = description,
                ["enum"] = jsonArray
            };
            jsonObject.Add(name, property);
        }

        private static void AddNumber(JsonObject jsonObject, string name, string description)
        {
            var property = new JsonObject
            {
                ["type"] = "number",
                ["description"] = description
            };
            jsonObject.Add(name, property);
        }
        private static void AddBoolean(JsonObject jsonObject, string name, string description)
        {
            var property = new JsonObject
            {
                ["type"] = "boolean",
                ["description"] = description
            };
            jsonObject.Add(name, property);
        }
    }
}
