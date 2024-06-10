using landerist_library.Configuration;
using landerist_library.Tools;
using System.Data;


namespace landerist_library.Insert.BaseDeDatosEmpresas
{
    public class InsertBaseDeDatosEmpresas() : WebsitesInserter(true)
    {
        public static void Start()
        {
            string file = PrivateConfig.INSERT_DIRECTORY + @"basededatosempresas.net\Inmobiliarias.csv";
            DataTable dataTable = Csv.ToDataTable(file);
            var uris = ToList(dataTable, "Website");
            Insert(uris);
        }
    }
}
