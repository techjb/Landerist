using landerist_library.Configuration;
using landerist_library.Parse.Media;
using landerist_library.Websites;
using landerist_orels.ES;
using landerist_orels;
using static landerist_library.Parse.ListingParser.StructuredOutputs.StructuredOutputEsListing;

namespace landerist_library.Parse.ListingParser.StructuredOutputs
{
    public class StructuredOutputEsParser(StructuredOutputEs structuredOutputEs)
    {
        public bool EsUnAnuncio = structuredOutputEs.EsUnAnuncio;

        public StructuredOutputEsListing? Anuncio = structuredOutputEs.Anuncio;

        public (PageType pageType, Listing? listing) Parse(Page page)
        {
            if (!EsUnAnuncio || Anuncio == null)
            {
                return (PageType.NotListingByParser, null);
            }

            try
            {
                //if (!IsValidResponse())
                //{
                //    return (PageType.MayBeListing, null);
                //}

                var listing = new Listing
                {
                    guid = page.UriHash,
                    listingStatus = ListingStatus.published, // todo: as ai for listing status
                    listingDate = GetListingDate(),
                    operation = GetOperation(),
                    propertyType = GetPropertyType(),
                    propertySubtype = GetPropertySubtype(),
                    price = GetPropertyPrice(),
                    description = GetDescription(),
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
                    securitySystems = Anuncio.TieneSistemasDeSeguridad,
                };

                SetMedia(listing, page);
                SetSource(listing, page);
                return (PageType.Listing, listing);
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("OpenAIStructuredOutput ParseListing", page.Uri, exception);
            }
            return (PageType.MayBeListing, null);
        }

        //private bool IsValidResponse()
        //{
        //    return
        //        IsValidListingDate() &&
        //        IsValidDescription()
        //    ;
        //}

        //private bool IsValidListingDate()
        //{
        //    var listingDate = GetListingDate();
        //    var maxListingDate = DateTime.Now.AddDays(1);
        //    var minListingDate = DateTime.Now.AddYears(-Config.MAX_YEARS_SINCE_PUBLISHED_LISTING);

        //    return listingDate <= maxListingDate && listingDate >= minListingDate;
        //}

        private DateTime GetListingDate()
        {
            var now = DateTime.Now;
            if (DateTime.TryParse(Anuncio?.FechaDePublicación, out DateTime listingDateParsed))
            {
                var maxListingDate = now.AddDays(1);
                var minListingDate = now.AddYears(-Config.MAX_YEARS_SINCE_PUBLISHED_LISTING);
                if (listingDateParsed >= minListingDate && listingDateParsed <= maxListingDate)
                {
                    return listingDateParsed;
                }
            }
            return now;
        }

        //private bool IsValidDescription()
        //{
        //    var description = GetDescription();
        //    return !string.IsNullOrEmpty(description);
        //}

        private string GetDescription()
        {
            if (Anuncio == null || string.IsNullOrEmpty(Anuncio.DescripciónDelAnuncio))
            {
                return string.Empty;
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

        private static string GetSourceName(Page page)
        {
            return page.Website.Host;
        }

        private string? GetSourceGuid()
        {
            if (string.IsNullOrEmpty(Anuncio!.ReferenciaDelAnuncio))
            {
                return null;
            }
            return Tools.Strings.Clean(Anuncio!.ReferenciaDelAnuncio);
        }

        private static Uri GetSourceUrl(Page page)
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
            char[] trimChars = { '*', '-', ' ', ',', '.' };
            return Tools.Strings.Clean(Anuncio!.DirecciónDelInmueble).Trim(trimChars);
        }

        private string? GetCadastralReference()
        {
            if (string.IsNullOrEmpty(Anuncio!.ReferenciaDelAnuncio))
            {
                return null;
            }
            var referenciaCatastral = Tools.Strings.Clean(Anuncio!.ReferenciaDelAnuncio).ToUpper();
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

        private void SetMedia(Listing listing, Page page)
        {
            if (Anuncio!.ImagenesDelAnuncio is null ||
                Anuncio!.ImagenesDelAnuncio.Count.Equals(0) ||
                !Config.MEDIA_PARSER_ENABLED)
            {
                return;
            }
            MediaParser mediaParser = new(page);
            var list = Anuncio!.ImagenesDelAnuncio?.Select(img => (img.Url, img.Titulo)).ToList();
            mediaParser.AddMediaImages(listing, list);
        }

        private void SetSource(Listing listing, Page page)
        {
            var source = new Source
            {
                sourceGuid = GetSourceGuid(),
                sourceUrl = GetSourceUrl(page),
                sourceName = GetSourceName(page),
            };
            listing.sources.Add(source);
        }
    }
}
