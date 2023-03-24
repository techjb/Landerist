using Newtonsoft.Json;
using System.Reflection;

namespace landerist_orels
{
    public enum ListingStatus
    {
        published,
        unpublished
    }

    public enum Operation
    {
        buy,
        sell,
        rent,
        holiday_rent,
        share,
        auction,
        transfer
    }

    public enum PropertyType
    {
        home,
        room,
        premise,
        industrial,
        garage,
        storage,
        office,
        land,
        building
    }

    public enum PropertySubtype
    {
        flat,
        appartment,
        penthouse,
        bungalow,
        duplex,
        detached,
        semi_detached,
        terraced,
        developed,
        buildable,
        non_building
    }
    
    public enum ConstructionStatus
    {
        @new,
        good,
        for_renovation,
        refurbished
    }

    public enum Feature
    {
        terrace,
        garden,
        garage,
        motorbike_garage,
        pool,
        lift,
        disabled_access,
        storage_room,
        furnished,
        non_furnished,
        heating,
        air_conditioning,
        pets_allowed,
        security_systems
    }


    public class Listing
    {
        [JsonProperty(Order = 1)]
        public string guid { get; set; }

        [JsonProperty(Order = 2)]
        public ListingStatus listingStatus { get; set; }

        [JsonProperty(Order = 3)]
        public DateTime? listingDate { get; set; }

        [JsonProperty(Order = 4)]
        public DateTime? unlistingDate { get; set; }

        [JsonProperty(Order = 5)]
        public Operation operation { get; set; }

        [JsonProperty(Order = 6)]
        public PropertyType propertyType { get; set; }

        [JsonProperty(Order = 7)]
        public PropertySubtype? propertySubType { get; set; }

        [JsonProperty(Order = 8)]
        public List<Media>? media { get; set; }

        [JsonProperty(Order = 9)]
        public Price? price { get; set; }

        [JsonProperty(Order = 10)]
        public string? description { get; set; }

        [JsonProperty(Order = 11)]
        public string? dataSourceName { get; set; }

        [JsonProperty(Order = 12)]
        public string? dataSourceGuid { get; set; }

        [JsonProperty(Order = 13)]
        public DateTime? dataSourceUpdate { get; set; }

        [JsonProperty(Order = 14)]
        public Uri? dataSourceUrl { get; set; }

        [JsonProperty(Order = 15)]
        public string? contactName { get; set; }

        [JsonProperty(Order = 16)]
        public string? contactPhone { get; set; }

        [JsonProperty(Order = 17)]
        public string? contactEmail { get; set; }

        [JsonProperty(Order = 18)]
        public Uri? contactUrl { get; set; }

        [JsonProperty(Order = 19)]
        public string? contactOther { get; set; }

        [JsonProperty(Order = 20)]
        public string? address { get; set; }

        [JsonProperty(Order = 21)]
        public double? latitude { get; set; }

        [JsonProperty(Order = 22)]
        public double? longitude { get; set; }

        [JsonProperty(Order = 23)]
        public bool? locationIsAccurate { get; set; }

        [JsonProperty(Order = 24)]
        public string? cadastralReference { get; set; }

        [JsonProperty(Order = 25)]
        public double? propertySize { get; set; }

        [JsonProperty(Order = 26)]
        public double? landSize { get; set; }

        [JsonProperty(Order = 27)]
        public int? constructionYear { get; set; }

        [JsonProperty(Order = 28)]
        public ConstructionStatus? constructionStatus { get; set; }

        [JsonProperty(Order = 29)]
        public int? floors { get; set; }

        [JsonProperty(Order = 30)]
        public string? floor { get; set; }

        [JsonProperty(Order = 31)]
        public int? bedrooms { get; set; }

        [JsonProperty(Order = 32)]
        public int? bathrooms { get; set; }

        [JsonProperty(Order = 33)]
        public int? parkings { get; set; }

        [JsonProperty(Order = 34)]

        public List<Feature>? features;

        public void AddMedia(Media media)
        {
            if (this.media == null)
            {
                this.media = new();
            }
            this.media.Add(media);
        }

        public void AddFeature(bool? value, Feature feature)
        {
            if (value != null && (bool)value)
            {
                if (features == null)
                {
                    features = new();
                }
                features.Add(feature);
            }
        }
    }
}
