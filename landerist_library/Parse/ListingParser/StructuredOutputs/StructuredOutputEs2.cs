using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System.ComponentModel.DataAnnotations;
using System.Reflection;


namespace landerist_library.Parse.ListingParser.StructuredOutputs
{
    public class StructuredOutputEs2
    {
        [JsonProperty(StructuredOutputEsJson.FunctionNameIsListing, Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FunctionNameIsListingDescription)]
        public bool EsUnAnuncio { get; private set; }


        [JsonProperty(StructuredOutputEsJson.FunctionNameListing, Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FunctionNameListingDescription)]
        public DatosDelAnuncio? Anuncio { get; private set; }

    }

    public class DatosDelAnuncio
    {
        public enum TiposDeOperacion
        {
            venta,
            alquiler
        }

        public enum TiposDeInmueble
        {
            [Display(Description = "Vivienda. Propiedad residencial completa destinada a vivienda familiar: piso, ático, dúplex, apartamento, chalet, adosado, casa rústica")]
            vivienda,

            [Display(Description = "Dormitorio. Habitación individual o espacio para dormir")]
            dormitorio,

            [Display(Description = "Local Comercial. Espacio destinado a actividades comerciales y venta al público")]
            local_comercial,

            [Display(Description = "Nave industrial. Instalación industrial para actividades de producción o almacenamiento")]
            nave_industrial,

            [Display(Description = "Garaje. Plaza de aparcamiento cubierta para vehículos")]
            garaje,

            [Display(Description = "Trastero. Espacio pequeño destinado al almacenamiento de objetos personales")]
            trastero,

            [Display(Description = "Oficina. Espacio destinado a actividades profesionales y administrativas")]
            oficina,

            [Display(Description = "Parcela. Terreno o parcela sin construcción, apto para edificación")]
            parcela,

            [Display(Description = "Edificio. Construcción completa que puede contener múltiples unidades")]
            edificio
        }

        public enum SubtiposDeInmueble
        {
            piso,
            apartamento,
            ático,
            bungalow,
            duplex,
            chalet_independiente,
            chalet_pareado,
            chalet_adosado,
            parcela_urbana,
            parcela_urbanizable,
            parcela_no_urbanizable
        };

        public enum EstadosDeLaConstrucción
        {
            obra_nueva,
            buen_estado,
            a_reformar,
            en_ruinas
        };


