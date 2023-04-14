namespace landerist_library.Index
{
    internal class ProhibitedWords
    {
        private static readonly HashSet<string> ES = new(StringComparer.OrdinalIgnoreCase)
        {
            "contact",
            "privacidad",
            "legal",
            "filosofia",
            "nosotros",
            "conozcanos",
            "servicios",
            "acercade",
            "consejos",
            "quienessomos",
            "colaboradores",
            "equipo",
            "noticias",
            "tasacion",
            "empresa",
            "trabaja",
            "empleo",
            "localizacion",
            "dondeestamos",
            "valora",
            "actualidad",
            "articulos",
            "blog",
            "vendetuinmueble",
            "condiciones",
            "historia",
            "franquicia",
            "empleo",
            "publicatuinmueble",
            "favorit",
            "experiencia",
            "donaciones",
            "quierovender",
            "rgpd",

        };

        public static bool Contains(Uri uri)
        {
            string absolutePath = uri.AbsolutePath
               .Replace("-", string.Empty)
               .Replace("_", string.Empty)
               .Replace("/", string.Empty)
               ;
            foreach(string word in ES)
            {
                if (absolutePath.Contains(word))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
