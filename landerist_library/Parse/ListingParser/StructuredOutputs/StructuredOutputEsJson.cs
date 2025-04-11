using System.Reflection;
using System.Text.Json.Nodes;

namespace landerist_library.Parse.ListingParser.StructuredOutputs
{
    public class StructuredOutputEsJson
    {
        public const string FunctionNameIsListing = "es_un_anuncio";

        public const string FunctionNameIsListingDescription = "La página web corresponde a un único anuncio inmobiliario";

        public const string FunctionNameIsNotListing = "no_es_un_anuncio";

        public const string FunctionDescriptionIsNotListing = "La página web no corresponde a un único anuncio inmobiliario";

        public const string FunctionNameListing = "anuncio";

        public const string FunctionNameListingDescription = "En caso de ser un anuncio, contiene la información del anuncio.";

#pragma warning disable IDE1006


        public const string FechaDePublicaciónDescription = "fecha de la publicación del anuncio en ISO-8601 (YYYY-MM-DD)";

        [Description(FechaDePublicaciónDescription)]
        public string? fecha_de_publicacion { get; set; } = null;


        public const string TipoDeOperaciónDescription = "tipo de operación inmobiliaria";

        [Description(TipoDeOperaciónDescription)]
        public string? tipo_de_operacion { get; set; } = null;


        public const string TipoDeInmuebleDescription = "tipología del inmueble";

        [Description(TipoDeInmuebleDescription)]
        public string? tipo_de_inmueble { get; set; } = null;


        public const string SubtipoDeInmuebleDescription = "subtipo de inmueble";

        [Description(SubtipoDeInmuebleDescription)]
        public string? subtipo_de_inmueble { get; set; } = null;


        public const string PrecioDelAnuncioDescription = "precio del anuncio en euros";

        [Description(PrecioDelAnuncioDescription)]
        public decimal? precio_del_anuncio { get; set; } = null;


        public const string DescripciónDelAnuncioDescription = "texto plano con la descripción detallada del anuncio";

        [Description(DescripciónDelAnuncioDescription)]
        public string? descripcion_del_anuncio { get; set; } = null;


        public const string ReferenciaDelAnuncioDescription = "código de referencia del anuncio";

        [Description(ReferenciaDelAnuncioDescription)]
        public string? referencia_del_anuncio { get; set; } = null;


        public const string TeléfonoDeContactoDescription = "número de teléfono de contacto";

        [Description(TeléfonoDeContactoDescription)]
        public string? telefono_de_contacto { get; set; } = null;


        public const string EmailDeContactoDescription = "dirección de email de contacto";

        [Description(EmailDeContactoDescription)]
        public string? email_de_contacto { get; set; } = null;


        public const string DirecciónDelInmuebleDescription = "dirección en la que se encuentra el inmueble";

        [Description(DirecciónDelInmuebleDescription)]
        public string? direccion_del_inmueble { get; set; } = null;

        public const string ReferenciaCatastralDescription = "referencia catastral del anuncio (14 o 20 caracteres)";

        [Description(ReferenciaCatastralDescription)]
        public string? referencia_catastral { get; set; } = null;

        public const string TamañoDelInmuebleDescription = "número de metros cuadrados del inmueble";

        [Description(TamañoDelInmuebleDescription)]
        public double? tamanio_del_inmueble { get; set; } = null;


        public const string TamañoDeLaParcelaDescription = "número de metros cuadrados de la parcela";

        [Description(TamañoDeLaParcelaDescription)]
        public double? tamanio_de_la_parcela { get; set; } = null;


        public const string AñoDeConstrucciónDescription = "año de construcción del inmueble";

        [Description(AñoDeConstrucciónDescription)]
        public double? anio_de_construccion { get; set; } = null;


        public const string EstadoDeLaConstrucciónDescription = "estado de la construcción en el que se encuentra el inmueble";

        [Description(EstadoDeLaConstrucciónDescription)]
        public string? estado_de_la_construccion { get; set; } = null;

        public const string PlantasDelEdificioDescription = "número de plantas del edificio";

        [Description(PlantasDelEdificioDescription)]
        public double? plantas_del_edificio { get; set; } = null;

        public const string PlantaDelInmuebleDescription = "número de planta en la que se ubica el inmueble";

        [Description(PlantaDelInmuebleDescription)]
        public string? planta_del_inmueble { get; set; } = null;

