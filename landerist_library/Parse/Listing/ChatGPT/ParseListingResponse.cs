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

        public string? TipoDeOperación { get; set; } = null;

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
        

        public landerist_orels.ES.Listing? ToListing(Page page)
        {
            return null;
        }
    }
}
