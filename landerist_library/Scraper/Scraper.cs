using landerist_library.Websites;
using System.Net;

namespace landerist_library.Scraper
{
    public class Scraper
    {
        public static void SetRealUri()
        {
            var websites = Websites.Websites.GetAll();
            int total = websites.Count;
            int counter = 0;
            int erros = 0;
            var sync = new object();
            Parallel.ForEach(websites,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1}, 
                website =>
            {
                lock (sync)
                {
                    counter++;
                }
                double progressPercentage = Math.Round((double)counter * 100 / total, 2);
                Console.WriteLine(counter + "/" + total + " (" + progressPercentage + "%) " +
                    "Errors: " + erros + " " + website.Uri.ToString());
                try
                {
                    SetRealUri(website);
                }
                catch
                {
                    erros++;
                }

            });
        }

        private static void SetRealUri(Website website)
        {
            using var client = new HttpClient();
            var response = client.GetAsync(website.Uri).GetAwaiter().GetResult();

            if (response.StatusCode.Equals(HttpStatusCode.PermanentRedirect)
                || response.StatusCode.Equals(HttpStatusCode.TemporaryRedirect)
                || response.StatusCode.Equals(HttpStatusCode.Moved)
                || response.StatusCode.Equals(HttpStatusCode.MovedPermanently)
                )
            {
                if (response.Headers.Location != null)
                {
                    website.UpdateUri(response.Headers.Location);
                }
            }
        }

        public static void UpdateRobotsAndIpAddress()
        {
            var websites = Websites.Websites.GetAll();
            int total = websites.Count;
            int counter = 0;
            var sync = new object();
            Parallel.ForEach(websites, website =>
            {
                lock (sync)
                {
                    counter++;
                }
                double progressPercentage = Math.Round((double)counter * 100 / total, 2);
                Console.WriteLine(counter + "/" + total + " (" + progressPercentage + "%) " + website.Uri.ToString());
                _ = UpdateRobotsAndIpAddressAsync(website);
            });
        }

        private static async Task UpdateRobotsAndIpAddressAsync(Website website)
        {
            string ipAddress = GetIpAddress(website);
            if (!ipAddress.Equals(string.Empty))
            {
                website.UpdateIpAddress(ipAddress);
            }

            string robotsTxt = GetRobotsTxtAsync(website);
            if (!robotsTxt.Equals(string.Empty))
            {
                website.UpdateRobotsTxt(robotsTxt);
            }
        }

        private static string GetIpAddress(Website website)
        {
            IPAddress[] ipAddresses = Dns.GetHostAddresses(website.Domain);
            if (ipAddresses.Length != 1)
            {
                return string.Empty;
            }
            return ipAddresses[0].ToString();
        }

        public static string GetRobotsTxtAsync(Website website)
        {
            var httpClient = new HttpClient();
            var robotsTxtUrl = new Uri(website.Uri, "/robots.txt");
            var response = httpClient.GetAsync(robotsTxtUrl).GetAwaiter().GetResult();

            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
