using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace landerist_library.Download
{
    public class SeleniumDownloader
    {

        public static void GetChrome(Uri uri)
        {
            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;

            ChromeOptions options = new();
            //options.AddArgument("headless"); // Ejecutar en modo sin cabeza (sin UI)

            options.AddUserProfilePreference("profile.default_content_setting_values.images", 2); // 2: bloquear imágenes
            options.AddUserProfilePreference("profile.default_content_setting_values.stylesheets", 2); // No funciona, no es posible
            options.AddUserProfilePreference("profile.default_content_setting_values.cookies", 2);
            options.AddUserProfilePreference("profile.default_content_setting_values.plugins", 2);
            options.AddUserProfilePreference("profile.default_content_setting_values.popups", 2);
            options.AddUserProfilePreference("profile.default_content_setting_values.fonts", 2);
            options.AddUserProfilePreference("profile.default_content_setting_values.css", 2);// No funciona, no es posible
            options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 2);
            options.AddUserProfilePreference("profile.default_content_setting_values.media_stream", 2);

            // Establecer la política de seguridad de contenido para bloquear otros recursos
            //string contentSecurityPolicy = "default-src 'self' 'unsafe-inline' 'unsafe-eval'; object-src 'none'";
            //options.AddArgument($"--content-security-policy={contentSecurityPolicy}");

            // Inicializa el driver de Chrome (asegúrate de que ChromeDriver esté instalado y en tu PATH)
            IWebDriver driver = new ChromeDriver(driverService, options);


            // Navega a una página web
            driver.Navigate().GoToUrl(uri);

            // Realiza acciones o extrae información de la página web

            string pageHtml = driver.PageSource;

            // Cierra el navegador y libera recursos
            driver.Quit();

            //HtmlDocument htmlDocument = new();
            //htmlDocument.LoadHtml(pageHtml);


        }

        public static void GetFirefox(Uri uri)
        {
            // Configura el FirefoxDriver
            FirefoxDriverService driverService = FirefoxDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true; // Para ocultar la ventana de comandos

            // Crea una instancia de FirefoxDriver
            FirefoxOptions options = new();

            // Deshabilitar la carga de imágenes
            options.SetPreference("permissions.default.image", 2); // 2: bloquear imágenes

            // Deshabilitar la carga de archivos CSS
            options.SetPreference("permissions.default.stylesheet", 2); // 2: bloquear hojas de estilo

            // Deshabilitar cookies
            options.SetPreference("network.cookie.cookieBehavior", 2); // 2: deshabilitar cookies

            // Establecer la política de seguridad de contenido para bloquear otros recursos
            //string contentSecurityPolicy = "default-src 'self' 'unsafe-inline' 'unsafe-eval'; object-src 'none'";
            //options.SetPreference("security.csp.enable", true);
            //options.SetPreference("security.csp.experimentalEnabled", true);
            //options.SetPreference("security.csp.source.pre_path", contentSecurityPolicy);

            var firefoxProfile = new FirefoxProfile();

            firefoxProfile.SetPreference("permissions.default.stylesheet", 2);
            firefoxProfile.SetPreference("permissions.default.image", 2);
            options.Profile = firefoxProfile;


            //IWebDriver driver = new FirefoxDriver(driverService, options);
            IWebDriver driver = new FirefoxDriver(driverService, options);

            // Navega a la página web
            driver.Navigate().GoToUrl(uri);

            string pageHtml = driver.PageSource;

            // Cierra el navegador y libera recursos
            driver.Quit();
        }
    }
}
