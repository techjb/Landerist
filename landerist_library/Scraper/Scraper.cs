using landerist_library.Websites;
using System;
using System.Net;

namespace landerist_library.Scraper
{
    public class Scraper
    {
        private const string UserAgentChrome = "Mozilla/5.0 (compatible; AcmeInc/1.0)";
        public static void SetRealUriToAll()
        {
            var websites = Websites.Websites.GetAll();
            SetRealUri(websites);
        }

        public static void SetRealUriStatusCodeNotOk()
        {

        }

        private static void SetRealUri(List<Website> websites)
        {
            int total = websites.Count;
            int counter = 0;
            int erros = 0;
            var sync = new object();
            Parallel.ForEach(websites,
                //new ParallelOptions() { MaxDegreeOfParallelism = 1}, 
                website =>
                {

                    //if (!website.Uri.Equals("https://www.inmolaseras.es/"))
                    //{
                    //    return;
                    //}
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
                    catch (Exception exception)
                    {
                        //var uri = website.Uri;
                        erros++;
                    }

                });
        }


        private static void SetRealUri(Website website)
        {
            HttpClientHandler handler = new()
            {
                AllowAutoRedirect = false
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgentChrome);
            HttpRequestMessage request = new(HttpMethod.Head, website.Uri);

            var response = client.SendAsync(request).GetAwaiter().GetResult();
            var statusCode = response.StatusCode;
            if (response.StatusCode.Equals(HttpStatusCode.Found)
                || response.StatusCode.Equals(HttpStatusCode.PermanentRedirect)
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
            website.UpdateUriStatusCode((short)statusCode);
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
                UpdateRobotsAndIpAddressAsync(website);
            });
        }

        private static void UpdateRobotsAndIpAddressAsync(Website website)
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
            try
            {
                IPAddress[] ipAddresses = Dns.GetHostAddresses(website.Domain);
                if (ipAddresses.Length > 0)
                {
                    return ipAddresses[0].ToString();
                }                
            }
            catch { }
            return string.Empty;
            
        }

        public static string GetRobotsTxtAsync(Website website)
        {
            var httpClient = new HttpClient();
            var robotsTxtUrl = new Uri(website.Uri, "/robots.txt");
            try
            {
                var response = httpClient.GetAsync(robotsTxtUrl).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {

            }

            return string.Empty;
        }
    }
}
