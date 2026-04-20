using landerist_library.Websites;
using landerist_library.Index;

namespace landerist_tests
{
    internal static class IndexTests
    {
        public static void Run()
        {

            //var Website = new Website(new Uri("https://www.tecnocasa.es/"));
            var Website = new Website(new Uri("https://www.engelvoelkers.com/es/es"));
            
            //Websites.UpdateNumPages(Website);   
            Website.SetSitemap();
        }
    }
}
