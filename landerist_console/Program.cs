using landerist_library;
using landerist_library.Insert;
using landerist_library.Scrape;
using landerist_library.Websites;
using landerist_library.Export;
using System.Reflection.Metadata;
using System.Text;
using landerist_library.Parse;
using Newtonsoft.Json;

namespace landerist_console
{
    internal class Program
    {

        private static DateTime DateStart;
        
        static void Main(string[] args)
        {
            Console.Title = "Landerist Console";            
            SetDateStart();
            Config.Init(false);
            Run();
            SetFinish();
            EndBeep();
        }

        private static void SetDateStart()
        {
            DateStart = DateTime.Now;
            string textStarted =
                "STARTED at " + DateStart.ToShortDateString() + " " + DateStart.ToString(@"hh\:mm\:ss") + "\n";
            Console.WriteLine(textStarted);
        }

        private static void SetFinish()
        {
            DateTime dateFinished = DateTime.Now;
            string textFinished =
                "FINISHED at " + dateFinished.ToShortDateString() + " " + dateFinished.ToString(@"hh\:mm\:ss") +
                "\nDuration: " + (dateFinished - DateStart).ToString(@"hh\:mm\:ss") + ". ";
            Console.WriteLine("\n" + textFinished);
        }

        private static void Run()
        {

            var uri = new Uri("https://www.saroga.es/");
            //var uri = new Uri("https://mabelan.es/");
            //var uri = new Uri("https://www.saguar.immo/");
            //var uri = new Uri("https://www.inmolocalgestion.com/");
            //var uri = new Uri("https://www.expimad.com/");
            
            var website = new Website(uri);
            //website.Remove(); return;

            //new UrisInserter().Insert(uri);
            //new UrisInserter().FromCsv();
            //new Websites().RemoveBlockedDomains();

            //new Websites().SetHttpStatusCodesToNull();
            //new Websites().InsertUpdateUrisFromNotOk();
            //new Websites().SetHttpStatusCodesToAll();            
            //new Websites().SetRobotsTxtToHttpStatusCodeOk();
            //new Websites().SetIpAdressToAll();            
            //new Websites().CountCanAccesToMainUri();
            //new Websites().CountRobotsSiteMaps();
            //new Websites().CalculateHashes();
            //new Websites().InsertMainPages();


            //website.SetHttpStatusCode();
            //website.SetRobotsTxt();
            //website.SetIpAddress();
            //website.InsertMainPage();

            //new Scraper().ScrapeMainPage(website);
            //new Scraper().ScrapePages(website);
            //new Scraper().ScrapeAllPages();

            //new Csv().Export(true);
            //new Json().Export(true);

            //string squema = ListingResponseSchema.GetSchema();            
            //ChatGPT.IsRequestAllowed("tseaadsf");

            string json = "{\"fecha de publicación\": \"ABCDEFGHIJKLMNOPQRS\",\"tipo de operación\": \"venta\",\"tipo de inmueble\": \"edificio\",\"subtipo de inmueble\": \"chalet independiente\",\"precio del anuncio\": 0.0,\"descripción del anuncio\": \"ABCDEFGHIJKLMNOPQRSTUVWXYZA\",\"referencia del anuncio\": \"ABCDE\",\"dirección del inmueble\": \"ABCDEFGHIJKLMNOPQRSTUVWXY\",\"referencia catastral\": \"ABCDEFGHIJKLMNOPQRSTUVWXYZA\",\"metros cuadrados del inmueble\": 0.0,\"metros cuadrados de la parcela\": 0.0,\"año de construcción\": 0.0,\"estado del inmueble\": \"a reformar\",\"plantas del edificio\": 0.0,\"planta del inmueble\": \"ABCD\",\"número de dormitorios\": 0.0,\"número de baños\": 0.0,\"número de parkings\": 0.0,\"tiene terraza\": true,\"tiene jardín\": true,\"tiene garaje\": true,\"tiene parking para moto\": true,\"tiene piscina\": false,\"tiene ascensor\": true,\"tiene acceso para discapacitados\": true,\"tiene trastero\": true,\"está amueblado\": true,\"no está amueblado\": false,\"tiene calefacción\": false,\"tiene aire acondicionado\": false,\"permite mascotas\": false,\"tiene sistemas de seguridad\": false}";
            ListingResponse? listingResponse = JsonConvert.DeserializeObject<ListingResponse>(json);
            var listing = listingResponse.ToListing(new Page(website));
            new landerist_library.ES.Listings().Insert(listing);
        }

        private static void EndBeep()
        {
            while (true)
            {
                Console.Beep(400, 1000);
                Thread.Sleep(300);
            }
        }
    }
}