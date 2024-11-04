using System.Text.Json.Serialization;


namespace landerist_library.Parse.Listing.OpenAI
{
    public class OpenAIStructuredOutput
    {
        [JsonInclude]
        [JsonPropertyName("es_un_anuncio")]        
        [Description("La página web corresponde a un único anuncio inmobiliario")]
        public bool EsUnAnuncio { get; private set; }


        [JsonInclude]
        [JsonPropertyName("anuncio")]
        [Description("datos del anuncio, null en caso de no ser un anuncio")]
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
            vivienda,
            dormitorio,
            local_comercial,
            nave_industrial,
            garaje,
            trastero,
            oficina,
            parcela,
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



        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.fecha_de_publicacion))]
        [Description(ParseListingTool.FechaDePublicaciónDescription)]        
        public string? FechaDePublicación { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tipo_de_operacion))]
        [Description(ParseListingTool.TipoDeOperaciónDescription)]
        public TiposDeOperacion? TipoDeOperación { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tipo_de_inmueble))]
        [Description(ParseListingTool.TipoDeInmuebleDescription)]
        public TiposDeInmueble? TipoDeInmueble { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.subtipo_de_inmueble))]
        [Description(ParseListingTool.SubtipoDeInmuebleDescription)]
        public SubtiposDeInmueble? SubtipoDeInmueble { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.precio_del_anuncio))]
        [Description(ParseListingTool.PrecioDelAnuncioDescription)]
        public decimal? PrecioDelAnuncio{ get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.descripcion_del_anuncio))]
        [Description(ParseListingTool.DescripciónDelAnuncioDescription)]
        public string? DescripciónDelAnuncio { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.referencia_del_anuncio))]
        [Description(ParseListingTool.ReferenciaDelAnuncioDescription)]
        public string? ReferenciaDelAnuncio { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.telefono_de_contacto))]
        [Description(ParseListingTool.TeléfonoDeContactoDescription)]
        public string? TeléfonoDeContacto { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.email_de_contacto))]
        [Description(ParseListingTool.EmailDeContactoDescription)]
        public string? EmailDeContacto { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.direccion_del_inmueble))]
        [Description(ParseListingTool.DirecciónDelInmuebleDescription)]
        public string? DirecciónDelInmueble { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.referencia_catastral))]
        [Description(ParseListingTool.ReferenciaCatastralDescription)]
        public string? ReferenciaCatastral { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tamanio_del_inmueble))]
        [Description(ParseListingTool.TamañoDelInmuebleDescription)]
        public double? TamañoDelInmueble { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tamanio_de_la_parcela))]
        [Description(ParseListingTool.TamañoDeLaParcelaDescription)]
        public double? TamañoDeLaParcela { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.anio_de_construccion))]
        [Description(ParseListingTool.AñoDeConstrucciónDescription)]
        public int? AñoDeConstrucción { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.estado_de_la_construccion))]
        [Description(ParseListingTool.EstadoDeLaConstrucciónDescription)]
        public EstadosDeLaConstrucción? EstadoDeLaConstrucción { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.plantas_del_edificio))]
        [Description(ParseListingTool.PlantasDelEdificioDescription)]
        public int? PlantasDelEdificio { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.planta_del_inmueble))]
        [Description(ParseListingTool.PlantaDelInmuebleDescription)]
        public string? PlantaDelInmueble { get; private set; }


        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.numero_de_dormitorios))]
        [Description(ParseListingTool.NúmeroDeDormitoriosDescription)]
        public int? NúmeroDeDormitorios { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.numero_de_banios))]
        [Description(ParseListingTool.NúmeroDeBañosDescription)]
        public int? NúmeroDeBaños { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.numero_de_parkings))]
        [Description(ParseListingTool.NúmeroDeParkingsDescription)]
        public int? NúmeroDeParkings { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_terraza))]
        [Description(ParseListingTool.TieneTerrazaDescription)]
        public bool? TieneTerraza { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_jardin))]
        [Description(ParseListingTool.TieneJardínDescription)]
        public bool? TieneJardín { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_garaje))]
        [Description(ParseListingTool.TieneGarajeDescription)]
        public bool? TieneGaraje { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_parking_para_moto))]
        [Description(ParseListingTool.TieneParkingParaMotoDescription)]
        public bool? TieneParkingParaMoto { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_piscina))]
        [Description(ParseListingTool.TienePiscinaDescription)]
        public bool? TienePiscina { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_ascensor))]
        [Description(ParseListingTool.TieneAscensorDescription)]
        public bool? TieneAscensor { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_acceso_para_discapacitados))]
        [Description(ParseListingTool.TieneAccesoParaDiscapacitadosDescription)]
        public bool? TieneAccesoParaDiscapacitados { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_trastero))]
        [Description(ParseListingTool.TieneTrasteroDescription)]
        public bool? TieneTrastero { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.esta_amueblado))]
        [Description(ParseListingTool.EstaAmuebladoDescription)]
        public bool? EstaAmueblado { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.no_esta_amueblado))]
        [Description(ParseListingTool.NoEstaAmuebladoDescription)]
        public bool? NoEstaAmueblado { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_calefaccion))]
        [Description(ParseListingTool.TieneCalefacciónDescription)]
        public bool? TieneCalefacción { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_aire_acondicionado))]
        [Description(ParseListingTool.TieneAireAcondicionadoDescription)]
        public bool? TieneAireAcondicionado { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.permite_mascotas))]
        [Description(ParseListingTool.PermiteMascotasDescription)]
        public bool? PermiteMascotas { get; private set; }

        [JsonInclude]
        [JsonPropertyName(nameof(ParseListingTool.tiene_sistemas_de_seguridad))]
        [Description(ParseListingTool.TieneSistemasDeSeguridadDescription)]
        public bool? TieneSistemasDeSeguridad { get; private set; }

    }
}
