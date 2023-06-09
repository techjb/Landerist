﻿using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;
using Newtonsoft.Json;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ChatGPTResponse
    {
        [JsonProperty("fecha de publicación")]
        public string? FechaDePublicación { get; set; } = null;

        [JsonProperty("tipo de operación")]
        public string? TipoDeOperacion { get; set; } = null;

        [JsonProperty("tipo de anuncio")]
        public string? TipoDeInmueble { get; set; } = null;

        [JsonProperty("subtipo")]
        public string? SubtipoDeInmueble { get; set; } = null;

        [JsonProperty("precio del anuncio")]
        public decimal? PrecioDelAnuncio { get; set; } = null;

        [JsonProperty("descripción del anuncio")]
        public string? DescripciónDelAnuncio { get; set; } = null;

        [JsonProperty("referencia del anuncio")]
        public string? ReferenciaDelAnuncio { get; set; } = null;

        [JsonProperty("teléfono de contacto")]
        public string? TeléfonoDeContacto { get; set; } = null;

        [JsonProperty("email de contacto")]
        public string? EmailDeContacto { get; set; } = null;

        [JsonProperty("dirección del inmueble")]
        public string? DirecciónDelInmueble { get; set; } = null;

        [JsonProperty("referencia catastral")]
        public string? ReferenciaCatastral { get; set; } = null;

        [JsonProperty("metros cuadrados del inmueble")]
        public double? TamañoDelInmueble { get; set; } = null;

        [JsonProperty("metros cuadrados de la parcela")]
        public double? TamañoDeLaParcela { get; set; } = null;

        [JsonProperty("año de construcción")]
        public double? AñoDeConstrucción { get; set; } = null;

        [JsonProperty("estado del inmueble")]
        public string? EstadoDeLaConstrucción { get; set; } = null;

        [JsonProperty("plantas del edificio")]
        public double? PlantasDelEdificio { get; set; } = null;

        [JsonProperty("planta del inmueble")]
        public string? PlantaDelInmueble { get; set; } = null;

        [JsonProperty("número de dormitorios")]
        public double? NúmeroDeDormitorios { get; set; } = null;

        [JsonProperty("número de baños")]
        public double? NúmeroDeBaños { get; set; } = null;

        [JsonProperty("número de parkings")]
        public double? NúmeroDeParkings { get; set; } = null;

        [JsonProperty("tiene terraza")]
        public bool? TieneTerraza { get; set; } = null;

        [JsonProperty("tiene jardín")]
        public bool? TieneJardín { get; set; } = null;

        [JsonProperty("tiene garaje")]
        public bool? TieneGaraje { get; set; } = null;

        [JsonProperty("tiene parking para moto")]
        public bool? TieneParkingParaMoto { get; set; } = null;

        [JsonProperty("tiene piscina")]
        public bool? TienePiscina { get; set; } = null;

        [JsonProperty("tiene ascensor")]
        public bool? TieneAscensor { get; set; } = null;

        [JsonProperty("tiene acceso para discapacitados")]
        public bool? TieneAccesoParaDiscapacitados { get; set; } = null;

        [JsonProperty("tiene trastero")]
        public bool? TieneTrastero { get; set; } = null;

        [JsonProperty("está amueblado")]
        public bool? EstáAmueblado { get; set; } = null;

        [JsonProperty("no está amueblado")]
        public bool? NoEstáAmueblado { get; set; } = null;

        [JsonProperty("tiene calefacción")]
        public bool? TienCalefácción { get; set; } = null;

        [JsonProperty("tiene aire acondicionado")]
        public bool? TienAireAcondicionado { get; set; } = null;

        [JsonProperty("permite mascotas")]
        public bool? PermiteMascotas { get; set; } = null;

        [JsonProperty("tiene sistemas de seguridad")]
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
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_LOCAL_COMERCIAL) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_GARAJE) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_TRASTERO) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_OFICINA) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_PARCELA) ||
                    TipoDeInmueble.Equals(TIPO_DE_INMUEBLE_EDIFICIO);
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
            switch (TipoDeOperacion)
            {
                case OPERACION_VENTA: return Operation.sell;
                case OPERACION_ALQUILER: return Operation.rent;
                default: return Operation.sell;
            }
        }

        private PropertyType GetPropertyType()
        {
            switch (TipoDeInmueble)
            {
                case TIPO_DE_INMUEBLE_VIVIENDA: return PropertyType.home;
                case TIPO_DE_INMUEBLE_DORMITORIO: return PropertyType.room;
                case TIPO_DE_INMUEBLE_LOCAL_COMERCIAL: return PropertyType.premise;
                case TIPO_DE_INMUEBLE_NAVE_INDUSTRIAL: return PropertyType.industrial;
                case TIPO_DE_INMUEBLE_GARAJE: return PropertyType.garage;
                case TIPO_DE_INMUEBLE_TRASTERO: return PropertyType.storage;
                case TIPO_DE_INMUEBLE_OFICINA: return PropertyType.office;
                case TIPO_DE_INMUEBLE_PARCELA: return PropertyType.land;
                case TIPO_DE_INMUEBLE_EDIFICIO: return PropertyType.building;
                default: return PropertyType.home;
            }
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

                case PropertyType.room:
                    break;
                case PropertyType.premise:
                    break;
                case PropertyType.industrial:
                    break;
                case PropertyType.garage:
                    break;
                case PropertyType.storage:
                    break;
                case PropertyType.office:
                    break;
                case PropertyType.building:
                    break;
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
