using landerist_library.Configuration;
using landerist_library.Websites;
using System.Linq;

namespace landerist_library.Index
{
    public class ProhibitedUrls
    {
        private static readonly HashSet<string> ProhibitedStartsWith_ES = new(StringComparer.OrdinalIgnoreCase)
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

        };

        public static bool IsProhibited(Uri uri, LanguageCode languageCode)
        {
            if (Config.TRAINING_MODE)
            {
                return false;
            }
            var prohibitedStartsWith = GetProhibitedStartsWith(languageCode);
            var directories = uri.AbsolutePath.ToLower().Split('/');
            foreach (var directory in directories)
            {
                if (string.IsNullOrEmpty(directory))
                {
                    continue;
                }
                string directoryCleaned = directory
                    .Replace("-", string.Empty)
                    .Replace("_", string.Empty);

                bool isProhibited = prohibitedStartsWith.Any(item => directoryCleaned.StartsWith(item));
                if (isProhibited)
                {
                    return true;
                }                
            }
            return false;
        }

        private static HashSet<string> GetProhibitedStartsWith(LanguageCode languageCode)
        {
            switch (languageCode)
            {
                case LanguageCode.es: return ProhibitedStartsWith_ES;
                default: return new HashSet<string>();
            }
        }

        public static void FindNewProhibitedStartsWith()
        {
            var urls = Pages.GetIsNotListingUris();
            var dictionary = ToDictionary(urls);
            foreach (var entry in dictionary)
            {
                Console.WriteLine(entry.Key + " " + entry.Value);
            }
        }

        private static Dictionary<string, int> ToDictionary(List<string> urls)
        {
            Dictionary<string, int> dictionary = new();
            foreach (var url in urls)
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    continue;
                }
                
                var directories = GetDirectories(uri);
                foreach(var directory in directories)
                {
                    if (dictionary.ContainsKey(directory))
                    {
                        dictionary[directory] = dictionary[directory] + 1;
                    }
                    else
                    {
                        dictionary[directory] = 1;
                    }
                }
                //string directory = GetDirectory(uri);
                //if (directory.Equals(string.Empty))
                //{
                //    continue;
                //}
                //if (dictionary.ContainsKey(directory))
                //{
                //    dictionary[directory] = dictionary[directory] + 1;
                //}
                //else
                //{
                //    dictionary[directory] = 1;
                //}
            }
            var sortedDict = from entry in dictionary orderby entry.Value descending select entry;
            dictionary = sortedDict.ToDictionary(x => x.Key, x => x.Value);
            dictionary = dictionary.Take(300).ToDictionary(x => x.Key, x => x.Value);
            return dictionary;
        }

        private static List<string> GetDirectories(Uri uri)
        {
            var directories = uri.AbsolutePath.ToLower().Split('/');
            List<string> dirs = new();
            var prohibitedStartsWith = GetProhibitedStartsWith(LanguageCode.es);
            foreach (var directory in directories)
            {
                string directoryCleaned = directory
                    .Replace("-", string.Empty)
                    .Replace("_", string.Empty);

                if (directoryCleaned.Equals(string.Empty))
                {
                    continue;
                }

                bool isProhibited = prohibitedStartsWith.Any(item => directoryCleaned.StartsWith(item));
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
