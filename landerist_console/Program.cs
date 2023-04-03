using landerist_library;
using landerist_library.Insert;
using landerist_library.Scrape;
using landerist_library.Websites;
using landerist_library.Export;
using System.Reflection.Metadata;
using System.Text;

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
            //var uri = new Uri("https://www.goolzoom.com");
            var uri = new Uri("https://www.saroga.es/");
            var website = new Website(uri);
            //website.Remove();
            
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

            new Scraper().ScrapeMainPage(website);
            //new Scrape().AllPages();

            //new Csv().Export(true);
            //new Json().Export(true);

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