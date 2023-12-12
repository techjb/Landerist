using landerist_library.Websites;

namespace landerist_library.Parse.Location
{
    public class LauIdParser(Page page, landerist_orels.ES.Listing listing)
    {
        private readonly Page Page = page;

        private readonly landerist_orels.ES.Listing Listing = listing;

        public void SetLauId()
        {
            switch (Page.Website.CountryCode)
            {
                // More precise map
                case CountryCode.ES:
                    {
                        Listing.lauId = Delimitations.CNIGParser.GetNatCode(Listing);
                    }
                    break;
                default:
                    {
                        Listing.lauId = Delimitations.LAUParser.GetId(Listing);
                    }
                    break;
            }
        }
    }
}
