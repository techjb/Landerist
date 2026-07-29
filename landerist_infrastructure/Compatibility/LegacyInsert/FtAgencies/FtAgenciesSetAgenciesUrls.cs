using HtmlAgilityPack;
using landerist_library.Database;
using landerist_library.Tools;

namespace landerist_library.Insert.FtAgencies
{
    public class FtAgenciesSetAgenciesUrls
    {
        public static void Start()
        {
            Console.WriteLine("Reading Id urls ..");
            var hashSet = FtAgenciesUrls.GetNotScrapped();
            int total = hashSet.Count;
            int counter = 0;
            int errors = 0;
            int noErrors = 0;
            Parallel.ForEach(hashSet, new ParallelOptions()
            {
                //MaxDegreeOfParallelism = Config.MAX_DEGREE_OF_PARALLELISM
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
                FtAgenciesUrls.Update(url, agencyUrl);

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

                var link = htmlDocument.DocumentNode
                    .SelectNodes("//a[@class='sui-LinkBasic' and @rel='nofollow noopener noreferrer']")
                    ?.Select(a => a.GetAttributeValue("href", string.Empty))
                    .Where(href => !string.IsNullOrEmpty(href) && !href.StartsWith("https://www.google."))
                    .Distinct()
                    .ToList() ?? [];
                if (link.Count.Equals(1))
                {
                    agencyUrl = link[0];
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
