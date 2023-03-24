using Newtonsoft.Json;

namespace landerist_orels
{
    public enum Currency
    {
        EUR
    }
    public class Price
    {
        [JsonProperty(Order = 1)]
        public decimal amount { get; set; }

        [JsonProperty(Order = 2)]
        public Currency currency { get; set; } = Currency.EUR;
    }
}