        public const string NúmeroDeDormitoriosDescription = "número de dormitorios";

        [Description(NúmeroDeDormitoriosDescription)]
        public double? numero_de_dormitorios { get; set; } = null;

        public const string NúmeroDeBañosDescription = "número de baños";

        [Description(NúmeroDeBañosDescription)]
        public double? numero_de_banios { get; set; } = null;

        public const string NúmeroDeParkingsDescription = "número de parkings";

        [Description(NúmeroDeParkingsDescription)]
        public double? numero_de_parkings { get; set; } = null;

        public const string TieneTerrazaDescription = "tiene terraza";

        [Description(TieneTerrazaDescription)]
        public bool? tiene_terraza { get; set; } = null;

        public const string TieneJardínDescription = "tiene jardín";

        [Description(TieneJardínDescription)]
        public bool? tiene_jardin { get; set; } = null;

        public const string TieneGarajeDescription = "tiene garaje";

        [Description(TieneGarajeDescription)]
        public bool? tiene_garaje { get; set; } = null;


        public const string TieneParkingParaMotoDescription = "tiene parking para moto";

        [Description(TieneParkingParaMotoDescription)]
        public bool? tiene_parking_para_moto { get; set; } = null;

        public const string TienePiscinaDescription = "tiene piscina";

        [Description(TienePiscinaDescription)]
        public bool? tiene_piscina { get; set; } = null;

        public const string TieneAscensorDescription = "tiene ascensor";

        [Description(TieneAscensorDescription)]
        public bool? tiene_ascensor { get; set; } = null;

        public const string TieneAccesoParaDiscapacitadosDescription = "tiene acceso para discapacitados";

        [Description(TieneAccesoParaDiscapacitadosDescription)]
        public bool? tiene_acceso_para_discapacitados { get; set; } = null;

        public const string TieneTrasteroDescription = "tiene trastero";

        [Description(TieneTrasteroDescription)]
        public bool? tiene_trastero { get; set; } = null;

        public const string EstaAmuebladoDescription = "está amueblado";

        [Description(EstaAmuebladoDescription)]
        public bool? esta_amueblado { get; set; } = null;

        public const string NoEstaAmuebladoDescription = "no está amueblado";

        [Description(NoEstaAmuebladoDescription)]
        public bool? no_esta_amueblado { get; set; } = null;

        public const string TieneCalefacciónDescription = "tiene calefacción";

        [Description(TieneCalefacciónDescription)]
        public bool? tiene_calefaccion { get; set; } = null;

        public const string TieneAireAcondicionadoDescription = "tiene aire acondicionado";

        [Description(TieneAireAcondicionadoDescription)]
        public bool? tiene_aire_acondicionado { get; set; } = null;

        public const string PermiteMascotasDescription = "se permiten mascotas";

        [Description(PermiteMascotasDescription)]
        public bool? permite_mascotas { get; set; } = null;

        public const string TieneSistemasDeSeguridadDescription = "tiene sistemas de seguridad";

        [Description(TieneSistemasDeSeguridadDescription)]
        public bool? tiene_sistemas_de_seguridad { get; set; } = null;


        public const string UrlsDeImagenesDelAnuncio = "Diez urls de imágenes del anuncio";
        [Description(UrlsDeImagenesDelAnuncio)]
        public string[]? urls_de_imagenes_del_anuncio { get; set; } = null;

        public const long MAX_URLS_DE_IMAGENES_DEL_ANUNCIO = 10;


#pragma warning restore IDE1006

        protected const string OPERACION_VENTA = "venta";

        protected const string OPERACION_ALQUILER = "alquiler";

        protected const string TIPO_DE_INMUEBLE_VIVIENDA = "vivienda";

        protected const string TIPO_DE_INMUEBLE_DORMITORIO = "dormitorio";

        protected const string TIPO_DE_INMUEBLE_LOCAL_COMERCIAL = "local_comercial";

        protected const string TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL = "nave_industrial";

        protected const string TIPO_DE_INMUEBLE_GARAJE = "garaje";

        protected const string TIPO_DE_INMUEBLE_TRASTERO = "trastero";

        protected const string TIPO_DE_INMUEBLE_OFICINA = "oficina";

        protected const string TIPO_DE_INMUEBLE_PARCELA = "terreno_o_parcela";

