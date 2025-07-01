namespace landerist_library.Parse.Location.GoogleMaps
{
#pragma warning disable IDE1006
    public class GeocodeData
    {
        public Result[]? results { get; set; }
    }

    public class Result
    {
        public Geometry? geometry { get; set; }

        public Building[]? buildings { get; set; }

        public string[]? types { get; set; }
    }

    public class Geometry
    {
        public Location? location { get; set; }

        public string? location_type { get; set; }
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
