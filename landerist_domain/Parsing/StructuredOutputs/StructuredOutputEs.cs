using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;


namespace landerist_domain.Parsing.StructuredOutputs
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

    public enum CalificacionesDeEficienciaEnergetica
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G
    };

    public enum EstadosDePublicación
    {
        publicado,
        despublicado
    };

    [DataContract]
    public class StructuredOutputEs
    {
        [DataMember(Name = StructuredOutputEsJson.FunctionNameListing, IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FunctionNameListingDescription)]
        public Anuncio? Anuncio { get; set; }
    } 


    [DataContract]
    public class Anuncio
    {

        [DataMember(Name = nameof(StructuredOutputEsJson.fecha_de_publicación), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FechaDePublicaciónDescription)]
        public string? FechaDePublicación { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.estado_de_publicación), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EstadoDePublicaciónDescription)]
        public EstadosDePublicación? EstadoDePublicación { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tipo_de_operación), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TipoDeOperaciónDescription)]
        public TiposDeOperacion? TipoDeOperación { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tipo_de_inmueble), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TipoDeInmuebleDescription)]
        public TiposDeInmueble? TipoDeInmueble { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.subtipo_de_inmueble), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.SubtipoDeInmuebleDescription)]
        public SubtiposDeInmueble? SubtipoDeInmueble { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.precio_del_anuncio), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PrecioDelAnuncioDescription)]
        public decimal? PrecioDelAnuncio { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.descripción_del_anuncio), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.DescripciónDelAnuncioDescription)]
        public string? DescripciónDelAnuncio { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.referencia_del_anuncio), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ReferenciaDelAnuncioDescription)]
        public string? ReferenciaDelAnuncio { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.nombre_de_contacto), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NombreDeContactoDescription)]
        public string? NombreDeContacto { get; set; }

        [DataMember(Name = nameof(StructuredOutputEsJson.teléfono_de_contacto), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TeléfonoDeContactoDescription)]
        public string? TeléfonoDeContacto { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.email_de_contacto), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EmailDeContactoDescription)]
        public string? EmailDeContacto { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.dirección_del_inmueble), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.DirecciónDelInmuebleDescription)]
        public string? DirecciónDelInmueble { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.referencia_catastral), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ReferenciaCatastralDescription)]
        public string? ReferenciaCatastral { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tamaño_del_inmueble), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TamañoDelInmuebleDescription)]
        public double? TamañoDelInmueble { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tamaño_de_la_parcela), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TamañoDeLaParcelaDescription)]
        public double? TamañoDeLaParcela { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.año_de_construcción), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.AñoDeConstrucciónDescription)]
        public int? AñoDeConstrucción { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.estado_de_la_construcción), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EstadoDeLaConstrucciónDescription)]
        public EstadosDeLaConstrucción? EstadoDeLaConstrucción { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.calificación_energética), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.CalificaciónEnergéticaDescription)]
        public CalificacionesDeEficienciaEnergetica? CalificaciónEnergética { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.plantas_del_edificio), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PlantasDelEdificioDescription)]
        public int? PlantasDelEdificio { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.planta_del_inmueble), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PlantaDelInmuebleDescription)]
        public string? PlantaDelInmueble { get; set; }



        [DataMember(Name = nameof(StructuredOutputEsJson.número_de_dormitorios), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeDormitoriosDescription)]
        public int? NúmeroDeDormitorios { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.número_de_baños), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeBañosDescription)]
        public int? NúmeroDeBaños { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.número_de_parkings), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeParkingsDescription)]
        public int? NúmeroDeParkings { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_terraza), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneTerrazaDescription)]
        public bool? TieneTerraza { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_jardín), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneJardínDescription)]
        public bool? TieneJardín { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_garaje), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneGarajeDescription)]
        public bool? TieneGaraje { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_parking_para_moto), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneParkingParaMotoDescription)]
        public bool? TieneParkingParaMoto { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_piscina), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TienePiscinaDescription)]
        public bool? TienePiscina { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_ascensor), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAscensorDescription)]
        public bool? TieneAscensor { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_acceso_para_discapacitados), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAccesoParaDiscapacitadosDescription)]
        public bool? TieneAccesoParaDiscapacitados { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_trastero), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneTrasteroDescription)]
        public bool? TieneTrastero { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.está_amueblado), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EstaAmuebladoDescription)]
        public bool? EstaAmueblado { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.no_está_amueblado), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NoEstaAmuebladoDescription)]
        public bool? NoEstaAmueblado { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_calefacción), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneCalefacciónDescription)]
        public bool? TieneCalefacción { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_aire_acondicionado), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAireAcondicionadoDescription)]
        public bool? TieneAireAcondicionado { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.permite_mascotas), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PermiteMascotasDescription)]
        public bool? PermiteMascotas { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.tiene_sistemas_de_seguridad), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneSistemasDeSeguridadDescription)]
        public bool? TieneSistemasDeSeguridad { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.imágenes_del_anuncio), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ImagenesDelAnuncio)]

        public List<ImagenDelAnuncio>? ImagenesDelAnuncio { get; set; }
    }


    [DataContract]
    public class ImagenDelAnuncio
    {
        [DataMember(Name = nameof(StructuredOutputEsJson.url_de_la_imagen), IsRequired = true, EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.UrlDeLaImagen)]
        public string? Url { get; set; }


        [DataMember(Name = nameof(StructuredOutputEsJson.título_de_la_imagen), EmitDefaultValue = true)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TituloDeLaImagen)]
        public string? Titulo { get; set; }

        public ImagenDelAnuncio(string url, string? titulo)
        {
            Url = url;
            Titulo = titulo;
        }
    }
}
