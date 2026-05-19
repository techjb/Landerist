namespace landerist_library.Websites
{
    // https://www.w3schools.com/tags/ref_country_codes.asp
    public enum CountryCode
    {
        ES
    }

    // https://www.w3schools.com/tags/ref_language_codes.asp
    public enum LanguageCode
    {
        es
    }

    public class Country
    {
        public CountryCode CountryCode { get; set; }

        public LanguageCode LanguageCode { get; set; }

        public string CountryName { get; set; } = string.Empty;
    }

    public static class Countries
    {
        public static readonly IReadOnlyList<Country> All =
        [
            new()
            {
                CountryCode = CountryCode.ES,
                LanguageCode = LanguageCode.es,
                CountryName = "Spain"
            }
        ];
    }
}
