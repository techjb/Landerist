using landerist_library.Websites;
using landerist_library.Index;

namespace landerist_tests
{
    internal static class IndexTests
    {
        public static void Run()
        {

            
            var Website = new Website(new Uri("https://www.tecnocasa.es/"));
            Website.SetSitemap();
        }
    }
}
