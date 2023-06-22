using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ChatGPTResponseSchema : ChatGPTResponse
    {
        public static string GetSchema()
        {
            List<string> list = new();

            AddStringProperty(list, nameof(FechaDePublicación));
            AddEnumProperty(list, nameof(TipoDeOperacion), new string[] {
                OPERACION_VENTA,
                OPERACION_ALQUILER
            });
            AddEnumProperty(list, nameof(TipoDeInmueble), new string[] {
                TIPO_DE_INMUEBLE_VIVIENDA,
                TIPO_DE_INMUEBLE_DORMITORIO,
                TIPO_DE_INMUEBLE_LOCAL_COMERCIAL,
                TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL,
                TIPO_DE_INMUEBLE_GARAJE,
                TIPO_DE_INMUEBLE_TRASTERO,
                TIPO_DE_INMUEBLE_OFICINA,
                TIPO_DE_INMUEBLE_PARCELA,
                TIPO_DE_INMUEBLE_EDIFICIO
            });

            AddEnumProperty(list, nameof(SubtipoDeInmueble), new string[] {
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
            });

            AddNumberProperty(list, nameof(PrecioDelAnuncio));
            AddStringProperty(list, nameof(DescripciónDelAnuncio));
            AddStringProperty(list, nameof(ReferenciaDelAnuncio));
            AddStringProperty(list, nameof(TeléfonoDeContacto));
            AddStringProperty(list, nameof(EmailDeContacto));
            AddStringProperty(list, nameof(DirecciónDelInmueble));
            AddStringProperty(list, nameof(ReferenciaCatastral));
            AddNumberProperty(list, nameof(TamañoDelInmueble));
            AddNumberProperty(list, nameof(TamañoDeLaParcela));
            AddNumberProperty(list, nameof(AñoDeConstrucción));
            AddEnumProperty(list, nameof(EstadoDeLaConstrucción), new string[] {
                ESTADO_DE_LA_CONSTRUCCIÓN_OBRA_NUEVA,
                ESTADO_DE_LA_CONSTRUCCIÓN_BUENO,
                ESTADO_DE_LA_CONSTRUCCIÓN_A_REFORMAR,
                ESTADO_DE_LA_CONSTRUCCIÓN_EN_RUINAS
            });
            AddNumberProperty(list, nameof(PlantasDelEdificio));
            AddStringProperty(list, nameof(PlantaDelInmueble));
            AddNumberProperty(list, nameof(NúmeroDeDormitorios));
            AddNumberProperty(list, nameof(NúmeroDeBaños));
            AddNumberProperty(list, nameof(NúmeroDeParkings));
            AddBooleanProperty(list, nameof(TieneTerraza));
            AddBooleanProperty(list, nameof(TieneJardín));
            AddBooleanProperty(list, nameof(TieneGaraje));
            AddBooleanProperty(list, nameof(TieneParkingParaMoto));
            AddBooleanProperty(list, nameof(TienePiscina));
            AddBooleanProperty(list, nameof(TieneAscensor));
            AddBooleanProperty(list, nameof(TieneAccesoParaDiscapacitados));
            AddBooleanProperty(list, nameof(TieneTrastero));
            AddBooleanProperty(list, nameof(EstáAmueblado));
            AddBooleanProperty(list, nameof(NoEstáAmueblado));
            AddBooleanProperty(list, nameof(TienCalefácción));
            AddBooleanProperty(list, nameof(TienAireAcondicionado));
            AddBooleanProperty(list, nameof(PermiteMascotas));
            AddBooleanProperty(list, nameof(TieneSistemasDeSeguridad));

            string json = "{" + string.Join(",", list.ToArray()) + "}";
            return json;
        }

        private static string Format(string json)
        {
            JObject jObject = JObject.Parse(json);
            return jObject.ToString(Formatting.Indented);
        }

        private static void AddStringProperty(List<string> list, string name)
        {
            AddProperty(list, name, "string", Array.Empty<string>());
        }

        private static void AddNumberProperty(List<string> list, string name)
        {
            AddProperty(list, name, "number", Array.Empty<string>());
        }

        private static void AddBooleanProperty(List<string> list, string name)
        {
            AddProperty(list, name, "boolean", Array.Empty<string>());
        }

        private static void AddEnumProperty(List<string> list, string name, string[] enums)
        {
            AddProperty(list, name, "string", enums);
        }

        private static void AddProperty(List<string> list, string name, string type, string[] enums)
        {
            type = "\"" + type + "\"";
            if (enums.Length > 0)
            {
                type += " // ('" + string.Join("', '", enums) + "') ";
            }
            name = GetJsonName(name);
            string property = "\"" + name + "\": " + type + "";
            list.Add(property);
        }

        private static string GetJsonName(string name)
        {
            PropertyInfo propertyInfo = typeof(ChatGPTResponse).GetProperty(name)!;
            JsonPropertyAttribute? jsonPropertyAttribute = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
            if (jsonPropertyAttribute != null && jsonPropertyAttribute.PropertyName != null)
            {
                return jsonPropertyAttribute.PropertyName;
            }
            return name!;
        }
    }
}
