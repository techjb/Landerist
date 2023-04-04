using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Parse
{
    internal class ListingResponse
    {
        public string? FechaDePublicación { get; set; } = null;

        public string? TipoDeOperacion { get; set; } = null;

        public string? TipoDeInmueble { get; set; } = null;

        public string? SubtipoDeInmueble { get; set; } = null;

        public decimal? PrecioDelAnuncio { get; set; } = null;

        public string? DescripciónDelAnuncio { get; set; } = null;

        public string? NombreDeLaFuenteDelDato { get; set; } = null;

        public string? ReferenciaDelAnuncio { get; set; } = null;

        public string? DirecciónDelInmueble { get; set; } = null;

        public string? RererenciaCatastral { get; set; } = null;

        public int? TamañoDelInmueble { get; set; } = null;

        public int? TamañoDeLaParcela { get; set; } = null;

        public int? AñoDeConstrucción { get; set; } = null;

        public string? EstadoDeLaConstrucción { get; set; } = null;

        public int? NúmeroDePlantasDelEdificio { get; set; } = null;

        public int? PlantaDelInmueble { get; set; } = null;

        public int? NúmeroDeDormitorios { get; set; } = null;

        public int? NúmeroDeBaños { get; set; } = null;

        public int? NúmeroDeParkings { get; set; } = null;

        public string? Características { get; set; } = null;


        private const string OPERACION_VENTA = "venta";

        private const string OPERACION_ALQUILER = "alquiler";

        private const string TIPO_DE_INMUEBLE_VIVIENDA = "vivienda";

        private const string TIPO_DE_INMUEBLE_DORMITORIO = "dormitorio";

        private const string TIPO_DE_INMUEBLE_LOCALCOMERCIAL = "local_comercial";

        private const string TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL = "nave_industrial";

        private const string TIPO_DE_INMUEBLE_GARAJE = "garaje";

        private const string TIPO_DE_INMUEBLE_TRASTERO = "trastero";

        private const string TIPO_DE_INMUEBLE_OFICINA = "oficina";

        private const string TIPO_DE_INMUEBLE_PARCELA = "terreno_o_parcela";

        private const string TIPO_DE_INMUEBLE_EDIFICIO = "edificio";

        private const string SUBTIPO_DE_INMUEBLE_PISO = "piso";

        private const string SUBTIPO_DE_INMUEBLE_APARTAMENTO = "apartamento";

        private const string SUBTIPO_DE_INMUEBLE_ÁTICO = "ático";

        private const string SUBTIPO_DE_INMUEBLE_BUNGALOW = "bungalow";

        private const string SUBTIPO_DE_INMUEBLE_DUPLEX = "dúplex";

        private const string SUBTIPO_DE_INMUEBLE_CHALET_INDEPENDIENTE = "chalet_independiente";

        private const string SUBTIPO_DE_INMUEBLE_CHALET_PAREADO = "chalet_pareado";

        private const string SUBTIPO_DE_INMUEBLE_CHALET_ADOSADO = "chalet_adosado";

        private const string SUBTIPO_DE_INMUEBLE_PARCELA_URBANA = "parcela_urbana";

        private const string SUBTIPO_DE_INMUEBLE_PARCELA_URBANIZABLE = "parcela_urbanizable";

        private const string SUBTIPO_DE_INMUEBLE_PARCELA_NO_URBANIZABLE = "parcela_no_urbanizable";


        public Listing? ToListing(Page page)
        {
            if (!IsValidResponse())
            {
                return null;
            }
            var listing = new Listing
            {
                guid = page.UriHash,
                listingStatus = GetListingStatus(),
                listingDate = GetListingDate(),
                operation = GetOperation(),
                propertyType = GetPropertyType(),
                propertySubtype = GetPropertySubtype(),
            };
            return listing;
        }

        private bool IsValidResponse()
        {
            return
                IsValidTipoDeOperacion() &&
                IsValidTipoDeInmueble();
        }

        private bool IsValidTipoDeOperacion()
        {
            if (TipoDeOperacion != null)
            {
                return
                    TipoDeOperacion.Equals(OPERACION_VENTA) ||
                    TipoDeOperacion.Equals(OPERACION_ALQUILER);
            }
            return false;
        }

        private bool IsValidTipoDeInmueble()
        {
            if (TipoDeInmueble != null)
            {
                return
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_VIVIENDA) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_DORMITORIO) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_LOCALCOMERCIAL) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_GARAJE) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_TRASTERO) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_OFICINA) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_PARCELA) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_EDIFICIO);
            }
            return false;
        }


        private ListingStatus GetListingStatus()
        {
            return ListingStatus.published;
        }

        private DateTime GetListingDate()
        {
            if (FechaDePublicación != null)
            {
                if (DateTime.TryParse(FechaDePublicación, out DateTime listingDate))
                {
                    return listingDate;
                }
            }
            return DateTime.Now;
        }

        private Operation GetOperation()
        {
            switch (TipoDeOperacion)
            {
                case OPERACION_VENTA: return Operation.sell;
                case OPERACION_ALQUILER: return Operation.rent;
                default:
                    break;
            }
            return Operation.sell;
        }

        private PropertyType GetPropertyType()
        {
            switch (TipoDeInmueble)
            {
                case TIPO_DE_INMUEBLE_VIVIENDA: return PropertyType.home;
                case TIPO_DE_INMUEBLE_DORMITORIO: return PropertyType.room;
                case TIPO_DE_INMUEBLE_LOCALCOMERCIAL: return PropertyType.premise;
                case TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL: return PropertyType.industrial;
                case TIPO_DE_INMUEBLE_GARAJE: return PropertyType.garage;
                case TIPO_DE_INMUEBLE_TRASTERO: return PropertyType.storage;
                case TIPO_DE_INMUEBLE_OFICINA: return PropertyType.office;
                case TIPO_DE_INMUEBLE_PARCELA: return PropertyType.land;
                case TIPO_DE_INMUEBLE_EDIFICIO: return PropertyType.building;
                default:
                    break;
            }

            return PropertyType.home;
        }

        private PropertySubtype? GetPropertySubtype()
        {
            PropertySubtype? propertySubtype = null;
            if (SubtipoDeInmueble != null)
            {
                switch (SubtipoDeInmueble)
                {
                    case SUBTIPO_DE_INMUEBLE_PISO:
                        {
                            propertySubtype = PropertySubtype.flat;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_APARTAMENTO:
                        {
                            propertySubtype = PropertySubtype.appartment;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_ÁTICO:
                        {
                            propertySubtype = PropertySubtype.penthouse;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_BUNGALOW:
                        {
                            propertySubtype = PropertySubtype.bungalow;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_DUPLEX:
                        {
                            propertySubtype = PropertySubtype.duplex;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_CHALET_INDEPENDIENTE:
                        {
                            propertySubtype = PropertySubtype.detached;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_CHALET_PAREADO:
                        {
                            propertySubtype = PropertySubtype.semi_detached;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_CHALET_ADOSADO:
                        {
                            propertySubtype = PropertySubtype.terraced;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_PARCELA_URBANA:
                        {
                            propertySubtype = PropertySubtype.developed;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_PARCELA_URBANIZABLE:
                        {
                            propertySubtype = PropertySubtype.buildable;
                        }
                        break;
                    case SUBTIPO_DE_INMUEBLE_PARCELA_NO_URBANIZABLE:
                        {
                            propertySubtype = PropertySubtype.non_building;
                        }
                        break;
                    default:
                        break;
                }
                if (!IsValidPropertySubtype(propertySubtype))
                {
                    propertySubtype = null;
                }
            }

            return propertySubtype;
        }

        private bool IsValidPropertySubtype(PropertySubtype? propertySubtype)
        {
            if (propertySubtype == null)
            {
                return true;
            }
            PropertySubtype propertySubtypeNonNull = (PropertySubtype)propertySubtype;
            var propertyType = GetPropertyType();
            switch (propertyType)
            {
                case PropertyType.home:
                    {
                        return IsValidSubtypeHome(propertySubtypeNonNull);
                    }
                case PropertyType.land:
                    {
                        return IsValidSubtypeLand(propertySubtypeNonNull);

                    }
                default: break;
            }
            return false;
        }

        private bool IsValidSubtypeHome(PropertySubtype propertySubtype)
        {
            return
                propertySubtype.Equals(PropertySubtype.flat) ||
                propertySubtype.Equals(PropertySubtype.appartment) ||
                propertySubtype.Equals(PropertySubtype.penthouse) ||
                propertySubtype.Equals(PropertySubtype.bungalow) ||
                propertySubtype.Equals(PropertySubtype.duplex) ||
                propertySubtype.Equals(PropertySubtype.detached) ||
                propertySubtype.Equals(PropertySubtype.semi_detached) ||
                propertySubtype.Equals(PropertySubtype.terraced);
        }

        private bool IsValidSubtypeLand(PropertySubtype propertySubtype)
        {
            return
                propertySubtype.Equals(PropertySubtype.developed) ||
                propertySubtype.Equals(PropertySubtype.buildable) ||
                propertySubtype.Equals(PropertySubtype.non_building);
        }
    }
}
