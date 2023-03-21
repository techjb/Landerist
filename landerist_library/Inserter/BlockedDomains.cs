﻿namespace landerist_library.Inserter
{
    internal class BlockedDomains
    {
        private static readonly HashSet<string> Domains = new ()
        {
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
        };

        public static bool IsBlocked(Uri uri)
        {
            string[] parts = uri.Host.Split('.');  
            string domain = parts[^2] + "." + parts[^1];

            return Domains.Contains(domain, StringComparer.OrdinalIgnoreCase);
        }
    }
}