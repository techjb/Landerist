namespace landerist_library.Insert
{
    internal class BlockedDomains
    {
        private static readonly HashSet<string> Domains =
        [
            "google.com",
            "google.es",
            "idealista.com",
            "fotocasa.es",
            "yaencontre.com",
            "pisos.com",
            "habitaclia.com",
            "todopisos.es",
            "tucasa.com",
            "hinkspain.com",
            "aquimicasa.net",
            "arkadia.com",
            "costadelhome.com",
            "domaza.es",
            "green-acres.es",
            "hogaria.net",
            "immomaresme.com",
            "indomio.es",
            "immovario.com",
            "kyero.com",
            "listglobally.com",
            "1001portales.com",
            "globaliza.com",
            "luxuryestate.com",
            "miracasa.es",
            "misoficinas.es",
            "aplaceinthesun.com",
            "spainhomes.com",
            "spainhouses.net",
            "vivados.es",
            "zoopla.co.uk",
            "terrenos.es",
            "thinkspain.com",
            "trovimap.com",
            "inmobiliaria.com",
            "ventaobranueva.es",
            "enalquiler.com",
            "trovit.es",
            "nestoria.es",
            "mitula.com",
            "nuroa.com",
            "rightmove.co.uk",
            "belbex.com",
            "luxuryestate.com",
            "milanuncios.com",
            "fincasrusticas.org",
            "poligonoyparcela.com",
            "tablondeanuncios.com",
            "anunciofinca.com",
            "todopisos.es",
            "housage.es",
            "viveku.es",
            "blogspot.com",
            "kyero.com",            

        ];

        public static bool IsBlocked(Uri uri)
        {
            return IsBlocked(uri.Host);
        }

        public static bool IsBlocked(string host)
        {
            if (!host.Contains('.'))
            {
                return false;
            }

            string[] parts = host.Split('.');
            string domain = parts[^2] + "." + parts[^1];

            return Domains.Contains(domain, StringComparer.OrdinalIgnoreCase);
        }

        public static void DeleteBlockedWebsites()
        {
            var websites = Websites.Websites.GetAll();
            int counter = 0;
            foreach (var website in websites)
            {
                if (IsBlocked(website.MainUri))
                {
                    website.Delete();
                    counter++;
                }
            }
            Console.WriteLine("Deleted: " + counter + " websites.");
        }
    }
}
