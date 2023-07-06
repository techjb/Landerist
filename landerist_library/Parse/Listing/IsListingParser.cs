using landerist_library.Index;
using landerist_library.Websites;
using System.Collections.Concurrent;
using System.IO;

namespace landerist_library.Parse.Listing
{
    public class IsListingParser
    {
        private static readonly HashSet<string> ProhibitedEndsSegments = new(StringComparer.OrdinalIgnoreCase)
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
            "alquiler.php",
            "venta.php",
            "inicio",
            "apartamento",
            "promociones.php",
            "venta-garajes",            
            "vacacional",
            "alquiler-pisos",
            "venta-pisos",
            "venta-locales",
            "alquiler-locales",
            //"alquiler.html",
            //"venta.html"
            "venta-edificios_singulares",
            "terrenos",
            "parcelas",
            "unifamiliares",
            "alquiler-oficinas",
            "pisos-apartamentos",
            "busqueda-avanzada",
            "search-form-top.php",
            "alquiler-naves",
            "email-protection",
            "venta-naves",
            "vivienda",
            "registro",
            "mapa-footer",
            "eres-propietario",
            "es",
            "en",
            "fr",
            "de",
            "ca",
            "ru",
            "1",
            "2",
            "3",
            "4",
            "i-t-e-inspeccion-tecnica-de-edificios",





        };
        public static bool IsListing(Page page)
        {
            if (page == null)
            {
                return false;
            }
            if (page.IsMainPage())
            {
                return false;
            }
            if (IsProhibitedEndSegment(page.Uri))
            {
                return false;
            }

            page.SetResponseBodyText();
            if (ResponseBodyTextIsError(page.ResponseBodyText))
            {
                page.ResponseBodyText = null;
                return false;
            }
            return true;
        }

        public static bool IsProhibitedEndSegment(Uri uri)
        {
            if (!string.IsNullOrEmpty(uri.Query))
            {
                return false;
            }
            var lastSegment = GetLastSegment(uri);
            return ProhibitedEndsSegments.Any(item => lastSegment.Equals(item, StringComparison.OrdinalIgnoreCase));
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

        private static bool ResponseBodyTextIsError(string? responseBodyText)
        {
            if (responseBodyText == null)
            {
                return false;
            }
            return
                responseBodyText.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.StartsWith("404", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.Contains("Página no encontrada", StringComparison.OrdinalIgnoreCase) ||
                responseBodyText.Contains("Page Not found", StringComparison.OrdinalIgnoreCase)
                ;
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
            Dictionary<string, int> dictionary = new();
            int total = urls.Count;
            var sync = new object();
            int counter = 0;
            Parallel.ForEach(urls,
              //new ParallelOptions() { MaxDegreeOfParallelism = 1},                
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
                  if (ProhibitedEndsSegments.Contains(lastSegment))
                  {
                      return;
                  }

                  lock (sync)
                  {
                      if (dictionary.ContainsKey(lastSegment))
                      {
                          dictionary[lastSegment] = dictionary[lastSegment] + 1;
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
