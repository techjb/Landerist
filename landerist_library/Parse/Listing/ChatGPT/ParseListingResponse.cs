using landerist_library.Websites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ParseListingResponse
    {

        public string? FechaDePublicación { get; set; } = null;

        public string? TipoDeOperacion { get; set; } = null;

        public string? TipoDeInmueble { get; set; } = null;

        public string? SubtipoDeInmueble { get; set; } = null;

        public decimal? PrecioDelAnuncio { get; set; } = null;

        public string? DescripciónDelAnuncio { get; set; } = null;

        public string? ReferenciaDelAnuncio { get; set; } = null;

        public string? TeléfonoDeContacto { get; set; } = null;

        public string? EmailDeContacto { get; set; } = null;

        public string? DirecciónDelInmueble { get; set; } = null;

        public string? ReferenciaCatastral { get; set; } = null;

        public double? TamañoDelInmueble { get; set; } = null;

        public double? TamañoDeLaParcela { get; set; } = null;

        public double? AñoDeConstrucción { get; set; } = null;

        public string? EstadoDeLaConstrucción { get; set; } = null;

        public double? PlantasDelEdificio { get; set; } = null;

        public string? PlantaDelInmueble { get; set; } = null;

        public double? NúmeroDeDormitorios { get; set; } = null;

        public double? NúmeroDeBaños { get; set; } = null;

        public double? NúmeroDeParkings { get; set; } = null;

        public bool? TieneTerraza { get; set; } = null;

        public bool? TieneJardín { get; set; } = null;

        public bool? TieneGaraje { get; set; } = null;

        public bool? TieneParkingParaMoto { get; set; } = null;

        public bool? TienePiscina { get; set; } = null;

        public bool? TieneAscensor { get; set; } = null;

        public bool? TieneAccesoParaDiscapacitados { get; set; } = null;

        public bool? TieneTrastero { get; set; } = null;

        public bool? EstáAmueblado { get; set; } = null;

        public bool? NoEstáAmueblado { get; set; } = null;

        public bool? TienCalefácción { get; set; } = null;

        public bool? TienAireAcondicionado { get; set; } = null;

        public bool? PermiteMascotas { get; set; } = null;

        public bool? TieneSistemasDeSeguridad { get; set; } = null;


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

        public landerist_orels.ES.Listing? ToListing(Page page)
        {
            return null;
        }
    }
}
