using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.Collections;
using System.Text.Json.Nodes;

namespace landerist_library.Parse.Listing.VertexAI
{
    public class VertexAITools : ParseListingTool
    {
        public static Tool GetTools()
        {
            return new Tool
            {
                FunctionDeclarations =
                {
                    IsListingFunctionDeclaration(),
                    IsNotListingFunctionDeclaration()
                }
            };
        }

        private static FunctionDeclaration IsListingFunctionDeclaration()
        {
            MapField<string, OpenApiSchema> properties = [];

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
            AddString(properties, nameof(ReferenciaCatastral), "referencia catastral (14 o 20 caracteres)");
            AddNumber(properties, nameof(TamañoDelInmueble), "metros cuadrados del inmueble");
            AddNumber(properties, nameof(TamañoDeLaParcela), "metros cuadrados de la parcela");
            AddNumber(properties, nameof(AñoDeConstrucción), "año de construcción del inmueble");
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

            return new FunctionDeclaration
            {
                Name = FunctionNameIsListing,
                Description = FunctionDescriptionIsListing,
                Parameters = new OpenApiSchema
                {
                    Type = Google.Cloud.AIPlatform.V1.Type.Object,
                    Properties = { properties },
                    Required = { }
                }
            };
        }

        private static void AddString(MapField<string, OpenApiSchema> mapField, string name, string description)
        {
            Add(mapField, name, Google.Cloud.AIPlatform.V1.Type.String, description);
        }

        private static void AddEnum(MapField<string, OpenApiSchema> mapField, string name, string description, JsonArray jsonArray)
        {
            OpenApiSchema openApiSchema = new()
            {
                Type = Google.Cloud.AIPlatform.V1.Type.String,
                Description = description,
                Enum =
                {
                    ToEnumList(jsonArray)
                }
            };
            mapField.Add(name, openApiSchema);
        }

        private static void AddNumber(MapField<string, OpenApiSchema> mapField, string name, string description)
        {
            Add(mapField, name, Google.Cloud.AIPlatform.V1.Type.Number, description);
        }

        private static void AddBoolean(MapField<string, OpenApiSchema> mapField, string name, string description)
        {
            Add(mapField, name, Google.Cloud.AIPlatform.V1.Type.Boolean, description);
        }

        private static void Add(MapField<string, OpenApiSchema> mapField, string name, Google.Cloud.AIPlatform.V1.Type type, string description)
        {
            OpenApiSchema openApiSchema = new()
            {
                Type = type,
                Description = description,
            };
            mapField.Add(name, openApiSchema);
        }

        private static List<string> ToEnumList(JsonArray jsonArray)
        {
            var list = new List<string>();
            foreach (var item in jsonArray)
            {
                if (item is null)
                {
                    continue;
                }
                var value = item.GetValue<string>();
                list.Add(value);
            }
            return list;
        }


        private static FunctionDeclaration IsNotListingFunctionDeclaration()
        {
            return new FunctionDeclaration
            {
                Name = FunctionNameIsNotListing,
                Description = FunctionDescriptionIsNotListing,
            };
        }
    }
}
