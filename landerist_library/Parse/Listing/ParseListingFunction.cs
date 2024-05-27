using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Text.Json.Nodes;
using landerist_library.Configuration;

namespace landerist_library.Parse.Listing
{
    public class ParseListingFunction
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


        protected static readonly JsonArray TiposDeOperación =
        [
            OPERACION_VENTA,
            OPERACION_ALQUILER
        ];

        protected static readonly JsonArray TiposDeInmueble =
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

        protected static readonly JsonArray SubtiposDeInmueble =
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

        protected static readonly JsonArray EstadosDeLaConstrucción =
        [
            ESTADO_DE_LA_CONSTRUCCIÓN_OBRA_NUEVA,
            ESTADO_DE_LA_CONSTRUCCIÓN_BUENO,
            ESTADO_DE_LA_CONSTRUCCIÓN_A_REFORMAR,
            ESTADO_DE_LA_CONSTRUCCIÓN_EN_RUINAS
        ];

        public landerist_orels.ES.Listing? ToListing(Page page)
        {
            if (!IsValidResponse())
            {
                return null;
            }

            var listing = new landerist_orels.ES.Listing
            {
                guid = page.UriHash,
                listingStatus = GetListingStatus(),
                listingDate = GetListingDate(),
                operation = GetOperation(),
                propertyType = GetPropertyType(),
                propertySubtype = GetPropertySubtype(),
                price = GetPropertyPrice(),
                description = GetDescription(),
                dataSourceName = GetDataSourceName(page),
                dataSourceGuid = GetDataSourceGuid(),
                dataSourceUpdate = GetDataSourceUpdate(),
                dataSourceUrl = GetDataSourceUrl(page),
                contactPhone = GetPhone(),
                contactEmail = GetEmail(),
                address = GetAddress(),
                cadastralReference = GetCadastralReference(),
                propertySize = GetPropertySize(),
                landSize = GetLandSize(),
                constructionYear = GetConstrunctionYear(),
                constructionStatus = GetConstructionStatus(),
                floors = GetFloors(),
                floor = GetFloor(),
                bedrooms = GetBedrooms(),
                bathrooms = GetBathrooms(),
                parkings = GetParkings(),
                terrace = TieneTerraza,
                garden = TieneJardín,
                garage = TieneGaraje,
                motorbikeGarage = TieneParkingParaMoto,
                pool = TienePiscina,
                lift = TieneAscensor,
                disabledAccess = TieneAccesoParaDiscapacitados,
                storageRoom = TieneTrastero,
                furnished = EstáAmueblado,
                nonFurnished = NoEstáAmueblado,
                heating = TienCalefácción,
                airConditioning = TienAireAcondicionado,
                petsAllowed = PermiteMascotas,
                securitySystems = TieneSistemasDeSeguridad,
            };
            return listing;
        }

        private bool IsValidResponse()
        {
            return
                IsValidTipoDeOperacion() &&
                IsValidTipoDeInmueble() &&
                IsValidListingDate() &&
                IsValidDescription()
                ;
        }

        private bool IsValidTipoDeOperacion()
        {
            if (TipoDeOperación != null)
            {
                return JsonArrayContains(TiposDeOperación, TipoDeOperación);
            }
            return false;
        }

        private bool IsValidTipoDeInmueble()
        {
            if (TipoDeInmueble != null)
            {
                return JsonArrayContains(TiposDeInmueble, TipoDeInmueble);
            }
            return false;
        }

        private bool IsValidListingDate()
        {
            var listingDate = GetListingDate();
            var maxListingDate = DateTime.Now.AddDays(1);
            var minListingDate = DateTime.Now.AddYears(-Config.MAX_YEARS_SINCE_PUBLISHED_LISTING);

            return listingDate <= maxListingDate && listingDate >= minListingDate;
        }

        private bool IsValidDescription()
        {
            var description = GetDescription();
            return !string.IsNullOrEmpty(description);
        }

