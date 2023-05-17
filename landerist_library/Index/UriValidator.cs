using landerist_library.Configuration;

namespace landerist_library.Index
{
    internal class UriValidator
    {
        private static readonly HashSet<string> ProhibitedWords_ES = new(StringComparer.OrdinalIgnoreCase)
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
            "servicio",
            "acercade",
            "consejo",
            "quienes",
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
            "hipoteca"
        };

        public static bool IsValid(Uri uri)
        {
            if(Config.TRAINING_MODE)
            {
                return true;
            }

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

                foreach (string word in ProhibitedWords_ES)
                {
                    if (directoryCleaned.StartsWith(word))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
