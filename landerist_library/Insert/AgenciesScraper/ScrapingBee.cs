using landerist_library.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace landerist_library.Insert.AgenciesScraper
{
    public class ScrapingBee
    {
        //public string DownloadString(string url)
        //{
        //    var apiEndPoint = GetApiEndPointUrl(url);
        //    HttpClient httpClient = new();
        //    var response = httpClient.GetAsync(apiEndPoint).Result;
        //    return response.Content.ReadAsStringAsync().Result;
        //}

        public static string GetApiEndPointUrl(string url, string encodedPostData)
        {
            url = HttpUtility.UrlEncode(url);

            return "https://app.scrapingbee.com/api/v1/?" +
                "api_key=" + PrivateConfig.SCRAPPINGBEE_APIKEY +
                "&url=" + url +
                //"&forward_headers=true" +
                //"&forward_body=true" +
                //"&body=" + encodedPostData
                //"&forward_headers=true" +

                "&render_js=false" +// true => 5 credits,  false => 1 credit.
                "&premium_proxy=true" // 10-25 credits
                //"&stealth_proxy=false"  // 75 credits
                //"&block_resources=false"
                ;
        }

        //public void DownloadFile(string url, string path)
        //{
        //    var apiEndPoing = GetApiEndPointUrl(url);
        //    WebClient webClient = new();
        //    webClient.DownloadFile(apiEndPoing, path);
        //}
    }
}
