using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;


namespace landerist_library.Parse.ListingParser.StructuredOutputs
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

    public class StructuredOutputEs
    {
        [JsonProperty(StructuredOutputEsJson.FunctionNameIsListing, Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FunctionNameIsListingDescription)]
        public bool EsUnAnuncio { get; set; }


        [JsonProperty(StructuredOutputEsJson.FunctionNameListing, Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FunctionNameListingDescription)]
        public Anuncio? Anuncio { get; set; }
    } 


    public class Anuncio
    {

        [JsonProperty(nameof(StructuredOutputEsJson.fecha_de_publicación), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FechaDePublicaciónDescription)]
        public string? FechaDePublicación { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tipo_de_operación), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TipoDeOperaciónDescription)]
        public TiposDeOperacion? TipoDeOperación { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tipo_de_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TipoDeInmuebleDescription)]
        public TiposDeInmueble? TipoDeInmueble { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.subtipo_de_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.SubtipoDeInmuebleDescription)]
        public SubtiposDeInmueble? SubtipoDeInmueble { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.precio_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PrecioDelAnuncioDescription)]
        public decimal? PrecioDelAnuncio { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.descripción_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.DescripciónDelAnuncioDescription)]
        public string? DescripciónDelAnuncio { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.referencia_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ReferenciaDelAnuncioDescription)]
        public string? ReferenciaDelAnuncio { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.nombre_de_contacto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NombreDeContactoDescription)]
        public string? NombreDeContacto { get; set; }

        [JsonProperty(nameof(StructuredOutputEsJson.teléfono_de_contacto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TeléfonoDeContactoDescription)]
        public string? TeléfonoDeContacto { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.email_de_contacto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EmailDeContactoDescription)]
        public string? EmailDeContacto { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.dirección_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.DirecciónDelInmuebleDescription)]
        public string? DirecciónDelInmueble { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.referencia_catastral), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ReferenciaCatastralDescription)]
        public string? ReferenciaCatastral { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tamaño_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TamañoDelInmuebleDescription)]
        public double? TamañoDelInmueble { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tamaño_de_la_parcela), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TamañoDeLaParcelaDescription)]
        public double? TamañoDeLaParcela { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.año_de_construcción), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.AñoDeConstrucciónDescription)]
        public int? AñoDeConstrucción { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.estado_de_la_construcción), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EstadoDeLaConstrucciónDescription)]
        public EstadosDeLaConstrucción? EstadoDeLaConstrucción { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.plantas_del_edificio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PlantasDelEdificioDescription)]
        public int? PlantasDelEdificio { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.planta_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PlantaDelInmuebleDescription)]
        public string? PlantaDelInmueble { get; set; }



        [JsonProperty(nameof(StructuredOutputEsJson.número_de_dormitorios), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeDormitoriosDescription)]
        public int? NúmeroDeDormitorios { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.número_de_baños), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeBañosDescription)]
        public int? NúmeroDeBaños { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.número_de_parkings), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeParkingsDescription)]
        public int? NúmeroDeParkings { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_terraza), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneTerrazaDescription)]
        public bool? TieneTerraza { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_jardín), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneJardínDescription)]
        public bool? TieneJardín { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_garaje), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneGarajeDescription)]
        public bool? TieneGaraje { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_parking_para_moto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneParkingParaMotoDescription)]
        public bool? TieneParkingParaMoto { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_piscina), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TienePiscinaDescription)]
        public bool? TienePiscina { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_ascensor), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAscensorDescription)]
        public bool? TieneAscensor { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_acceso_para_discapacitados), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAccesoParaDiscapacitadosDescription)]
        public bool? TieneAccesoParaDiscapacitados { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_trastero), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneTrasteroDescription)]
        public bool? TieneTrastero { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.está_amueblado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EstaAmuebladoDescription)]
        public bool? EstaAmueblado { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.no_está_amueblado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NoEstaAmuebladoDescription)]
        public bool? NoEstaAmueblado { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_calefacción), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneCalefacciónDescription)]
        public bool? TieneCalefacción { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_aire_acondicionado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAireAcondicionadoDescription)]
        public bool? TieneAireAcondicionado { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.permite_mascotas), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PermiteMascotasDescription)]
        public bool? PermiteMascotas { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_sistemas_de_seguridad), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneSistemasDeSeguridadDescription)]
        public bool? TieneSistemasDeSeguridad { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.imágenes_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ImagenesDelAnuncio)]

        public List<ImagenDelAnuncio>? ImagenesDelAnuncio { get; set; }
    }


    public class ImagenDelAnuncio
    {
        [JsonProperty(nameof(StructuredOutputEsJson.url_de_la_imagen), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.UrlDeLaImagen)]
        public string Url { get; set; }


        [JsonProperty(nameof(StructuredOutputEsJson.título_de_la_imagen), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TituloDeLaImagen)]
        public string? Titulo { get; set; }

        public ImagenDelAnuncio(string url, string? titulo)
        {
            Url = url;
            Titulo = titulo;
        }
    }
}