        protected const string TIPO_DE_INMUEBLE_EDIFICIO = "edificio";

        protected const string SUBTIPO_DE_INMUEBLE_PISO = "piso";

        protected const string SUBTIPO_DE_INMUEBLE_APARTAMENTO = "apartamento";

        protected const string SUBTIPO_DE_INMUEBLE_ÁTICO = "ático";

        protected const string SUBTIPO_DE_INMUEBLE_BUNGALOW = "bungalow";

        protected const string SUBTIPO_DE_INMUEBLE_DUPLEX = "dúplex";

        protected const string SUBTIPO_DE_INMUEBLE_CHALET_INDEPENDIENTE = "chalet_independiente";

        protected const string SUBTIPO_DE_INMUEBLE_CHALET_PAREADO = "chalet_pareado";

        protected const string SUBTIPO_DE_INMUEBLE_CHALET_ADOSADO = "chalet_adosado";

        protected const string SUBTIPO_DE_INMUEBLE_PARCELA_URBANA = "parcela_urbana";

        protected const string SUBTIPO_DE_INMUEBLE_PARCELA_URBANIZABLE = "parcela_urbanizable";

        protected const string SUBTIPO_DE_INMUEBLE_PARCELA_NO_URBANIZABLE = "parcela_no_urbanizable";

        protected const string ESTADO_DE_LA_CONSTRUCCIÓN_OBRA_NUEVA = "obra_nueva";

        protected const string ESTADO_DE_LA_CONSTRUCCIÓN_BUENO = "buen_estado";

        protected const string ESTADO_DE_LA_CONSTRUCCIÓN_A_REFORMAR = "a_reformar";

        protected const string ESTADO_DE_LA_CONSTRUCCIÓN_EN_RUINAS = "en_ruinas";


        protected readonly JsonArray TiposDeOperación =
        [
            OPERACION_VENTA,
            OPERACION_ALQUILER
        ];


        protected readonly JsonArray TiposDeInmueble =
        [
            TIPO_DE_INMUEBLE_VIVIENDA,
            TIPO_DE_INMUEBLE_DORMITORIO,
            TIPO_DE_INMUEBLE_LOCAL_COMERCIAL,
            TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL,
            TIPO_DE_INMUEBLE_GARAJE,
            TIPO_DE_INMUEBLE_TRASTERO,
            TIPO_DE_INMUEBLE_OFICINA,
            TIPO_DE_INMUEBLE_PARCELA,
            TIPO_DE_INMUEBLE_EDIFICIO
        ];


        protected readonly JsonArray SubtiposDeInmueble =
        [
            SUBTIPO_DE_INMUEBLE_PISO,
            SUBTIPO_DE_INMUEBLE_APARTAMENTO,
            SUBTIPO_DE_INMUEBLE_ÁTICO,
            SUBTIPO_DE_INMUEBLE_BUNGALOW,
            SUBTIPO_DE_INMUEBLE_DUPLEX,
            SUBTIPO_DE_INMUEBLE_CHALET_INDEPENDIENTE,
            SUBTIPO_DE_INMUEBLE_CHALET_PAREADO,
            SUBTIPO_DE_INMUEBLE_CHALET_ADOSADO,
            SUBTIPO_DE_INMUEBLE_PARCELA_URBANA,
            SUBTIPO_DE_INMUEBLE_PARCELA_URBANIZABLE,
            SUBTIPO_DE_INMUEBLE_PARCELA_NO_URBANIZABLE
        ];


        protected readonly JsonArray EstadosDeLaConstrucción =
        [
            ESTADO_DE_LA_CONSTRUCCIÓN_OBRA_NUEVA,
            ESTADO_DE_LA_CONSTRUCCIÓN_BUENO,
            ESTADO_DE_LA_CONSTRUCCIÓN_A_REFORMAR,
            ESTADO_DE_LA_CONSTRUCCIÓN_EN_RUINAS
        ];

        public static string GetDescriptionAttribute(string name)
        {
            PropertyInfo? propertyInfo = typeof(StructuredOutputEsJson).GetProperty(name);
            if (propertyInfo != null)
            {
                var descriptionAttribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(propertyInfo, typeof(DescriptionAttribute));
                if (descriptionAttribute != null)
                {
                    return descriptionAttribute.Description;
                }
            }
            return string.Empty;
        }
    }
}
