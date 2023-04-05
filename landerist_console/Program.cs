using landerist_library;
using landerist_library.Insert;
using landerist_library.Scrape;
using landerist_library.Websites;
using landerist_library.Export;
using System.Reflection.Metadata;
using System.Text;
using landerist_library.Parse;

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

            string text = "Lorem ipsum dolor sit amet consectetur adipiscing elit sapien feugiat rhoncus dis condimentum dapibus rutrum fermentum, parturient porttitor hac platea gravida laoreet penatibus nisl orci nec hendrerit fames varius mi. Varius felis pretium cubilia vivamus interdum libero dignissim, torquent sociosqu sagittis in bibendum taciti placerat, curabitur parturient maecenas fames primis blandit. Magna praesent euismod scelerisque erat suspendisse cubilia tempus vulputate class ornare, quam imperdiet ante enim mi eu faucibus hendrerit inceptos, consequat vitae orci ligula magnis integer ultrices molestie feugiat.";
            var Characters = text.Length;
            var tokens = ChatGPT.IsTextAllowed(text);
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