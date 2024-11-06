
using landerist_library.Configuration;
using landerist_library.Websites;
using landerist_orels.ES;
using Newtonsoft.Json;
using static landerist_library.Parse.Listing.OpenAI.Anuncio;

namespace landerist_library.Parse.Listing.OpenAI
{
    public class OpenAIStructuredOutput
    {
        [JsonProperty("es_un_anuncio")]
        [System.ComponentModel.Description("La página web corresponde a un único anuncio inmobiliario")]
        public bool EsUnAnuncio { get; private set; }


        [JsonProperty("anuncio", Required = Required.AllowNull)]
        [System.ComponentModel.Description("datos del anuncio, null en caso de no ser un anuncio")]

        public Anuncio? Anuncio { get; private set; }


        public (PageType pageType, landerist_orels.ES.Listing? listing) ParseListing(Page page)
        {
            if (!EsUnAnuncio)
            {
                return (PageType.NotListingByParser, null);
            }

            if (Anuncio == null)
            {
                return (PageType.MayBeListing, null);
            }
            try
            {
                if (!IsValidResponse())
                {
                    return (PageType.ListingButNotParsed, null);
                }

                var listing = new landerist_orels.ES.Listing
                {
                    guid = page.UriHash,
                    listingStatus = ListingStatus.published,
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
                    terrace = Anuncio.TieneTerraza,
                    garden = Anuncio.TieneJardín,
                    garage = Anuncio.TieneGaraje,
                    motorbikeGarage = Anuncio.TieneParkingParaMoto,
                    pool = Anuncio.TienePiscina,
                    lift = Anuncio.TieneAscensor,
                    disabledAccess = Anuncio.TieneAccesoParaDiscapacitados,
                    storageRoom = Anuncio.TieneTrastero,
                    furnished = Anuncio.EstaAmueblado,
                    nonFurnished = Anuncio.NoEstaAmueblado,
                    heating = Anuncio.TieneCalefacción,
                    airConditioning = Anuncio.TieneAireAcondicionado,
                    petsAllowed = Anuncio.PermiteMascotas,
                    securitySystems = Anuncio.TieneSistemasDeSeguridad
                };

                return (PageType.Listing, listing);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("OpenAIStructuredOutput ParseListing", page.Uri, exception);
            }
            return (PageType.MayBeListing, null);
        }

        private bool IsValidResponse()
        {
            return IsValidListingDate() &&
                IsValidDescription()
            ;
        }

        private bool IsValidListingDate()
        {
            var listingDate = GetListingDate();
            var maxListingDate = DateTime.Now.AddDays(1);
            var minListingDate = DateTime.Now.AddYears(-Config.MAX_YEARS_SINCE_PUBLISHED_LISTING);

            return listingDate <= maxListingDate && listingDate >= minListingDate;
        }

        private DateTime GetListingDate()
        {
            if (Anuncio != null && Anuncio.FechaDePublicación != null)
            {
                if (DateTime.TryParse(Anuncio.FechaDePublicación, out DateTime listingDate))
                {
                    return listingDate;
                }
            }
            return DateTime.Now;
        }

        private bool IsValidDescription()
        {
            var description = GetDescription();
            return !string.IsNullOrEmpty(description);
        }

        private string? GetDescription()
        {
            if (Anuncio == null || string.IsNullOrEmpty(Anuncio.DescripciónDelAnuncio))
            {
                return null;
            }
            return Tools.Strings.Clean(Anuncio.DescripciónDelAnuncio);
        }

        private Operation GetOperation()
        {
            return Anuncio!.TipoDeOperación switch
            {
                TiposDeOperacion.venta => Operation.sell,
                TiposDeOperacion.alquiler => Operation.rent,
                _ => Operation.sell,
            };
        }

