using landerist_orels.ES;

namespace landerist_library.Application.Statistics;

public interface IPageStatisticsRepository
{
    Dictionary<string, object?> GroupByPageType(ListingStatus? listingStatus = null);

    Dictionary<string, object?> GroupByHttpStatusCode(ListingStatus? listingStatus = null);

    Dictionary<string, object?> GroupByNextScrape();

    Dictionary<string, object?> CountByHttpStatusCode();
}
