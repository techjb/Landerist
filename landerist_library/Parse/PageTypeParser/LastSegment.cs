using landerist_library.Index;
using landerist_library.Websites;

namespace landerist_library.Parse.PageTypeParser
{
    public class LastSegment
    {
        private static readonly HashSet<string> ProhibitedLatestSegments = new(StringComparer.OrdinalIgnoreCase)
        {
            "promociones",
            "propiedades",
            "inmuebles",
            "mapa",
            "casas",
            "pisos",
            "alquiler",
            "garajes",
            "locales",
            "venta",
            "solares",
            "buscador",
            "oficinas",
            "destacados",
            "mapa-del-sitio",
            "privado",
            "naves-industriales",
            "propietarios",
            "venta-alquiler",
            "obra-nueva",
            "chalets",
            "en_venta",
            "vender",
            "comprar",
            "trabajos",
            "inmobiliaria",
            "edificios",
            "venta-chalets",
            "alquiler-locales",
            "trasteros",
            "venta-garajes",
            "promociones.php",
            "barcelona",
            "madrid",
            "en-barcelona",
            "en-madrid",
            "marbella",
            "valencia",
            "all",
            "alquilar",
            "centro",
            "%c3%baltima-hora",
            "última-hora",
            "captación",
            "captacion",
            "piso",
            "casa",
            "inicio",
            "apartamento",
            "alquiler.php",
            "alquiler_vacacional.php",
            "venta.php",
            "novedades.php",
            "promociones.php",
            "agencia-inmobiliaria.php",
            "setup-config.php",
            "search-form-top.php",
            "buscador.php",
            "destacados.php",
            "alquilar-tu-vivienda.php",
            "search-form-top-obra-nueva.php",
            "popup.php",
            "ventas.php",
            "comprar.php",
            "propiedades.php",
            "ventas.php",
            "index.php",
            "vender.php",
            "alertas.php",
            "destacados.php",
            "arquitectura.php",
            "vender_alquiler.php",
            "ipc.php",
            "inmuebles.php",
            "legislacion.php",
            "venta-casas",
            "venta-garajes",
            "vacacional",
            "alquiler-pisos",
            "venta-pisos",
            "venta-locales",
            "alquiler-locales",
            "locales-o-naves",
            "venta-oficinas",
            "alquiler.html",
            "venta.html",
            "inicio.html",
            "index.html",
            "mis-propiedades.html",
            "inmuebles.html",
            "inmuebles.html",
            "404.html",
            "alertas.html",
            "locales-comerciales.html",
            "mapa.html",
            "vender.html",
            "buscador.html",
            "en-venta-1.html",
            "en-alquiler-2.html",
            "inmobiliaria.html",
            "propiedades.html",
            "venta-edificios_singulares",
            "terrenos",
            "parcelas",
            "unifamiliares",
            "alquiler-oficinas",
            "pisos-apartamentos",
            "busqueda-avanzada",
            "alquiler-naves",
            "email-protection",
            "venta-naves",
            "vivienda",
            "registro",
            "mapa-footer",
            "eres-propietario",
            "otros-tipos",
            "i-t-e-inspeccion-tecnica-de-edificios",
            "preguntas-frecuentes",
            "comercializados",
            "alquile-o-venda-su-propiedad",
            "obra-nueva-en-",
            "terms_of_use",
            "garaje-trastero-en-",
            "properties",
            "rss"
        };

        public static bool NotListingByLastSegment(Uri uri)
        {
            if (!string.IsNullOrEmpty(uri.Query))
            {
                return false;
            }
            var lastSegment = GetLastSegment(uri);
            return ProhibitedLatestSegments.Any(item => lastSegment.Equals(item, StringComparison.OrdinalIgnoreCase));
        }


        private static string GetLastSegment(Uri uri)
        {
            string[] segments = uri.Segments;
            string segment = segments[0].TrimEnd('/').ToLower();
            for (int i = segments.Length - 1; i >= 0; i--)
            {
                var currentSegment = segments[i].TrimEnd('/').ToLower();
                if (!currentSegment.Equals(string.Empty))
                {
                    segment = currentSegment;
                    break;
                }
            }
            return segment;
        }

        public static void FindProhibitedEndsSegments()
        {
            var urls = Pages.GetUris();
            var dictionary = ToDictionary(urls);
            int count = dictionary.Count;
            dictionary = dictionary.Take(100).ToDictionary(x => x.Key, x => x.Value);
            foreach (var entry in dictionary)
            {
                float percentage = entry.Value * 100 / count;
                Console.WriteLine(entry.Key + " " + entry.Value + " (" + Math.Round(percentage, 2) + "%)");
            }
        }

        private static Dictionary<string, int> ToDictionary(List<string> urls)
        {
            Dictionary<string, int> dictionary = [];
            int total = urls.Count;
            var sync = new object();
            int counter = 0;
            Parallel.ForEach(urls,
              new ParallelOptions()
              {
                  //MaxDegreeOfParallelism = Configuration.Config.MAX_DEGREE_OF_PARALLELISM
              },
              url =>
              {
                  Interlocked.Increment(ref counter);
                  Console.WriteLine(counter + " / " + total);

                  if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                  {
                      return;
                  }
                  if (!string.IsNullOrEmpty(uri.Query))
                  {
                      return;
                  }
                  if (ProhibitedUrls.IsProhibited(uri, LanguageCode.es))
                  {
                      return;
                  }

                  var lastSegment = GetLastSegment(uri);
                  if (ProhibitedLatestSegments.Contains(lastSegment))
                  {
                      return;
                  }

                  if (!lastSegment.EndsWith(".html"))
                  {
                      return;
                  }

                  lock (sync)
                  {
                      if (dictionary.TryGetValue(lastSegment, out int value))
                      {
                          dictionary[lastSegment] = value + 1;
                      }
                      else
                      {
                          dictionary[lastSegment] = 1;
                      }
                  }
              });


            dictionary = dictionary.Where(pair => pair.Value > 2).ToDictionary(pair => pair.Key, pair => pair.Value);
            var sortedDict = from entry in dictionary orderby entry.Value descending select entry;
            dictionary = sortedDict.ToDictionary(x => x.Key, x => x.Value);
            return dictionary;
        }

    }
}
