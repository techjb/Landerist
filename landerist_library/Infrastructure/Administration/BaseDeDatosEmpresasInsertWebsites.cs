using landerist_library.Application.Administration;
using landerist_library.Application.Websites;
using landerist_library.Configuration;
using landerist_library.Tools;
using landerist_library.Websites;
using System.Data;


namespace landerist_library.Infrastructure.Administration
{
    public class BaseDeDatosEmpresasInsertWebsites(IWebsiteAdministrationService websites, IWebsiteNetworkService network, IWebsiteSitemapService sitemaps) : WebsitesInserter(true, websites, network, sitemaps)
    {
        public void Start()
        {
            string file = AppConfig.INSERT_DIRECTORY + @"basededatosempresas.net\Inmobiliarias.csv";
            DataTable dataTable = Tools.Csv.ToDataTable(file);
            var uris = ToList(dataTable, "Website");
            Insert(uris);
        }
    }
}
