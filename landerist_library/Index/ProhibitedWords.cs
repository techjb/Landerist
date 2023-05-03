namespace landerist_library.Index
{
    internal class ProhibitedWords
    {
        private static readonly HashSet<string> ES = new(StringComparer.OrdinalIgnoreCase)
        {
            "contact",
            "politic",
            "privacid",
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
            "descarga"
        };

        public static bool Contains(Uri uri)
        {
            var absolutePaths = uri.AbsolutePath.Split('/');
            if (absolutePaths.Length <= 1)
            {
                return true;
            }

            string lastPath = absolutePaths[^1];
            lastPath = lastPath
               .Replace("-", string.Empty)
               .Replace("_", string.Empty)
               .ToLower()
               ;
            if (string.IsNullOrEmpty(lastPath))
            {
                return true;
            }
            foreach (string word in ES)
            {
                if (lastPath.StartsWith(word))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
