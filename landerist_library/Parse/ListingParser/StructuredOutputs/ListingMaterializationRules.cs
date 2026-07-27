namespace landerist_library.Parse.ListingParser.StructuredOutputs;

public sealed record ListingMaterializationRules(
    int MaxPublishedAgeYears,
    double MinPropertySize,
    double MaxPropertySize,
    double MinLandSize,
    double MaxLandSize,
    int MinConstructionYear,
    int MaxConstructionYearsFromNow,
    int MinFloors,
    int MaxFloors,
    int MinBedrooms,
    int MaxBedrooms,
    int MinBathrooms,
    int MaxBathrooms,
    int MinParkings,
    int MaxParkings,
    bool MediaEnabled)
{
    public static ListingMaterializationRules Default { get; } = new(
        MaxPublishedAgeYears: 5,
        MinPropertySize: 1,
        MaxPropertySize: 100_000,
        MinLandSize: 10,
        MaxLandSize: 10_000_000,
        MinConstructionYear: 1800,
        MaxConstructionYearsFromNow: 5,
        MinFloors: 0,
        MaxFloors: 500,
        MinBedrooms: 0,
        MaxBedrooms: 50,
        MinBathrooms: 0,
        MaxBathrooms: 20,
        MinParkings: 0,
        MaxParkings: 10_000,
        MediaEnabled: true);
}
