using HtmlAgilityPack;
using landerist_library.Database;
using landerist_library.Tools;

namespace landerist_library.Insert.IdAgencies
{
    public class IdAgenciesSetAgencyUrls
    {
        public static void Start()
        {
            Console.WriteLine("Reading Id urls ..");
            var hashSet = IdAgenciesUrls.GetNotScrapped();
            int total = hashSet.Count;
            int counter = 0;
            int errors = 0;
            int noErrors = 0;
            Parallel.ForEach(hashSet, new ParallelOptions()
            {
                MaxDegreeOfParallelism = 45,
            }, url =>
            {
                var agencyUrl = GetAgencyUrl(url);
                if (agencyUrl != null)
                {
                    Interlocked.Increment(ref noErrors);
                }
                else
                {
                    Interlocked.Increment(ref errors);
                }
                IdAgenciesUrls.Update(url, agencyUrl);

                Interlocked.Increment(ref counter);
                var percentage = Math.Round((double)counter * 100 / total, 2);
                Console.WriteLine(counter + "/" + total + " (" + percentage + "%) " +
                    "Errors: " + errors + " No errors: " + noErrors);
            });
        }

        private static string? GetAgencyUrl(string url)
        {
            string? agencyUrl = null;
            try
            {
                var html = ScrapingBee.DownloadString(url, true);
                HtmlDocument htmlDocument = new();
                htmlDocument.LoadHtml(html);

                if (htmlDocument.GetElementbyId("commercial-name") != null)
                {
                    var link = htmlDocument.DocumentNode.SelectSingleNode("//a[@class='icon-new-tab']");
                    if (link != null)
                    {
                        agencyUrl = link.GetAttributeValue("href", string.Empty).Trim();
                    }
                    else
                    {
                        agencyUrl = string.Empty;
                    }
                }
            }
            catch (Exception exeption)
            {
                Console.WriteLine($"Error: {exeption.Message}");
            }
            return agencyUrl;
        }
    }
}
