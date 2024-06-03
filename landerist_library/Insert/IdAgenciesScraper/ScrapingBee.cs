using landerist_library.Configuration;
using System.Web;

namespace landerist_library.Insert.AgenciesScraper
{
    public class ScrapingBee
    {
        public static string DownloadString(string url, bool allowAutoRedirect)
        {
            var apiEndPoint = GetApiEndPointUrl(url, allowAutoRedirect);
            HttpClient httpClient = new();
            var response = httpClient.GetAsync(apiEndPoint).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        private static string GetApiEndPointUrl(string url, bool allowAutoRedirect)
        {
            url = HttpUtility.UrlEncode(url);

            return "https://app.scrapingbee.com/api/v1/?" +
                "api_key=" + PrivateConfig.SCRAPPINGBEE_APIKEY +
                "&url=" + url +
                "&render_js=false" +// true => 5 credits,  false => 1 credit.
                "&premium_proxy=true"  // 10-25 credits

                //"&stealth_proxy=false"  // 75 credits
                //"&block_resources=false"
                ;
        }
    }
}
