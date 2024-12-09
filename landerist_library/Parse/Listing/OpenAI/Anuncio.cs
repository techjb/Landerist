using Newtonsoft.Json;

namespace landerist_library.Parse.Listing.OpenAI
{
    public class Anuncio
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


        [JsonProperty(nameof(ParseListingTool.fecha_de_publicacion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.FechaDePublicaciónDescription)]
        public string? FechaDePublicación { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tipo_de_operacion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TipoDeOperaciónDescription)]
        public TiposDeOperacion? TipoDeOperación { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tipo_de_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TipoDeInmuebleDescription)]
        public TiposDeInmueble? TipoDeInmueble { get; private set; }


        [JsonProperty(nameof(ParseListingTool.subtipo_de_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.SubtipoDeInmuebleDescription)]
        public SubtiposDeInmueble? SubtipoDeInmueble { get; private set; }


        [JsonProperty(nameof(ParseListingTool.precio_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.PrecioDelAnuncioDescription)]
        public decimal? PrecioDelAnuncio { get; private set; }


        [JsonProperty(nameof(ParseListingTool.descripcion_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.DescripciónDelAnuncioDescription)]
        public string? DescripciónDelAnuncio { get; private set; }


        [JsonProperty(nameof(ParseListingTool.referencia_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.ReferenciaDelAnuncioDescription)]
        public string? ReferenciaDelAnuncio { get; private set; }


        [JsonProperty(nameof(ParseListingTool.telefono_de_contacto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TeléfonoDeContactoDescription)]
        public string? TeléfonoDeContacto { get; private set; }


        [JsonProperty(nameof(ParseListingTool.email_de_contacto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.EmailDeContactoDescription)]
        public string? EmailDeContacto { get; private set; }


        [JsonProperty(nameof(ParseListingTool.direccion_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.DirecciónDelInmuebleDescription)]
        public string? DirecciónDelInmueble { get; private set; }


        [JsonProperty(nameof(ParseListingTool.referencia_catastral), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.ReferenciaCatastralDescription)]
        public string? ReferenciaCatastral { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tamanio_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TamañoDelInmuebleDescription)]
        public double? TamañoDelInmueble { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tamanio_de_la_parcela), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TamañoDeLaParcelaDescription)]
        public double? TamañoDeLaParcela { get; private set; }


        [JsonProperty(nameof(ParseListingTool.anio_de_construccion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.AñoDeConstrucciónDescription)]
        public int? AñoDeConstrucción { get; private set; }


        [JsonProperty(nameof(ParseListingTool.estado_de_la_construccion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.EstadoDeLaConstrucciónDescription)]
        public EstadosDeLaConstrucción? EstadoDeLaConstrucción { get; private set; }


        [JsonProperty(nameof(ParseListingTool.plantas_del_edificio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.PlantasDelEdificioDescription)]
        public int? PlantasDelEdificio { get; private set; }


        [JsonProperty(nameof(ParseListingTool.planta_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.PlantaDelInmuebleDescription)]
        public string? PlantaDelInmueble { get; private set; }



        [JsonProperty(nameof(ParseListingTool.numero_de_dormitorios), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.NúmeroDeDormitoriosDescription)]
        public int? NúmeroDeDormitorios { get; private set; }


        [JsonProperty(nameof(ParseListingTool.numero_de_banios), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.NúmeroDeBañosDescription)]
        public int? NúmeroDeBaños { get; private set; }


        [JsonProperty(nameof(ParseListingTool.numero_de_parkings), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.NúmeroDeParkingsDescription)]
        public int? NúmeroDeParkings { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_terraza), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneTerrazaDescription)]
        public bool? TieneTerraza { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_jardin), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneJardínDescription)]
        public bool? TieneJardín { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_garaje), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneGarajeDescription)]
        public bool? TieneGaraje { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_parking_para_moto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneParkingParaMotoDescription)]
        public bool? TieneParkingParaMoto { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_piscina), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TienePiscinaDescription)]
        public bool? TienePiscina { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_ascensor), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneAscensorDescription)]
        public bool? TieneAscensor { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_acceso_para_discapacitados), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneAccesoParaDiscapacitadosDescription)]
        public bool? TieneAccesoParaDiscapacitados { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_trastero), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneTrasteroDescription)]
        public bool? TieneTrastero { get; private set; }


        [JsonProperty(nameof(ParseListingTool.esta_amueblado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.EstaAmuebladoDescription)]
        public bool? EstaAmueblado { get; private set; }


        [JsonProperty(nameof(ParseListingTool.no_esta_amueblado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.NoEstaAmuebladoDescription)]
        public bool? NoEstaAmueblado { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_calefaccion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneCalefacciónDescription)]
        public bool? TieneCalefacción { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_aire_acondicionado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneAireAcondicionadoDescription)]
        public bool? TieneAireAcondicionado { get; private set; }


        [JsonProperty(nameof(ParseListingTool.permite_mascotas), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.PermiteMascotasDescription)]
        public bool? PermiteMascotas { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_sistemas_de_seguridad), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneSistemasDeSeguridadDescription)]
        public bool? TieneSistemasDeSeguridad { get; private set; }

        [JsonProperty(nameof(ParseListingTool.urls_de_imagenes_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.UrlsDeImagenesDelAnuncio)]
        public string[]? ImagenesDelAnuncio { get; private set; }

    }
}
