using landerist_library.Websites;
using landerist_orels.ES;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace landerist_library.Parse
{
    public class ListingParser
    {
        private readonly Page Page;

        private bool? IsListing = null;

        private Listing? Listing = null;

        private string ResponseBodyText = string.Empty;

        public ListingParser(Page page)
        {
            Page = page;
        }

        public Tuple<bool?, Listing?> GetListing()
        {
            SetResponseBodyText();
            if (ListingIsAllowed())
            {
                RequestListing();
            }
            return Tuple.Create(IsListing, Listing);
        }

        public bool ListingIsAllowed()
        {
            return
                !ResponseBodyText.Equals(string.Empty) &&
                ChatGPT.IsRequestAllowed(ResponseBodyText)
                //Page.LanguageIs("es") // permitimos cualquier lenguage.
                ;
        }

        private void SetResponseBodyText()
        {
            Page.LoadHtmlDocument();
            if (Page.HtmlDocument != null)
            {
                ResponseBodyText = new HtmlToText(Page.HtmlDocument).GetText();
            }
        }

        private void RequestListing()
        {
            var result = new ChatGPT().GetResponse(ResponseBodyText);
            if (result != null)
            {
                ParseListing(result);
            }
        }

        private void ParseListing(string json)
        {
            try
            {
                ListingResponse? listingResponse = JsonConvert.DeserializeObject<ListingResponse>(json);
                if (listingResponse != null)
                {
                    Listing = listingResponse.ToListing(Page);
                    if (Listing != null)
                    {
                        IsListing = true;
                    }
                }
            }
            catch
            {

            }
        }
    }
}
