using landerist_library.Websites;

namespace landerist_library.Parse.Location
{
    public class LauIdParser
    {
        private readonly Page Page;

        private readonly landerist_orels.ES.Listing Listing;

        public LauIdParser(Page page, landerist_orels.ES.Listing listing)
        {
            Page = page;
            Listing = listing;
        }

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