        private static bool JsonArrayContains(JsonArray jsonArray, string value)
        {
            var contains = false;
            foreach (var item in jsonArray)
            {
                if (item is null)
                {
                    continue;
                }
                if (item.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }


        private static ListingStatus GetListingStatus()
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
            return TipoDeOperación switch
            {
                OPERACION_VENTA => Operation.sell,
                OPERACION_ALQUILER => Operation.rent,
                _ => Operation.sell,
            };
        }

        private PropertyType GetPropertyType()
        {
            return TipoDeInmueble switch
            {
                TIPO_DE_INMUEBLE_VIVIENDA => PropertyType.home,
                TIPO_DE_INMUEBLE_DORMITORIO => PropertyType.room,
                TIPO_DE_INMUEBLE_LOCAL_COMERCIAL => PropertyType.premise,
                TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL => PropertyType.industrial,
                TIPO_DE_INMUEBLE_GARAJE => PropertyType.garage,
                TIPO_DE_INMUEBLE_TRASTERO => PropertyType.storage,
                TIPO_DE_INMUEBLE_OFICINA => PropertyType.office,
                TIPO_DE_INMUEBLE_PARCELA => PropertyType.land,
                TIPO_DE_INMUEBLE_EDIFICIO => PropertyType.building,
                _ => PropertyType.home,
            };
        }

        private PropertySubtype? GetPropertySubtype()
        {
            if (SubtipoDeInmueble != null)
            {
                PropertySubtype? propertySubtype = GetPropertySubtypeValue();
                if (IsValidPropertySubtype(propertySubtype))
                {
                    return propertySubtype;
                }
            }
            return null;
        }

        private PropertySubtype? GetPropertySubtypeValue()
        {
            return SubtipoDeInmueble switch
            {
                SUBTIPO_DE_INMUEBLE_PISO => (PropertySubtype?)PropertySubtype.flat,
                SUBTIPO_DE_INMUEBLE_APARTAMENTO => (PropertySubtype?)PropertySubtype.appartment,
                SUBTIPO_DE_INMUEBLE_ÁTICO => (PropertySubtype?)PropertySubtype.penthouse,
                SUBTIPO_DE_INMUEBLE_BUNGALOW => (PropertySubtype?)PropertySubtype.bungalow,
                SUBTIPO_DE_INMUEBLE_DUPLEX => (PropertySubtype?)PropertySubtype.duplex,
                SUBTIPO_DE_INMUEBLE_CHALET_INDEPENDIENTE => (PropertySubtype?)PropertySubtype.detached,
                SUBTIPO_DE_INMUEBLE_CHALET_PAREADO => (PropertySubtype?)PropertySubtype.semi_detached,
                SUBTIPO_DE_INMUEBLE_CHALET_ADOSADO => (PropertySubtype?)PropertySubtype.terraced,
                SUBTIPO_DE_INMUEBLE_PARCELA_URBANA => (PropertySubtype?)PropertySubtype.developed,
                SUBTIPO_DE_INMUEBLE_PARCELA_URBANIZABLE => (PropertySubtype?)PropertySubtype.buildable,
                SUBTIPO_DE_INMUEBLE_PARCELA_NO_URBANIZABLE => (PropertySubtype?)PropertySubtype.non_building,
                _ => null,
            };
        }

        private bool IsValidPropertySubtype(PropertySubtype? propertySubtype)
        {
            if (propertySubtype == null)
            {
                return true;
            }
            PropertySubtype propertySubtypeNotNull = (PropertySubtype)propertySubtype;
            var propertyType = GetPropertyType();
            switch (propertyType)
            {
                case PropertyType.home:
                    {
                        return IsValidSubtypeHome(propertySubtypeNotNull);
                    }
                case PropertyType.land:
                    {
                        return IsValidSubtypeLand(propertySubtypeNotNull);
                    }
                default: break;
            }
            return false;
        }

        private static bool IsValidSubtypeHome(PropertySubtype propertySubtype)
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

        private static bool IsValidSubtypeLand(PropertySubtype propertySubtype)
        {
            return
                propertySubtype.Equals(PropertySubtype.developed) ||
                propertySubtype.Equals(PropertySubtype.buildable) ||
                propertySubtype.Equals(PropertySubtype.non_building);
        }

        private Price? GetPropertyPrice()
        {
            if (PrecioDelAnuncio.HasValue)
            {
                var price = (decimal)PrecioDelAnuncio;
                if (price > 0)
                {
                    return new Price(price, Currency.EUR);
                }
            }
            return null;
        }

        private string? GetDescription()
        {
            if (string.IsNullOrEmpty(DescripciónDelAnuncio))
            {
                return null;
            }
            return Strings.Clean(DescripciónDelAnuncio);
        }

        private static string GetDataSourceName(Page page)
        {
            return page.Website.Host;
        }

        private string? GetDataSourceGuid()
        {
            if (string.IsNullOrEmpty(ReferenciaDelAnuncio))
            {
                return null;
            }
            return Strings.Clean(ReferenciaDelAnuncio);
        }

        private static DateTime GetDataSourceUpdate()
        {
            return DateTime.Now;
        }

        private static Uri GetDataSourceUrl(Page page)
        {
            return page.Uri;
        }

        private string? GetPhone()
        {
            if (string.IsNullOrEmpty(TeléfonoDeContacto))
            {
                return null;
            }
            TeléfonoDeContacto = Strings.Clean(TeléfonoDeContacto);
            if (!Validate.Phone(TeléfonoDeContacto))
            {
                return null;
            }
            return TeléfonoDeContacto;
        }

        private string? GetEmail()
        {
            if (string.IsNullOrEmpty(EmailDeContacto))
            {
                return null;
            }
            EmailDeContacto = Strings.Clean(EmailDeContacto);
            EmailDeContacto = Strings.RemoveSpaces(EmailDeContacto);
            if (!Validate.Email(EmailDeContacto))
            {
                return null;
            }
            return EmailDeContacto;
        }

        private string? GetAddress()
        {
            if (string.IsNullOrEmpty(DirecciónDelInmueble))
            {
                return null;
            }
            return Strings.Clean(DirecciónDelInmueble);
        }

        private string? GetCadastralReference()
        {
            if (string.IsNullOrEmpty(ReferenciaCatastral))
            {
                return null;
            }
            ReferenciaCatastral = Strings.Clean(ReferenciaCatastral);
            if (!Validate.CadastralReference(ReferenciaCatastral))
            {
                return null;
            }
            return ReferenciaCatastral;
        }

        private double? GetPropertySize()
        {
            if (TamañoDelInmueble.HasValue &&
                TamañoDelInmueble >= Config.MIN_PROPERTY_SIZE &&
                TamañoDelInmueble <= Config.MAX_PROPERTY_SIZE)
            {
                return TamañoDelInmueble;
            }
            return null;
        }

        private double? GetLandSize()
        {
            if (TamañoDeLaParcela.HasValue &&
                TamañoDeLaParcela >= Config.MIN_LAND_SIZE &&
                TamañoDeLaParcela <= Config.MAX_LAND_SIZE)
            {
                return TamañoDeLaParcela;
            }
            return null;
        }

        private int? GetConstrunctionYear()
        {
            var constructionYear = (int?)AñoDeConstrucción;
            var maxConstructionYear = DateTime.Now.AddYears(Config.MAX_CONSTRUCTION_YEARS_FROM_NOW).Year;

            if (constructionYear.HasValue &&
                constructionYear >= Config.MIN_CONSTRUCTION_YEAR &&
                constructionYear <= maxConstructionYear)
            {
                return constructionYear;
            }
            return null;
        }

        private ConstructionStatus? GetConstructionStatus()
        {
            if (string.IsNullOrEmpty(EstadoDeLaConstrucción))
            {
                return null;
            }
            return EstadoDeLaConstrucción switch
            {
                ESTADO_DE_LA_CONSTRUCCIÓN_OBRA_NUEVA => (ConstructionStatus?)ConstructionStatus.@new,
                ESTADO_DE_LA_CONSTRUCCIÓN_BUENO => (ConstructionStatus?)ConstructionStatus.good,
                ESTADO_DE_LA_CONSTRUCCIÓN_A_REFORMAR => (ConstructionStatus?)ConstructionStatus.for_renovation,
                ESTADO_DE_LA_CONSTRUCCIÓN_EN_RUINAS => (ConstructionStatus?)ConstructionStatus.refurbished,
                _ => null,
            };
        }

        private int? GetFloors()
        {
            var floors = (int?)PlantasDelEdificio;
            if (floors.HasValue &&
                floors >= Config.MIN_FLOORS &&
                floors <= Config.MAX_FLOORS)
            {
                return floors;
            }
            return null;
        }

        private string? GetFloor()
        {
            if (string.IsNullOrEmpty(PlantaDelInmueble))
            {
                return null;
            }
            return Strings.Clean(PlantaDelInmueble);
        }

        private int? GetBedrooms()
        {
            var bedrooms = (int?)NúmeroDeDormitorios;
            if (bedrooms.HasValue &&
                bedrooms >= Config.MIN_BEDROOMS &&
                bedrooms <= Config.MAX_BEDROOMS)
            {
                return bedrooms;
            }
            return null;
        }

        private int? GetBathrooms()
        {
            var bathrooms = (int?)NúmeroDeBaños;
            if (bathrooms.HasValue &&
                bathrooms >= Config.MIN_BATHROOMS &&
                bathrooms <= Config.MAX_BATHROOMS)
            {
                return bathrooms;
            }
            return null;
        }

        private int? GetParkings()
        {
            var parkings = (int?)NúmeroDeParkings;
            if (parkings.HasValue &&
                parkings >= Config.MIN_PARKINGS &&
                parkings <= Config.MAX_PARKINGS)
            {
                return parkings;
            }
            return null;
        }
    }
}
