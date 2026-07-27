using landerist_orels.ES;

namespace landerist_library.Pages;

public partial class Page
{
    public void SetLastScrape()
    {
        LastScrape = DateTime.Now;
    }

    public void SetLastParseListing()
    {
        LastParseListing = DateTime.Now;
    }

    public void SetNextScrape(ListingStatus? listingStatus)
    {
        DateTime calculationDate = LastScrape ?? Inserted;
        SetNextScrape(calculationDate, listingStatus);
    }

    public void SetNextScrapeFromNow(ListingStatus? listingStatus)
    {
        SetNextScrape(DateTime.Now, listingStatus);
    }

    private void SetNextScrape(DateTime calculationDate, ListingStatus? listingStatus)
    {
        NextScrape = PageNextScrapeCalculator.Calculate(this, calculationDate, listingStatus);
    }
}