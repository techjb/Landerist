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
            "resize"
        };

        public static bool Contains(Uri uri)
        {
            string absolutePath = uri.AbsolutePath
               .Replace("-", string.Empty)
               .Replace("_", string.Empty)
               .Replace("/", string.Empty)
               .ToLower()
               ;
            foreach(string word in ES)
            {
                if (absolutePath.StartsWith(word))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
