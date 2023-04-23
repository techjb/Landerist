using Newtonsoft.Json;

namespace landerist_orels.ES
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

        public Price()
        {

        }

        public Price(decimal amount, Currency currency)
        {
            this.amount = amount;
            this.currency = currency;
        }
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }
            return Equals(obj as Price);
        }

        public bool Equals(Price other)
        {
            if (other == null)
            {
                return false;
            }
            return amount == other.amount && currency == other.currency;
        }

        public override int GetHashCode()
        {
            int hashCode = 289573841;
            hashCode = hashCode * -1521134295 + amount.GetHashCode();
            hashCode = hashCode * -1521134295 + currency.GetHashCode();
            return hashCode;
        }
    }
}