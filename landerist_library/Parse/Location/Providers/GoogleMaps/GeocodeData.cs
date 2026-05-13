namespace landerist_library.Parse.Location.Providers.GoogleMaps
{
#pragma warning disable IDE1006
    public class GeocodeData
    {
        public Result[]? results { get; set; }

        public string? status { get; set; }

        public string? error_message { get; set; }
    }

    public class Result
    {
        public AddressComponent[]? address_components { get; set; }

        public string? formatted_address { get; set; }

        public Geometry? geometry { get; set; }

        public Building[]? buildings { get; set; }

        public bool? partial_match { get; set; }

        public string? place_id { get; set; }

        public string[]? types { get; set; }
    }

    public class Geometry
    {
        public Bounds? bounds { get; set; }

        public Location? location { get; set; }

        public string? location_type { get; set; }

        public Bounds? viewport { get; set; }
    }

    public class AddressComponent
    {
        public string? long_name { get; set; }

        public string? short_name { get; set; }

        public string[]? types { get; set; }
    }

    public class Bounds
    {
        public Location? northeast { get; set; }

        public Location? southwest { get; set; }
    }

    public class Building
    {
        public string? place_id { get; set; }

        public BuildingOutline[]? building_outlines { get; set; }
    }

    public class BuildingOutline
    {
        public DisplayPolygon? display_polygon { get; set; }
    }

    public class DisplayPolygon
    {
        public object? coordinates { get; set; }

        public string? type { get; set; }
    }

    public class Location
    {
        public double? lat { get; set; }
        public double? lng { get; set; }
    }

#pragma warning restore IDE1006
}

