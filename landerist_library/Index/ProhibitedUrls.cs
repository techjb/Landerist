using landerist_library.Websites;

namespace landerist_library.Index
{
    public class ProhibitedUrls
    {
        private static readonly HashSet<string> Prohibited_ES = new(StringComparer.OrdinalIgnoreCase)
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
        };

        public static bool IsProhibited(Uri uri, LanguageCode languageCode)
        {
            var prohibitedContains = GetProhibitedContains(languageCode);
            var absolutePath = uri.AbsolutePath.ToLower()
                    .Replace("-", string.Empty)
                    .Replace("_", string.Empty);

            var directories = absolutePath.Split('/');
            foreach (var directory in directories)
            {
                if (string.IsNullOrEmpty(directory))
                {
                    continue;
                }

                if (prohibitedContains.Any(directory.Contains))
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
                LanguageCode.es => Prohibited_ES,
                _ => [],
            };
        }

        public static void FindNewProhibitedStartsWith()
        {
            var urls = Pages.GetUris(false);
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
            dictionary = sortedDict.ToDictionary(x => x.Key, x => x.Value);
            dictionary = dictionary.Take(300).ToDictionary(x => x.Key, x => x.Value);
            return dictionary;
        }

        private static List<string> GetDirectories(Uri uri)
        {
            var prohibitedContains = GetProhibitedContains(LanguageCode.es);
            var absolutePath = uri.AbsolutePath.ToLower()
                    .Replace("-", string.Empty)
                    .Replace("_", string.Empty);

            var directories = absolutePath.Split('/');

            List<string> dirs = [];
            foreach (var directory in directories)
            {
                if (string.IsNullOrEmpty(directory))
                {
                    continue;
                }

                bool isProhibited = prohibitedContains.Any(directory.Contains);
                if (!isProhibited)
                {
                    dirs.Add(directory);
                }

            }
            return dirs;
        }

        private static string GetDirectory(Uri uri)
        {
            var directories = uri.AbsolutePath.ToLower().Split('/');
            foreach (var directory in directories)
            {
                if (directory.Equals(string.Empty)
                    || directory.Equals("es"))
                {
                    continue;
                }
                return directory;
            }
            return string.Empty;
        }
    }
}