        [JsonProperty(nameof(StructuredOutputEsJson.fecha_de_publicación), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FechaDePublicaciónDescription)]
        public string? FechaDePublicación { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tipo_de_operación), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TipoDeOperaciónDescription)]
        public TiposDeOperacion? TipoDeOperación { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tipo_de_inmueble), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TipoDeInmuebleDescription)]
        public TiposDeInmueble? TipoDeInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.subtipo_de_inmueble), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.SubtipoDeInmuebleDescription)]
        public SubtiposDeInmueble? SubtipoDeInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.precio_del_anuncio), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PrecioDelAnuncioDescription)]
        public decimal? PrecioDelAnuncio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.descripción_del_anuncio), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.DescripciónDelAnuncioDescription)]
        public string? DescripciónDelAnuncio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.referencia_del_anuncio), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ReferenciaDelAnuncioDescription)]
        public string? ReferenciaDelAnuncio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.nombre_de_contacto), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NombreDeContactoDescription)]
        public string? NombreDeContacto { get; private set; }

        [JsonProperty(nameof(StructuredOutputEsJson.teléfono_de_contacto), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TeléfonoDeContactoDescription)]
        public string? TeléfonoDeContacto { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.email_de_contacto), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EmailDeContactoDescription)]
        public string? EmailDeContacto { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.dirección_del_inmueble), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.DirecciónDelInmuebleDescription)]
        public string? DirecciónDelInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.referencia_catastral), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ReferenciaCatastralDescription)]
        public string? ReferenciaCatastral { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tamaño_del_inmueble), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TamañoDelInmuebleDescription)]
        public double? TamañoDelInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tamaño_de_la_parcela), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TamañoDeLaParcelaDescription)]
        public double? TamañoDeLaParcela { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.año_de_construcción), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.AñoDeConstrucciónDescription)]
        public int? AñoDeConstrucción { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.estado_de_la_construcción), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EstadoDeLaConstrucciónDescription)]
        public EstadosDeLaConstrucción? EstadoDeLaConstrucción { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.plantas_del_edificio), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PlantasDelEdificioDescription)]
        public int? PlantasDelEdificio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.planta_del_inmueble), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PlantaDelInmuebleDescription)]
        public string? PlantaDelInmueble { get; private set; }



        [JsonProperty(nameof(StructuredOutputEsJson.número_de_dormitorios), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeDormitoriosDescription)]
        public int? NúmeroDeDormitorios { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.número_de_baños), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeBañosDescription)]
        public int? NúmeroDeBaños { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.número_de_parkings), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeParkingsDescription)]
        public int? NúmeroDeParkings { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_terraza), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneTerrazaDescription)]
        public bool? TieneTerraza { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_jardín), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneJardínDescription)]
        public bool? TieneJardín { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_garaje), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneGarajeDescription)]
        public bool? TieneGaraje { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_parking_para_moto), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneParkingParaMotoDescription)]
        public bool? TieneParkingParaMoto { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_piscina), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TienePiscinaDescription)]
        public bool? TienePiscina { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_ascensor), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAscensorDescription)]
        public bool? TieneAscensor { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_acceso_para_discapacitados), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAccesoParaDiscapacitadosDescription)]
        public bool? TieneAccesoParaDiscapacitados { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_trastero), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneTrasteroDescription)]
        public bool? TieneTrastero { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.está_amueblado), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EstaAmuebladoDescription)]
        public bool? EstaAmueblado { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.no_está_amueblado), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NoEstaAmuebladoDescription)]
        public bool? NoEstaAmueblado { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_calefacción), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneCalefacciónDescription)]
        public bool? TieneCalefacción { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_aire_acondicionado), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAireAcondicionadoDescription)]
        public bool? TieneAireAcondicionado { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.permite_mascotas), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PermiteMascotasDescription)]
        public bool? PermiteMascotas { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_sistemas_de_seguridad), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneSistemasDeSeguridadDescription)]
        public bool? TieneSistemasDeSeguridad { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.imágenes_del_anuncio), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ImagenesDelAnuncio)]

        public List<ImagenDelAnuncio>? ImagenesDelAnuncio { get; private set; }
    }

    //public class EnumDescriptionAttribute : Attribute
    //{
    //    public string Description { get; }

    //    public EnumDescriptionAttribute(string description)
    //    {
    //        Description = description;
    //    }
    //}

    //public enum TiposDeInmuebleCustom
    //{
    //    [EnumDescription("Propiedad residencial completa destinada a vivienda familiar")]
    //    vivienda,

    //    [EnumDescription("Habitación individual o espacio para dormir dentro de una vivienda compartida")]
    //    dormitorio,

    //    [EnumDescription("Espacio destinado a actividades comerciales y venta al público")]
    //    local_comercial,

    //    [EnumDescription("Instalación industrial para actividades de producción o almacenamiento")]
    //    nave_industrial,

    //    [EnumDescription("Plaza de aparcamiento cubierta para vehículos")]
    //    garaje,

    //    [EnumDescription("Espacio pequeño destinado al almacenamiento de objetos personales")]
    //    trastero,

    //    [EnumDescription("Espacio destinado a actividades profesionales y administrativas")]
    //    oficina,

    //    [EnumDescription("Terreno sin construcción, apto para edificación")]
    //    parcela,

    //    [EnumDescription("Construcción completa que puede contener múltiples unidades")]
    //    edificio
    //}


    //public class OneOfEnumSchemaProvider : JSchemaGenerationProvider
    //{
    //    public override bool CanGenerateSchema(JSchemaTypeGenerationContext context)
    //        => context.ObjectType.IsEnum;

    //    public override JSchema GetSchema(JSchemaTypeGenerationContext context)
    //    {
    //        var schema = new JSchema { Type = JSchemaType.String };

    //        foreach (var name in Enum.GetNames(context.ObjectType))
    //        {
    //            var field = context.ObjectType.GetField(name, BindingFlags.Public | BindingFlags.Static);

    //            // Aquí extraemos el DisplayAttribute de manera "convencional":
    //            var displayAttr = field?
    //                .GetCustomAttributes(typeof(DisplayAttribute), inherit: false)
    //                .OfType<DisplayAttribute>()
    //                .FirstOrDefault();

    //            var desc = displayAttr?.Description;

    //            // Construimos el subschema para este valor
    //            var item = new JSchema
    //            {
    //                Type = JSchemaType.String,
    //                Enum = { new JValue(name) },
    //                Description = desc
    //            };

    //            schema.OneOf.Add(item);
    //        }

    //        return schema;
    //    }
    //}
    //public class OneOfEnumConverter : JsonConverter<Anuncio.TiposDeInmueble?>
    //{
    //    public override void WriteJson(JsonWriter writer, Anuncio.TiposDeInmueble? value, JsonSerializer serializer)
    //    {
    //        if (value.HasValue)
    //        {
    //            writer.WriteValue(value.Value.ToString().ToLowerInvariant());
    //        }
    //        else
    //        {
    //            writer.WriteNull();
    //        }
    //    }

    //    public override Anuncio.TiposDeInmueble? ReadJson(JsonReader reader, Type objectType, Anuncio.TiposDeInmueble? existingValue, bool hasExistingValue, JsonSerializer serializer)
    //    {
    //        if (reader.TokenType == JsonToken.Null)
    //            return null;

    //        var stringValue = reader.Value?.ToString();
    //        if (Enum.TryParse<Anuncio.TiposDeInmueble>(stringValue, true, out var result))
    //            return result;

    //        return null;
    //    }
    //}


}