        private PropertyType GetPropertyType()
        {
            return Anuncio!.TipoDeInmueble switch
            {
                TiposDeInmueble.vivienda => PropertyType.home,
                TiposDeInmueble.dormitorio => PropertyType.room,
                TiposDeInmueble.local_comercial => PropertyType.premise,
                TiposDeInmueble.nave_industrial => PropertyType.industrial,
                TiposDeInmueble.garaje => PropertyType.garage,
                TiposDeInmueble.trastero => PropertyType.storage,
                TiposDeInmueble.oficina => PropertyType.office,
                TiposDeInmueble.parcela => PropertyType.land,
                TiposDeInmueble.edificio => PropertyType.building,
                _ => PropertyType.home,
            };
        }

        private PropertySubtype? GetPropertySubtype()
        {
            if (Anuncio!.SubtipoDeInmueble == null)
            {
                return null;
            }

            return Anuncio!.SubtipoDeInmueble switch
            {
                SubtiposDeInmueble.piso => (PropertySubtype?)PropertySubtype.flat,
                SubtiposDeInmueble.apartamento => (PropertySubtype?)PropertySubtype.appartment,
                SubtiposDeInmueble.ático => (PropertySubtype?)PropertySubtype.penthouse,
                SubtiposDeInmueble.bungalow => (PropertySubtype?)PropertySubtype.bungalow,
                SubtiposDeInmueble.duplex => (PropertySubtype?)PropertySubtype.duplex,
                SubtiposDeInmueble.chalet_independiente => (PropertySubtype?)PropertySubtype.detached,
                SubtiposDeInmueble.chalet_pareado => (PropertySubtype?)PropertySubtype.semi_detached,
                SubtiposDeInmueble.chalet_adosado => (PropertySubtype?)PropertySubtype.terraced,
                SubtiposDeInmueble.parcela_urbana => (PropertySubtype?)PropertySubtype.developed,
                SubtiposDeInmueble.parcela_urbanizable => (PropertySubtype?)PropertySubtype.buildable,
                SubtiposDeInmueble.parcela_no_urbanizable => (PropertySubtype?)PropertySubtype.non_building,
                _ => null,
            };
        }

        private Price? GetPropertyPrice()
        {
            if (Anuncio!.PrecioDelAnuncio.HasValue)
            {
                var price = (decimal)Anuncio!.PrecioDelAnuncio;
                if (price > 0)
                {
                    return new Price(price, Currency.EUR);
                }
            }
            return null;
        }

        private static string GetDataSourceName(Page page)
        {
            return page.Website.Host;
        }

        private string? GetDataSourceGuid()
        {
            if (string.IsNullOrEmpty(Anuncio!.ReferenciaDelAnuncio))
            {
                return null;
            }
            return Tools.Strings.Clean(Anuncio!.ReferenciaDelAnuncio);
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
            if (string.IsNullOrEmpty(Anuncio!.TeléfonoDeContacto))
            {
                return null;
            }
            var telefonoDeContacto = Tools.Strings.Clean(Anuncio!.TeléfonoDeContacto);
            if (!Tools.Validate.Phone(telefonoDeContacto))
            {
                return null;
            }
            return telefonoDeContacto;
        }

        private string? GetEmail()
        {
            if (string.IsNullOrEmpty(Anuncio!.EmailDeContacto))
            {
                return null;
            }
            var emailDeContacto = Tools.Strings.Clean(Anuncio!.EmailDeContacto);
            emailDeContacto = Tools.Strings.RemoveSpaces(emailDeContacto);
            if (!Tools.Validate.Email(emailDeContacto))
            {
                return null;
            }
            return emailDeContacto;
        }

        private string? GetAddress()
        {
            if (string.IsNullOrEmpty(Anuncio!.DirecciónDelInmueble))
            {
                return null;
            }
            return Tools.Strings.Clean(Anuncio!.DirecciónDelInmueble);
        }

