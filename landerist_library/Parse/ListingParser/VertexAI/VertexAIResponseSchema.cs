using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.Collections;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using static landerist_library.Parse.ListingParser.StructuredOutputs.StructuredOutputEsListing;


namespace landerist_library.Parse.ListingParser.VertexAI
{
    public class VertexAIResponseSchema
    {

        public readonly static OpenApiSchema ResponseSchema = new()
        {
            Type = Google.Cloud.AIPlatform.V1.Type.Object,
            Properties =
            {
                [StructuredOutputEsJson.FunctionNameIsListing] = new()
                {
                    Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                    Description = StructuredOutputEsJson.FunctionNameIsListingDescription,
                },
                [StructuredOutputEsJson.FunctionNameListing]= new()
                {
                    Description =StructuredOutputEsJson.FunctionNameListingDescription,
                    Type = Google.Cloud.AIPlatform.V1.Type.Object,
                    Nullable = true,
                    Properties =
                    {
                        [nameof(StructuredOutputEsJson.fecha_de_publicacion)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Format = "date",
                            Description = StructuredOutputEsJson.FechaDePublicaciónDescription,
                        },
                        [nameof(StructuredOutputEsJson.tipo_de_operacion)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum = { GetEnumValues(typeof(TiposDeOperacion)) },
                            Description = StructuredOutputEsJson.TipoDeOperaciónDescription,
                        },
                        [nameof(StructuredOutputEsJson.tipo_de_inmueble)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum = { GetEnumValues(typeof(TiposDeInmueble)) },
                            Description = StructuredOutputEsJson.TipoDeInmuebleDescription,
                        },
                        [nameof(StructuredOutputEsJson.subtipo_de_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum = { GetEnumValues(typeof(SubtiposDeInmueble)) },
                            Description =  StructuredOutputEsJson.SubtipoDeInmuebleDescription,
                        },
                        [nameof(StructuredOutputEsJson.precio_del_anuncio)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Format = "decimal",
                            Description = StructuredOutputEsJson.PrecioDelAnuncioDescription,
                        },
                        [nameof(StructuredOutputEsJson.descripcion_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsJson.DescripciónDelAnuncioDescription,
                        },
                        [nameof(StructuredOutputEsJson.referencia_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsJson.ReferenciaDelAnuncioDescription,
                        },
                        [nameof(StructuredOutputEsJson.telefono_de_contacto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsJson.TeléfonoDeContactoDescription,
                        },
                        [nameof(StructuredOutputEsJson.email_de_contacto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Format = "email",
                            Description = StructuredOutputEsJson.EmailDeContactoDescription,
                        },
                        [nameof(StructuredOutputEsJson.direccion_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsJson.DirecciónDelInmuebleDescription,
                        },
                        [nameof(StructuredOutputEsJson.referencia_catastral)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsJson.ReferenciaCatastralDescription,
                        },

                        [nameof(StructuredOutputEsJson.tamanio_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.TamañoDelInmuebleDescription,
                        },

                        [nameof(StructuredOutputEsJson.tamanio_de_la_parcela)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.TamañoDeLaParcelaDescription,
                        },
                        [nameof(StructuredOutputEsJson.anio_de_construccion)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.AñoDeConstrucciónDescription,
                        },
                        [nameof(StructuredOutputEsJson.estado_de_la_construccion)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum =  { GetEnumValues(typeof(EstadosDeLaConstrucción)) },
                            Description = StructuredOutputEsJson.EstadoDeLaConstrucciónDescription,
                        },
                        [nameof(StructuredOutputEsJson.plantas_del_edificio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.PlantasDelEdificioDescription,
                        },
                        [nameof(StructuredOutputEsJson.planta_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsJson.PlantaDelInmuebleDescription,
                        },
                        [nameof(StructuredOutputEsJson.numero_de_dormitorios)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.NúmeroDeDormitoriosDescription,
                        },
                        [nameof(StructuredOutputEsJson.numero_de_banios)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.NúmeroDeBañosDescription,
                        },
                        [nameof(StructuredOutputEsJson.numero_de_parkings)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description =  StructuredOutputEsJson.NúmeroDeParkingsDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_terraza)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneTerrazaDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_jardin)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneJardínDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_garaje)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneGarajeDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_parking_para_moto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneParkingParaMotoDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_piscina)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TienePiscinaDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_ascensor)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneAscensorDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_acceso_para_discapacitados)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneAccesoParaDiscapacitadosDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_trastero)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneTrasteroDescription,
                        },
                        [nameof(StructuredOutputEsJson.esta_amueblado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.EstaAmuebladoDescription,
                        },
                        [nameof(StructuredOutputEsJson.no_esta_amueblado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.NoEstaAmuebladoDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_calefaccion)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneCalefacciónDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_aire_acondicionado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneAireAcondicionadoDescription,
                        },
                        [nameof(StructuredOutputEsJson.permite_mascotas)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.PermiteMascotasDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_sistemas_de_seguridad)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.TieneSistemasDeSeguridadDescription,
                        },
                        [nameof(StructuredOutputEsJson.urls_de_imagenes_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Array,
                            Items = new()
                            {
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Format = "uri"
                            },
                            Description = StructuredOutputEsJson.UrlsDeImagenesDelAnuncio,
                        }
                    },
                    Required = {
                        nameof(StructuredOutputEsJson.fecha_de_publicacion),
                        nameof(StructuredOutputEsJson.tipo_de_operacion),
                        nameof(StructuredOutputEsJson.tipo_de_inmueble),
                        nameof(StructuredOutputEsJson.precio_del_anuncio)
                    }
                }
            },
            Required = { StructuredOutputEsJson.FunctionNameIsListing }
        };

        public static RepeatedField<string> GetEnumValues(System.Type enumType)
        {
            var repeatedField = new RepeatedField<string>();
            repeatedField.AddRange(Enum.GetNames(enumType));
            return repeatedField;
        }
    }
}

