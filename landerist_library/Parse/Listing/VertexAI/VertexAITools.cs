using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.Collections;
using System.Text.Json.Nodes;

namespace landerist_library.Parse.Listing.VertexAI
{
    public class VertexAITools : ParseListingTool
    {
        public Tool GetTools()
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

        private FunctionDeclaration IsListingFunctionDeclaration()
        {
            MapField<string, OpenApiSchema> properties = [];

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
            AddInteger(properties, nameof(anio_de_construccion));
            AddEnum(properties, nameof(estado_de_la_construccion), EstadosDeLaConstrucción);
            AddInteger(properties, nameof(plantas_del_edificio));
            AddString(properties, nameof(planta_del_inmueble));
            AddInteger(properties, nameof(numero_de_dormitorios));
            AddInteger(properties, nameof(numero_de_banios));
            AddInteger(properties, nameof(numero_de_parkings));
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

        private static void AddString(MapField<string, OpenApiSchema> mapField, string name)
        {
            Add(mapField, name, Google.Cloud.AIPlatform.V1.Type.String);
        }

        private static void AddEnum(MapField<string, OpenApiSchema> mapField, string name, JsonArray jsonArray)
        {
            OpenApiSchema openApiSchema = new()
            {
                Type = Google.Cloud.AIPlatform.V1.Type.String,
                Description = GetDescriptionAttribute(name),
                Enum =
                {
                    ToEnumList(jsonArray)
                }
            };
            mapField.Add(name, openApiSchema);
        }

        private static void AddNumber(MapField<string, OpenApiSchema> mapField, string name)
        {
            Add(mapField, name, Google.Cloud.AIPlatform.V1.Type.Number);
        }

        private static void AddInteger(MapField<string, OpenApiSchema> mapField, string name)
        {
            Add(mapField, name, Google.Cloud.AIPlatform.V1.Type.Integer);
        }

        private static void AddBoolean(MapField<string, OpenApiSchema> mapField, string name)
        {
            Add(mapField, name, Google.Cloud.AIPlatform.V1.Type.Boolean);
        }

        private static void Add(MapField<string, OpenApiSchema> mapField, string name, Google.Cloud.AIPlatform.V1.Type type)
        {
            var description = GetDescriptionAttribute(name);
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
