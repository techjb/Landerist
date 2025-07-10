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
                        [nameof(StructuredOutputEsJson.fecha_de_publicación)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Format = "date",
                            Description = StructuredOutputEsJson.FechaDePublicaciónDescription,
                        },
                        [nameof(StructuredOutputEsJson.tipo_de_operación)] = new()
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
                        [nameof(StructuredOutputEsJson.descripción_del_anuncio)] = new()
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
                        [nameof(StructuredOutputEsJson.nombre_de_contacto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsJson.NombreDeContactoDescription,
                        },
                        [nameof(StructuredOutputEsJson.teléfono_de_contacto)] = new()
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
                        [nameof(StructuredOutputEsJson.dirección_del_inmueble)] = new()
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

                        [nameof(StructuredOutputEsJson.tamaño_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.TamañoDelInmuebleDescription,
                        },

                        [nameof(StructuredOutputEsJson.tamaño_de_la_parcela)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.TamañoDeLaParcelaDescription,
                        },
                        [nameof(StructuredOutputEsJson.año_de_construcción)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.AñoDeConstrucciónDescription,
                        },
                        [nameof(StructuredOutputEsJson.estado_de_la_construcción)] = new()
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
                        [nameof(StructuredOutputEsJson.número_de_dormitorios)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.NúmeroDeDormitoriosDescription,
                        },
                        [nameof(StructuredOutputEsJson.número_de_baños)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsJson.NúmeroDeBañosDescription,
                        },
                        [nameof(StructuredOutputEsJson.número_de_parkings)] = new()
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
                        [nameof(StructuredOutputEsJson.tiene_jardín)] = new()
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
                        [nameof(StructuredOutputEsJson.está_amueblado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.EstaAmuebladoDescription,
                        },
                        [nameof(StructuredOutputEsJson.no_está_amueblado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsJson.NoEstaAmuebladoDescription,
                        },
                        [nameof(StructuredOutputEsJson.tiene_calefacción)] = new()
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
                        [nameof(StructuredOutputEsJson.imágenes_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Array,
                            Items = new()
                            {
                                Type = Google.Cloud.AIPlatform.V1.Type.Object,
                                Properties =
                                {
                                    [nameof(StructuredOutputEsJson.url_de_la_imagen)] = new()
                                    {
                                        Type = Google.Cloud.AIPlatform.V1.Type.String,
                                        Format = "uri",
                                        Description = StructuredOutputEsJson.UrlDeLaImagen,
                                    },
                                    [nameof(StructuredOutputEsJson.título_de_la_imagen)] = new()
                                    {
                                        Type = Google.Cloud.AIPlatform.V1.Type.String,
                                        Nullable = true,
                                        Description = StructuredOutputEsJson.TituloDeLaImagen,
                                    },
                                }
                            },
                            MaxItems = StructuredOutputEsJson.MAX_URLS_DE_IMAGENES_DEL_ANUNCIO,
                            Description = StructuredOutputEsJson.ImagenesDelAnuncio,
                        }
                    },
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

