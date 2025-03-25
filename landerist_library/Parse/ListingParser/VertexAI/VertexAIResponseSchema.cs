using Google.Cloud.AIPlatform.V1;
using Google.Protobuf.Collections;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using static landerist_library.Parse.ListingParser.StructuredOutputs.StructuredOutputEsListing;


namespace landerist_library.Parse.ListingParser.VertexAI
{
    public class VertexAIResponseSchema
    {


        public readonly static OpenApiSchema Schema = new()
        {
            Type = Google.Cloud.AIPlatform.V1.Type.Object,
            Properties =
            {
                [StructuredOutputEsParse.FunctionNameIsListing] = new()
                {
                    Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                    Description = StructuredOutputEsParse.FunctionNameIsListingDescription,
                },
                [StructuredOutputEsParse.FunctionNameListing]= new()
                {
                    Description =StructuredOutputEsParse.FunctionNameListingDescription,
                    Type = Google.Cloud.AIPlatform.V1.Type.Object,
                    Nullable = true,
                    Properties =
                    {
                        [nameof(StructuredOutputEsParse.fecha_de_publicacion)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Format = "date",
                            Description = StructuredOutputEsParse.FechaDePublicaciónDescription,
                        },
                        [nameof(StructuredOutputEsParse.tipo_de_operacion)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum = { GetEnumValues(typeof(TiposDeOperacion)) },
                            Description = StructuredOutputEsParse.TipoDeOperaciónDescription,
                        },
                        [nameof(StructuredOutputEsParse.tipo_de_inmueble)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum = { GetEnumValues(typeof(TiposDeInmueble)) },
                            Description = StructuredOutputEsParse.TipoDeInmuebleDescription,
                        },
                        [nameof(StructuredOutputEsParse.subtipo_de_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum = { GetEnumValues(typeof(SubtiposDeInmueble)) },
                            Description =  StructuredOutputEsParse.SubtipoDeInmuebleDescription,                            
                        },
                        [nameof(StructuredOutputEsParse.precio_del_anuncio)] = new()
                        {
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Format = "decimal",
                            Description = StructuredOutputEsParse.PrecioDelAnuncioDescription,
                        },
                        [nameof(StructuredOutputEsParse.descripcion_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsParse.DescripciónDelAnuncioDescription,
                        },
                        [nameof(StructuredOutputEsParse.referencia_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsParse.ReferenciaDelAnuncioDescription,
                        },
                        [nameof(StructuredOutputEsParse.telefono_de_contacto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsParse.TeléfonoDeContactoDescription,
                        },
                        [nameof(StructuredOutputEsParse.email_de_contacto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Format = "email",
                            Description = StructuredOutputEsParse.EmailDeContactoDescription,
                        },
                        [nameof(StructuredOutputEsParse.direccion_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsParse.DirecciónDelInmuebleDescription,
                        },
                        [nameof(StructuredOutputEsParse.referencia_catastral)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsParse.ReferenciaCatastralDescription,
                        },

                        [nameof(StructuredOutputEsParse.tamanio_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsParse.TamañoDelInmuebleDescription,
                        },

                        [nameof(StructuredOutputEsParse.tamanio_de_la_parcela)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsParse.TamañoDeLaParcelaDescription,
                        },
                        [nameof(StructuredOutputEsParse.anio_de_construccion)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsParse.AñoDeConstrucciónDescription,
                        },
                        [nameof(StructuredOutputEsParse.estado_de_la_construccion)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Enum =  { GetEnumValues(typeof(EstadosDeLaConstrucción)) },
                            Description = StructuredOutputEsParse.EstadoDeLaConstrucciónDescription,
                        },
                        [nameof(StructuredOutputEsParse.plantas_del_edificio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsParse.PlantasDelEdificioDescription,
                        },
                        [nameof(StructuredOutputEsParse.planta_del_inmueble)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.String,
                            Description = StructuredOutputEsParse.PlantaDelInmuebleDescription,
                        },
                        [nameof(StructuredOutputEsParse.numero_de_dormitorios)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsParse.NúmeroDeDormitoriosDescription,
                        },
                        [nameof(StructuredOutputEsParse.numero_de_banios)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description = StructuredOutputEsParse.NúmeroDeBañosDescription,
                        },
                        [nameof(StructuredOutputEsParse.numero_de_parkings)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Integer,
                            Description =  StructuredOutputEsParse.NúmeroDeParkingsDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_terraza)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneTerrazaDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_jardin)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneJardínDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_garaje)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneGarajeDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_parking_para_moto)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneParkingParaMotoDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_piscina)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TienePiscinaDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_ascensor)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneAscensorDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_acceso_para_discapacitados)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneAccesoParaDiscapacitadosDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_trastero)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneTrasteroDescription,
                        },
                        [nameof(StructuredOutputEsParse.esta_amueblado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.EstaAmuebladoDescription,
                        },
                        [nameof(StructuredOutputEsParse.no_esta_amueblado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.NoEstaAmuebladoDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_calefaccion)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneCalefacciónDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_aire_acondicionado)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneAireAcondicionadoDescription,
                        },
                        [nameof(StructuredOutputEsParse.permite_mascotas)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.PermiteMascotasDescription,
                        },
                        [nameof(StructuredOutputEsParse.tiene_sistemas_de_seguridad)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                            Description = StructuredOutputEsParse.TieneSistemasDeSeguridadDescription,
                        },
                        [nameof(StructuredOutputEsParse.urls_de_imagenes_del_anuncio)] = new()
                        {
                            Nullable = true,
                            Type = Google.Cloud.AIPlatform.V1.Type.Array,
                            Items = new()
                            {
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Format = "uri"
                            },
                            Description = StructuredOutputEsParse.UrlsDeImagenesDelAnuncio,
                        }
                    },
                    Required = {
                        nameof(StructuredOutputEsParse.fecha_de_publicacion),
                        nameof(StructuredOutputEsParse.tipo_de_operacion),
                        nameof(StructuredOutputEsParse.tipo_de_inmueble),
                        nameof(StructuredOutputEsParse.precio_del_anuncio)
                    }
                }
            },
            Required = { StructuredOutputEsParse.FunctionNameIsListing }
        };

        public static RepeatedField<string> GetEnumValues(System.Type enumType)
        {
            var repeatedField = new RepeatedField<string>();
            repeatedField.AddRange(Enum.GetNames(enumType));
            return repeatedField;
        }
    }
}

