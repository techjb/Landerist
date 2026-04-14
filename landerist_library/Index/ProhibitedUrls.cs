using System.Globalization;
using System.Text;
using landerist_library.Websites;

namespace landerist_library.Index
{
    public class ProhibitedUrls
    {
        public static readonly HashSet<string> Prohibited_ES = new(StringComparer.OrdinalIgnoreCase)
        {
            "contact",
            "politic",
            "privacid",
            "privacy",
            "aviso",
            "legal",
            "filosof",
            "nosotros",
            "conozca",
            "conoce",
            "servicio",
            "acercade",
            "consejo",
            "quien",
            "colabora",
            "equipo",
            "noticia",
            "tasacion",
            "empresa",
            "nuestr",
            "trabaja",
            "empleo",
            "localiza",
            "dondeesta",
            "valora",
            "actualidad",
            "articulos",
            "blog",
            "vendetu",
            "condiciones",
            "terminos",
            "términos",
            "historia",
            "franquicia",
            "empleo",
            "publica",
            "favorit",
            "experiencia",
            "donacion",
            "quiero",
            "rgpd",
            "mapaweb",
            "resize",
            "descarga",
            "cookie",
            "agente",
            "queja",
            "estadistica",
            "hipoteca",
            "abogado",
            "informacionlegal",
            "seguro",
            "financia",
            "gestoria",
            "login",
            "homestaging",
            "redessociales",
            "usuario",
            "user",
            "buscamos",
            "soypropietario",
            "certificadoenergetico",
            "asesora",
            "otrosservicios",
            "anuncia",
            "error",
            "nuevadireccion",
            "smarterror",
            "reformas",
            "certificado",
            "colegio",
            "avislegal",
            "textolegal",
            "sobrenosotros",
            "wplogin",
            "mapadesitio",
            "alerta",
            "quehacemos",
            //"searchform",
            "hipoteca",
            "encontrar",
            "boletín",
            "comovender",
            "venderconchm",
            "inmobiliariachm",
            "camaraseuropa",
            "legislacion",
            "enlacesinteres",
            "asesoria",
            "situacion",
            "flipover",
            "hosteleria",
            "hipoteca",
            "noticias",
            "quiénessomos",
            "financiación",
            "financiacion",
            "imprimi",
            "print",
            "whatsapp"
        };

        private static readonly HashSet<string> ProhibitedEsNormalized =
            Prohibited_ES.Select(NormalizeSegment).ToHashSet(StringComparer.Ordinal);

        public static bool IsProhibited(Uri uri, LanguageCode languageCode)
        {
            ArgumentNullException.ThrowIfNull(uri);

            var prohibitedContains = GetProhibitedContains(languageCode);
            if (prohibitedContains.Count == 0)
            {
                return false;
            }

            var directories = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            foreach (var directory in directories)
            {
                var normalizedDirectory = NormalizeSegment(directory);
                if (normalizedDirectory.Length == 0)
                {
                    continue;
                }

                if (prohibitedContains.Any(normalizedDirectory.Contains))
                {
                    return true;
                }
            }

            return false;
        }

        private static HashSet<string> GetProhibitedContains(LanguageCode languageCode)
        {
            return languageCode switch
            {
                LanguageCode.es => ProhibitedEsNormalized,
                _ => []
            };
        }

        public static void FindNewProhibitedStartsWith()
        {
            var urls = Pages.Pages.GetUris(false);
            var dictionary = ToDictionary(urls);

            foreach (var entry in dictionary)
            {
                Console.WriteLine(entry.Key + " " + entry.Value);
            }
        }

        private static Dictionary<string, int> ToDictionary(List<string> urls)
        {
            Dictionary<string, int> dictionary = [];

            foreach (var url in urls)
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    continue;
                }

                var directories = GetDirectories(uri);
                foreach (var directory in directories)
                {
                    if (dictionary.TryGetValue(directory, out int value))
                    {
                        dictionary[directory] = value + 1;
                    }
                    else
                    {
                        dictionary[directory] = 1;
                    }
                }
            }

            var sortedDict = from entry in dictionary orderby entry.Value descending select entry;
            dictionary = sortedDict.Take(300).ToDictionary(x => x.Key, x => x.Value);

            return dictionary;
        }

        private static List<string> GetDirectories(Uri uri)
        {
            var prohibitedContains = GetProhibitedContains(LanguageCode.es);
            var directories = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            List<string> dirs = [];

            foreach (var directory in directories)
            {
                var normalizedDirectory = NormalizeSegment(directory);
                if (normalizedDirectory.Length == 0)
                {
                    continue;
                }

                bool isProhibited = prohibitedContains.Any(normalizedDirectory.Contains);
                if (!isProhibited)
                {
                    dirs.Add(normalizedDirectory);
                }
            }

            return dirs;
        }

        private static string NormalizeSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var cleaned = Uri.UnescapeDataString(value)
                .ToLowerInvariant()
                .Replace("-", string.Empty)
                .Replace("_", string.Empty);

            var normalized = cleaned.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);

            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
