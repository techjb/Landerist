using landerist_library.Infrastructure.Sql;
using landerist_orels.ES;

namespace landerist_library.Pages
{
    public partial class Pages
    {
        private static readonly PageStatisticsRepository StatisticsRepository = new(global::landerist_library.Database.LegacyDatabase.Create());

        public static Dictionary<string, object?> GroupByPageType(ListingStatus? listingStatus = null)
        {
            return StatisticsRepository.GroupByPageType(listingStatus);
        }

        public static Dictionary<string, object?> GroupByHttpStatusCode(ListingStatus? listingStatus = null)
        {
            return StatisticsRepository.GroupByHttpStatusCode(listingStatus);
        }

        public static Dictionary<string, object?> GroupByNextScrape()
        {
            return StatisticsRepository.GroupByNextScrape();
        }

        public static Dictionary<string, object?> CountByHttpStatusCode()
        {
            return StatisticsRepository.CountByHttpStatusCode();
        }
    }
}
