using landerist_library.Configuration;
using landerist_library.Database;

namespace landerist_library.Insert.IdAgencies
{
    public class IdAgenciesExport
    {
        public static void Start()
        {
            var urls = IdAgenciesUrls.GetAgencies();
            var hosts = Websites.Websites.GetHosts();
            HashSet<string> uris = [];
            int errors = 0;
            int alreadyInserted = 0;
            int total = urls.Count;
            foreach (var url in urls)
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
                {
                    errors++;
                    continue;
                }

                if (hosts.Contains(uri.Host))
                {
                    alreadyInserted++;
                    continue;
                }
                uris.Add(uri.ToString());
            }

            Console.WriteLine(
                "Urls: " + total + " " +
                "Errors: " + errors + " " +
                "Already inserted: " + alreadyInserted + " " +
                "Uris: " + uris.Count);

            string fileName = PrivateConfig.INSERT_DIRECTORY + @"IdAgencies\Urls.csv";
            Tools.Csv.Write(uris, fileName);
        }
    }
}
