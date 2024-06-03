using landerist_library.Tools;
using landerist_library.Websites;
using landerist_orels.ES;
using System.Text.Json.Nodes;
using landerist_library.Configuration;
using System.Reflection;

namespace landerist_library.Parse.Listing
{
    public class ParseListingTool
    {
        public const string FunctionNameIsListing = "si_es_un_anuncio";

        public static readonly string FunctionDescriptionIsListing = "Sí es un único anuncio inmobiliario";

        public const string FunctionNameIsNotListing = "no_es_un_anuncio";

        public static readonly string FunctionDescriptionIsNotListing = "No es un único anuncio inmobiliario";

#pragma warning disable IDE1006


        [Description("fecha de la publicación del anuncio")]
        public string? fecha_de_publicación { get; set; } = null;

        [Description("tipo de operación inmobiliaria")]
        public string? tipo_de_operación { get; set; } = null;

        [Description("tipología del inmueble")]
        public string? tipo_de_inmueble { get; set; } = null;

        [Description("subtipo de inmueble")]
        public string? subtipo_de_inmueble { get; set; } = null;

        [Description("precio del anuncio en euros")]
        public decimal? precio_del_anuncio { get; set; } = null;

        [Description("texto con la descripción detallada del anuncio")]
        public string? descripción_del_anuncio { get; set; } = null;

        [Description("código de referencia del anuncio")]
        public string? referencia_del_anuncio { get; set; } = null;

        [Description("número de teléfono de contacto")]
        public string? teléfono_de_contacto { get; set; } = null;

        [Description("dirección de email de contacto")]
        public string? email_de_contacto { get; set; } = null;

        [Description("dirección en la que se encuentra el inmueble")]
        public string? dirección_del_inmueble { get; set; } = null;

        [Description("referencia catastral del anuncio (14 o 20 caracteres)")]
        public string? referencia_catastral { get; set; } = null;

        [Description("número de metros cuadrados del inmueble")]
        public double? tamaño_del_inmueble { get; set; } = null;

        [Description("número de metros cuadrados de la parcela")]
        public double? tamaño_de_la_parcela { get; set; } = null;

        [Description("año de construcción del inmueble")]
        public double? año_de_construcción { get; set; } = null;

        [Description("estado de la construcción en el que se encuentra el inmueble")]
        public string? estado_de_la_construcción { get; set; } = null;

        [Description("número de plantas del edificio")]
        public double? plantas_del_edificio { get; set; } = null;

        [Description("número de planta en la que se ubica el inmueble")]
        public string? plantas_del_inmueble { get; set; } = null;

        [Description("número de dormitorios")]
        public double? número_de_dormitorios { get; set; } = null;

        [Description("número de baños")]
        public double? número_de_baños { get; set; } = null;

        [Description("número de parkings")]
        public double? número_de_parkings { get; set; } = null;

        [Description("tiene terraza")]
        public bool? tiene_terraza { get; set; } = null;

        [Description("tiene jardín")]
        public bool? tiene_jardín { get; set; } = null;

        [Description("tiene garaje")]
        public bool? tiene_garaje { get; set; } = null;

        [Description("tiene parking para moto")]
        public bool? tiene_parking_para_moto { get; set; } = null;

        [Description("tiene piscina")]
        public bool? tiene_piscina { get; set; } = null;

        [Description("tiene ascensor")]
        public bool? tiene_ascensor { get; set; } = null;

        [Description("tiene acceso para discapacitados")]
        public bool? tiene_acceso_para_discapacitados { get; set; } = null;

        [Description("tiene trastero")]
        public bool? tiene_trastero { get; set; } = null;

        [Description("está amueblado")]
        public bool? está_amueblado { get; set; } = null;

        [Description("no está amueblado")]
        public bool? no_está_amueblado { get; set; } = null;

        [Description("tiene calefacción")]
        public bool? tiene_calefacción { get; set; } = null;

        [Description("tiene aire acondicionado")]
        public bool? tiene_aire_acondicionado { get; set; } = null;

        [Description("se permiten mascotas")]
        public bool? permite_mascotas { get; set; } = null;

        [Description("tiene sistemas de seguridad")]
        public bool? tiene_sistemas_de_seguridad { get; set; } = null;

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
                terrace = tiene_terraza,
                garden = tiene_jardín,
                garage = tiene_garaje,
                motorbikeGarage = tiene_parking_para_moto,
                pool = tiene_piscina,
                lift = tiene_ascensor,
                disabledAccess = tiene_acceso_para_discapacitados,
                storageRoom = tiene_trastero,
                furnished = está_amueblado,
                nonFurnished = no_está_amueblado,
                heating = tiene_calefacción,
                airConditioning = tiene_aire_acondicionado,
                petsAllowed = permite_mascotas,
                securitySystems = tiene_sistemas_de_seguridad,
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
            if (tipo_de_operación != null)
            {
                return JsonArrayContains(TiposDeOperación, tipo_de_operación);
            }
            return false;
        }

