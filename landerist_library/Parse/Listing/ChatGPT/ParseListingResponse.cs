using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;
using System.ComponentModel;
using System.Text.Json.Nodes;

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


        protected static readonly JsonArray TiposDeOperación = new()
        {
            OPERACION_VENTA,
            OPERACION_ALQUILER
        };

        protected static readonly JsonArray TiposDeInmueble = new()
        {
            TIPO_DE_INMUEBLE_VIVIENDA,
            TIPO_DE_INMUEBLE_DORMITORIO,
            TIPO_DE_INMUEBLE_LOCAL_COMERCIAL,
            TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL,
            TIPO_DE_INMUEBLE_GARAJE,
            TIPO_DE_INMUEBLE_TRASTERO,
            TIPO_DE_INMUEBLE_OFICINA,
            TIPO_DE_INMUEBLE_PARCELA,
            TIPO_DE_INMUEBLE_EDIFICIO
        };

        protected static readonly JsonArray SubtiposDeInmueble = new()
        {
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
        };

        protected static readonly JsonArray EstadosDeLaConstrucción = new()
        {
            ESTADO_DE_LA_CONSTRUCCIÓN_OBRA_NUEVA,
            ESTADO_DE_LA_CONSTRUCCIÓN_BUENO,
            ESTADO_DE_LA_CONSTRUCCIÓN_A_REFORMAR,
            ESTADO_DE_LA_CONSTRUCCIÓN_EN_RUINAS
        };

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
                propertySize = TamañoDelInmueble,
                landSize = TamañoDeLaParcela,
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
                IsValidTipoDeInmueble();
        }

        private bool IsValidTipoDeOperacion()
        {
            if (TipoDeOperación != null)
            {
                return JsonArrayContains(TiposDeOperación, TipoDeOperación);
            }
            return false;
        }


        private static bool JsonArrayContains(JsonArray jsonArray, string value)
        {
            var contains = false;
            foreach (var item in jsonArray)
            {
                if (item.ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }

        private bool IsValidTipoDeInmueble()
        {
            if (TipoDeInmueble != null)
            {
                return JsonArrayContains(TiposDeInmueble, TipoDeInmueble); 
            }
            return false;
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
                    if (listingDate < DateTime.Now)
                    {
                        return listingDate;
                    }
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
            if (PrecioDelAnuncio == null)
            {
                return null;
            }

            var price = (decimal)PrecioDelAnuncio;
            if (price <= 0)
            {
                return null;
            }
            return new Price(price, Currency.EUR);
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

        private int? GetConstrunctionYear()
        {
            return (int?)AñoDeConstrucción;
        }

        private ConstructionStatus? GetConstructionStatus()
        {
            if (string.IsNullOrEmpty(EstadoDeLaConstrucción))
            {
                return null;
            }
            switch (EstadoDeLaConstrucción)
            {
                case ESTADO_DE_LA_CONSTRUCCIÓN_OBRA_NUEVA: return ConstructionStatus.@new;
                case ESTADO_DE_LA_CONSTRUCCIÓN_BUENO: return ConstructionStatus.good;
                case ESTADO_DE_LA_CONSTRUCCIÓN_A_REFORMAR: return ConstructionStatus.for_renovation;
                case ESTADO_DE_LA_CONSTRUCCIÓN_EN_RUINAS: return ConstructionStatus.refurbished;
                default: return null;
            }
        }

        private int? GetFloors()
        {
            return (int?)PlantasDelEdificio;
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
            return (int?)NúmeroDeDormitorios;
        }

        private int? GetBathrooms()
        {
            return (int?)NúmeroDeBaños;
        }

        private int? GetParkings()
        {
            return (int?)NúmeroDeParkings;
        }
    }
}
