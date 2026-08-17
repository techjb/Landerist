using landerist_domain.Parsing.Materialization;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_library.Pages;
using landerist_orels;
using landerist_orels.ES;

namespace landerist_library.Infrastructure.Ai.StructuredOutputs;

internal sealed class StructuredOutputListingMapper(
    Anuncio announcement,
    ListingMaterializationRules rules,
    TimeProvider timeProvider,
    StructuredOutputMaterializationOperations operations)
{
    internal Listing Create(Page page) => new()
    {
        guid = page.UriHash,
        listingStatus = GetListingStatus(),
        listingDate = GetListingDate(),
        operation = GetOperation(),
        propertyType = GetPropertyType(),
        propertySubtype = GetPropertySubtype(),
        price = GetPropertyPrice(),
        description = GetDescription(),
        contactName = GetContactName(),
        contactPhone = GetContactPhone(),
        contactEmail = GetContactEmail(),
        address = GetAddress(),
        cadastralReference = GetCadastralReference(),
        propertySize = GetPropertySize(),
        landSize = GetLandSize(),
        constructionYear = GetConstructionYear(),
        constructionStatus = GetConstructionStatus(),
        energyEfficiencyRating = GetEnergyEfficiencyRating(),
        floors = GetFloors(),
        floor = GetFloor(),
        bedrooms = GetBedrooms(),
        bathrooms = GetBathrooms(),
        parkings = GetParkings(),
        terrace = announcement.TieneTerraza,
        garden = announcement.TieneJardín,
        garage = announcement.TieneGaraje,
        motorbikeGarage = announcement.TieneParkingParaMoto,
        pool = announcement.TienePiscina,
        lift = announcement.TieneAscensor,
        disabledAccess = announcement.TieneAccesoParaDiscapacitados,
        storageRoom = announcement.TieneTrastero,
        furnished = announcement.EstaAmueblado,
        nonFurnished = announcement.NoEstaAmueblado,
        heating = announcement.TieneCalefacción,
        airConditioning = announcement.TieneAireAcondicionado,
        petsAllowed = announcement.PermiteMascotas,
        securitySystems = announcement.TieneSistemasDeSeguridad
    };

    private ListingStatus GetListingStatus() =>
        announcement.EstadoDePublicación == EstadosDePublicación.despublicado
            ? ListingStatus.unpublished
            : ListingStatus.published;

    private DateTime GetListingDate()
    {
        DateTime now = timeProvider.GetLocalNow().DateTime;
        if (DateTime.TryParse(announcement.FechaDePublicación, out DateTime parsed) &&
            parsed >= now.AddYears(-rules.MaxPublishedAgeYears) &&
            parsed <= now.AddDays(1))
        {
            return parsed;
        }
        return now;
    }

    private string GetDescription() => string.IsNullOrEmpty(announcement.DescripciónDelAnuncio)
        ? string.Empty
        : operations.Clean(announcement.DescripciónDelAnuncio);

    private Operation GetOperation() => announcement.TipoDeOperación switch
    {
        TiposDeOperacion.alquiler => Operation.rent,
        _ => Operation.sell
    };

    private PropertyType GetPropertyType() => announcement.TipoDeInmueble switch
    {
        TiposDeInmueble.dormitorio => PropertyType.room,
        TiposDeInmueble.local_comercial => PropertyType.premise,
        TiposDeInmueble.nave_industrial => PropertyType.industrial,
        TiposDeInmueble.garaje => PropertyType.garage,
        TiposDeInmueble.trastero => PropertyType.storage,
        TiposDeInmueble.oficina => PropertyType.office,
        TiposDeInmueble.parcela => PropertyType.land,
        TiposDeInmueble.edificio => PropertyType.building,
        _ => PropertyType.home
    };

    private PropertySubtype? GetPropertySubtype() =>
        (GetPropertyType(), announcement.SubtipoDeInmueble) switch
        {
            (PropertyType.home, SubtiposDeInmueble.piso) => PropertySubtype.flat,
            (PropertyType.home, SubtiposDeInmueble.apartamento) => PropertySubtype.apartment,
            (PropertyType.home, SubtiposDeInmueble.ático) => PropertySubtype.penthouse,
            (PropertyType.home, SubtiposDeInmueble.bungalow) => PropertySubtype.bungalow,
            (PropertyType.home, SubtiposDeInmueble.duplex) => PropertySubtype.duplex,
            (PropertyType.home, SubtiposDeInmueble.chalet_independiente) => PropertySubtype.detached,
            (PropertyType.home, SubtiposDeInmueble.chalet_pareado) => PropertySubtype.semi_detached,
            (PropertyType.home, SubtiposDeInmueble.chalet_adosado) => PropertySubtype.terraced,
            (PropertyType.land, SubtiposDeInmueble.parcela_urbana) => PropertySubtype.developed,
            (PropertyType.land, SubtiposDeInmueble.parcela_urbanizable) => PropertySubtype.buildable,
            (PropertyType.land, SubtiposDeInmueble.parcela_no_urbanizable) => PropertySubtype.non_building,
            _ => null
        };

    private Price? GetPropertyPrice() => announcement.PrecioDelAnuncio is > 0
        ? new Price((decimal)announcement.PrecioDelAnuncio, Currency.EUR)
        : null;

    private string? GetContactName() => CleanOptional(announcement.NombreDeContacto);

    private string? GetContactPhone()
    {
        string? value = CleanOptional(announcement.TeléfonoDeContacto);
        if (value is null) return null;
        return operations.ValidatePhone(value) ? value : null;
    }

    private string? GetContactEmail()
    {
        string? value = CleanOptional(announcement.EmailDeContacto);
        if (value is null) return null;
        value = operations.RemoveSpaces(value);
        return operations.ValidateEmail(value) ? value : null;
    }

    private string? GetAddress()
    {
        string? value = CleanOptional(announcement.DirecciónDelInmueble);
        if (value is null) return null;
        value = value.Trim('*', '-', ' ', ',', '.');
        return value.Length == 0 ? null : value;
    }

    private string? GetCadastralReference()
    {
        string? value = CleanOptional(announcement.ReferenciaDelAnuncio)?.ToUpper();
        if (value is null) return null;
        return operations.ValidateCadastralReference(value) ? value : null;
    }

    private double? GetPropertySize() => InRange(
        announcement.TamañoDelInmueble,
        rules.MinPropertySize,
        rules.MaxPropertySize);

    private double? GetLandSize() => InRange(
        announcement.TamañoDeLaParcela,
        rules.MinLandSize,
        rules.MaxLandSize);

    private int? GetConstructionYear()
    {
        int? value = announcement.AñoDeConstrucción;
        int maximum = timeProvider.GetLocalNow().DateTime
            .AddYears(rules.MaxConstructionYearsFromNow).Year;
        return InRange(value, rules.MinConstructionYear, maximum);
    }

    private ConstructionStatus? GetConstructionStatus() =>
        announcement.EstadoDeLaConstrucción switch
        {
            EstadosDeLaConstrucción.obra_nueva => ConstructionStatus.@new,
            EstadosDeLaConstrucción.buen_estado => ConstructionStatus.good,
            EstadosDeLaConstrucción.a_reformar => ConstructionStatus.for_renovation,
            EstadosDeLaConstrucción.en_ruinas => ConstructionStatus.refurbished,
            _ => null
        };

    private EnergyEfficiencyRating? GetEnergyEfficiencyRating()
    {
        if (GetPropertyType() == PropertyType.land) return null;
        return announcement.CalificaciónEnergética switch
        {
            CalificacionesDeEficienciaEnergetica.A => EnergyEfficiencyRating.A,
            CalificacionesDeEficienciaEnergetica.B => EnergyEfficiencyRating.B,
            CalificacionesDeEficienciaEnergetica.C => EnergyEfficiencyRating.C,
            CalificacionesDeEficienciaEnergetica.D => EnergyEfficiencyRating.D,
            CalificacionesDeEficienciaEnergetica.E => EnergyEfficiencyRating.E,
            CalificacionesDeEficienciaEnergetica.F => EnergyEfficiencyRating.F,
            CalificacionesDeEficienciaEnergetica.G => EnergyEfficiencyRating.G,
            _ => null
        };
    }

    private int? GetFloors() => InRange(announcement.PlantasDelEdificio, rules.MinFloors, rules.MaxFloors);
    private string? GetFloor() => CleanOptional(announcement.PlantaDelInmueble);
    private int? GetBedrooms() => InRange(announcement.NúmeroDeDormitorios, rules.MinBedrooms, rules.MaxBedrooms);
    private int? GetBathrooms() => InRange(announcement.NúmeroDeBaños, rules.MinBathrooms, rules.MaxBathrooms);
    private int? GetParkings() => InRange(announcement.NúmeroDeParkings, rules.MinParkings, rules.MaxParkings);

    private string? CleanOptional(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        string cleaned = operations.Clean(value);
        return cleaned.Length == 0 ? null : cleaned;
    }

    private static double? InRange(double? value, double minimum, double maximum) =>
        value >= minimum && value <= maximum ? value : null;

    private static int? InRange(int? value, int minimum, int maximum) =>
        value >= minimum && value <= maximum ? value : null;
}