        private string? GetCadastralReference()
        {
            if (string.IsNullOrEmpty(Anuncio!.ReferenciaDelAnuncio))
            {
                return null;
            }
            var referenciaCatastral = Tools.Strings.Clean(Anuncio!.ReferenciaDelAnuncio);
            if (!Tools.Validate.CadastralReference(referenciaCatastral))
            {
                return null;
            }
            return referenciaCatastral;
        }

        private double? GetPropertySize()
        {
            if (Anuncio!.TamañoDelInmueble.HasValue &&
                Anuncio!.TamañoDelInmueble >= Config.MIN_PROPERTY_SIZE &&
                Anuncio!.TamañoDelInmueble <= Config.MAX_PROPERTY_SIZE)
            {
                return Anuncio!.TamañoDelInmueble;
            }
            return null;
        }

        private double? GetLandSize()
        {
            if (Anuncio!.TamañoDeLaParcela.HasValue &&
                Anuncio!.TamañoDeLaParcela >= Config.MIN_LAND_SIZE &&
                Anuncio!.TamañoDeLaParcela <= Config.MAX_LAND_SIZE)
            {
                return Anuncio!.TamañoDeLaParcela;
            }
            return null;
        }

        private int? GetConstrunctionYear()
        {
            var constructionYear = Anuncio!.AñoDeConstrucción;
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
            if (Anuncio!.EstadoDeLaConstrucción == null)
            {
                return null;
            }

            return Anuncio!.EstadoDeLaConstrucción switch
            {
                EstadosDeLaConstrucción.obra_nueva => (ConstructionStatus?)ConstructionStatus.@new,
                EstadosDeLaConstrucción.buen_estado => (ConstructionStatus?)ConstructionStatus.good,
                EstadosDeLaConstrucción.a_reformar => (ConstructionStatus?)ConstructionStatus.for_renovation,
                EstadosDeLaConstrucción.en_ruinas => (ConstructionStatus?)ConstructionStatus.refurbished,
                _ => null,
            };
        }

        private int? GetFloors()
        {
            var floors = Anuncio!.PlantasDelEdificio;
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
            if (string.IsNullOrEmpty(Anuncio!.PlantaDelInmueble))
            {
                return null;
            }
            return Tools.Strings.Clean(Anuncio!.PlantaDelInmueble);
        }

        private int? GetBedrooms()
        {
            var bedrooms = Anuncio!.NúmeroDeDormitorios;
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
            var bathrooms = Anuncio!.NúmeroDeBaños;
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
            var parkings = Anuncio!.NúmeroDeParkings;
            if (parkings.HasValue &&
                parkings >= Config.MIN_PARKINGS &&
                parkings <= Config.MAX_PARKINGS)
            {
                return parkings;
            }
            return null;
        }







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




        [JsonProperty(nameof(ParseListingTool.fecha_de_publicacion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.FechaDePublicaciónDescription)]
        public string? FechaDePublicación { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tipo_de_operacion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TipoDeOperaciónDescription)]
        public TiposDeOperacion? TipoDeOperación { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tipo_de_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TipoDeInmuebleDescription)]
        public TiposDeInmueble? TipoDeInmueble { get; private set; }


