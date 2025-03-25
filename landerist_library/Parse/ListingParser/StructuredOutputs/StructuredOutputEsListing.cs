using Newtonsoft.Json;

namespace landerist_library.Parse.ListingParser.StructuredOutputs
{
    public class StructuredOutputEsListing
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


        [JsonProperty(nameof(StructuredOutputEsParse.fecha_de_publicacion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.FechaDePublicaciónDescription)]
        public string? FechaDePublicación { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tipo_de_operacion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TipoDeOperaciónDescription)]
        public TiposDeOperacion? TipoDeOperación { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tipo_de_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TipoDeInmuebleDescription)]
        public TiposDeInmueble? TipoDeInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.subtipo_de_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.SubtipoDeInmuebleDescription)]
        public SubtiposDeInmueble? SubtipoDeInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.precio_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.PrecioDelAnuncioDescription)]
        public decimal? PrecioDelAnuncio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.descripcion_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.DescripciónDelAnuncioDescription)]
        public string? DescripciónDelAnuncio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.referencia_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.ReferenciaDelAnuncioDescription)]
        public string? ReferenciaDelAnuncio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.telefono_de_contacto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TeléfonoDeContactoDescription)]
        public string? TeléfonoDeContacto { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.email_de_contacto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.EmailDeContactoDescription)]
        public string? EmailDeContacto { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.direccion_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.DirecciónDelInmuebleDescription)]
        public string? DirecciónDelInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.referencia_catastral), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.ReferenciaCatastralDescription)]
        public string? ReferenciaCatastral { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tamanio_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TamañoDelInmuebleDescription)]
        public double? TamañoDelInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tamanio_de_la_parcela), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TamañoDeLaParcelaDescription)]
        public double? TamañoDeLaParcela { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.anio_de_construccion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.AñoDeConstrucciónDescription)]
        public int? AñoDeConstrucción { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.estado_de_la_construccion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.EstadoDeLaConstrucciónDescription)]
        public EstadosDeLaConstrucción? EstadoDeLaConstrucción { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.plantas_del_edificio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.PlantasDelEdificioDescription)]
        public int? PlantasDelEdificio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.planta_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.PlantaDelInmuebleDescription)]
        public string? PlantaDelInmueble { get; private set; }



        [JsonProperty(nameof(StructuredOutputEsParse.numero_de_dormitorios), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.NúmeroDeDormitoriosDescription)]
        public int? NúmeroDeDormitorios { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.numero_de_banios), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.NúmeroDeBañosDescription)]
        public int? NúmeroDeBaños { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.numero_de_parkings), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.NúmeroDeParkingsDescription)]
        public int? NúmeroDeParkings { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_terraza), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneTerrazaDescription)]
        public bool? TieneTerraza { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_jardin), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneJardínDescription)]
        public bool? TieneJardín { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_garaje), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneGarajeDescription)]
        public bool? TieneGaraje { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_parking_para_moto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneParkingParaMotoDescription)]
        public bool? TieneParkingParaMoto { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_piscina), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TienePiscinaDescription)]
        public bool? TienePiscina { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_ascensor), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneAscensorDescription)]
        public bool? TieneAscensor { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_acceso_para_discapacitados), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneAccesoParaDiscapacitadosDescription)]
        public bool? TieneAccesoParaDiscapacitados { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_trastero), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneTrasteroDescription)]
        public bool? TieneTrastero { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.esta_amueblado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.EstaAmuebladoDescription)]
        public bool? EstaAmueblado { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.no_esta_amueblado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.NoEstaAmuebladoDescription)]
        public bool? NoEstaAmueblado { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_calefaccion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneCalefacciónDescription)]
        public bool? TieneCalefacción { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_aire_acondicionado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneAireAcondicionadoDescription)]
        public bool? TieneAireAcondicionado { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.permite_mascotas), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.PermiteMascotasDescription)]
        public bool? PermiteMascotas { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsParse.tiene_sistemas_de_seguridad), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.TieneSistemasDeSeguridadDescription)]
        public bool? TieneSistemasDeSeguridad { get; private set; }

        [JsonProperty(nameof(StructuredOutputEsParse.urls_de_imagenes_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(StructuredOutputEsParse.UrlsDeImagenesDelAnuncio)]
        public string[]? ImagenesDelAnuncio { get; private set; }

    }
}
