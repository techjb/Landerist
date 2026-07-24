using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_library.Parse.Location
{
    public class LauIdParser(CountryCode countryCode, Listing listing)
    {
        private readonly CountryCode _countryCode = countryCode;
        private readonly Listing _listing = listing;

        public void SetLauIdAndLauName()
        {
            switch (_countryCode)
            {
                // More precise map
                case CountryCode.ES:
                {
                    var natCodeAndNameUnit = Delimitations.CNIGParser.GetNatCodeAndNameUnit(_listing);
                    if (natCodeAndNameUnit != null)
                    {
                        _listing.lauId = natCodeAndNameUnit.Value.natCode;
                        _listing.lauName = natCodeAndNameUnit.Value.nameUnit;
                    }
                    break;
                }

                default:
                {
                    var lauIdAndLauName = Delimitations.LAUParser.GetLauIdAndLauName(_listing);
                    if (lauIdAndLauName != null)
                    {
                        _listing.lauId = lauIdAndLauName.Value.lauId;
                        _listing.lauName = lauIdAndLauName.Value.lauName;
                    }
                    break;
                }
            }
        }

    }
}