        private bool IsValidTipoDeInmueble()
        {
            if (tipo_de_inmueble != null)
            {
                return JsonArrayContains(TiposDeInmueble, tipo_de_inmueble);
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
            if (fecha_de_publicación != null)
            {
                if (DateTime.TryParse(fecha_de_publicación, out DateTime listingDate))
                {
                    return listingDate;
                }
            }
            return DateTime.Now;
        }

        private Operation GetOperation()
        {
            return tipo_de_operación switch
            {
                OPERACION_VENTA => Operation.sell,
                OPERACION_ALQUILER => Operation.rent,
                _ => Operation.sell,
            };
        }

        private PropertyType GetPropertyType()
        {
            return tipo_de_inmueble switch
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
            if (subtipo_de_inmueble != null)
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
            return subtipo_de_inmueble switch
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
            if (precio_del_anuncio.HasValue)
            {
                var price = (decimal)precio_del_anuncio;
                if (price > 0)
                {
                    return new Price(price, Currency.EUR);
                }
            }
            return null;
        }

        private string? GetDescription()
        {
            if (string.IsNullOrEmpty(descripción_del_anuncio))
            {
                return null;
            }
            return Strings.Clean(descripción_del_anuncio);
        }

        private static string GetDataSourceName(Page page)
        {
            return page.Website.Host;
        }

        private string? GetDataSourceGuid()
        {
            if (string.IsNullOrEmpty(referencia_del_anuncio))
            {
                return null;
            }
            return Strings.Clean(referencia_del_anuncio);
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
            if (string.IsNullOrEmpty(teléfono_de_contacto))
            {
                return null;
            }
            teléfono_de_contacto = Strings.Clean(teléfono_de_contacto);
            if (!Validate.Phone(teléfono_de_contacto))
            {
                return null;
            }
            return teléfono_de_contacto;
        }

        private string? GetEmail()
        {
            if (string.IsNullOrEmpty(email_de_contacto))
            {
                return null;
            }
            email_de_contacto = Strings.Clean(email_de_contacto);
            email_de_contacto = Strings.RemoveSpaces(email_de_contacto);
            if (!Validate.Email(email_de_contacto))
            {
                return null;
            }
            return email_de_contacto;
        }

        private string? GetAddress()
        {
            if (string.IsNullOrEmpty(dirección_del_inmueble))
            {
                return null;
            }
            return Strings.Clean(dirección_del_inmueble);
        }

        private string? GetCadastralReference()
        {
            if (string.IsNullOrEmpty(referencia_catastral))
            {
                return null;
            }
            referencia_catastral = Strings.Clean(referencia_catastral);
            if (!Validate.CadastralReference(referencia_catastral))
            {
                return null;
            }
            return referencia_catastral;
        }

        private double? GetPropertySize()
        {
            if (tamaño_del_inmueble.HasValue &&
                tamaño_del_inmueble >= Config.MIN_PROPERTY_SIZE &&
                tamaño_del_inmueble <= Config.MAX_PROPERTY_SIZE)
            {
                return tamaño_del_inmueble;
            }
            return null;
        }

        private double? GetLandSize()
        {
            if (tamaño_de_la_parcela.HasValue &&
                tamaño_de_la_parcela >= Config.MIN_LAND_SIZE &&
                tamaño_de_la_parcela <= Config.MAX_LAND_SIZE)
            {
                return tamaño_de_la_parcela;
            }
            return null;
        }

        private int? GetConstrunctionYear()
        {
            var constructionYear = (int?)año_de_construcción;
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
            if (string.IsNullOrEmpty(estado_de_la_construcción))
            {
                return null;
            }
            return estado_de_la_construcción switch
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
            var floors = (int?)plantas_del_edificio;
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
            if (string.IsNullOrEmpty(plantas_del_inmueble))
            {
                return null;
            }
            return Strings.Clean(plantas_del_inmueble);
        }

        private int? GetBedrooms()
        {
            var bedrooms = (int?)número_de_dormitorios;
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
            var bathrooms = (int?)número_de_baños;
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
            var parkings = (int?)número_de_parkings;
            if (parkings.HasValue &&
                parkings >= Config.MIN_PARKINGS &&
                parkings <= Config.MAX_PARKINGS)
            {
                return parkings;
            }
            return null;
        }

        protected static string GetDescriptionAttribute(string name)
        {
            PropertyInfo? propertyInfo = typeof(ParseListingTool).GetProperty(name);
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
