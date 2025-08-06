using Newtonsoft.Json;

namespace landerist_library.Parse.ListingParser.StructuredOutputs
{
    public class StructuredOutputEs
    {
        [JsonProperty(StructuredOutputEsJson.FunctionNameIsListing, Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FunctionNameIsListingDescription)]
        public bool EsUnAnuncio { get; private set; }


        [JsonProperty(StructuredOutputEsJson.FunctionNameListing, Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FunctionNameListingDescription)]
        public Anuncio? Anuncio { get; private set; }

    }

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


        [JsonProperty(nameof(StructuredOutputEsJson.fecha_de_publicación), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.FechaDePublicaciónDescription)]
        public string? FechaDePublicación { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tipo_de_operación), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TipoDeOperaciónDescription)]
        public TiposDeOperacion? TipoDeOperación { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tipo_de_inmueble), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TipoDeInmuebleDescription)]
        public TiposDeInmueble? TipoDeInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.subtipo_de_inmueble), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.SubtipoDeInmuebleDescription)]
        public SubtiposDeInmueble? SubtipoDeInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.precio_del_anuncio), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PrecioDelAnuncioDescription)]
        public decimal? PrecioDelAnuncio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.descripción_del_anuncio), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.DescripciónDelAnuncioDescription)]
        public string? DescripciónDelAnuncio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.referencia_del_anuncio), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ReferenciaDelAnuncioDescription)]
        public string? ReferenciaDelAnuncio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.nombre_de_contacto), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NombreDeContactoDescription)]
        public string? NombreDeContacto { get; private set; }

        [JsonProperty(nameof(StructuredOutputEsJson.teléfono_de_contacto), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TeléfonoDeContactoDescription)]
        public string? TeléfonoDeContacto { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.email_de_contacto), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EmailDeContactoDescription)]
        public string? EmailDeContacto { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.dirección_del_inmueble), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.DirecciónDelInmuebleDescription)]
        public string? DirecciónDelInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.referencia_catastral), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ReferenciaCatastralDescription)]
        public string? ReferenciaCatastral { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tamaño_del_inmueble), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TamañoDelInmuebleDescription)]
        public double? TamañoDelInmueble { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tamaño_de_la_parcela), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TamañoDeLaParcelaDescription)]
        public double? TamañoDeLaParcela { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.año_de_construcción), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.AñoDeConstrucciónDescription)]
        public int? AñoDeConstrucción { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.estado_de_la_construcción), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EstadoDeLaConstrucciónDescription)]
        public EstadosDeLaConstrucción? EstadoDeLaConstrucción { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.plantas_del_edificio), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PlantasDelEdificioDescription)]
        public int? PlantasDelEdificio { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.planta_del_inmueble), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PlantaDelInmuebleDescription)]
        public string? PlantaDelInmueble { get; private set; }



        [JsonProperty(nameof(StructuredOutputEsJson.número_de_dormitorios), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeDormitoriosDescription)]
        public int? NúmeroDeDormitorios { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.número_de_baños), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeBañosDescription)]
        public int? NúmeroDeBaños { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.número_de_parkings), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NúmeroDeParkingsDescription)]
        public int? NúmeroDeParkings { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_terraza), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneTerrazaDescription)]
        public bool? TieneTerraza { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_jardín), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneJardínDescription)]
        public bool? TieneJardín { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_garaje), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneGarajeDescription)]
        public bool? TieneGaraje { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_parking_para_moto), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneParkingParaMotoDescription)]
        public bool? TieneParkingParaMoto { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_piscina), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TienePiscinaDescription)]
        public bool? TienePiscina { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_ascensor), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAscensorDescription)]
        public bool? TieneAscensor { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_acceso_para_discapacitados), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAccesoParaDiscapacitadosDescription)]
        public bool? TieneAccesoParaDiscapacitados { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_trastero), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneTrasteroDescription)]
        public bool? TieneTrastero { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.está_amueblado), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.EstaAmuebladoDescription)]
        public bool? EstaAmueblado { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.no_está_amueblado), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.NoEstaAmuebladoDescription)]
        public bool? NoEstaAmueblado { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_calefacción), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneCalefacciónDescription)]
        public bool? TieneCalefacción { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_aire_acondicionado), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneAireAcondicionadoDescription)]
        public bool? TieneAireAcondicionado { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.permite_mascotas), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.PermiteMascotasDescription)]
        public bool? PermiteMascotas { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.tiene_sistemas_de_seguridad), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TieneSistemasDeSeguridadDescription)]
        public bool? TieneSistemasDeSeguridad { get; private set; }


        //[JsonIgnore]
        [JsonProperty(nameof(StructuredOutputEsJson.imágenes_del_anuncio), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.ImagenesDelAnuncio)]

        public List<ImagenDelAnuncio>? ImagenesDelAnuncio { get; private set; }
    }





    public class ImagenDelAnuncio
    {
        [JsonProperty(nameof(StructuredOutputEsJson.url_de_la_imagen), Required = Required.Always)]
        [System.ComponentModel.Description(StructuredOutputEsJson.UrlDeLaImagen)]
        public string Url { get; private set; }


        [JsonProperty(nameof(StructuredOutputEsJson.título_de_la_imagen), Required = Required.Default)]
        [System.ComponentModel.Description(StructuredOutputEsJson.TituloDeLaImagen)]
        public string? Titulo { get; private set; }

        public ImagenDelAnuncio(string url, string? titulo)
        {
            Url = url;
            Titulo = titulo;
        }
    }

}
