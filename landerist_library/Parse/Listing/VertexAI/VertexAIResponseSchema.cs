using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.Collections;
using static landerist_library.Parse.Listing.OpenAI.Anuncio;


namespace landerist_library.Parse.Listing.VertexAI
{
    public class VertexAIResponseSchema
    {


        public readonly static OpenApiSchema Schema = new()
        {
            Type = Google.Cloud.AIPlatform.V1.Type.Object,
            Properties =
            {
                [ParseListingTool.FunctionNameIsListing] = new()
                {
                    Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                    Description = ParseListingTool.FunctionNameIsListingDescription,
                },
                [ParseListingTool.FunctionNameListing]= new()
                {
                    Description =ParseListingTool.FunctionNameListingDescription,
                    Type = Google.Cloud.AIPlatform.V1.Type.Object,
                    Nullable = true,
                    Properties =
                    {
                        [nameof(ParseListingTool.fecha_de_publicacion)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Format = "date",
                            Description = ParseListingTool.FechaDePublicaciónDescription,
                        },
                        [nameof(ParseListingTool.tipo_de_operacion)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum = { GetEnumValues(typeof(TiposDeOperacion)) },
                            Description = ParseListingTool.TipoDeOperaciónDescription,
                        },
                        [nameof(ParseListingTool.tipo_de_inmueble)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum = { GetEnumValues(typeof(TiposDeInmueble)) },
                            Description = ParseListingTool.TipoDeInmuebleDescription,
                        },
                        [nameof(ParseListingTool.subtipo_de_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum = { GetEnumValues(typeof(SubtiposDeInmueble)) },
                            Description =  ParseListingTool.SubtipoDeInmuebleDescription,
                        },
                        [nameof(ParseListingTool.precio_del_anuncio)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Format = "decimal",
                            Description = ParseListingTool.PrecioDelAnuncioDescription,
                        },
                        [nameof(ParseListingTool.descripcion_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = ParseListingTool.DescripciónDelAnuncioDescription,
                        },
                        [nameof(ParseListingTool.referencia_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = ParseListingTool.ReferenciaDelAnuncioDescription,
                        },
                        [nameof(ParseListingTool.telefono_de_contacto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = ParseListingTool.TeléfonoDeContactoDescription,
                        },
                        [nameof(ParseListingTool.email_de_contacto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Format = "email",
                            Description = ParseListingTool.EmailDeContactoDescription,
                        },
                        [nameof(ParseListingTool.direccion_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = ParseListingTool.DirecciónDelInmuebleDescription,
                        },
                        [nameof(ParseListingTool.referencia_catastral)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = ParseListingTool.ReferenciaCatastralDescription,
                        },

                        [nameof(ParseListingTool.tamanio_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = ParseListingTool.TamañoDelInmuebleDescription,
                        },

                        [nameof(ParseListingTool.tamanio_de_la_parcela)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = ParseListingTool.TamañoDeLaParcelaDescription,
                        },
                        [nameof(ParseListingTool.anio_de_construccion)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = ParseListingTool.AñoDeConstrucciónDescription,
                        },
                        [nameof(ParseListingTool.estado_de_la_construccion)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum =  { GetEnumValues(typeof(EstadosDeLaConstrucción)) },
                            Description = ParseListingTool.EstadoDeLaConstrucciónDescription,
                        },
                        [nameof(ParseListingTool.plantas_del_edificio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = ParseListingTool.PlantasDelEdificioDescription,
                        },
                        [nameof(ParseListingTool.planta_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = ParseListingTool.PlantaDelInmuebleDescription,
                        },
                        [nameof(ParseListingTool.numero_de_dormitorios)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = ParseListingTool.NúmeroDeDormitoriosDescription,
                        },
                        [nameof(ParseListingTool.numero_de_banios)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = ParseListingTool.NúmeroDeBañosDescription,
                        },
                        [nameof(ParseListingTool.numero_de_parkings)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description =  ParseListingTool.NúmeroDeParkingsDescription,
                        },
                        [nameof(ParseListingTool.tiene_terraza)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneTerrazaDescription,
                        },
                        [nameof(ParseListingTool.tiene_jardin)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneJardínDescription,
                        },
                        [nameof(ParseListingTool.tiene_garaje)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneGarajeDescription,
                        },
                        [nameof(ParseListingTool.tiene_parking_para_moto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneParkingParaMotoDescription,
                        },
                        [nameof(ParseListingTool.tiene_piscina)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TienePiscinaDescription,
                        },
                        [nameof(ParseListingTool.tiene_ascensor)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneAscensorDescription,
                        },
                        [nameof(ParseListingTool.tiene_acceso_para_discapacitados)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneAccesoParaDiscapacitadosDescription,
                        },
                        [nameof(ParseListingTool.tiene_trastero)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneTrasteroDescription,
                        },
                        [nameof(ParseListingTool.esta_amueblado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.EstaAmuebladoDescription,
                        },
                        [nameof(ParseListingTool.no_esta_amueblado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.NoEstaAmuebladoDescription,
                        },
                        [nameof(ParseListingTool.tiene_calefaccion)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneCalefacciónDescription,
                        },
                        [nameof(ParseListingTool.tiene_aire_acondicionado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneAireAcondicionadoDescription,
                        },
                        [nameof(ParseListingTool.permite_mascotas)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.PermiteMascotasDescription,
                        },
                        [nameof(ParseListingTool.tiene_sistemas_de_seguridad)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = ParseListingTool.TieneSistemasDeSeguridadDescription,
                        },
                        [nameof(ParseListingTool.urls_de_imagenes_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Array,
                            Items = new()
                            {
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Format = "uri"
                            },
                            Description = ParseListingTool.UrlsDeImagenesDelAnuncio,
                        }
                    },
                    Required = {
                        nameof(ParseListingTool.fecha_de_publicacion),
                        nameof(ParseListingTool.tipo_de_operacion),
                        nameof(ParseListingTool.tipo_de_inmueble),
                        nameof(ParseListingTool.precio_del_anuncio)
                    }
                }
            },
            Required = { ParseListingTool.FunctionNameIsListing }
        };

        public static RepeatedField<string> GetEnumValues(System.Type enumType)
        {
            var repeatedField = new RepeatedField<string>();
            repeatedField.AddRange(Enum.GetNames(enumType));
            return repeatedField;
        }
    }
}