        [JsonProperty(nameof(ParseListingTool.subtipo_de_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.SubtipoDeInmuebleDescription)]
        public SubtiposDeInmueble? SubtipoDeInmueble { get; private set; }


        [JsonProperty(nameof(ParseListingTool.precio_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.PrecioDelAnuncioDescription)]
        public decimal? PrecioDelAnuncio { get; private set; }


        [JsonProperty(nameof(ParseListingTool.descripcion_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.DescripciónDelAnuncioDescription)]
        public string? DescripciónDelAnuncio { get; private set; }


        [JsonProperty(nameof(ParseListingTool.referencia_del_anuncio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.ReferenciaDelAnuncioDescription)]
        public string? ReferenciaDelAnuncio { get; private set; }


        [JsonProperty(nameof(ParseListingTool.telefono_de_contacto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TeléfonoDeContactoDescription)]
        public string? TeléfonoDeContacto { get; private set; }


        [JsonProperty(nameof(ParseListingTool.email_de_contacto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.EmailDeContactoDescription)]
        public string? EmailDeContacto { get; private set; }


        [JsonProperty(nameof(ParseListingTool.direccion_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.DirecciónDelInmuebleDescription)]
        public string? DirecciónDelInmueble { get; private set; }


        [JsonProperty(nameof(ParseListingTool.referencia_catastral), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.ReferenciaCatastralDescription)]
        public string? ReferenciaCatastral { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tamanio_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TamañoDelInmuebleDescription)]
        public double? TamañoDelInmueble { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tamanio_de_la_parcela), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TamañoDeLaParcelaDescription)]
        public double? TamañoDeLaParcela { get; private set; }


        [JsonProperty(nameof(ParseListingTool.anio_de_construccion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.AñoDeConstrucciónDescription)]
        public int? AñoDeConstrucción { get; private set; }


        [JsonProperty(nameof(ParseListingTool.estado_de_la_construccion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.EstadoDeLaConstrucciónDescription)]
        public EstadosDeLaConstrucción? EstadoDeLaConstrucción { get; private set; }


        [JsonProperty(nameof(ParseListingTool.plantas_del_edificio), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.PlantasDelEdificioDescription)]
        public int? PlantasDelEdificio { get; private set; }


        [JsonProperty(nameof(ParseListingTool.planta_del_inmueble), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.PlantaDelInmuebleDescription)]
        public string? PlantaDelInmueble { get; private set; }



        [JsonProperty(nameof(ParseListingTool.numero_de_dormitorios), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.NúmeroDeDormitoriosDescription)]
        public int? NúmeroDeDormitorios { get; private set; }


        [JsonProperty(nameof(ParseListingTool.numero_de_banios), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.NúmeroDeBañosDescription)]
        public int? NúmeroDeBaños { get; private set; }


        [JsonProperty(nameof(ParseListingTool.numero_de_parkings), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.NúmeroDeParkingsDescription)]
        public int? NúmeroDeParkings { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_terraza), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneTerrazaDescription)]
        public bool? TieneTerraza { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_jardin), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneJardínDescription)]
        public bool? TieneJardín { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_garaje), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneGarajeDescription)]
        public bool? TieneGaraje { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_parking_para_moto), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneParkingParaMotoDescription)]
        public bool? TieneParkingParaMoto { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_piscina), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TienePiscinaDescription)]
        public bool? TienePiscina { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_ascensor), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneAscensorDescription)]
        public bool? TieneAscensor { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_acceso_para_discapacitados), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneAccesoParaDiscapacitadosDescription)]
        public bool? TieneAccesoParaDiscapacitados { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_trastero), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneTrasteroDescription)]
        public bool? TieneTrastero { get; private set; }


        [JsonProperty(nameof(ParseListingTool.esta_amueblado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.EstaAmuebladoDescription)]
        public bool? EstaAmueblado { get; private set; }


        [JsonProperty(nameof(ParseListingTool.no_esta_amueblado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.NoEstaAmuebladoDescription)]
        public bool? NoEstaAmueblado { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_calefaccion), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneCalefacciónDescription)]
        public bool? TieneCalefacción { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_aire_acondicionado), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneAireAcondicionadoDescription)]
        public bool? TieneAireAcondicionado { get; private set; }


        [JsonProperty(nameof(ParseListingTool.permite_mascotas), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.PermiteMascotasDescription)]
        public bool? PermiteMascotas { get; private set; }


        [JsonProperty(nameof(ParseListingTool.tiene_sistemas_de_seguridad), Required = Required.AllowNull)]
        [System.ComponentModel.Description(ParseListingTool.TieneSistemasDeSeguridadDescription)]
        public bool? TieneSistemasDeSeguridad { get; private set; }

    }
}
